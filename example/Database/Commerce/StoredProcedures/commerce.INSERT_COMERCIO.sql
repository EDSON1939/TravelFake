-- =============================================================================
-- SP:          commerce.INSERT_COMERCIO
-- Descripcion: Inserta un nuevo comercio y retorna el ID generado
-- Parametros:
--   @COME_NOMBRE_VC  NVARCHAR(50)   Nombre del comercio
--   @COME_NIT_VC     VARCHAR(13)    NIT del comercio (único)
--   @COME_CUENTA_ID_IT BIGINT       Identificador de la cuenta (TODO: se asignará)
-- Retorna: BIGINT — ID del comercio insertado (SCOPE_IDENTITY)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'commerce' AND p.name = 'INSERT_COMERCIO'
)
    DROP PROCEDURE commerce.INSERT_COMERCIO;
GO

CREATE PROCEDURE commerce.INSERT_COMERCIO
    @COME_NOMBRE_VC     NVARCHAR(50),
    @COME_NIT_VC        VARCHAR(13),
    @COME_CUENTA_ID_IT  BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO commerce.COMERCIO (
        COME_NOMBRE_VC,
        COME_NIT_VC,
        COME_CUENTA_ID_IT,
        COME_ACTIVO_BT,
        COME_FECHA_CREACION_DT
    )
    VALUES (
        @COME_NOMBRE_VC,
        @COME_NIT_VC,
        @COME_CUENTA_ID_IT,
        1,
        GETDATE()
    );

    -- ExecuteScalarAsync lee el primer valor del primer resultado
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS COME_ID_IT;
END
GO
