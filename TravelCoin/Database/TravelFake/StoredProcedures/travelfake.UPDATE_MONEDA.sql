-- =============================================================================
-- SP:          travelfake.UPDATE_MONEDA
-- Base:        TRAVELFAKE
-- Descripcion: Actualiza los datos de una moneda existente
-- Parametros:
--   @MONEDAS_ID_IT        BIGINT         ID de la moneda
--   @MONEDAS_NOMBRE_VC    NVARCHAR(100)  Nuevo nombre
--   @MONEDAS_CODIGO_VC    NVARCHAR(100)  Nuevo código
--   @MONEDAS_TCCOMPRA_DC  DECIMAL(18,2)  Nuevo tipo de cambio de compra
--   @MONEDAS_ACTIVO_BT    BIT            Nuevo estado (1=activo, 0=inactivo)
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
    WHERE s.name = 'travelfake' AND p.name = 'UPDATE_MONEDA'
)
    DROP PROCEDURE travelfake.UPDATE_MONEDA;
GO

CREATE PROCEDURE travelfake.UPDATE_MONEDA
    @MONEDAS_ID_IT        BIGINT,
    @MONEDAS_NOMBRE_VC    NVARCHAR(100),
    @MONEDAS_CODIGO_VC    NVARCHAR(100),
    @MONEDAS_TCCOMPRA_DC  DECIMAL(18,2),
    @MONEDAS_ACTIVO_BT    BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE MONEDAS
    SET    MONEDAS_NOMBRE_VC              = @MONEDAS_NOMBRE_VC,
           MONEDAS_CODIGO_VC              = @MONEDAS_CODIGO_VC,
           MONEDAS_TCCOMPRA_DC            = @MONEDAS_TCCOMPRA_DC,
           MONEDAS_ACTIVO_BT              = @MONEDAS_ACTIVO_BT,
           MONEDAS_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  MONEDAS_ID_IT = @MONEDAS_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
