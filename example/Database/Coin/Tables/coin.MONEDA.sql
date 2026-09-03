-- =============================================================================
-- Schema: coin
-- Tabla:  MONEDA
-- Descripcion: Catálogo de monedas/criptomonedas del sistema YastaCoin
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   MONE = prefijo de tabla MONEDA
--   _IT  = INT / BIGINT
--   _VC  = VARCHAR / NVARCHAR
--   _BT  = BIT
--   _DT  = DATETIME
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'coin')
BEGIN
    EXEC('CREATE SCHEMA coin');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'coin' AND t.name = 'MONEDA'
)
BEGIN
    CREATE TABLE coin.MONEDA (
        MONE_ID_IT                  BIGINT          NOT NULL IDENTITY(1,1),
        MONE_NOMBRE_VC              NVARCHAR(100)   NOT NULL,
        MONE_CODIGO_VC              VARCHAR(10)     NOT NULL,
        MONE_SIMBOLO_VC             VARCHAR(10)     NOT NULL,
        MONE_ACTIVO_BT              BIT             NOT NULL    DEFAULT 1,
        MONE_FECHA_CREACION_DT      DATETIME        NOT NULL    DEFAULT GETDATE(),
        MONE_FECHA_ACTUALIZACION_DT DATETIME        NULL,

        CONSTRAINT PK_MONEDA        PRIMARY KEY (MONE_ID_IT),
        CONSTRAINT UQ_MONEDA_CODIGO UNIQUE      (MONE_CODIGO_VC)
    );

    -- Índice para búsqueda por código (uso frecuente)
    CREATE NONCLUSTERED INDEX IX_MONEDA_CODIGO
        ON coin.MONEDA (MONE_CODIGO_VC)
        INCLUDE (MONE_NOMBRE_VC, MONE_SIMBOLO_VC, MONE_ACTIVO_BT);

    PRINT 'Tabla coin.MONEDA creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla coin.MONEDA ya existe.';
END
GO

-- Datos semilla
IF NOT EXISTS (SELECT 1 FROM coin.MONEDA WHERE MONE_CODIGO_VC = 'BTC')
BEGIN
    INSERT INTO coin.MONEDA (MONE_NOMBRE_VC, MONE_CODIGO_VC, MONE_SIMBOLO_VC)
    VALUES
        ('Bitcoin',  'BTC',  '₿'),
        ('Ethereum', 'ETH',  'Ξ'),
        ('USDT',     'USDT', '₮');

    PRINT 'Datos semilla insertados.';
END
GO
