-- =============================================================================
-- SP:          pay.EJECUTAR_PAGO_QR
-- Descripcion: Nucleo del pago. Debita a la cuenta del cliente en su moneda,
--              acredita a la cuenta del comercio en BOB y escribe los DOS
--              asientos, todo dentro de una sola transaccion. O pasa entero o
--              no pasa nada: nunca queda un debito sin su credito.
--
--              Llega con todo ya validado (cliente, QR, comercio, moneda y tipo
--              de cambio los resolvio el microservicio por gRPC). Lo unico que
--              no se puede decidir afuera es el saldo, porque solo es confiable
--              con la fila de la cuenta bloqueada.
--
-- CONCURRENCIA
--   Las dos cuentas se bloquean en UNA sola sentencia con UPDLOCK + HOLDLOCK.
--   Al ser una sola sentencia, el motor toma los bloqueos en el orden del
--   indice, identico para todas las sesiones, y no puede producirse el abrazo
--   mortal clasico de bloquear A-luego-B en un hilo y B-luego-A en el otro.
--   Con la fila tomada, dos pagos simultaneos del mismo cliente se serializan:
--   el segundo lee el saldo ya descontado y, si no alcanza, recibe -3.
--
-- IDEMPOTENCIA
--   Se busca la clave antes de tocar saldo y, si ya existe, se devuelve el
--   asiento original. La carrera que se cuela entre esa lectura y el INSERT la
--   ataja UQ_PAY_MOVIMIENTO_IDEMPOTENCIA y se resuelve en el CATCH.
--
-- QR DE UN SOLO USO
--   Lo garantiza UQ_PAY_MOVIMIENTO_QR, no una llamada al servicio de QR: si dos
--   clientes distintos pagan el mismo QR a la vez, el motor deja pasar un unico
--   debito y el otro recibe -6.
--
-- Parametros:
--   @CUENTA_ORIGEN_VC   VARCHAR(20)    Cuenta del cliente que paga
--   @CUENTA_DESTINO_VC  VARCHAR(20)    Cuenta del comercio que cobra
--   @COMERCIO_ID_IT     BIGINT         ID del comercio
--   @QR_CODIGO_VC       VARCHAR(64)    Codigo QR pagado
--   @MONTO_ORIGEN_DE    DECIMAL(18,8)  Monto en la moneda del cliente
--   @MONEDA_ORIGEN_VC   VARCHAR(10)    Moneda del cliente (ej: USD)
--   @TIPO_CAMBIO_DE     DECIMAL(18,8)  Tipo de cambio aplicado (ej: 6.96)
--   @MONTO_DESTINO_DE   DECIMAL(18,8)  Monto convertido (ej: 139.20)
--   @MONEDA_DESTINO_VC  VARCHAR(10)    Moneda del comercio (BOB)
--   @REFERENCIA_VC      VARCHAR(64)    Referencia del QR
--   @IDEMPOTENCIA_VC    VARCHAR(64)    idempotencyKey del llamador
--   @TRANSACCION_VC     VARCHAR(36)    Codigo de la operacion
--   @DESCRIPCION_VC     NVARCHAR(250)  Glosa
--
-- Retorna: BIGINT
--   > 0  ID del asiento de DEBITO (o del original, si fue idempotente)
--   -1   La cuenta del cliente no existe
--   -2   La cuenta del cliente esta inactiva
--   -3   Saldo insuficiente
--   -4   La cuenta del comercio no existe
--   -5   La cuenta del comercio esta inactiva
--   -6   El QR ya fue pagado
--   -7   La moneda enviada no coincide con la de alguna de las cuentas
--   -8   El monto convertido no corresponde al tipo de cambio enviado
--   -9   La clave de idempotencia ya fue usada por otra cuenta
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'pay' AND p.name = 'EJECUTAR_PAGO_QR'
)
    DROP PROCEDURE pay.EJECUTAR_PAGO_QR;
GO

