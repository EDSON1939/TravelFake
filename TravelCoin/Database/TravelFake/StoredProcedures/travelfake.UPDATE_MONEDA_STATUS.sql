-- =============================================================================
-- SP:          travelfake.UPDATE_MONEDA_STATUS
-- Base:        TRAVELFAKE
-- Descripcion: Actualiza el estado activo/inactivo de una moneda (baja lógica).
--              Una moneda inactiva no puede usarse para convertir montos.
-- Parametros:
--   @MONEDAS_ID_IT      BIGINT  ID de la moneda
--   @MONEDAS_ACTIVO_BT  BIT     Nuevo estado (1=activo, 0=inactivo)
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
    WHERE s.name = 'travelfake' AND p.name = 'UPDATE_MONEDA_STATUS'
)
    DROP PROCEDURE travelfake.UPDATE_MONEDA_STATUS;
GO

CREATE PROCEDURE travelfake.UPDATE_MONEDA_STATUS
    @MONEDAS_ID_IT      BIGINT,
    @MONEDAS_ACTIVO_BT  BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE MONEDAS
    SET    MONEDAS_ACTIVO_BT              = @MONEDAS_ACTIVO_BT,
           MONEDAS_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  MONEDAS_ID_IT = @MONEDAS_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
