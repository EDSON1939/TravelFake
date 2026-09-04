-- =============================================================================
-- Schema: aud
-- Tabla:  BITACORA
-- Descripcion: Registro de QUIEN llamo a QUE. Una fila por cada RPC atendido,
--              incluidas las consultas y las llamadas que terminaron en error.
--              Responde "quien hizo esto, cuando, desde donde y como le fue".
--
-- No confundir con aud.AUDITORIA, que es la otra mitad y guarda COMO QUEDO EL
-- DATO. Estan separadas a proposito:
--
--   - Una consulta genera bitacora y NO genera auditoria: no cambio nada.
--     Son la mayoria de las llamadas, y en una sola tabla dejarian nulas todas
--     las columnas de dato anterior/nuevo.
--   - Un pago genera UNA bitacora y VARIAS auditorias: dos asientos y dos
--     saldos. La relacion es 1 a N, no 1 a 1.
--   - La bitacora se escribe siempre, aunque la operacion falle o haga
--     rollback. La auditoria vive dentro de la transaccion del cambio y
--     desaparece con el si la operacion se deshace.
--
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   BITA = prefijo de tabla BITACORA
--   _IT  = INT / BIGINT
--   _VC  = VARCHAR / NVARCHAR
--   _DT  = DATETIME
--
-- Este script se instala en la base de CADA microservicio: cada uno es dueno
-- de su historia, igual que es dueno de sus datos.
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'aud')
BEGIN
    EXEC('CREATE SCHEMA aud');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'aud' AND t.name = 'BITACORA'
)
BEGIN
    CREATE TABLE aud.BITACORA (
        BITA_ID_IT              BIGINT          NOT NULL IDENTITY(1,1),

        -- Que se llamo.
        BITA_SERVICIO_VC        VARCHAR(60)     NOT NULL,
        -- Ruta gRPC completa: /accounts_movements.AccountsAndMovements/ExecuteQrPayment
        BITA_METODO_VC          VARCHAR(160)    NOT NULL,
        -- Intencion de la llamada, no el verbo HTTP: esto es gRPC y todo viaja
        -- por POST. Ver CK_AUD_BITACORA_OPERACION.
        BITA_OPERACION_VC       VARCHAR(10)     NOT NULL,

        -- Quien la hizo. Sale de los claims del JWT; queda NULL si el token no
        -- identifica a nadie (llamada entre servicios, por ejemplo).
        BITA_USUARIO_ID_IT      BIGINT          NULL,
        BITA_USUARIO_VC         VARCHAR(60)     NULL,
        BITA_CLIENTE_ID_IT      BIGINT          NULL,
        BITA_ROL_VC             VARCHAR(20)     NULL,

        -- Desde donde. IPv6 entra en 45 caracteres.
        BITA_IP_VC              VARCHAR(45)     NULL,

        -- TraceId de OpenTelemetry: es lo que permite saltar de esta fila a los
        -- logs y a las trazas de la misma llamada sin adivinar por fecha.
        BITA_TRAZA_VC           VARCHAR(64)     NULL,

        -- Cuerpo del request en JSON. Nunca lleva contrasenas ni tokens: eso lo
        -- filtra el interceptor antes de escribir, no una consulta despues.
        BITA_PETICION_NV        NVARCHAR(MAX)   NULL,

        -- Como le fue.
        BITA_ESTADO_VC          VARCHAR(10)     NOT NULL,
        -- status_code del BaseResponse: SUC000, INSUFFICIENT_FUNDS, etc.
        BITA_CODIGO_VC          VARCHAR(40)     NULL,
        BITA_MENSAJE_NV         NVARCHAR(500)   NULL,
        BITA_DURACION_IT        INT             NOT NULL    DEFAULT 0,

        BITA_FECHA_DT           DATETIME        NOT NULL    DEFAULT GETDATE(),

        CONSTRAINT PK_AUD_BITACORA PRIMARY KEY (BITA_ID_IT),

        -- CONSULTA cubre los GET. ALTA, CAMBIO y BAJA son los tres que dejan
        -- rastro en aud.AUDITORIA. BAJA es siempre logica en este sistema: el
        -- registro no se borra, se le pone fecha de eliminacion.
        CONSTRAINT CK_AUD_BITACORA_OPERACION CHECK
            (BITA_OPERACION_VC IN ('CONSULTA', 'ALTA', 'CAMBIO', 'BAJA')),

        CONSTRAINT CK_AUD_BITACORA_ESTADO CHECK
            (BITA_ESTADO_VC IN ('OK', 'ERROR'))
    );

    -- "Que hizo este usuario", que es la pregunta que se le hace a una bitacora.
    CREATE NONCLUSTERED INDEX IX_AUD_BITACORA_USUARIO_FECHA
        ON aud.BITACORA (BITA_USUARIO_ID_IT, BITA_FECHA_DT DESC)
        INCLUDE (BITA_METODO_VC, BITA_OPERACION_VC, BITA_ESTADO_VC);

    -- Barrido por rango de fechas, que es como se depura y como se exporta.
    CREATE NONCLUSTERED INDEX IX_AUD_BITACORA_FECHA
        ON aud.BITACORA (BITA_FECHA_DT DESC);

    -- Salto desde un log o una traza hacia la fila que la origino.
    CREATE NONCLUSTERED INDEX IX_AUD_BITACORA_TRAZA
        ON aud.BITACORA (BITA_TRAZA_VC);

    PRINT 'Tabla aud.BITACORA creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla aud.BITACORA ya existe.';
END
GO
