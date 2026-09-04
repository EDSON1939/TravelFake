SET NOCOUNT ON;
PRINT '=== 1. Alta de cuentas ===';
DECLARE @c1 BIGINT, @c2 BIGINT;
CREATE TABLE #r (v BIGINT);

INSERT INTO #r EXEC commerce.INSERT_CUENTA 'CLIENT', 1, 2, 'USD', 500, 'traza-alta-1', 7;
SELECT @c1 = v FROM #r; DELETE #r;
INSERT INTO #r EXEC commerce.INSERT_CUENTA 'COMMERCE', 55, 1, 'BOB', 0, 'traza-alta-2', 7;
SELECT @c2 = v FROM #r; DELETE #r;
SELECT CASE WHEN @c1 = 1 AND @c2 = 2 THEN 'OK  cuentas 1 y 2' ELSE 'FALLO cuentas' END AS T01;

SELECT CASE WHEN COUNT(*) = 2 THEN 'OK  alta con saldo audita cuenta + asiento de apertura'
            ELSE 'FALLO auditoria del alta: ' + CAST(COUNT(*) AS VARCHAR) END AS T02
FROM aud.AUDITORIA WHERE AUDI_TRAZA_VC = 'traza-alta-1';

SELECT CASE WHEN COUNT(*) = 1 THEN 'OK  alta sin saldo audita solo la cuenta'
            ELSE 'FALLO auditoria alta sin saldo: ' + CAST(COUNT(*) AS VARCHAR) END AS T03
FROM aud.AUDITORIA WHERE AUDI_TRAZA_VC = 'traza-alta-2';

PRINT '=== 2. Pago QR ===';
DECLARE @pago BIGINT;
INSERT INTO #r EXEC commerce.EJECUTAR_PAGO_QR
    'QR0000000001','QR0000000002',55,'QR-BO-0001',
    20,'USD',6.96,139.20,'BOB','REF-1','TX-2026-000001','tx-guid-1','Almuerzo',
    'traza-pago', 7;
SELECT @pago = v FROM #r; DELETE #r;
SELECT CASE WHEN @pago > 0 THEN 'OK  pago aplicado' ELSE 'FALLO pago: ' + CAST(@pago AS VARCHAR) END AS T04;

SELECT CASE WHEN (SELECT CUEN_SALDO_DE FROM commerce.CUENTA WHERE CUEN_ID_IT=1) = 480.00
             AND (SELECT CUEN_SALDO_DE FROM commerce.CUENTA WHERE CUEN_ID_IT=2) = 139.20
            THEN 'OK  saldos 480 USD / 139.20 BOB' ELSE 'FALLO saldos' END AS T05;

SELECT CASE WHEN COUNT(*) = 4 THEN 'OK  el pago deja 4 auditorias (2 asientos + 2 saldos)'
            ELSE 'FALLO auditorias del pago: ' + CAST(COUNT(*) AS VARCHAR) END AS T06
FROM aud.AUDITORIA WHERE AUDI_TRAZA_VC = 'traza-pago';

SELECT CASE WHEN JSON_VALUE(AUDI_DATO_ANTERIOR_NV,'$.CUEN_SALDO_DE') = '500.00000000'
             AND JSON_VALUE(AUDI_DATO_NUEVO_NV,   '$.CUEN_SALDO_DE') = '480.00000000'
            THEN 'OK  el JSON guarda el saldo antes y despues'
            ELSE 'FALLO json antes/despues' END AS T07
FROM aud.AUDITORIA
WHERE AUDI_TRAZA_VC='traza-pago' AND AUDI_TABLA_VC='CUENTA' AND AUDI_LLAVE_VC='1';

PRINT '=== 3. Idempotencia del pago ===';
INSERT INTO #r EXEC commerce.EJECUTAR_PAGO_QR
    'QR0000000001','QR0000000002',55,'QR-BO-0001',
    20,'USD',6.96,139.20,'BOB','REF-1','TX-2026-000001','tx-guid-2','Almuerzo', 'traza-rep', 7;
SELECT CASE WHEN (SELECT v FROM #r) = @pago THEN 'OK  reintento identico devuelve el asiento original'
            ELSE 'FALLO reintento' END AS T08;
DELETE #r;

-- Correccion: antes el SP comparaba solo cuenta y QR, asi que esto pasaba.
INSERT INTO #r EXEC commerce.EJECUTAR_PAGO_QR
    'QR0000000001','QR0000000002',55,'QR-BO-0001',
    30,'USD',6.96,208.80,'BOB','REF-1','TX-2026-000001','tx-guid-3','Otro monto', 'traza-mal', 7;
SELECT CASE WHEN (SELECT v FROM #r) = -9 THEN 'OK  misma clave con otro monto -> -9'
            ELSE 'FALLO clave reusada con otro monto: ' + CAST((SELECT v FROM #r) AS VARCHAR) END AS T09;
DELETE #r;

SELECT CASE WHEN (SELECT CUEN_SALDO_DE FROM commerce.CUENTA WHERE CUEN_ID_IT=1) = 480.00
            THEN 'OK  ningun reintento volvio a cobrar' ELSE 'FALLO cobro doble' END AS T10;

PRINT '=== 4. Movimiento suelto e idempotencia ===';
DECLARE @rec BIGINT;
INSERT INTO #r EXEC commerce.APLICAR_MOVIMIENTO 'QR0000000001','CREDITO',100,'RECARGA','TOPUP-001','Recarga','traza-rec',7;
SELECT @rec = v FROM #r; DELETE #r;
SELECT CASE WHEN @rec > 0 AND (SELECT CUEN_SALDO_DE FROM commerce.CUENTA WHERE CUEN_ID_IT=1) = 580.00
            THEN 'OK  recarga aplicada, saldo 580' ELSE 'FALLO recarga' END AS T11;

INSERT INTO #r EXEC commerce.APLICAR_MOVIMIENTO 'QR0000000001','CREDITO',100,'RECARGA','TOPUP-001','Recarga','traza-rec2',7;
SELECT CASE WHEN (SELECT v FROM #r) = @rec AND (SELECT CUEN_SALDO_DE FROM commerce.CUENTA WHERE CUEN_ID_IT=1) = 580.00
            THEN 'OK  recarga repetida no acredita de nuevo' ELSE 'FALLO recarga repetida' END AS T12;
DELETE #r;

-- Correccion del hallazgo: antes devolvia exito con el id del pago y no debitaba.
INSERT INTO #r EXEC commerce.APLICAR_MOVIMIENTO 'QR0000000001','DEBITO',500,'AJUSTE','TX-2026-000001','Ajuste','traza-mal2',7;
SELECT CASE WHEN (SELECT v FROM #r) = -9 THEN 'OK  clave del pago reusada en un debito -> -9'
            ELSE 'FALLO clave reusada en ApplyMovement: ' + CAST((SELECT v FROM #r) AS VARCHAR) END AS T13;
DELETE #r;

INSERT INTO #r EXEC commerce.APLICAR_MOVIMIENTO 'QR0000000001','CREDITO',999,'X','TOPUP-001','Otro monto','traza-mal3',7;
SELECT CASE WHEN (SELECT v FROM #r) = -9 THEN 'OK  misma clave con otro monto -> -9'
            ELSE 'FALLO monto distinto: ' + CAST((SELECT v FROM #r) AS VARCHAR) END AS T14;
DELETE #r;

PRINT '=== 5. Saldo insuficiente y rollback de auditoria ===';
INSERT INTO #r EXEC commerce.APLICAR_MOVIMIENTO 'QR0000000001','DEBITO',99999,'X','TX-SINSALDO','Sin saldo','traza-sinsaldo',7;
SELECT CASE WHEN (SELECT v FROM #r) = -3 THEN 'OK  saldo insuficiente -> -3' ELSE 'FALLO saldo' END AS T15;
DELETE #r;
SELECT CASE WHEN COUNT(*) = 0 THEN 'OK  una operacion rechazada no deja auditoria'
            ELSE 'FALLO auditoria fantasma' END AS T16
FROM aud.AUDITORIA WHERE AUDI_TRAZA_VC = 'traza-sinsaldo';

PRINT '=== 6. Bitacora y enlace ===';
INSERT INTO #r EXEC aud.INSERT_BITACORA
     @SERVICIO_VC='accounts-movements-service', @METODO_VC='/x/ExecuteQrPayment',
     @OPERACION_VC='CAMBIO', @USUARIO_ID_IT=7, @TRAZA_VC='traza-pago',
     @ESTADO_VC='OK', @CODIGO_VC='SUC000', @DURACION_IT=42;
DECLARE @bit BIGINT = (SELECT v FROM #r); DELETE #r;
SELECT CASE WHEN COUNT(*) = 4 THEN 'OK  la bitacora adopto las 4 auditorias del pago'
            ELSE 'FALLO enlace: ' + CAST(COUNT(*) AS VARCHAR) END AS T17
FROM aud.AUDITORIA WHERE AUDI_BITACORA_ID_IT = @bit;

PRINT '=== 7. Integridad final ===';
SELECT CASE WHEN COUNT(*) = 0 THEN 'OK  todo saldo es reconstruible desde sus asientos'
            ELSE 'FALLO descuadre en ' + CAST(COUNT(*) AS VARCHAR) + ' cuenta(s)' END AS T18
FROM (
  SELECT c.CUEN_ID_IT
  FROM commerce.CUENTA c
  LEFT JOIN commerce.MOVIMIENTO m ON m.MOVI_CUENTA_ID_IT = c.CUEN_ID_IT
  GROUP BY c.CUEN_ID_IT, c.CUEN_SALDO_DE
  HAVING c.CUEN_SALDO_DE <> ISNULL(SUM(CASE WHEN m.MOVI_TIPO_VC='CREDITO' THEN m.MOVI_MONTO_DE ELSE -m.MOVI_MONTO_DE END),0)
) d;

SELECT CASE WHEN MIN(CUEN_SALDO_DE) >= 0 THEN 'OK  ninguna cuenta en negativo' ELSE 'FALLO saldo negativo' END AS T19
FROM commerce.CUENTA;

DROP TABLE #r;
