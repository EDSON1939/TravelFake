-- =============================================================================
-- SP:          travelfake.INSERT_MONEDA
-- Base:        TRAVELFAKE
-- Descripcion: Inserta una nueva moneda y retorna el ID generado.
--              MONEDAS es un histórico de tipos de cambio: al registrar una
--              nueva cotización, el registro vigente de ese mismo código pasa
--              a MONEDAS_ACTIVO_BT = 0 y solo la última queda activa.
-- Parametros:
--   @MONEDAS_NOMBRE_VC    NVARCHAR(100)  Nombre de la moneda
--   @MONEDAS_CODIGO_VC    NVARCHAR(100)  Código de la moneda (ej: USD, EUR, BOB)
--   @MONEDAS_TCCOMPRA_DC  DECIMAL(18,2)  Tipo de cambio de compra en bolivianos
-- Retorna: BIGINT — ID de la moneda insertada (SCOPE_IDENTITY)
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'travelfake')
BEGIN
    EXEC('CREATE SCHEMA travelfake');
END
GO

IF EXISTS (
    SELECT 1 FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    WHERE s.name = 'travelfake' AND p.name = 'INSERT_MONEDA'
)
    DROP PROCEDURE travelfake.INSERT_MONEDA;
GO

CREATE PROCEDURE travelfake.INSERT_MONEDA
    @MONEDAS_NOMBRE_VC    NVARCHAR(100),
    @MONEDAS_CODIGO_VC    NVARCHAR(100),
    @MONEDAS_TCCOMPRA_DC  DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    -- Si algo falla dentro de la transacción se revierte todo: nunca queda la
    -- moneda anterior desactivada sin que la nueva cotización se haya insertado.
    SET XACT_ABORT ON;

    DECLARE @MONEDAS_ID_IT BIGINT;

    BEGIN TRANSACTION;

        -- La cotización vigente de este código deja de estar activa.
        -- UPDLOCK/HOLDLOCK bloquea el rango del código durante la transacción:
        -- evita que dos altas simultáneas de la misma moneda queden ambas activas.
        UPDATE MONEDAS WITH (UPDLOCK, HOLDLOCK)
        SET    MONEDAS_ACTIVO_BT              = 0,
               MONEDAS_FECHA_ACTUALIZACION_DT = GETDATE()
        WHERE  MONEDAS_CODIGO_VC = @MONEDAS_CODIGO_VC
          AND  MONEDAS_ACTIVO_BT = 1;

        INSERT INTO MONEDAS (
            MONEDAS_NOMBRE_VC,
            MONEDAS_CODIGO_VC,
            MONEDAS_TCCOMPRA_DC,
            MONEDAS_ACTIVO_BT,
            MONEDAS_FECHA_CREACION_DT
        )
        VALUES (
            @MONEDAS_NOMBRE_VC,
            @MONEDAS_CODIGO_VC,
            @MONEDAS_TCCOMPRA_DC,
            1,
            GETDATE()
        );

        -- SCOPE_IDENTITY() devuelve NUMERIC(38,0): se castea a BIGINT porque
        -- ExecuteScalarAsync lee el primer valor del primer resultado como long.
        SET @MONEDAS_ID_IT = CAST(SCOPE_IDENTITY() AS BIGINT);

    COMMIT TRANSACTION;

    SELECT @MONEDAS_ID_IT AS MONEDAS_ID_IT;
END
GO
