-- =============================================================================
-- SP:          pay.APLICAR_MOVIMIENTO
-- Descripcion: Aplica un credito o debito sobre UNA cuenta de forma atomica:
--              bloquea la fila, valida saldo, escribe el asiento en
--              pay.MOVIMIENTO y actualiza pay.CUENTA en la misma transaccion.
--              Es la unica via por la que cambia el saldo fuera del pago QR
--              (recargas, ajustes, reversas).
--
--              Es idempotente: si ya existe un asiento con la misma clave sobre
--              la misma cuenta, no aplica nada y devuelve el ID del original.
--              Un reintento del llamador (timeout de red, retry de gRPC) por lo
--              tanto no duplica dinero.
--
-- Parametros:
--   @CUEN_NUMERO_VC        VARCHAR(20)    Numero de cuenta
--   @MOVI_TIPO_VC          VARCHAR(10)    'CREDITO' o 'DEBITO'
--   @MOVI_MONTO_DE         DECIMAL(18,8)  Monto, siempre positivo
--   @MOVI_REFERENCIA_VC    VARCHAR(64)    Referencia / glosa corta
--   @MOVI_IDEMPOTENCIA_VC  VARCHAR(64)    Clave de idempotencia del llamador
--   @MOVI_DESCRIPCION_VC   NVARCHAR(250)  Descripcion
--
-- Retorna: BIGINT
--   > 0  ID del asiento aplicado (o del ya existente, si fue idempotente)
--   -1   La cuenta no existe
--   -2   La cuenta esta inactiva
--   -3   Saldo insuficiente
--   -9   La clave de idempotencia ya fue usada por otra cuenta
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'pay' AND p.name = 'APLICAR_MOVIMIENTO'
)
    DROP PROCEDURE pay.APLICAR_MOVIMIENTO;
GO

