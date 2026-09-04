-- =============================================================================
-- SP:          travelfake.UPDATE_CLIENTE
-- Base:        TRAVELFAKE
-- Descripcion: Actualiza los datos de un cliente existente
-- Parametros:
--   @CLIENTES_ID_IT        BIGINT         ID del cliente
--   @CLIENTES_NOMBRE_VC    NVARCHAR(100)  Nuevo nombre
--   @CLIENTES_APELLIDO_VC  NVARCHAR(100)  Nuevo apellido
--   @CLIENTES_EMAIL_VC     NVARCHAR(100)  Nuevo correo electrónico
--   @CLIENTES_TELEFONO_VC  NVARCHAR(100)  Nuevo teléfono
--   @CLIENTES_PAIS_ID_IT   BIGINT         Nuevo país (FK_PAIS)
--   @CLIENTES_ACTIVO_BT    BIT            Nuevo estado (1=activo, 0=inactivo)
-- Retorna: BIGINT — filas afectadas
--          ( 1 = éxito, 0 = no encontrado, -1 = país inexistente )
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'UPDATE_CLIENTE'
)
    DROP PROCEDURE travelfake.UPDATE_CLIENTE;
GO

CREATE PROCEDURE travelfake.UPDATE_CLIENTE
    @CLIENTES_ID_IT        BIGINT,
    @CLIENTES_NOMBRE_VC    NVARCHAR(100),
    @CLIENTES_APELLIDO_VC  NVARCHAR(100),
    @CLIENTES_EMAIL_VC     NVARCHAR(100),
    @CLIENTES_TELEFONO_VC  NVARCHAR(100),
    @CLIENTES_PAIS_ID_IT   BIGINT,
    @CLIENTES_ACTIVO_BT    BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM PAIS WHERE PAIS_ID_IT = @CLIENTES_PAIS_ID_IT)
    BEGIN
        -- País inexistente: se evita el error 547 de la FK
        SELECT CAST(-1 AS BIGINT) AS FilasAfectadas;
        RETURN;
    END

    UPDATE CLIENTES
    SET    CLIENTES_NOMBRE_VC              = @CLIENTES_NOMBRE_VC,
           CLIENTES_APELLIDO_VC            = @CLIENTES_APELLIDO_VC,
           CLIENTES_EMAIL_VC               = @CLIENTES_EMAIL_VC,
           CLIENTES_TELEFONO_VC            = @CLIENTES_TELEFONO_VC,
           CLIENTES_PAIS_ID_IT             = @CLIENTES_PAIS_ID_IT,
           CLIENTES_ACTIVO_BT              = @CLIENTES_ACTIVO_BT,
           CLIENTES_FECHA_ACTUALIZACION_DT = GETDATE()
    WHERE  CLIENTES_ID_IT = @CLIENTES_ID_IT;

    -- CAST a BIGINT: @@ROWCOUNT es INT y ExecuteScalarAsync lo castea a long
    SELECT CAST(@@ROWCOUNT AS BIGINT) AS FilasAfectadas;
END
GO
