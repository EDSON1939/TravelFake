-- =============================================================================
-- SP:          qr.UPDATE_QR_CONSUMIDO
-- Descripcion: Consume un código QR de unico uso (pasa de ACTIVO a USED).
--   Solo afecta QRs con tipo UNICO y estado ACTIVO: los de uso multiple
--   permanecen ACTIVOS hasta expirar.
-- Parametros:
--   @QR_ID_IT  BIGINT  ID del QR a consumir
-- Retorna: BIGINT — filas afectadas (1=consumido, 0=no elegible)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'qr' AND p.name = 'UPDATE_QR_CONSUMIDO'
)
    DROP PROCEDURE qr.UPDATE_QR_CONSUMIDO;
GO

CREATE PROCEDURE qr.UPDATE_QR_CONSUMIDO
    @QR_ID_IT BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE qr.QR
    SET    QR_ESTADO_VC              = 'USED',
           QR_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  QR_ID_IT       = @QR_ID_IT
      AND  QR_ESTADO_VC   = 'ACTIVE'
      AND  QR_TIPO_VC     = 'UNICO'
      AND  QR_ACTIVO_BT   = 1;

    -- ExecuteScalarAsync lee el primer valor del primer resultado
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;

END
GO