-- Los INSERT sobre pay.MOVIMIENTO tocan el indice filtrado UQ_PAY_MOVIMIENTO_QR,
-- que exige QUOTED_IDENTIFIER ON. El valor queda grabado JUNTO al procedimiento
-- al crearlo: SSMS lo trae activado, pero sqlcmd lo deja OFF, y sin esta linea el
-- SP se crea igual y recien falla al ejecutarse (Msg 1934).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE pay.APLICAR_MOVIMIENTO
    @CUEN_NUMERO_VC       VARCHAR(20),
    @MOVI_TIPO_VC         VARCHAR(10),
    @MOVI_MONTO_DE        DECIMAL(18,8),
    @MOVI_REFERENCIA_VC   VARCHAR(64),
    @MOVI_IDEMPOTENCIA_VC VARCHAR(64),
    @MOVI_DESCRIPCION_VC  NVARCHAR(250),
    -- Auditoria: quien origino el cambio y bajo que traza. Llevan default NULL
    -- para no romper a ningun llamador que todavia no los mande; sin ellos el
    -- movimiento se aplica igual, solo que la fila de aud.AUDITORIA queda sin
    -- actor y sin enlace a la bitacora.
    @AUDITORIA_TRAZA_VC   VARCHAR(64)   = NULL,
    @AUDITORIA_USUARIO_IT BIGINT        = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @CuentaId       BIGINT,
            @Moneda         VARCHAR(10),
            @SaldoAnterior  DECIMAL(18,8),
            @SaldoPosterior DECIMAL(18,8),
            @Activo         BIT,
            @MovimientoId   BIGINT,
            @MovimientoCuenta BIGINT,
            @MovimientoTipo   VARCHAR(10),
            @MovimientoMonto  DECIMAL(18,8),
            @MovimientoJson   NVARCHAR(MAX),
            @CuentaAntes      NVARCHAR(MAX),
            @CuentaDespues    NVARCHAR(MAX),
            @Resultado      BIGINT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- UPDLOCK + HOLDLOCK serializa a los concurrentes sobre ESTA cuenta: sin
        -- esto, dos debitos simultaneos podrian ambos leer saldo suficiente.
        SELECT @CuentaId      = CUEN_ID_IT,
               @Moneda        = CUEN_MONEDA_CODIGO_VC,
               @SaldoAnterior = CUEN_SALDO_DE,
               @Activo        = CUEN_ACTIVO_BT
        FROM   pay.CUENTA WITH (UPDLOCK, HOLDLOCK)
        WHERE  CUEN_NUMERO_VC            = @CUEN_NUMERO_VC
          AND  CUEN_FECHA_ELIMINACION_DT IS NULL;

        IF @CuentaId IS NULL
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-1 AS BIGINT) AS Resultado;
            RETURN;
        END

        -- Idempotencia: la clave ya fue aplicada, devolvemos el asiento original.
        -- La clave es unica en todo el sistema, asi que se busca sin cuenta.
        --
        -- Solo cuenta como reintento si el asiento hallado es de ESTA cuenta y
        -- ademas del MISMO tipo y monto. Comparar solo la cuenta no alcanza:
        -- reusar la clave de una recarga para un debito devolveria exito con el
        -- id de la recarga y el debito no se aplicaria nunca, sin que el
        -- llamador se entere. Es el mismo criterio que usa pay.EJECUTAR_PAGO_QR.
        SELECT @MovimientoId     = MOVI_ID_IT,
               @MovimientoCuenta = MOVI_CUENTA_ID_IT,
               @MovimientoTipo   = MOVI_TIPO_VC,
               @MovimientoMonto  = MOVI_MONTO_DE
        FROM   pay.MOVIMIENTO
        WHERE  MOVI_IDEMPOTENCIA_VC = @MOVI_IDEMPOTENCIA_VC;

        IF @MovimientoId IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;

            IF @MovimientoCuenta = @CuentaId
               AND @MovimientoTipo  = @MOVI_TIPO_VC
               AND @MovimientoMonto = @MOVI_MONTO_DE
                SELECT CAST(@MovimientoId AS BIGINT) AS Resultado;
            ELSE
                SELECT CAST(-9 AS BIGINT) AS Resultado;

            RETURN;
        END

        IF @Activo = 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-2 AS BIGINT) AS Resultado;
            RETURN;
        END

        IF @MOVI_TIPO_VC = 'DEBITO'
        BEGIN
            IF @SaldoAnterior < @MOVI_MONTO_DE
            BEGIN
                COMMIT TRANSACTION;
                SELECT CAST(-3 AS BIGINT) AS Resultado;
                RETURN;
            END

            SET @SaldoPosterior = @SaldoAnterior - @MOVI_MONTO_DE;
        END
        ELSE
        BEGIN
            SET @SaldoPosterior = @SaldoAnterior + @MOVI_MONTO_DE;
        END

        -- Movimiento sin conversion: origen y destino son la misma moneda y el
        -- tipo de cambio es 1. El asiento igual guarda los campos completos para
        -- que el historial se lea de una sola forma.
        INSERT INTO pay.MOVIMIENTO (
            MOVI_CUENTA_ID_IT,
            MOVI_TIPO_VC,
            MOVI_ESTADO_VC,
            MOVI_MONTO_DE,
            MOVI_SALDO_ANTERIOR_DE,
            MOVI_SALDO_POSTERIOR_DE,
            MOVI_MONTO_ORIGEN_DE,
            MOVI_MONEDA_ORIGEN_VC,
            MOVI_TIPO_CAMBIO_DE,
            MOVI_MONTO_DESTINO_DE,
            MOVI_MONEDA_DESTINO_VC,
            MOVI_REFERENCIA_VC,
            MOVI_IDEMPOTENCIA_VC,
            MOVI_TRANSACCION_VC,
            MOVI_DESCRIPCION_VC,
            MOVI_FECHA_CREACION_DT
        )
        VALUES (
            @CuentaId,
            @MOVI_TIPO_VC,
            'COMPLETED',
            @MOVI_MONTO_DE,
            @SaldoAnterior,
            @SaldoPosterior,
            @MOVI_MONTO_DE,
            @Moneda,
            1,
            @MOVI_MONTO_DE,
            @Moneda,
            @MOVI_REFERENCIA_VC,
            @MOVI_IDEMPOTENCIA_VC,
            CONVERT(VARCHAR(36), NEWID()),
            @MOVI_DESCRIPCION_VC,
            GETDATE()
        );

        SET @Resultado = CAST(SCOPE_IDENTITY() AS BIGINT);

        -- El asiento recien nacido, como quedo guardado. El JSON pasa por una
        -- variable porque T-SQL no admite una subconsulta como argumento de EXEC.
        SET @MovimientoJson = (SELECT * FROM pay.MOVIMIENTO WHERE MOVI_ID_IT = @Resultado
                               FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        EXEC aud.REGISTRAR_AUDITORIA
             'pay', 'MOVIMIENTO', @Resultado, 'INSERT',
             NULL, @MovimientoJson,
             @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        -- La cuenta antes y despues del cambio de saldo.
        SET @CuentaAntes = (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @CuentaId
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        UPDATE pay.CUENTA
        SET    CUEN_SALDO_DE               = @SaldoPosterior,
               CUEN_FECHA_ACTUALIZACION_DT = GETDATE()
        WHERE  CUEN_ID_IT = @CuentaId;

        SET @CuentaDespues = (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @CuentaId
                              FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        EXEC aud.REGISTRAR_AUDITORIA
             'pay', 'CUENTA', @CuentaId, 'UPDATE',
             @CuentaAntes, @CuentaDespues,
             @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        COMMIT TRANSACTION;

        -- ExecuteScalarAsync lee el primer valor del primer resultado
        SELECT @Resultado AS Resultado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        -- Carrera contra UQ_PAY_MOVIMIENTO_IDEMPOTENCIA: otro hilo aplico la
        -- misma clave entre nuestra lectura y nuestro insert. El resultado
        -- correcto sigue siendo el ID de ese asiento, no un error.
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            SELECT @MovimientoId     = MOVI_ID_IT,
                   @MovimientoCuenta = MOVI_CUENTA_ID_IT,
                   @MovimientoTipo   = MOVI_TIPO_VC,
                   @MovimientoMonto  = MOVI_MONTO_DE
            FROM   pay.MOVIMIENTO
            WHERE  MOVI_IDEMPOTENCIA_VC = @MOVI_IDEMPOTENCIA_VC;

            IF @MovimientoCuenta = @CuentaId
               AND @MovimientoTipo  = @MOVI_TIPO_VC
               AND @MovimientoMonto = @MOVI_MONTO_DE
                SELECT CAST(@MovimientoId AS BIGINT) AS Resultado;
            ELSE
                SELECT CAST(-9 AS BIGINT) AS Resultado;

            RETURN;
        END;

        THROW;
    END CATCH
END
GO
