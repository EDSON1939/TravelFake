-- =============================================================================
-- SP:          qr.INSERT_QR
-- Descripcion: Inserta un nuevo código QR y retorna el ID generado
-- Parametros:
--   @QR_CODIGO_VC             VARCHAR(40)    Código único del QR generado
--   @QR_COMERCIO_ID_IT        BIGINT         Comercio dueño del QR
--   @QR_MONTO_DE              DECIMAL(18,2)  Monto del QR
--   @QR_TIPO_VC               VARCHAR(10)    UNICO | MULTIPLE
--   @QR_FECHA_EXPIRACION_DT   DATETIME       Fecha límite de validez
-- Retorna: BIGINT — ID del QR insertado (SCOPE_IDENTITY)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'qr' AND p.name = 'INSERT_QR'
)
    DROP PROCEDURE qr.INSERT_QR;
GO

CREATE PROCEDURE qr.INSERT_QR
    @QR_CODIGO_VC             VARCHAR(40),
    @QR_COMERCIO_ID_IT        BIGINT,
    @QR_MONTO_DE              DECIMAL(18,2),
    @QR_TIPO_VC               VARCHAR(10),
    @QR_FECHA_EXPIRACION_DT   DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO qr.QR (
        QR_CODIGO_VC,
        QR_COMERCIO_ID_IT,
        QR_MONTO_DE,
        QR_TIPO_VC,
        QR_FECHA_EXPIRACION_DT,
        QR_ESTADO_VC,
        QR_ACTIVO_BT,
        QR_FECHA_CREACION_DT
    )
    VALUES (
        @QR_CODIGO_VC,
        @QR_COMERCIO_ID_IT,
        @QR_MONTO_DE,
        @QR_TIPO_VC,
        @QR_FECHA_EXPIRACION_DT,
        'ACTIVE',
        1,
        GETDATE()
    );

    -- ExecuteScalarAsync lee el primer valor del primer resultado
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS QR_ID_IT;
END
GO