-- =============================================================================
-- SP:          travelfake.UPDATE_CLIENTE_STATUS
-- Base:        TRAVELFAKE
-- Descripcion: Actualiza el estado activo/inactivo de un cliente (baja lógica)
-- Parametros:
--   @CLIENTES_ID_IT      BIGINT  ID del cliente
--   @CLIENTES_ACTIVO_BT  BIT     Nuevo estado (1=activo, 0=inactivo)
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
    WHERE s.name = 'travelfake' AND p.name = 'UPDATE_CLIENTE_STATUS'
)
    DROP PROCEDURE travelfake.UPDATE_CLIENTE_STATUS;
GO

CREATE PROCEDURE travelfake.UPDATE_CLIENTE_STATUS
    @CLIENTES_ID_IT      BIGINT,
    @CLIENTES_ACTIVO_BT  BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CLIENTES
    SET    CLIENTES_ACTIVO_BT              = @CLIENTES_ACTIVO_BT,
           CLIENTES_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  CLIENTES_ID_IT = @CLIENTES_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
