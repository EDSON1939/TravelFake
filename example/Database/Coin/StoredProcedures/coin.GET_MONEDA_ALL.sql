-- =============================================================================
-- SP:          coin.GET_MONEDA_ALL
-- Descripcion: Obtiene el listado de monedas con filtro opcional por estado
-- Parametros:
--   @MONE_SOLO_ACTIVAS_BT  BIT  1 = solo activas, 0 = todas
-- Retorna: Lista de coin.MONEDA ordenada por nombre
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'coin' AND p.name = 'GET_MONEDA_ALL'
)
    DROP PROCEDURE coin.GET_MONEDA_ALL;
GO

CREATE PROCEDURE coin.GET_MONEDA_ALL
    @MONE_SOLO_ACTIVAS_BT BIT = 1
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
    WHERE  (@MONE_SOLO_ACTIVAS_BT = 0 OR MONE_ACTIVO_BT = 1)
    ORDER BY MONE_NOMBRE_VC ASC;
END
GO
