-- =============================================================================
-- SP:          travelfake.INSERT_CLIENTE
-- Base:        TRAVELFAKE
-- Descripcion: Inserta un nuevo cliente y retorna el ID generado
-- Parametros:
--   @CLIENTES_NOMBRE_VC    NVARCHAR(100)  Nombre del cliente
--   @CLIENTES_APELLIDO_VC  NVARCHAR(100)  Apellido del cliente
--   @CLIENTES_EMAIL_VC     NVARCHAR(100)  Correo electrónico
--   @CLIENTES_TELEFONO_VC  NVARCHAR(100)  Teléfono de contacto
--   @CLIENTES_PAIS_ID_IT   BIGINT         ID del país (FK_PAIS)
-- Retorna: BIGINT — ID del cliente insertado (SCOPE_IDENTITY)
--                   -1 si el país indicado no existe
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'INSERT_CLIENTE'
)
    DROP PROCEDURE travelfake.INSERT_CLIENTE;
GO

CREATE PROCEDURE travelfake.INSERT_CLIENTE
    @CLIENTES_NOMBRE_VC    NVARCHAR(100),
    @CLIENTES_APELLIDO_VC  NVARCHAR(100),
    @CLIENTES_EMAIL_VC     NVARCHAR(100),
    @CLIENTES_TELEFONO_VC  NVARCHAR(100),
    @CLIENTES_PAIS_ID_IT   BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM PAIS WHERE PAIS_ID_IT = @CLIENTES_PAIS_ID_IT)
    BEGIN
        -- País inexistente: se evita el error 547 de la FK
        SELECT CAST(-1 AS BIGINT) AS CLIENTES_ID_IT;
        RETURN;
    END

    INSERT INTO CLIENTES (
        CLIENTES_NOMBRE_VC,
        CLIENTES_APELLIDO_VC,
        CLIENTES_EMAIL_VC,
        CLIENTES_TELEFONO_VC,
        CLIENTES_PAIS_ID_IT,
        CLIENTES_ACTIVO_BT,
        CLIENTES_FECHA_CREACION_DT
    )
    VALUES (
        @CLIENTES_NOMBRE_VC,
        @CLIENTES_APELLIDO_VC,
        @CLIENTES_EMAIL_VC,
        @CLIENTES_TELEFONO_VC,
        @CLIENTES_PAIS_ID_IT,
        1,
        GETDATE()
    );

    -- ExecuteScalarAsync lee el primer valor del primer resultado y lo castea a long:
    -- SCOPE_IDENTITY() devuelve NUMERIC(38,0), por eso el CAST explícito a BIGINT.
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS CLIENTES_ID_IT;
END
GO
