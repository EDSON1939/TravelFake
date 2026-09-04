-- =============================================================================
-- Reto:   QR Bolivia
-- Schema: commerce
-- Tabla:  USUARIO
-- Descripcion: Credenciales del sistema. Un usuario puede estar vinculado a un
--              CLIENTE del negocio (y entonces su token lleva el claim
--              client_id) o ser un operador interno sin cliente asociado.
--
--              La contrasena nunca se guarda en claro: la columna lleva el
--              PBKDF2 en formato "iteraciones.salt.hash" que arma
--              Auth.Infrastructure.Security.PasswordHasher.
--
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   USUA = prefijo de tabla USUARIO
--   _IT  = INT / BIGINT
--   _VC  = VARCHAR / NVARCHAR
--   _BT  = BIT
--   _DT  = DATETIME
--
-- Nota: USUA_CLIENTE_ID_IT no lleva FK. CLIENTE es propiedad del microservicio
-- de Clientes; esa integridad se valida por gRPC antes de crear el usuario, no
-- con una restriccion del motor.
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'commerce')
BEGIN
    EXEC('CREATE SCHEMA commerce');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND t.name = 'USUARIO'
)
BEGIN
    CREATE TABLE commerce.USUARIO (
        USUA_ID_IT                  BIGINT          NOT NULL IDENTITY(1,1),
        USUA_USERNAME_VC            VARCHAR(50)     NOT NULL,
        -- PBKDF2 "iteraciones.salt.hash". Nunca sale de Auth y nunca entra a la
        -- auditoria: los SP enumeran columnas justamente para excluirla.
        USUA_PASSWORD_HASH_VC       VARCHAR(256)    NOT NULL,
        USUA_CLIENTE_ID_IT          BIGINT          NULL,
        USUA_NOMBRE_VC              NVARCHAR(150)   NOT NULL,
        USUA_ROL_VC                 VARCHAR(20)     NOT NULL,
        USUA_ACTIVO_BT              BIT             NOT NULL    DEFAULT 1,

        -- Freno de fuerza bruta. El contador se reinicia con cada login exitoso
        -- y al cumplirse el bloqueo.
        USUA_INTENTOS_FALLIDOS_IT   INT             NOT NULL    DEFAULT 0,
        -- En UTC, no en hora local: LoginCommandHandler la compara contra
        -- DateTime.UtcNow. Guardarla con GETDATE() en un huso negativo dejaria
        -- el bloqueo vencido en el mismo instante en que se aplica.
        USUA_BLOQUEADO_HASTA_DT     DATETIME        NULL,

        USUA_ULTIMO_ACCESO_DT       DATETIME        NULL,
        USUA_FECHA_CREACION_DT      DATETIME        NOT NULL    DEFAULT GETDATE(),
        USUA_FECHA_ACTUALIZACION_DT DATETIME        NULL,
        USUA_FECHA_ELIMINACION_DT   DATETIME        NULL,

        CONSTRAINT PK_COMMERCE_USUARIO          PRIMARY KEY (USUA_ID_IT),
        CONSTRAINT CK_COMMERCE_USUARIO_ROL      CHECK (USUA_ROL_VC IN ('CLIENTE', 'AGENTE', 'ADMIN')),
        CONSTRAINT CK_COMMERCE_USUARIO_INTENTOS CHECK (USUA_INTENTOS_FALLIDOS_IT >= 0)
    );

    PRINT 'Tabla commerce.USUARIO creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla commerce.USUARIO ya existe.';
END
GO

-- Los indices filtrados exigen QUOTED_IDENTIFIER ON. SSMS lo trae activado,
-- sqlcmd lo deja OFF por defecto, asi que lo forzamos aqui.
SET QUOTED_IDENTIFIER ON;
GO

-- Username unico ENTRE LOS VIGENTES. Va filtrado y no como UNIQUE a secas
-- porque la baja es logica: con una restriccion plana, un usuario dado de baja
-- bloquearia su nombre para siempre.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_COMMERCE_USUARIO_USERNAME')
    CREATE UNIQUE NONCLUSTERED INDEX UQ_COMMERCE_USUARIO_USERNAME
        ON commerce.USUARIO (USUA_USERNAME_VC)
        WHERE USUA_FECHA_ELIMINACION_DT IS NULL;
GO

-- Un cliente tiene una sola credencial: dos usuarios sobre el mismo cliente
-- serian dos accesos a la misma cuenta bancaria. Los operadores internos van
-- con cliente NULL y el filtro los deja fuera de la restriccion.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_COMMERCE_USUARIO_CLIENTE')
    CREATE UNIQUE NONCLUSTERED INDEX UQ_COMMERCE_USUARIO_CLIENTE
        ON commerce.USUARIO (USUA_CLIENTE_ID_IT)
        WHERE USUA_CLIENTE_ID_IT IS NOT NULL AND USUA_FECHA_ELIMINACION_DT IS NULL;
GO
