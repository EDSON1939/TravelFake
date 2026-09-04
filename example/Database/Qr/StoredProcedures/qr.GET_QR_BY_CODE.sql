-- =============================================================================
-- SP:          qr.GET_QR_BY_CODE
-- Descripcion: Obtiene un código QR por su código único
-- Parametros:
--   @QR_CODIGO_VC  VARCHAR(40)  Código del QR escaneado
-- Retorna: 1 fila de qr.QR o vacío si no existe
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'qr' AND p.name = 'GET_QR_BY_CODE'
)
    DROP PROCEDURE qr.GET_QR_BY_CODE;
GO

CREATE PROCEDURE qr.GET_QR_BY_CODE
    @QR_CODIGO_VC VARCHAR(40)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT QR_ID_IT                  AS QrId,
           QR_CODIGO_VC              AS Codigo,
           QR_COMERCIO_ID_IT         AS ComercioId,
           QR_MONTO_DE               AS Monto,
           QR_TIPO_VC                AS Tipo,
           QR_FECHA_EXPIRACION_DT    AS FechaExpiracion,
           QR_ESTADO_VC              AS Estado,
           QR_ACTIVO_BT              AS Activo,
           QR_FECHA_CREACION_DT      AS FechaCreacion,
           QR_FECHA_ACTUALIZACION_DT AS FechaActualizacion
    FROM   qr.QR
    WHERE  QR_CODIGO_VC = @QR_CODIGO_VC;
END
GO