-- =============================================================================
-- SP:          travelfake.GET_PAIS_BY_ID
-- Base:        TRAVELFAKE
-- Descripcion: Obtiene un país por su identificador
-- Parametros:
--   @PAIS_ID_IT  BIGINT  ID del país
-- Retorna: 1 fila de PAIS o vacío si no existe
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'GET_PAIS_BY_ID'
)
    DROP PROCEDURE travelfake.GET_PAIS_BY_ID;
GO

CREATE PROCEDURE travelfake.GET_PAIS_BY_ID
    @PAIS_ID_IT BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PAIS_ID_IT                   AS CountryId,
           PAIS_NOMBRE_VC               AS Name,
           PAIS_CODIGO_VC               AS Code,
           PAIS_ACTIVO_BT               AS IsActive,
           PAIS_FECHA_CREACION_DT       AS CreatedAt,
           PAIS_FECHA_ACTUALIZACION_DT  AS UpdatedAt
    FROM   PAIS
    WHERE  PAIS_ID_IT = @PAIS_ID_IT;
END
GO
