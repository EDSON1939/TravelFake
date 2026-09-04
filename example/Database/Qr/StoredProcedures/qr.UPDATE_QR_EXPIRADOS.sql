-- =============================================================================
-- SP:          qr.UPDATE_QR_EXPIRADOS
-- Descripcion: Marca como EXPIRED todo QR ACTIVO cuya fecha de expiracion ya
--   vencio. Lo ejecuta el servicio en background cada hora.
-- Retorna: BIGINT — cantidad de QRs expirados en esta ejecución
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'qr' AND p.name = 'UPDATE_QR_EXPIRADOS'
)
    DROP PROCEDURE qr.UPDATE_QR_EXPIRADOS;
GO

CREATE PROCEDURE qr.UPDATE_QR_EXPIRADOS
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE qr.QR
    SET    QR_ESTADO_VC              = 'EXPIRED',
           QR_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  QR_ESTADO_VC            = 'ACTIVE'
      AND  QR_ACTIVO_BT            = 1
      AND  QR_FECHA_EXPIRACION_DT  < GETDATE();

    -- ExecuteScalarAsync lee el primer valor del primer resultado
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO