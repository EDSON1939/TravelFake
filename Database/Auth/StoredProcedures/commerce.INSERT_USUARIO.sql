-- =============================================================================
-- SP:          commerce.INSERT_USUARIO
-- Descripcion: Da de alta una credencial. La contrasena llega ya hasheada: el
--              SP no la interpreta ni la devuelve, solo la guarda.
--
-- Parametros:
--   @USUA_USERNAME_VC      VARCHAR(50)    Nombre de usuario, ya normalizado
--   @USUA_PASSWORD_HASH_VC VARCHAR(256)   PBKDF2 "iteraciones.salt.hash"
--   @USUA_CLIENTE_ID_IT    BIGINT         Cliente asociado, NULL en operadores
--   @USUA_NOMBRE_VC        NVARCHAR(150)  Nombre completo
--   @USUA_ROL_VC           VARCHAR(20)    CLIENTE | AGENTE | ADMIN
--
-- Retorna: BIGINT
--   > 0  ID del usuario creado
--   -1   El username ya existe, o el cliente ya tiene una credencial
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND p.name = 'INSERT_USUARIO'
)
    DROP PROCEDURE commerce.INSERT_USUARIO;
GO

-- Los INSERT sobre commerce.USUARIO tocan los indices filtrados
-- UQ_COMMERCE_USUARIO_USERNAME y UQ_COMMERCE_USUARIO_CLIENTE, que exigen
-- QUOTED_IDENTIFIER ON. El valor queda grabado JUNTO al procedimiento al
-- crearlo: SSMS lo trae activado, pero sqlcmd lo deja OFF, y sin esta linea el
-- SP se crea igual y recien falla al ejecutarse (Msg 1934).
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE commerce.INSERT_USUARIO
    @USUA_USERNAME_VC      VARCHAR(50),
    @USUA_PASSWORD_HASH_VC VARCHAR(256),
    @USUA_CLIENTE_ID_IT    BIGINT,
    @USUA_NOMBRE_VC        NVARCHAR(150),
    @USUA_ROL_VC           VARCHAR(20),
    -- Auditoria: quien dio de alta la credencial y bajo que traza. Default NULL
    -- para no romper llamadores que todavia no los manden.
    @AUDITORIA_TRAZA_VC    VARCHAR(64) = NULL,
    @AUDITORIA_USUARIO_IT  BIGINT      = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @UsuarioId BIGINT,
            @Json      NVARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO commerce.USUARIO (
            USUA_USERNAME_VC,
            USUA_PASSWORD_HASH_VC,
            USUA_CLIENTE_ID_IT,
            USUA_NOMBRE_VC,
            USUA_ROL_VC,
            USUA_ACTIVO_BT,
            USUA_INTENTOS_FALLIDOS_IT,
            USUA_FECHA_CREACION_DT
        )
        VALUES (
            @USUA_USERNAME_VC,
            @USUA_PASSWORD_HASH_VC,
            @USUA_CLIENTE_ID_IT,
            @USUA_NOMBRE_VC,
            @USUA_ROL_VC,
            1,
            0,
            GETDATE()
        );

        SET @UsuarioId = CAST(SCOPE_IDENTITY() AS BIGINT);

        -- Las columnas van enumeradas a proposito, no con SELECT *: el hash de
        -- la contrasena no puede terminar copiado en aud.AUDITORIA, que es una
        -- tabla de lectura mucho mas amplia que la propia commerce.USUARIO.
        SET @Json = (SELECT USUA_ID_IT, USUA_USERNAME_VC, USUA_CLIENTE_ID_IT,
                            USUA_NOMBRE_VC, USUA_ROL_VC, USUA_ACTIVO_BT,
                            USUA_FECHA_CREACION_DT
                     FROM   commerce.USUARIO
                     WHERE  USUA_ID_IT = @UsuarioId
                     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        EXEC aud.REGISTRAR_AUDITORIA 'commerce', 'USUARIO', @UsuarioId, 'INSERT',
             NULL, @Json, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        COMMIT TRANSACTION;

        -- ExecuteScalarAsync lee el primer valor del primer resultado
        SELECT CAST(@UsuarioId AS BIGINT) AS Resultado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        -- Username repetido o cliente que ya tiene credencial. Es una regla de
        -- negocio, no una falla del sistema: el handler ya hace el pre-chequeo
        -- y esto es la red para la carrera entre dos altas simultaneas.
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            SELECT CAST(-1 AS BIGINT) AS Resultado;
            RETURN;
        END;

        THROW;
    END CATCH
END
GO
