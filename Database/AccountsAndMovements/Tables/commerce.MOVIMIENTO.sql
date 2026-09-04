-- =============================================================================
-- Reto:   QR Bolivia
-- Schema: commerce
-- Tabla:  MOVIMIENTO
-- Descripcion: Libro mayor y registro de transacciones. Cada fila es un asiento
--              inmutable con el saldo antes y despues, de modo que el saldo de
--              commerce.CUENTA siempre puede reconstruirse desde aqui.
--
--              Un pago QR escribe DOS filas con el mismo MOVI_TRANSACCION_VC:
--              el DEBITO de la cuenta del cliente (en su moneda) y el CREDITO
--              de la cuenta del comercio (en BOB).
--
--              Los campos de conversion se guardan en el asiento y nunca se
--              recalculan: la operacion historica no cambia si manana cambia el
--              tipo de cambio.
--
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   MOVI = prefijo de tabla MOVIMIENTO
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'commerce')
BEGIN
    EXEC('CREATE SCHEMA commerce');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND t.name = 'MOVIMIENTO'
)
BEGIN
    CREATE TABLE commerce.MOVIMIENTO (
        MOVI_ID_IT                  BIGINT          NOT NULL IDENTITY(1,1),
        MOVI_CUENTA_ID_IT           BIGINT          NOT NULL,
        MOVI_TIPO_VC                VARCHAR(10)     NOT NULL,
        MOVI_ESTADO_VC              VARCHAR(20)     NOT NULL    DEFAULT 'COMPLETED',

        -- Movimiento real sobre la cuenta, en la moneda de la cuenta.
        MOVI_MONTO_DE               DECIMAL(18,8)   NOT NULL,
        MOVI_SALDO_ANTERIOR_DE      DECIMAL(18,8)   NOT NULL,
        MOVI_SALDO_POSTERIOR_DE     DECIMAL(18,8)   NOT NULL,

        -- Fotografia de la conversion aplicada en el momento del pago.
        MOVI_MONTO_ORIGEN_DE        DECIMAL(18,8)   NOT NULL,
        MOVI_MONEDA_ORIGEN_VC       VARCHAR(10)     NOT NULL,
        MOVI_TIPO_CAMBIO_DE         DECIMAL(18,8)   NOT NULL    DEFAULT 1,
        MOVI_MONTO_DESTINO_DE       DECIMAL(18,8)   NOT NULL,
        MOVI_MONEDA_DESTINO_VC      VARCHAR(10)     NOT NULL,

        MOVI_COMERCIO_ID_IT         BIGINT          NULL,
        MOVI_QR_CODIGO_VC           VARCHAR(64)     NULL,
        MOVI_REFERENCIA_VC          VARCHAR(64)     NOT NULL    DEFAULT '',
        -- Clave de idempotencia del llamador (ej: TX-2026-000001).
        MOVI_IDEMPOTENCIA_VC        VARCHAR(64)     NOT NULL,
        -- Codigo de la operacion: une el debito del cliente con el credito del comercio.
        MOVI_TRANSACCION_VC         VARCHAR(36)     NOT NULL,
        MOVI_DESCRIPCION_VC         NVARCHAR(250)   NOT NULL    DEFAULT '',
        MOVI_FECHA_CREACION_DT      DATETIME        NOT NULL    DEFAULT GETDATE(),

        CONSTRAINT PK_COMMERCE_MOVIMIENTO        PRIMARY KEY (MOVI_ID_IT),
        CONSTRAINT CK_COMMERCE_MOVIMIENTO_MONTO  CHECK       (MOVI_MONTO_DE > 0),
        CONSTRAINT CK_COMMERCE_MOVIMIENTO_TIPO   CHECK       (MOVI_TIPO_VC IN ('CREDITO', 'DEBITO')),
        CONSTRAINT CK_COMMERCE_MOVIMIENTO_ESTADO CHECK       (MOVI_ESTADO_VC IN ('PENDING', 'COMPLETED', 'FAILED', 'CANCELLED')),
        CONSTRAINT CK_COMMERCE_MOVIMIENTO_CAMBIO CHECK       (MOVI_TIPO_CAMBIO_DE > 0),
        CONSTRAINT FK_COMMERCE_MOVIMIENTO_CUENTA FOREIGN KEY (MOVI_CUENTA_ID_IT)
            REFERENCES commerce.CUENTA (CUEN_ID_IT),
        -- Idempotencia a nivel motor: dos asientos con la misma clave son
        -- imposibles. Es la red que sostiene "no cobrar dos veces" aunque dos
        -- hilos entren a la vez con el mismo idempotencyKey.
        --
        -- El alcance es GLOBAL, no por cuenta: el idempotencyKey identifica una
        -- operacion del sistema (TX-2026-000001), no una operacion dentro de una
        -- cuenta. Con alcance por cuenta, dos clientes que mandaran la misma
        -- clave a la vez pasarian los dos, y en cambio uno detras del otro el
        -- segundo seria rechazado: el mismo caso daria resultados distintos
        -- segun el momento. El asiento del comercio lleva la clave sufijada con
        -- "-IN", asi que el par debito/credito no choca entre si.
        CONSTRAINT UQ_COMMERCE_MOVIMIENTO_IDEMPOTENCIA UNIQUE (MOVI_IDEMPOTENCIA_VC)
    );

    -- Extracto de la cuenta, mas reciente primero.
    CREATE NONCLUSTERED INDEX IX_COMMERCE_MOVIMIENTO_CUENTA_FECHA
        ON commerce.MOVIMIENTO (MOVI_CUENTA_ID_IT, MOVI_FECHA_CREACION_DT DESC)
        INCLUDE (MOVI_TIPO_VC, MOVI_ESTADO_VC, MOVI_MONTO_DE, MOVI_SALDO_POSTERIOR_DE,
                 MOVI_TRANSACCION_VC);

    -- Busqueda por codigo de operacion: devuelve el par debito/credito.
    CREATE NONCLUSTERED INDEX IX_COMMERCE_MOVIMIENTO_TRANSACCION
        ON commerce.MOVIMIENTO (MOVI_TRANSACCION_VC);

    PRINT 'Tabla commerce.MOVIMIENTO creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla commerce.MOVIMIENTO ya existe.';
END
GO

-- Los indices filtrados exigen QUOTED_IDENTIFIER ON. SSMS lo trae activado,
-- sqlcmd lo deja OFF por defecto, asi que lo forzamos aqui.
SET QUOTED_IDENTIFIER ON;
GO

-- Un QR se paga UNA sola vez. Este indice es la garantia dura del requisito: si
-- dos clientes escanean el mismo QR a la vez, el motor deja pasar un unico
-- debito y el segundo recibe QR_ALREADY_USED. No depende de que el
-- microservicio de QR alcance a marcarlo como usado.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_COMMERCE_MOVIMIENTO_QR')
    CREATE UNIQUE NONCLUSTERED INDEX UQ_COMMERCE_MOVIMIENTO_QR
        ON commerce.MOVIMIENTO (MOVI_QR_CODIGO_VC)
        WHERE MOVI_QR_CODIGO_VC IS NOT NULL AND MOVI_TIPO_VC = 'DEBITO';
GO

-- No hace falta un indice aparte para buscar por clave de idempotencia:
-- UQ_COMMERCE_MOVIMIENTO_IDEMPOTENCIA ya es un indice unico sobre esa columna y
-- resuelve el seek exacto que hace el pago antes de tocar saldo.
