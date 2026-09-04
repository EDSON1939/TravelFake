-- =============================================================================
-- SP:          commerce.UPDATE_COMERCIO_ESTADO
-- Descripcion: Actualiza el estado activo/inactivo de un comercio
-- Parametros:
--   @COME_ID_IT      BIGINT  ID del comercio
--   @COME_ACTIVO_BT  BIT     Nuevo estado (1=activo, 0=inactivo)
-- Retorna: BIGINT — filas afectadas (1=éxito, 0=no encontrada)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND p.name = 'UPDATE_COMERCIO_ESTADO'
)
    DROP PROCEDURE commerce.UPDATE_COMERCIO_ESTADO;
GO

CREATE PROCEDURE commerce.UPDATE_COMERCIO_ESTADO
    @COME_ID_IT      BIGINT,
    @COME_ACTIVO_BT  BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE commerce.COMERCIO
    SET    COME_ACTIVO_BT              = @COME_ACTIVO_BT,
           COME_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  COME_ID_IT = @COME_ID_IT;

    -- @@ROWCOUNT es INT, por eso se convierte a BIGINT.
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
