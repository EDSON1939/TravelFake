-- =============================================================================
-- SP:          travelfake.GET_CLIENTE_ALL
-- Base:        TRAVELFAKE
-- Descripcion: Obtiene el listado de clientes junto con el país asociado.
--              Permite filtrar por estado y por país.
-- Parametros:
--   @CLIENTES_SOLO_ACTIVOS_BT  BIT     1 = solo activos, 0 = todos
--   @CLIENTES_PAIS_ID_IT       BIGINT  NULL = todos los países
-- Retorna: Lista de CLIENTES ordenada por apellido y nombre
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'GET_CLIENTE_ALL'
)
    DROP PROCEDURE travelfake.GET_CLIENTE_ALL;
GO

CREATE PROCEDURE travelfake.GET_CLIENTE_ALL
    @CLIENTES_SOLO_ACTIVOS_BT BIT    = 1,
    @CLIENTES_PAIS_ID_IT      BIGINT = NULL
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
    WHERE  (@CLIENTES_SOLO_ACTIVOS_BT = 0 OR C.CLIENTES_ACTIVO_BT = 1)
      AND  (@CLIENTES_PAIS_ID_IT IS NULL OR C.CLIENTES_PAIS_ID_IT = @CLIENTES_PAIS_ID_IT)
    ORDER BY C.CLIENTES_APELLIDO_VC ASC,
             C.CLIENTES_NOMBRE_VC   ASC;
END
GO
