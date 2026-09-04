-- =============================================================================
-- SP:          qr.GET_QR_BY_ID
-- Descripcion: Obtiene un código QR por su identificador
-- Parametros:
--   @QR_ID_IT  BIGINT  ID del QR
-- Retorna: 1 fila de qr.QR o vacío si no existe
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'qr' AND p.name = 'GET_QR_BY_ID'
)
    DROP PROCEDURE qr.GET_QR_BY_ID;
GO

CREATE PROCEDURE qr.GET_QR_BY_ID
    @QR_ID_IT BIGINT
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
    WHERE  QR_ID_IT = @QR_ID_IT;
END
GO