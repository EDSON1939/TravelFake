-- =============================================================================
-- SP:          travelfake.INSERT_PAIS
-- Base:        TRAVELFAKE
-- Descripcion: Inserta un nuevo país y retorna el ID generado
-- Parametros:
--   @PAIS_NOMBRE_VC  NVARCHAR(100)  Nombre del país
--   @PAIS_CODIGO_VC  NVARCHAR(100)  Código del país (ej: PE, CO, MX)
-- Retorna: BIGINT — ID del país insertado (SCOPE_IDENTITY)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'INSERT_PAIS'
)
    DROP PROCEDURE travelfake.INSERT_PAIS;
GO

CREATE PROCEDURE travelfake.INSERT_PAIS
    @PAIS_NOMBRE_VC  NVARCHAR(100),
    @PAIS_CODIGO_VC  NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PAIS (
        PAIS_NOMBRE_VC,
        PAIS_CODIGO_VC,
        PAIS_ACTIVO_BT,
        PAIS_FECHA_CREACION_DT
    )
    VALUES (
        @PAIS_NOMBRE_VC,
        @PAIS_CODIGO_VC,
        1,
        GETDATE()
    );

    -- ExecuteScalarAsync lee el primer valor del primer resultado y lo castea a long:
    -- SCOPE_IDENTITY() devuelve NUMERIC(38,0), por eso el CAST explícito a BIGINT.
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS PAIS_ID_IT;
END
GO
