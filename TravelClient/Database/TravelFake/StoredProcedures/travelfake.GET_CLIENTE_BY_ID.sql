-- =============================================================================
-- SP:          travelfake.GET_CLIENTE_BY_ID
-- Base:        TRAVELFAKE
-- Descripcion: Obtiene un cliente por su identificador, con el país asociado
-- Parametros:
--   @CLIENTES_ID_IT  BIGINT  ID del cliente
-- Retorna: 1 fila de CLIENTES o vacío si no existe
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'GET_CLIENTE_BY_ID'
)
    DROP PROCEDURE travelfake.GET_CLIENTE_BY_ID;
GO

CREATE PROCEDURE travelfake.GET_CLIENTE_BY_ID
    @CLIENTES_ID_IT BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT C.CLIENTES_ID_IT                   AS CustomerId,
           C.CLIENTES_NOMBRE_VC               AS FirstName,
           C.CLIENTES_APELLIDO_VC             AS LastName,
           C.CLIENTES_EMAIL_VC                AS Email,
           C.CLIENTES_TELEFONO_VC             AS Phone,
           C.CLIENTES_PAIS_ID_IT              AS CountryId,
           P.PAIS_NOMBRE_VC                   AS CountryName,
           P.PAIS_CODIGO_VC                   AS CountryCode,
           C.CLIENTES_ACTIVO_BT               AS IsActive,
           C.CLIENTES_FECHA_CREACION_DT       AS CreatedAt,
           C.CLIENTES_FECHA_ACTUALIZACION_DT  AS UpdatedAt
    FROM   CLIENTES C
           INNER JOIN PAIS P ON P.PAIS_ID_IT = C.CLIENTES_PAIS_ID_IT
    WHERE  C.CLIENTES_ID_IT = @CLIENTES_ID_IT;
END
GO