-- Los INSERT sobre pay.MOVIMIENTO tocan el indice filtrado UQ_PAY_MOVIMIENTO_QR,
-- que exige QUOTED_IDENTIFIER ON. El valor queda grabado JUNTO al procedimiento
-- al crearlo: SSMS lo trae activado, pero sqlcmd lo deja OFF, y sin esta linea el
-- SP se crea igual y recien falla al ejecutarse (Msg 1934).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE pay.EJECUTAR_PAGO_QR
    @CUENTA_ORIGEN_VC  VARCHAR(20),
    @CUENTA_DESTINO_VC VARCHAR(20),
    @COMERCIO_ID_IT    BIGINT,
    @QR_CODIGO_VC      VARCHAR(64),
    @MONTO_ORIGEN_DE   DECIMAL(18,8),
    @MONEDA_ORIGEN_VC  VARCHAR(10),
    @TIPO_CAMBIO_DE    DECIMAL(18,8),
    @MONTO_DESTINO_DE  DECIMAL(18,8),
    @MONEDA_DESTINO_VC VARCHAR(10),
    @REFERENCIA_VC     VARCHAR(64),
    @IDEMPOTENCIA_VC   VARCHAR(64),
    @TRANSACCION_VC    VARCHAR(36),
    @DESCRIPCION_VC    NVARCHAR(250),
    -- Auditoria: quien origino el pago y bajo que traza. Default NULL para no
    -- romper llamadores que todavia no los manden.
    @AUDITORIA_TRAZA_VC   VARCHAR(64) = NULL,
    @AUDITORIA_USUARIO_IT BIGINT      = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OrigenId        BIGINT,
            @OrigenMoneda    VARCHAR(10),
            @OrigenSaldo     DECIMAL(18,8),
            @OrigenActivo    BIT,
            @DestinoId       BIGINT,
            @DestinoMoneda   VARCHAR(10),
            @DestinoSaldo    DECIMAL(18,8),
            @DestinoActivo   BIT,
            @OrigenPosterior DECIMAL(18,8),
            @DestinoPosterior DECIMAL(18,8),
            @MovimientoId    BIGINT,
            @MovimientoCuenta BIGINT,
            @MovimientoQr    VARCHAR(64),
            @MovimientoMonto  DECIMAL(18,8),
            @MovimientoMoneda VARCHAR(10),
            @CreditoId       BIGINT,
            @DebitoJson      NVARCHAR(MAX),
            @CreditoJson     NVARCHAR(MAX),
            @OrigenAntes     NVARCHAR(MAX),
            @OrigenDespues   NVARCHAR(MAX),
            @DestinoAntes    NVARCHAR(MAX),
            @DestinoDespues  NVARCHAR(MAX),
            @Resultado       BIGINT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Las dos cuentas en una sola sentencia: bloqueos en orden de indice,
        -- igual para todas las sesiones. Ver la nota de CONCURRENCIA arriba.
        --
        -- Cada CASE lleva ELSE y la agregacion va envuelta en ISNULL: sin eso, la
        -- fila que no corresponde aporta NULL y el motor emite el aviso "Null
        -- value is eliminated by an aggregate" en CADA pago. Como el IDENTITY
        -- arranca en 1, el 0 sirve de centinela inequivoco de "no existe".
        SELECT @OrigenId      = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_ORIGEN_VC  THEN CUEN_ID_IT            ELSE 0  END), 0),
               @OrigenMoneda  = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_ORIGEN_VC  THEN CUEN_MONEDA_CODIGO_VC ELSE '' END), ''),
               @OrigenSaldo   = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_ORIGEN_VC  THEN CUEN_SALDO_DE         ELSE 0  END), 0),
               @OrigenActivo  = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_ORIGEN_VC  THEN CAST(CUEN_ACTIVO_BT AS TINYINT) ELSE 0 END), 0),
               @DestinoId     = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_DESTINO_VC THEN CUEN_ID_IT            ELSE 0  END), 0),
               @DestinoMoneda = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_DESTINO_VC THEN CUEN_MONEDA_CODIGO_VC ELSE '' END), ''),
               @DestinoSaldo  = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_DESTINO_VC THEN CUEN_SALDO_DE         ELSE 0  END), 0),
               @DestinoActivo = ISNULL(MAX(CASE WHEN CUEN_NUMERO_VC = @CUENTA_DESTINO_VC THEN CAST(CUEN_ACTIVO_BT AS TINYINT) ELSE 0 END), 0)
        FROM   pay.CUENTA WITH (UPDLOCK, HOLDLOCK)
        WHERE  CUEN_NUMERO_VC IN (@CUENTA_ORIGEN_VC, @CUENTA_DESTINO_VC)
          AND  CUEN_FECHA_ELIMINACION_DT IS NULL;

        IF @OrigenId = 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-1 AS BIGINT) AS Resultado;
            RETURN;
        END

        -- Idempotencia antes que cualquier otra regla: si esta operacion ya se
        -- ejecuto, la respuesta correcta es la original aunque hoy el cliente
        -- este sin saldo o la cuenta este inactiva.
        --
        -- La busqueda es por clave sola, sin cuenta, porque la clave es unica en
        -- todo el sistema. Solo cuenta como reintento si el asiento hallado es
        -- de ESTA cuenta, de ESTE QR y por EL MISMO importe y moneda; si no, la
        -- clave fue reusada para otra cosa y devolver ese movimiento seria darle
        -- a un cliente el pago de otro, o dar por pagado un QR que nunca se
        -- cobro.
        --
        -- La comparacion replica exactamente la del handler. Si fuera mas laxa
        -- aca, el mismo caso daria un resultado por el camino normal y otro
        -- distinto cuando se resuelve por carrera: el resultado dependeria del
        -- momento, que es justo lo que la idempotencia existe para evitar.
        SELECT @MovimientoId     = MOVI_ID_IT,
               @MovimientoCuenta = MOVI_CUENTA_ID_IT,
               @MovimientoQr     = ISNULL(MOVI_QR_CODIGO_VC, ''),
               @MovimientoMonto  = MOVI_MONTO_ORIGEN_DE,
               @MovimientoMoneda = MOVI_MONEDA_ORIGEN_VC
        FROM   pay.MOVIMIENTO
        WHERE  MOVI_IDEMPOTENCIA_VC = @IDEMPOTENCIA_VC;

        IF @MovimientoId IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;

            IF @MovimientoCuenta  = @OrigenId
               AND @MovimientoQr     = @QR_CODIGO_VC
               AND @MovimientoMonto  = @MONTO_ORIGEN_DE
               AND @MovimientoMoneda = @MONEDA_ORIGEN_VC
                SELECT CAST(@MovimientoId AS BIGINT) AS Resultado;
            ELSE
                SELECT CAST(-9 AS BIGINT) AS Resultado;

            RETURN;
        END

        IF @OrigenActivo = 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-2 AS BIGINT) AS Resultado;
            RETURN;
        END

        IF @DestinoId = 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-4 AS BIGINT) AS Resultado;
            RETURN;
        END

        IF @DestinoActivo = 0
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-5 AS BIGINT) AS Resultado;
            RETURN;
        END

        -- El QR ya fue pagado. La verificacion explicita da el codigo correcto
        -- en el caso normal; el indice unico cubre la carrera.
        IF EXISTS (
            SELECT 1 FROM pay.MOVIMIENTO
            WHERE MOVI_QR_CODIGO_VC = @QR_CODIGO_VC
              AND MOVI_TIPO_VC      = 'DEBITO'
        )
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-6 AS BIGINT) AS Resultado;
            RETURN;
        END

        -- Las monedas enviadas tienen que ser las de las cuentas: si no, el
        -- asiento quedaria contando plata que no existe en esa moneda.
        IF @OrigenMoneda <> @MONEDA_ORIGEN_VC OR @DestinoMoneda <> @MONEDA_DESTINO_VC
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-7 AS BIGINT) AS Resultado;
            RETURN;
        END

        -- Defensa en profundidad: el monto convertido tiene que ser coherente
        -- con el tipo de cambio que se va a grabar. Sin esto, un llamador con un
        -- bug podria dejar en el historico una conversion que no cierra.
        IF ABS(@MONTO_DESTINO_DE - ROUND(@MONTO_ORIGEN_DE * @TIPO_CAMBIO_DE, 2)) > 0.01
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-8 AS BIGINT) AS Resultado;
            RETURN;
        END

        IF @OrigenSaldo < @MONTO_ORIGEN_DE
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(-3 AS BIGINT) AS Resultado;
            RETURN;
        END

        SET @OrigenPosterior  = @OrigenSaldo  - @MONTO_ORIGEN_DE;
        SET @DestinoPosterior = @DestinoSaldo + @MONTO_DESTINO_DE;

        -- ── Asiento 1: DEBITO al cliente, en su moneda ───────────────────────
        INSERT INTO pay.MOVIMIENTO (
            MOVI_CUENTA_ID_IT, MOVI_TIPO_VC, MOVI_ESTADO_VC,
            MOVI_MONTO_DE, MOVI_SALDO_ANTERIOR_DE, MOVI_SALDO_POSTERIOR_DE,
            MOVI_MONTO_ORIGEN_DE, MOVI_MONEDA_ORIGEN_VC, MOVI_TIPO_CAMBIO_DE,
            MOVI_MONTO_DESTINO_DE, MOVI_MONEDA_DESTINO_VC,
            MOVI_COMERCIO_ID_IT, MOVI_QR_CODIGO_VC, MOVI_REFERENCIA_VC,
            MOVI_IDEMPOTENCIA_VC, MOVI_TRANSACCION_VC, MOVI_DESCRIPCION_VC,
            MOVI_FECHA_CREACION_DT
        )
        VALUES (
            @OrigenId, 'DEBITO', 'COMPLETED',
            @MONTO_ORIGEN_DE, @OrigenSaldo, @OrigenPosterior,
            @MONTO_ORIGEN_DE, @MONEDA_ORIGEN_VC, @TIPO_CAMBIO_DE,
            @MONTO_DESTINO_DE, @MONEDA_DESTINO_VC,
            @COMERCIO_ID_IT, @QR_CODIGO_VC, @REFERENCIA_VC,
            @IDEMPOTENCIA_VC, @TRANSACCION_VC, @DESCRIPCION_VC,
            GETDATE()
        );

        SET @Resultado = CAST(SCOPE_IDENTITY() AS BIGINT);

        -- ── Asiento 2: CREDITO al comercio, en BOB ───────────────────────────
        -- Lleva el mismo TRANSACCION y la clave de idempotencia sufijada con
        -- "-IN". El sufijo es obligatorio siempre, no una precaucion: la
        -- restriccion UQ_PAY_MOVIMIENTO_IDEMPOTENCIA es GLOBAL sobre la clave
        -- sola, asi que sin el sufijo el credito chocaria contra el debito en
        -- todos los pagos, no solo en un caso raro.
        INSERT INTO pay.MOVIMIENTO (
            MOVI_CUENTA_ID_IT, MOVI_TIPO_VC, MOVI_ESTADO_VC,
            MOVI_MONTO_DE, MOVI_SALDO_ANTERIOR_DE, MOVI_SALDO_POSTERIOR_DE,
            MOVI_MONTO_ORIGEN_DE, MOVI_MONEDA_ORIGEN_VC, MOVI_TIPO_CAMBIO_DE,
            MOVI_MONTO_DESTINO_DE, MOVI_MONEDA_DESTINO_VC,
            MOVI_COMERCIO_ID_IT, MOVI_QR_CODIGO_VC, MOVI_REFERENCIA_VC,
            MOVI_IDEMPOTENCIA_VC, MOVI_TRANSACCION_VC, MOVI_DESCRIPCION_VC,
            MOVI_FECHA_CREACION_DT
        )
        VALUES (
            @DestinoId, 'CREDITO', 'COMPLETED',
            @MONTO_DESTINO_DE, @DestinoSaldo, @DestinoPosterior,
            @MONTO_ORIGEN_DE, @MONEDA_ORIGEN_VC, @TIPO_CAMBIO_DE,
            @MONTO_DESTINO_DE, @MONEDA_DESTINO_VC,
            @COMERCIO_ID_IT, NULL, @REFERENCIA_VC,
            @IDEMPOTENCIA_VC + '-IN', @TRANSACCION_VC, @DESCRIPCION_VC,
            GETDATE()
        );

        SET @CreditoId = CAST(SCOPE_IDENTITY() AS BIGINT);

        -- ── Auditoria ────────────────────────────────────────────────────────
        -- Un pago deja CUATRO rastros: los dos asientos y los dos saldos. Todos
        -- dentro de esta transaccion, asi que o quedan los cuatro o no queda
        -- ninguno, igual que el dinero.
        SET @OrigenAntes  = (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @OrigenId
                             FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        SET @DestinoAntes = (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @DestinoId
                             FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        UPDATE pay.CUENTA
        SET    CUEN_SALDO_DE               = @OrigenPosterior,
               CUEN_FECHA_ACTUALIZACION_DT = GETDATE()
        WHERE  CUEN_ID_IT = @OrigenId;

        UPDATE pay.CUENTA
        SET    CUEN_SALDO_DE               = @DestinoPosterior,
               CUEN_FECHA_ACTUALIZACION_DT = GETDATE()
        WHERE  CUEN_ID_IT = @DestinoId;

        SET @OrigenDespues  = (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @OrigenId
                               FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        SET @DestinoDespues = (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @DestinoId
                               FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        -- Los JSON pasan por variables: T-SQL no admite una subconsulta como
        -- argumento de EXEC.
        SET @DebitoJson  = (SELECT * FROM pay.MOVIMIENTO WHERE MOVI_ID_IT = @Resultado
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
        SET @CreditoJson = (SELECT * FROM pay.MOVIMIENTO WHERE MOVI_ID_IT = @CreditoId
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        EXEC aud.REGISTRAR_AUDITORIA 'pay', 'MOVIMIENTO', @Resultado, 'INSERT',
             NULL, @DebitoJson, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        EXEC aud.REGISTRAR_AUDITORIA 'pay', 'MOVIMIENTO', @CreditoId, 'INSERT',
             NULL, @CreditoJson, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        EXEC aud.REGISTRAR_AUDITORIA 'pay', 'CUENTA', @OrigenId, 'UPDATE',
             @OrigenAntes, @OrigenDespues,
             @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        EXEC aud.REGISTRAR_AUDITORIA 'pay', 'CUENTA', @DestinoId, 'UPDATE',
             @DestinoAntes, @DestinoDespues,
             @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        COMMIT TRANSACTION;

        -- ExecuteScalarAsync lee el primer valor del primer resultado
        SELECT @Resultado AS Resultado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            -- Carrera con la misma clave de idempotencia: el otro hilo ya
            -- aplico ESTA operacion, su asiento es la respuesta correcta. Si el
            -- asiento resulta ser de otra cuenta, la clave fue reusada.
            SELECT @MovimientoId     = MOVI_ID_IT,
                   @MovimientoCuenta = MOVI_CUENTA_ID_IT,
                   @MovimientoQr     = ISNULL(MOVI_QR_CODIGO_VC, ''),
                   @MovimientoMonto  = MOVI_MONTO_ORIGEN_DE,
                   @MovimientoMoneda = MOVI_MONEDA_ORIGEN_VC
            FROM   pay.MOVIMIENTO
            WHERE  MOVI_IDEMPOTENCIA_VC = @IDEMPOTENCIA_VC;

            IF @MovimientoId IS NOT NULL
            BEGIN
                IF @MovimientoCuenta  = @OrigenId
                   AND @MovimientoQr     = @QR_CODIGO_VC
                   AND @MovimientoMonto  = @MONTO_ORIGEN_DE
                   AND @MovimientoMoneda = @MONEDA_ORIGEN_VC
                    SELECT CAST(@MovimientoId AS BIGINT) AS Resultado;
                ELSE
                    SELECT CAST(-9 AS BIGINT) AS Resultado;

                RETURN;
            END

            -- Carrera contra UQ_PAY_MOVIMIENTO_QR: otro cliente pago el mismo
            -- QR primero. No es un error del sistema, es el QR ya usado.
            SELECT CAST(-6 AS BIGINT) AS Resultado;
            RETURN;
        END;

        THROW;
    END CATCH
END
GO
