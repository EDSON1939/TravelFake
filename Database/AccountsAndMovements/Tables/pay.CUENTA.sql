-- =============================================================================
-- Reto:   QR Bolivia
-- Schema: pay
-- Tabla:  CUENTA
-- Descripcion: Cuentas con saldo. El titular puede ser un CLIENTE extranjero
--              (opera en su moneda: PEN, USD, EUR...) o un COMERCIO boliviano
--              (siempre BOB). Una sola tabla para los dos lados del pago: el
--              mecanismo de saldo, bloqueo y asiento es identico para ambos.
--
-- Convencion de nombres: TPRE_CAMPO_TIPO
--   CUEN = prefijo de tabla CUENTA
--   _IT  = INT / BIGINT
--   _VC  = VARCHAR / NVARCHAR
--   _DE  = DECIMAL
--   _BT  = BIT
--   _DT  = DATETIME
--
-- Nota: CUEN_TITULAR_ID_IT no lleva FK. CLIENTE y COMERCIO son propiedad de
-- otros microservicios; esa integridad se valida por gRPC antes de crear la
-- cuenta, no con una restriccion del motor.
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'pay')
BEGIN
    EXEC('CREATE SCHEMA pay');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'pay' AND t.name = 'CUENTA'
)
BEGIN
    CREATE TABLE pay.CUENTA (
        CUEN_ID_IT                  BIGINT          NOT NULL IDENTITY(1,1),
        CUEN_NUMERO_VC              VARCHAR(20)     NOT NULL,
        CUEN_TITULAR_TIPO_VC        VARCHAR(20)     NOT NULL,
        CUEN_TITULAR_ID_IT          BIGINT          NOT NULL,
        CUEN_MONEDA_ID_IT           BIGINT          NOT NULL,
        -- Codigo desnormalizado: evita un salto gRPC a Monedas en cada lectura
        -- de cuenta y deja el asiento historico legible por si solo.
        CUEN_MONEDA_CODIGO_VC       VARCHAR(10)     NOT NULL,
        CUEN_SALDO_DE               DECIMAL(18,8)   NOT NULL    DEFAULT 0,
        CUEN_ACTIVO_BT              BIT             NOT NULL    DEFAULT 1,
        CUEN_FECHA_CREACION_DT      DATETIME        NOT NULL    DEFAULT GETDATE(),
        CUEN_FECHA_ACTUALIZACION_DT DATETIME        NULL,
        CUEN_FECHA_ELIMINACION_DT   DATETIME        NULL,

        CONSTRAINT PK_PAY_CUENTA         PRIMARY KEY (CUEN_ID_IT),
        CONSTRAINT UQ_PAY_CUENTA_NUMERO  UNIQUE      (CUEN_NUMERO_VC),
        -- Un titular tiene a lo sumo una cuenta por moneda.
        CONSTRAINT UQ_PAY_CUENTA_TITULAR UNIQUE
            (CUEN_TITULAR_TIPO_VC, CUEN_TITULAR_ID_IT, CUEN_MONEDA_ID_IT),
        -- Condicion de aprobacion del reto: no debe existir saldo negativo. El SP
        -- ya lo valida con la fila bloqueada; esta restriccion es la red final.
        CONSTRAINT CK_PAY_CUENTA_SALDO   CHECK (CUEN_SALDO_DE >= 0),
        CONSTRAINT CK_PAY_CUENTA_TITULAR CHECK (CUEN_TITULAR_TIPO_VC IN ('CLIENTE', 'COMERCIO'))
    );

    -- Resolucion (titular, moneda) -> cuenta, que es la lectura del pago.
    CREATE NONCLUSTERED INDEX IX_PAY_CUENTA_TITULAR
        ON pay.CUENTA (CUEN_TITULAR_TIPO_VC, CUEN_TITULAR_ID_IT)
        INCLUDE (CUEN_NUMERO_VC, CUEN_MONEDA_ID_IT, CUEN_MONEDA_CODIGO_VC,
                 CUEN_SALDO_DE, CUEN_ACTIVO_BT);

    PRINT 'Tabla pay.CUENTA creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla pay.CUENTA ya existe.';
END
GO
