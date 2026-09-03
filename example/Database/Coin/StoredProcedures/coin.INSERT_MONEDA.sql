-- =============================================================================
-- SP:          coin.INSERT_MONEDA
-- Descripcion: Inserta una nueva moneda y retorna el ID generado
-- Parametros:
--   @MONE_NOMBRE_VC   NVARCHAR(100)  Nombre de la moneda
--   @MONE_CODIGO_VC   VARCHAR(10)    Código único (ej: BTC)
--   @MONE_SIMBOLO_VC  VARCHAR(10)    Símbolo visual (ej: ₿)
-- Retorna: BIGINT — ID de la moneda insertada (SCOPE_IDENTITY)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'coin' AND p.name = 'INSERT_MONEDA'
)
    DROP PROCEDURE coin.INSERT_MONEDA;
GO

CREATE PROCEDURE coin.INSERT_MONEDA
    @MONE_NOMBRE_VC   NVARCHAR(100),
    @MONE_CODIGO_VC   VARCHAR(10),
    @MONE_SIMBOLO_VC  VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO coin.MONEDA (
        MONE_NOMBRE_VC,
        MONE_CODIGO_VC,
        MONE_SIMBOLO_VC,
        MONE_ACTIVO_BT,
        MONE_FECHA_CREACION_DT
    )
    VALUES (
        @MONE_NOMBRE_VC,
        @MONE_CODIGO_VC,
        @MONE_SIMBOLO_VC,
        1,
        GETDATE()
    );

    -- ExecuteScalarAsync lee el primer valor del primer resultado
    SELECT SCOPE_IDENTITY() AS MONE_ID_IT;
END
GO
