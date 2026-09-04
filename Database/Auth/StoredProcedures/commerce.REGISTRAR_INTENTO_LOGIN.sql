-- =============================================================================
-- SP:          commerce.REGISTRAR_INTENTO_LOGIN
-- Descripcion: Sella el resultado de un intento de autenticacion. Es lo que
--              frena la fuerza bruta: sin el contador se puede probar
--              contrasenas indefinidamente.
--
--              Exitoso: reinicia el contador, levanta el bloqueo y sella el
--              ultimo acceso. Fallido: suma uno y, al llegar al maximo, aplica
--              el bloqueo y vuelve el contador a cero, para que al vencer el
--              castigo el usuario tenga de nuevo su cupo completo.
--
--              El bloqueo se guarda en UTC: LoginCommandHandler lo compara
--              contra DateTime.UtcNow, y con hora local en un huso negativo el
--              bloqueo nace vencido.
--
-- Parametros:
--   @USUA_ID_IT      BIGINT  ID del usuario
--   @Exitoso         BIT     1 si la contrasena era correcta
--   @MaxIntentos     INT     Fallos tolerados antes de bloquear
--   @MinutosBloqueo  INT     Duracion del bloqueo
--
-- Retorna: BIGINT
--    1  Intento registrado
--    0  El usuario no existe o esta dado de baja
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND p.name = 'REGISTRAR_INTENTO_LOGIN'
)
    DROP PROCEDURE commerce.REGISTRAR_INTENTO_LOGIN;
GO

-- commerce.USUARIO tiene indices filtrados, que exigen QUOTED_IDENTIFIER ON al
-- crear cualquier procedimiento que la escriba. Ver commerce.INSERT_USUARIO.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE PROCEDURE commerce.REGISTRAR_INTENTO_LOGIN
    @USUA_ID_IT           BIGINT,
    @Exitoso              BIT,
    @MaxIntentos          INT,
    @MinutosBloqueo       INT,
    @AUDITORIA_TRAZA_VC   VARCHAR(64) = NULL,
    @AUDITORIA_USUARIO_IT BIGINT      = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Afectadas INT,
            @Intentos  INT,
            @Antes     NVARCHAR(MAX),
            @Despues   NVARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- La fila se bloquea antes de leer el contador: dos intentos fallidos
        -- simultaneos que lo leyeran a la vez sumarian uno solo, y ese es
        -- justamente el caso que el freno tiene que atajar.
        SELECT @Intentos = USUA_INTENTOS_FALLIDOS_IT
        FROM   commerce.USUARIO WITH (UPDLOCK, HOLDLOCK)
        WHERE  USUA_ID_IT                = @USUA_ID_IT
          AND  USUA_FECHA_ELIMINACION_DT IS NULL;

        IF @Intentos IS NULL
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(0 AS BIGINT) AS Resultado;
            RETURN;
        END

        -- Sin hash de contrasena en el JSON. Ver commerce.INSERT_USUARIO.
        SET @Antes = (SELECT USUA_ID_IT, USUA_USERNAME_VC, USUA_ACTIVO_BT,
                             USUA_INTENTOS_FALLIDOS_IT, USUA_BLOQUEADO_HASTA_DT,
                             USUA_ULTIMO_ACCESO_DT
                      FROM   commerce.USUARIO
                      WHERE  USUA_ID_IT = @USUA_ID_IT
                      FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        IF @Exitoso = 1
        BEGIN
            UPDATE commerce.USUARIO
            SET    USUA_INTENTOS_FALLIDOS_IT   = 0,
                   USUA_BLOQUEADO_HASTA_DT     = NULL,
                   USUA_ULTIMO_ACCESO_DT       = GETDATE(),
                   USUA_FECHA_ACTUALIZACION_DT = GETDATE()
            WHERE  USUA_ID_IT = @USUA_ID_IT;

            SET @Afectadas = @@ROWCOUNT;
        END
        ELSE
        BEGIN
            SET @Intentos = @Intentos + 1;

            UPDATE commerce.USUARIO
            SET    USUA_INTENTOS_FALLIDOS_IT   = CASE WHEN @Intentos >= @MaxIntentos
                                                      THEN 0 ELSE @Intentos END,
                   USUA_BLOQUEADO_HASTA_DT     = CASE WHEN @Intentos >= @MaxIntentos
                                                      THEN DATEADD(MINUTE, @MinutosBloqueo, GETUTCDATE())
                                                      ELSE USUA_BLOQUEADO_HASTA_DT END,
                   USUA_FECHA_ACTUALIZACION_DT = GETDATE()
            WHERE  USUA_ID_IT = @USUA_ID_IT;

            SET @Afectadas = @@ROWCOUNT;
        END

        SET @Despues = (SELECT USUA_ID_IT, USUA_USERNAME_VC, USUA_ACTIVO_BT,
                               USUA_INTENTOS_FALLIDOS_IT, USUA_BLOQUEADO_HASTA_DT,
                               USUA_ULTIMO_ACCESO_DT
                        FROM   commerce.USUARIO
                        WHERE  USUA_ID_IT = @USUA_ID_IT
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);

        EXEC aud.REGISTRAR_AUDITORIA 'commerce', 'USUARIO', @USUA_ID_IT, 'UPDATE',
             @Antes, @Despues, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;

        COMMIT TRANSACTION;

        SELECT CAST(@Afectadas AS BIGINT) AS Resultado;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
