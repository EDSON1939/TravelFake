-- =============================================================================
-- SP:          travelfake.GET_MONEDA_ALL
-- Base:        TRAVELFAKE
-- Descripcion: Obtiene el listado de monedas con filtro opcional por estado
-- Parametros:
--   @MONEDAS_SOLO_ACTIVAS_BT  BIT  1 = solo activas, 0 = todas
-- Retorna: Lista de MONEDAS ordenada por nombre
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'GET_MONEDA_ALL'
)
    DROP PROCEDURE travelfake.GET_MONEDA_ALL;
GO

CREATE PROCEDURE travelfake.GET_MONEDA_ALL
    @MONEDAS_SOLO_ACTIVAS_BT BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MONEDAS_ID_IT                   AS CoinId,
           MONEDAS_NOMBRE_VC               AS Name,
           MONEDAS_CODIGO_VC               AS Code,
           MONEDAS_TCCOMPRA_DC             AS BuyRate,
           MONEDAS_ACTIVO_BT               AS IsActive,
           MONEDAS_FECHA_CREACION_DT       AS CreatedAt,
           MONEDAS_FECHA_ACTUALIZACION_DT  AS UpdatedAt
    FROM   MONEDAS
    WHERE  (@MONEDAS_SOLO_ACTIVAS_BT = 0 OR MONEDAS_ACTIVO_BT = 1)
    ORDER BY MONEDAS_NOMBRE_VC ASC;
END
GO
