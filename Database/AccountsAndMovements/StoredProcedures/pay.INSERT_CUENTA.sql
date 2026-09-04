-- =============================================================================
-- SP:          pay.INSERT_CUENTA
-- Descripcion: Crea una cuenta y le asigna un numero unico derivado del ID
--              generado, de modo que no puede colisionar.
--
--              Si llega un saldo inicial, no se escribe directo en la columna:
--              se asienta un CREDITO de apertura en pay.MOVIMIENTO dentro de la
--              misma transaccion. Asi el libro mayor explica el saldo desde el
--              primer segundo y la cuenta nunca tiene plata sin respaldo.
--
-- Parametros:
--   @CUEN_TITULAR_TIPO_VC   VARCHAR(20)    'CLIENTE' o 'COMERCIO'
--   @CUEN_TITULAR_ID_IT     BIGINT         ID del cliente o del comercio
--   @CUEN_MONEDA_ID_IT      BIGINT         ID de la moneda
--   @CUEN_MONEDA_CODIGO_VC  VARCHAR(10)    Codigo de la moneda (ej: PEN, BOB)
--   @CUEN_SALDO_INICIAL_DE  DECIMAL(18,8)  Saldo de apertura (0 = sin apertura)
--
-- Retorna: BIGINT
--   > 0  ID de la cuenta creada
--   -1   El titular ya tiene una cuenta en esa moneda
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'pay' AND p.name = 'INSERT_CUENTA'
)
    DROP PROCEDURE pay.INSERT_CUENTA;
GO

-- Los INSERT sobre pay.MOVIMIENTO tocan el indice filtrado UQ_PAY_MOVIMIENTO_QR,
-- que exige QUOTED_IDENTIFIER ON. El valor queda grabado JUNTO al procedimiento
-- al crearlo: SSMS lo trae activado, pero sqlcmd lo deja OFF, y sin esta linea el
-- SP se crea igual y recien falla al ejecutarse (Msg 1934).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE pay.INSERT_CUENTA
    @CUEN_TITULAR_TIPO_VC  VARCHAR(20),
    @CUEN_TITULAR_ID_IT    BIGINT,
    @CUEN_MONEDA_ID_IT     BIGINT,
    @CUEN_MONEDA_CODIGO_VC VARCHAR(10),
    @CUEN_SALDO_INICIAL_DE DECIMAL(18,8),
    -- Auditoria: quien dio de alta la cuenta y bajo que traza. Default NULL para
    -- no romper llamadores que todavia no los manden.
    @AUDITORIA_TRAZA_VC    VARCHAR(64) = NULL,
    @AUDITORIA_USUARIO_IT  BIGINT      = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @CuentaId BIGINT,
            @Numero   VARCHAR(20),
            @AperturaId BIGINT,
            @Json       NVARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- El numero definitivo se deriva del IDENTITY, asi que primero
        -- insertamos con un placeholder unico (NEWID) y luego lo reemplazamos.
        INSERT INTO pay.CUENTA (
            CUEN_NUMERO_VC,
            CUEN_TITULAR_TIPO_VC,
            CUEN_TITULAR_ID_IT,
            CUEN_MONEDA_ID_IT,
            CUEN_MONEDA_CODIGO_VC,
            CUEN_SALDO_DE,
            CUEN_ACTIVO_BT,
            CUEN_FECHA_CREACION_DT
        )
        VALUES (
            LEFT(CONVERT(VARCHAR(36), NEWID()), 20),
            @CUEN_TITULAR_TIPO_VC,
            @CUEN_TITULAR_ID_IT,
            @CUEN_MONEDA_ID_IT,
            @CUEN_MONEDA_CODIGO_VC,
            0,
            1,
            GETDATE()
        );

        SET @CuentaId = CAST(SCOPE_IDENTITY() AS BIGINT);
        SET @Numero   = 'QR' + RIGHT('0000000000' + CAST(@CuentaId AS VARCHAR(10)), 10);

        UPDATE pay.CUENTA
        SET    CUEN_NUMERO_VC = @Numero
        WHERE  CUEN_ID_IT = @CuentaId;

        IF @CUEN_SALDO_INICIAL_DE > 0
        BEGIN
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
                'CREDITO',
                'COMPLETED',
                @CUEN_SALDO_INICIAL_DE,
                0,
                @CUEN_SALDO_INICIAL_DE,
                @CUEN_SALDO_INICIAL_DE,
                @CUEN_MONEDA_CODIGO_VC,
                1,
                @CUEN_SALDO_INICIAL_DE,
                @CUEN_MONEDA_CODIGO_VC,
                'APERTURA',
                'OPEN-' + @Numero,
                CONVERT(VARCHAR(36), NEWID()),
                N'Apertura de cuenta',
                GETDATE()
            );

            SET @AperturaId = CAST(SCOPE_IDENTITY() AS BIGINT);

            UPDATE pay.CUENTA
            SET    CUEN_SALDO_DE = @CUEN_SALDO_INICIAL_DE
            WHERE  CUEN_ID_IT = @CuentaId;

            -- El JSON va a una variable primero: T-SQL no admite una subconsulta
            -- como argumento de EXEC.
            SET @Json = (SELECT * FROM pay.MOVIMIENTO WHERE MOVI_ID_IT = @AperturaId
                         FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

            EXEC aud.REGISTRAR_AUDITORIA 'pay', 'MOVIMIENTO', @AperturaId, 'INSERT',
                 NULL, @Json, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;
        END

        -- El alta de la cuenta se audita una sola vez y con el registro ya
        -- completo: numero asignado y, si hubo apertura, el saldo aplicado.
        SET @Json = (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @CuentaId
                     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        EXEC aud.REGISTRAR_AUDITORIA 'pay', 'CUENTA', @CuentaId, 'INSERT',
             NULL, @Json, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        COMMIT TRANSACTION;

        -- ExecuteScalarAsync lee el primer valor del primer resultado
        SELECT CAST(@CuentaId AS BIGINT) AS Resultado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        -- UQ_PAY_CUENTA_TITULAR: el titular ya tiene cuenta en esa moneda. Es
        -- una regla de negocio, no una falla del sistema.
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            SELECT CAST(-1 AS BIGINT) AS Resultado;
            RETURN;
        END;

        THROW;
    END CATCH
END
GO
