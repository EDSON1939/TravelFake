-- =============================================================================
-- SP:          travelfake.DELETE_PAIS
-- Base:        TRAVELFAKE
-- Descripcion: Elimina físicamente un país. No elimina si tiene clientes
--              asociados (FK_PAIS). Para una baja lógica use
--              travelfake.UPDATE_PAIS_STATUS.
-- Parametros:
--   @PAIS_ID_IT  BIGINT  ID del país
-- Retorna: BIGINT — filas afectadas
--          ( 1 = éxito, 0 = no encontrado, -1 = tiene clientes asociados )
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'DELETE_PAIS'
)
    DROP PROCEDURE travelfake.DELETE_PAIS;
GO

CREATE PROCEDURE travelfake.DELETE_PAIS
    @PAIS_ID_IT BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM CLIENTES WHERE CLIENTES_PAIS_ID_IT = @PAIS_ID_IT)
    BEGIN
        -- Integridad referencial: el país está en uso
        SELECT CAST(-1 AS BIGINT) AS FilasAfectadas;
        RETURN;
    END

    DELETE FROM PAIS
    WHERE  PAIS_ID_IT = @PAIS_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
