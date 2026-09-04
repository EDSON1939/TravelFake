-- =============================================================================
-- SP:          travelfake.UPDATE_PAIS
-- Base:        TRAVELFAKE
-- Descripcion: Actualiza los datos de un país existente
-- Parametros:
--   @PAIS_ID_IT      BIGINT         ID del país
--   @PAIS_NOMBRE_VC  NVARCHAR(100)  Nuevo nombre del país
--   @PAIS_CODIGO_VC  NVARCHAR(100)  Nuevo código del país
--   @PAIS_ACTIVO_BT  BIT            Nuevo estado (1=activo, 0=inactivo)
-- Retorna: BIGINT — filas afectadas (1=éxito, 0=no encontrado)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'UPDATE_PAIS'
)
    DROP PROCEDURE travelfake.UPDATE_PAIS;
GO

CREATE PROCEDURE travelfake.UPDATE_PAIS
    @PAIS_ID_IT      BIGINT,
    @PAIS_NOMBRE_VC  NVARCHAR(100),
    @PAIS_CODIGO_VC  NVARCHAR(100),
    @PAIS_ACTIVO_BT  BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE PAIS
    SET    PAIS_NOMBRE_VC              = @PAIS_NOMBRE_VC,
           PAIS_CODIGO_VC              = @PAIS_CODIGO_VC,
           PAIS_ACTIVO_BT              = @PAIS_ACTIVO_BT,
           PAIS_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  PAIS_ID_IT = @PAIS_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
