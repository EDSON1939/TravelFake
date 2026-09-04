-- =============================================================================
-- Schema: commerce
-- Tabla:  COMERCIO
-- Descripcion: Catálogo de comercios del sistema YastaCoin
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   COME = prefijo de tabla COMERCIO
--   _IT  = INT / BIGINT
--   _VC  = VARCHAR / NVARCHAR
--   _BT  = BIT
--   _DT  = DATETIME
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'commerce')
BEGIN
    EXEC('CREATE SCHEMA commerce');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND t.name = 'COMERCIO'
)
BEGIN
    CREATE TABLE commerce.COMERCIO (
        COME_ID_IT                  BIGINT          NOT NULL IDENTITY(1,1),
        COME_NOMBRE_VC              NVARCHAR(50)    NOT NULL,
        COME_NIT_VC                 VARCHAR(13)     NOT NULL,
        COME_CUENTA_ID_IT           BIGINT          NULL,
        COME_ACTIVO_BT              BIT             NOT NULL    DEFAULT 1,
        COME_FECHA_CREACION_DT      DATETIME        NOT NULL    DEFAULT GETDATE(),
        COME_FECHA_ACTUALIZACION_DT DATETIME        NULL,

        CONSTRAINT PK_COMERCIO       PRIMARY KEY (COME_ID_IT),
        CONSTRAINT UQ_COMERCIO_NIT   UNIQUE      (COME_NIT_VC)
    );

    -- Índice para búsqueda por nombre y estado (uso frecuente)
    CREATE NONCLUSTERED INDEX IX_COMERCIO_NOMBRE
        ON commerce.COMERCIO (COME_NOMBRE_VC ASC);

    PRINT 'Tabla commerce.COMERCIO creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla commerce.COMERCIO ya existe.';
END
GO
