-- =============================================================================
-- SP:          travelfake.DELETE_MONEDA
-- Base:        TRAVELFAKE
-- Descripcion: Elimina físicamente una moneda.
--              Para una baja lógica utilice travelfake.UPDATE_MONEDA_STATUS.
-- Parametros:
--   @MONEDAS_ID_IT  BIGINT  ID de la moneda
-- Retorna: BIGINT — filas afectadas (1=éxito, 0=no encontrado)
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'DELETE_MONEDA'
)
    DROP PROCEDURE travelfake.DELETE_MONEDA;
GO

CREATE PROCEDURE travelfake.DELETE_MONEDA
    @MONEDAS_ID_IT BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM MONEDAS
    WHERE  MONEDAS_ID_IT = @MONEDAS_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
