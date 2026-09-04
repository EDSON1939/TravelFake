-- =============================================================================
-- SP:          travelfake.GET_PAIS_ALL
-- Base:        TRAVELFAKE
-- Descripcion: Obtiene el listado de países con filtro opcional por estado
-- Parametros:
--   @PAIS_SOLO_ACTIVOS_BT  BIT  1 = solo activos, 0 = todos
-- Retorna: Lista de PAIS ordenada por nombre
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'GET_PAIS_ALL'
)
    DROP PROCEDURE travelfake.GET_PAIS_ALL;
GO

CREATE PROCEDURE travelfake.GET_PAIS_ALL
    @PAIS_SOLO_ACTIVOS_BT BIT = 1
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
    WHERE  (@PAIS_SOLO_ACTIVOS_BT = 0 OR PAIS_ACTIVO_BT = 1)
    ORDER BY PAIS_NOMBRE_VC ASC;
END
GO
