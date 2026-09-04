-- =============================================================================
-- SP:          commerce.DELETE_USUARIO
-- Descripcion: Baja logica de una credencial. El usuario deja de poder
--              autenticarse pero la fila se conserva: los registros historicos
--              -bitacora, auditoria, movimientos- apuntan a este ID y borrarlo
--              dejaria esas referencias colgando.
--
--              Solo alcanza a los que siguen vigentes. Volver a darlo de baja
--              no cambia nada y devuelve 0, que el handler traduce a
--              DELETE_FAILED.
--
-- Parametros:
--   @USUA_ID_IT  BIGINT  ID del usuario
--
-- Retorna: BIGINT
--    1  Usuario dado de baja
--    0  No existe o ya estaba dado de baja
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND p.name = 'DELETE_USUARIO'
)
    DROP PROCEDURE commerce.DELETE_USUARIO;
GO

-- commerce.USUARIO tiene indices filtrados, que exigen QUOTED_IDENTIFIER ON al
-- crear cualquier procedimiento que la escriba. Ver commerce.INSERT_USUARIO.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE commerce.DELETE_USUARIO
    @USUA_ID_IT           BIGINT,
    @AUDITORIA_TRAZA_VC   VARCHAR(64) = NULL,
    @AUDITORIA_USUARIO_IT BIGINT      = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Afectadas INT,
            @Antes     NVARCHAR(MAX),
            @Despues   NVARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Sin hash de contrasena en el JSON. Ver commerce.INSERT_USUARIO.
        SET @Antes = (SELECT USUA_ID_IT, USUA_USERNAME_VC, USUA_CLIENTE_ID_IT,
                             USUA_NOMBRE_VC, USUA_ROL_VC, USUA_ACTIVO_BT,
                             USUA_FECHA_ELIMINACION_DT
                      FROM   commerce.USUARIO
                      WHERE  USUA_ID_IT = @USUA_ID_IT
                      FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        UPDATE commerce.USUARIO
        SET    USUA_ACTIVO_BT              = 0,
               USUA_FECHA_ELIMINACION_DT   = GETDATE(),
               USUA_FECHA_ACTUALIZACION_DT = GETDATE()
        WHERE  USUA_ID_IT                = @USUA_ID_IT
          AND  USUA_FECHA_ELIMINACION_DT IS NULL;

        SET @Afectadas = @@ROWCOUNT;

        IF @Afectadas > 0
        BEGIN
            SET @Despues = (SELECT USUA_ID_IT, USUA_USERNAME_VC, USUA_CLIENTE_ID_IT,
                                   USUA_NOMBRE_VC, USUA_ROL_VC, USUA_ACTIVO_BT,
                                   USUA_FECHA_ELIMINACION_DT
                            FROM   commerce.USUARIO
                            WHERE  USUA_ID_IT = @USUA_ID_IT
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

            -- Se registra como DELETE aunque por dentro sea un UPDATE: lo que
            -- importa es la intencion, no la mecanica.
            EXEC aud.REGISTRAR_AUDITORIA 'commerce', 'USUARIO', @USUA_ID_IT, 'DELETE',
                 @Antes, @Despues, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;
        END

        COMMIT TRANSACTION;

        SELECT CAST(@Afectadas AS BIGINT) AS Resultado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
