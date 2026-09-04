-- =============================================================================
-- SP:          commerce.UPDATE_COMERCIO
-- Descripcion: Actualiza el nombre y NIT de un comercio
-- Parametros:
--   @COME_ID_IT       BIGINT       ID del comercio
--   @COME_NOMBRE_VC   NVARCHAR(50) Nuevo nombre
--   @COME_NIT_VC      VARCHAR(13)  Nuevo NIT
-- Retorna: BIGINT — filas afectadas (1=éxito, 0=no encontrada)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND p.name = 'UPDATE_COMERCIO'
)
    DROP PROCEDURE commerce.UPDATE_COMERCIO;
GO

CREATE PROCEDURE commerce.UPDATE_COMERCIO
    @COME_ID_IT       BIGINT,
    @COME_NOMBRE_VC   NVARCHAR(50),
    @COME_NIT_VC      VARCHAR(13)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE commerce.COMERCIO
    SET    COME_NOMBRE_VC              = @COME_NOMBRE_VC,
           COME_NIT_VC                 = @COME_NIT_VC,
           COME_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  COME_ID_IT = @COME_ID_IT;

    -- @@ROWCOUNT es INT, por eso se convierte a BIGINT.
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
