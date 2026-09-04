-- =============================================================================
-- SP:          travelfake.GET_MONEDA_BY_ID
-- Base:        TRAVELFAKE
-- Descripcion: Obtiene una moneda por su identificador.
--              Es la fuente del tipo de cambio que usa la conversión a bolivianos.
-- Parametros:
--   @MONEDAS_ID_IT  BIGINT  ID de la moneda
-- Retorna: 1 fila de MONEDAS o vacío si no existe
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'GET_MONEDA_BY_ID'
)
    DROP PROCEDURE travelfake.GET_MONEDA_BY_ID;
GO

CREATE PROCEDURE travelfake.GET_MONEDA_BY_ID
    @MONEDAS_ID_IT BIGINT
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
    WHERE  MONEDAS_ID_IT = @MONEDAS_ID_IT;
END
GO
