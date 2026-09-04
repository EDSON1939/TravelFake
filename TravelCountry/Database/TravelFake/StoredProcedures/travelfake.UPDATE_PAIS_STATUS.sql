-- =============================================================================
-- SP:          travelfake.UPDATE_PAIS_STATUS
-- Base:        TRAVELFAKE
-- Descripcion: Actualiza el estado activo/inactivo de un país (baja lógica)
-- Parametros:
--   @PAIS_ID_IT      BIGINT  ID del país
--   @PAIS_ACTIVO_BT  BIT     Nuevo estado (1=activo, 0=inactivo)
-- Retorna: BIGINT — filas afectadas (1=éxito, 0=no encontrado)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'UPDATE_PAIS_STATUS'
)
    DROP PROCEDURE travelfake.UPDATE_PAIS_STATUS;
GO

CREATE PROCEDURE travelfake.UPDATE_PAIS_STATUS
    @PAIS_ID_IT      BIGINT,
    @PAIS_ACTIVO_BT  BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE PAIS
    SET    PAIS_ACTIVO_BT              = @PAIS_ACTIVO_BT,
           PAIS_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  PAIS_ID_IT = @PAIS_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
