-- =============================================================================
-- Schema: aud
-- Tabla:  AUDITORIA
-- Descripcion: Registro de COMO QUEDO EL DATO. Una fila por cada registro que
--              cambio, con el JSON de antes y el de despues. Responde "como
--              llego este campo a tener este valor y quien lo puso ahi".
--
-- Se escribe DENTRO de la transaccion que hace el cambio, desde el mismo stored
-- procedure. Esa decision es el punto entero de la tabla: si se escribiera
-- despues, desde la aplicacion, una caida entre el UPDATE y el INSERT dejaria
-- un cambio sin rastro, que es justo lo que una auditoria no puede permitir.
-- El precio es que un fallo al auditar deshace la operacion; para un libro
-- mayor es el intercambio correcto.
--
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   AUDI = prefijo de tabla AUDITORIA
--   _IT  = INT / BIGINT
--   _VC  = VARCHAR / NVARCHAR
--   _NV  = NVARCHAR(MAX)
--   _DT  = DATETIME
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'aud')
BEGIN
    EXEC('CREATE SCHEMA aud');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'aud' AND t.name = 'AUDITORIA'
)
BEGIN
    CREATE TABLE aud.AUDITORIA (
        AUDI_ID_IT              BIGINT          NOT NULL IDENTITY(1,1),

        -- Llamada que origino el cambio. NULL cuando el cambio no vino de un
        -- RPC: una carga inicial, un ajuste por script, un job.
        AUDI_BITACORA_ID_IT     BIGINT          NULL,

        -- Que registro cambio.
        AUDI_ESQUEMA_VC         VARCHAR(20)     NOT NULL,
        AUDI_TABLA_VC           VARCHAR(60)     NOT NULL,
        -- Clave primaria del registro, como texto: sirve igual para un BIGINT
        -- que para una clave compuesta.
        AUDI_LLAVE_VC           VARCHAR(64)     NOT NULL,

        -- Que se le hizo. Ver CK_AUD_AUDITORIA_OPERACION.
        AUDI_OPERACION_VC       VARCHAR(10)     NOT NULL,

        -- El registro antes y despues, en JSON.
        --   INSERT -> ANTERIOR NULL
        --   DELETE -> NUEVO NULL (aunque fisicamente sea un UPDATE, ver abajo)
        --   UPDATE -> los dos
        AUDI_DATO_ANTERIOR_NV   NVARCHAR(MAX)   NULL,
        AUDI_DATO_NUEVO_NV      NVARCHAR(MAX)   NULL,

        -- Quien. Se copia desde la bitacora en vez de joinear: una auditoria
        -- tiene que poder leerse sola, y estos valores no cambian nunca.
        AUDI_USUARIO_ID_IT      BIGINT          NULL,
        AUDI_TRAZA_VC           VARCHAR(64)     NULL,

        AUDI_FECHA_DT           DATETIME        NOT NULL    DEFAULT GETDATE(),

        CONSTRAINT PK_AUD_AUDITORIA PRIMARY KEY (AUDI_ID_IT),

        CONSTRAINT FK_AUD_AUDITORIA_BITACORA FOREIGN KEY (AUDI_BITACORA_ID_IT)
            REFERENCES aud.BITACORA (BITA_ID_IT),

        -- DELETE registra la INTENCION, no la mecanica. En este sistema las
        -- bajas son logicas: fisicamente son un UPDATE que le pone fecha a
        -- CUEN_FECHA_ELIMINACION_DT. Guardarlas como UPDATE obligaria a quien
        -- lea la auditoria a deducir que fue una baja comparando los dos JSON.
        CONSTRAINT CK_AUD_AUDITORIA_OPERACION CHECK
            (AUDI_OPERACION_VC IN ('INSERT', 'UPDATE', 'DELETE')),

        -- Un INSERT sin dato nuevo o un DELETE sin dato anterior es una fila
        -- inutil: no dice que paso. El motor lo impide.
        CONSTRAINT CK_AUD_AUDITORIA_DATOS CHECK (
            (AUDI_OPERACION_VC = 'INSERT' AND AUDI_DATO_NUEVO_NV    IS NOT NULL) OR
            (AUDI_OPERACION_VC = 'DELETE' AND AUDI_DATO_ANTERIOR_NV IS NOT NULL) OR
            (AUDI_OPERACION_VC = 'UPDATE' AND AUDI_DATO_ANTERIOR_NV IS NOT NULL
                                          AND AUDI_DATO_NUEVO_NV    IS NOT NULL)
        )
    );

    -- "Toda la historia de este registro", que es la consulta que justifica la
    -- tabla: se lee de la mas vieja a la mas nueva para reconstruir como llego
    -- a su estado actual.
    CREATE NONCLUSTERED INDEX IX_AUD_AUDITORIA_REGISTRO
        ON aud.AUDITORIA (AUDI_ESQUEMA_VC, AUDI_TABLA_VC, AUDI_LLAVE_VC, AUDI_FECHA_DT)
        INCLUDE (AUDI_OPERACION_VC, AUDI_USUARIO_ID_IT);

    -- "Que cambio esta llamada", desde la bitacora hacia aca.
    CREATE NONCLUSTERED INDEX IX_AUD_AUDITORIA_BITACORA
        ON aud.AUDITORIA (AUDI_BITACORA_ID_IT);

    CREATE NONCLUSTERED INDEX IX_AUD_AUDITORIA_FECHA
        ON aud.AUDITORIA (AUDI_FECHA_DT DESC);

    PRINT 'Tabla aud.AUDITORIA creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla aud.AUDITORIA ya existe.';
END
GO
