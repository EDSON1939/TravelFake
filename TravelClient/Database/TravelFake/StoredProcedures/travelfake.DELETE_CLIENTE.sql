-- =============================================================================
-- SP:          travelfake.DELETE_CLIENTE
-- Base:        TRAVELFAKE
-- Descripcion: Elimina físicamente un cliente.
--              Para una baja lógica utilice travelfake.UPDATE_CLIENTE_STATUS.
-- Parametros:
--   @CLIENTES_ID_IT  BIGINT  ID del cliente
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
    WHERE s.name = 'travelfake' AND p.name = 'DELETE_CLIENTE'
)
    DROP PROCEDURE travelfake.DELETE_CLIENTE;
GO

CREATE PROCEDURE travelfake.DELETE_CLIENTE
    @CLIENTES_ID_IT BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM CLIENTES
    WHERE  CLIENTES_ID_IT = @CLIENTES_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
