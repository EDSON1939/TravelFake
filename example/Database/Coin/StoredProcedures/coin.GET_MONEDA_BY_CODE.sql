-- =============================================================================
-- SP:          coin.GET_MONEDA_BY_CODE
-- Descripcion: Obtiene una moneda por su código único
-- Parametros:
--   @MONE_CODIGO_VC  VARCHAR(10)  Código de la moneda (ej: BTC, ETH)
-- Retorna: 1 fila de coin.MONEDA o vacío si no existe
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'coin' AND p.name = 'GET_MONEDA_BY_CODE'
)
    DROP PROCEDURE coin.GET_MONEDA_BY_CODE;
GO

CREATE PROCEDURE coin.GET_MONEDA_BY_CODE
    @MONE_CODIGO_VC VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MONE_ID_IT               AS CoinId,
           MONE_NOMBRE_VC           AS Name,
           MONE_CODIGO_VC           AS Code,
           MONE_SIMBOLO_VC          AS Symbol,
           MONE_ACTIVO_BT           AS IsActive,
           MONE_FECHA_CREACION_DT   AS CreatedAt
    FROM   coin.MONEDA
    WHERE  MONE_CODIGO_VC = @MONE_CODIGO_VC;
END
GO
