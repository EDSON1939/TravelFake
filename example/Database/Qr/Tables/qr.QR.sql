-- =============================================================================
-- Schema: qr
-- Tabla:  QR
-- Descripcion: Códigos QR de pago del sistema YastaCoin.
--   El QR puede ser de unico uso (UNICO) o de multiples usos (MULTIPLE).
--   Un servicio en background evalua cada hora la fecha de expiracion y
--   cambia el estado de ACTIVO a EXPIRED.
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   QR_  = prefijo de tabla QR
--   _IT  = INT / BIGINT
--   _VC  = VARCHAR / NVARCHAR
--   _BT  = BIT
--   _DT  = DATETIME
--   _DE  = DECIMAL
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'qr')
BEGIN
    EXEC('CREATE SCHEMA qr');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'qr' AND t.name = 'QR'
)
BEGIN
    CREATE TABLE qr.QR (
        QR_ID_IT                  BIGINT          NOT NULL IDENTITY(1,1),
        QR_CODIGO_VC              VARCHAR(40)     NOT NULL,
        QR_COMERCIO_ID_IT         BIGINT          NOT NULL,
        QR_MONTO_DE               DECIMAL(18,2)   NOT NULL,
        QR_TIPO_VC                VARCHAR(10)     NOT NULL    DEFAULT 'UNICO',   -- UNICO | MULTIPLE
        QR_FECHA_EXPIRACION_DT    DATETIME        NOT NULL,
        QR_ESTADO_VC              VARCHAR(10)     NOT NULL    DEFAULT 'ACTIVE',  -- ACTIVE | USED | EXPIRED
        QR_ACTIVO_BT              BIT             NOT NULL    DEFAULT 1,
        QR_FECHA_CREACION_DT      DATETIME        NOT NULL    DEFAULT GETDATE(),
        QR_FECHA_ACTUALIZACION_DT DATETIME        NULL,

        CONSTRAINT PK_QR          PRIMARY KEY (QR_ID_IT),
        CONSTRAINT UQ_QR_CODIGO   UNIQUE      (QR_CODIGO_VC)
    );

    -- Índice para búsqueda por código (uso frecuente)
    CREATE NONCLUSTERED INDEX IX_QR_CODIGO
        ON qr.QR (QR_CODIGO_VC)
        INCLUDE (QR_COMERCIO_ID_IT, QR_MONTO_DE, QR_TIPO_VC,
                 QR_FECHA_EXPIRACION_DT, QR_ESTADO_VC, QR_ACTIVO_BT);

    -- Índice para el servicio en background que expira códigos vencidos cada hora
    CREATE NONCLUSTERED INDEX IX_QR_ESTADO_FECHA
        ON qr.QR (QR_ESTADO_VC, QR_FECHA_EXPIRACION_DT)
        INCLUDE (QR_ACTIVO_BT);

    PRINT 'Tabla qr.QR creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla qr.QR ya existe.';
END
GO