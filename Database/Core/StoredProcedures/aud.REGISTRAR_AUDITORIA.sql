-- =============================================================================
-- Schema: aud
-- SP:     REGISTRAR_AUDITORIA
-- Descripcion: Guarda el antes y el despues de un registro que cambio.
--
-- Lo llaman los stored procedures de negocio DENTRO de su propia transaccion,
-- nunca la aplicacion. Esa es la garantia entera: si la operacion hace
-- rollback, la auditoria se va con ella, y si la auditoria falla, la operacion
-- no queda aplicada sin rastro. Un cambio auditado a medias es peor que no
-- auditar, porque parece completo.
--
-- Los JSON se arman en el SP llamador con FOR JSON PATH, WITHOUT_ARRAY_WRAPPER,
-- que devuelve el registro entero sin tener que enumerar columnas: si manana se
-- agrega un campo a la tabla, la auditoria lo guarda sola.
--
-- Ejemplo de uso desde un SP de negocio:
--
--   DECLARE @Antes NVARCHAR(MAX) =
--       (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @Id
--        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
--
--   UPDATE pay.CUENTA SET CUEN_SALDO_DE = @Nuevo WHERE CUEN_ID_IT = @Id;
--
--   DECLARE @Despues NVARCHAR(MAX) =
--       (SELECT * FROM pay.CUENTA WHERE CUEN_ID_IT = @Id
--        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
--
--   EXEC aud.REGISTRAR_AUDITORIA 'pay', 'CUENTA', @Id, 'UPDATE',
--        @Antes, @Despues, @AUDITORIA_USUARIO_IT, @AUDITORIA_TRAZA_VC;
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'aud' AND p.name = 'REGISTRAR_AUDITORIA'
)
    DROP PROCEDURE aud.REGISTRAR_AUDITORIA;
GO

CREATE PROCEDURE aud.REGISTRAR_AUDITORIA
    @ESQUEMA_VC       VARCHAR(20),
    @TABLA_VC         VARCHAR(60),
    @LLAVE_VC         VARCHAR(64),
    -- INSERT | UPDATE | DELETE. Una baja logica se registra como DELETE aunque
    -- por dentro sea un UPDATE: lo que importa es la intencion, no la mecanica.
    @OPERACION_VC     VARCHAR(10),
    @DATO_ANTERIOR_NV NVARCHAR(MAX) = NULL,
    @DATO_NUEVO_NV    NVARCHAR(MAX) = NULL,
    @USUARIO_ID_IT    BIGINT        = NULL,
    @TRAZA_VC         VARCHAR(64)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Sin XACT_ABORT ni transaccion propia: este SP corre DENTRO de la
    -- transaccion del llamador y tiene que compartir su destino. Abrir una
    -- transaccion aca romperia justamente lo que la tabla promete.

    INSERT INTO aud.AUDITORIA (
        AUDI_ESQUEMA_VC, AUDI_TABLA_VC, AUDI_LLAVE_VC, AUDI_OPERACION_VC,
        AUDI_DATO_ANTERIOR_NV, AUDI_DATO_NUEVO_NV,
        AUDI_USUARIO_ID_IT, AUDI_TRAZA_VC, AUDI_FECHA_DT
    )
    VALUES (
        @ESQUEMA_VC, @TABLA_VC, @LLAVE_VC, @OPERACION_VC,
        @DATO_ANTERIOR_NV, @DATO_NUEVO_NV,
        @USUARIO_ID_IT, @TRAZA_VC, GETDATE()
    );
END
GO

PRINT 'Procedimiento aud.REGISTRAR_AUDITORIA creado correctamente.';
GO
