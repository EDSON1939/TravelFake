-- =============================================================================
-- SP:          coin.UPDATE_MONEDA_STATUS
-- Descripcion: Actualiza el estado activo/inactivo de una moneda
-- Parametros:
--   @MONE_ID_IT      BIGINT  ID de la moneda
--   @MONE_ACTIVO_BT  BIT     Nuevo estado (1=activa, 0=inactiva)
-- Retorna: BIGINT — filas afectadas (1=éxito, 0=no encontrada)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'coin' AND p.name = 'UPDATE_MONEDA_STATUS'
)
    DROP PROCEDURE coin.UPDATE_MONEDA_STATUS;
GO

CREATE PROCEDURE coin.UPDATE_MONEDA_STATUS
    @MONE_ID_IT      BIGINT,
    @MONE_ACTIVO_BT  BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE coin.MONEDA
    SET    MONE_ACTIVO_BT              = @MONE_ACTIVO_BT,
           MONE_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  MONE_ID_IT = @MONE_ID_IT;

    -- ExecuteScalarAsync lee el primer valor del primer resultado
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO
