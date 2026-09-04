-- =============================================================================
-- Schema: aud
-- SP:     INSERT_BITACORA
-- Descripcion: Registra una llamada atendida. La escribe el interceptor gRPC al
--              terminar, porque recien ahi se conocen el resultado y la
--              duracion.
--
-- Como se escribe al final, las filas de aud.AUDITORIA que dejo esa misma
-- llamada ya existen y todavia no saben a que bitacora pertenecen. Este SP las
-- enlaza por TraceId, que es el unico dato que ambas mitades comparten en el
-- momento de escribirse.
--
-- Retorno: el BITA_ID_IT generado.
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'aud' AND p.name = 'INSERT_BITACORA'
)
    DROP PROCEDURE aud.INSERT_BITACORA;
GO

CREATE PROCEDURE aud.INSERT_BITACORA
    @SERVICIO_VC    VARCHAR(60),
    @METODO_VC      VARCHAR(160),
    @OPERACION_VC   VARCHAR(10),
    @USUARIO_ID_IT  BIGINT        = NULL,
    @USUARIO_VC     VARCHAR(60)   = NULL,
    @CLIENTE_ID_IT  BIGINT        = NULL,
    @ROL_VC         VARCHAR(20)   = NULL,
    @IP_VC          VARCHAR(45)   = NULL,
    @TRAZA_VC       VARCHAR(64)   = NULL,
    @PETICION_NV    NVARCHAR(MAX) = NULL,
    @ESTADO_VC      VARCHAR(10),
    @CODIGO_VC      VARCHAR(40)   = NULL,
    @MENSAJE_NV     NVARCHAR(500) = NULL,
    @DURACION_IT    INT           = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @BitacoraId BIGINT;

    INSERT INTO aud.BITACORA (
        BITA_SERVICIO_VC, BITA_METODO_VC, BITA_OPERACION_VC,
        BITA_USUARIO_ID_IT, BITA_USUARIO_VC, BITA_CLIENTE_ID_IT, BITA_ROL_VC,
        BITA_IP_VC, BITA_TRAZA_VC, BITA_PETICION_NV,
        BITA_ESTADO_VC, BITA_CODIGO_VC, BITA_MENSAJE_NV, BITA_DURACION_IT,
        BITA_FECHA_DT
    )
    VALUES (
        @SERVICIO_VC, @METODO_VC, @OPERACION_VC,
        @USUARIO_ID_IT, @USUARIO_VC, @CLIENTE_ID_IT, @ROL_VC,
        @IP_VC, @TRAZA_VC, @PETICION_NV,
        @ESTADO_VC, @CODIGO_VC, @MENSAJE_NV, @DURACION_IT,
        GETDATE()
    );

    SET @BitacoraId = CAST(SCOPE_IDENTITY() AS BIGINT);

    -- Enlaza los cambios de datos de esta misma llamada. La condicion de
    -- AUDI_BITACORA_ID_IT NULL evita pisar filas de una llamada anterior si el
    -- TraceId se reutilizara: solo se adoptan las huerfanas.
    IF @TRAZA_VC IS NOT NULL
    BEGIN
        UPDATE aud.AUDITORIA
        SET    AUDI_BITACORA_ID_IT = @BitacoraId
        WHERE  AUDI_TRAZA_VC       = @TRAZA_VC
          AND  AUDI_BITACORA_ID_IT IS NULL;
    END

    SELECT CAST(@BitacoraId AS BIGINT) AS Resultado;
END
GO

PRINT 'Procedimiento aud.INSERT_BITACORA creado correctamente.';
GO
