# AccountsAndMovements — núcleo de pagos QR Bolivia

Dueño de las **cuentas** (saldos) y los **movimientos** (libro mayor). Es el único
punto por el que la plata cambia de manos: valida contra los demás
microservicios, convierte la moneda y ejecuta el pago de forma atómica.

| | |
|---|---|
| Responsable | Edson · RETO 1 QR Bolivia (Edson + Mariel + Rene) |
| Puertos | `https://localhost:52130` · `http://localhost:52131` |
| Base | `DB_QRBolivia`, esquema `commerce` |
| Stack | C# · ASP.NET Core · gRPC · SQL con Dapper + ADO.NET, **sin EF Core** |

**Hace**: abrir y consultar cuentas de clientes extranjeros (en su moneda) y de
comercios bolivianos (en BOB); acreditar y debitar saldo de forma idempotente;
ejecutar el pago QR; consultar operaciones e historial.

**No hace**, porque es de otros microservicios del equipo: catálogo de clientes,
países, monedas y tipos de cambio, comercios, y generación/estado de los QR.

---

## 1. Estructura

Clean Architecture + CQRS con MediatR, validación con FluentValidation,
respuestas `BaseResponse<T>` — la arquitectura del proyecto base.

```
AccountsAndMovements.Domain/          Entidades, códigos de error, contratos
  Entities · Errors · ExternalServices · Repositories · Services/CurrencyConverter
AccountsAndMovements.Infrastructure/  SQL y clientes gRPC
  Persistence/Commands (SP) · Persistence/Queries (Dapper) · ExternalServices · Protos
AccountsAndMovements.Application/     Casos de uso
  Features/{Accounts,Payments,Movements}/
AccountsAndMovements.Api/             Borde gRPC
  Protos/accounts_movements.proto · Services (mensaje → MediatR)
Tests/AccountsAndMovements.Unit.Tests/
```

---

## 2. Base de datos

Stored procedures para todo lo que escribe, Dapper para lo que lee. Scripts en
`Database/AccountsAndMovements/`.

**`commerce.CUENTA`** — saldo por titular y moneda. El titular es `CLIENT` o
`COMMERCE`: una sola tabla para los dos lados del pago.

**`commerce.MOVIMIENTO`** — libro mayor. Cada fila es un asiento inmutable con
saldo anterior y posterior. Un pago escribe **dos** filas con el mismo
`MOVI_TRANSACCION_VC`: el DEBITO del cliente y el CREDITO del comercio. Los
campos de conversión se graban en el asiento y nunca se recalculan: **la
operación histórica no cambia si mañana cambia el tipo de cambio.**

| Restricción | Para qué |
|---|---|
| `CK_COMMERCE_CUENTA_SALDO` (`>= 0`) | Garantía final de que no hay saldo negativo |
| `UQ_COMMERCE_CUENTA_TITULAR` | Un titular, una cuenta por moneda |
| `UQ_COMMERCE_MOVIMIENTO_IDEMPOTENCIA` (global) | Imposible cobrar dos veces la misma operación |
| `UQ_COMMERCE_MOVIMIENTO_QR` (filtrado, solo DEBITO) | Un QR se paga una sola vez, aunque dos lo escaneen a la vez |

| SP | Qué resuelve |
|---|---|
| `commerce.INSERT_CUENTA` | Crea la cuenta y, si trae saldo inicial, su asiento de apertura |
| `commerce.APLICAR_MOVIMIENTO` | Crédito o débito, con bloqueo de fila e idempotencia |
| `commerce.EJECUTAR_PAGO_QR` | Pago completo: débito + crédito + dos asientos, todo o nada |

`aud.BITACORA` y `aud.AUDITORIA` (scripts en `Database/Core/`) guardan quién
llamó a qué y cómo quedó cada dato. Se instalan en la base de cada microservicio.

**Instalación** — completa en [`docs/EJECUCION.md`](../../docs/EJECUCION.md).
El orden importa: `aud` antes que `commerce`, porque los SP de `commerce` llaman
a `aud.REGISTRAR_AUDITORIA`.

> **`QUOTED_IDENTIFIER` y el índice filtrado.** SQL Server exige
> `QUOTED_IDENTIFIER ON` para cualquier DML sobre `commerce.MOVIMIENTO`. Los SP
> ya lo fijan al crearse y `Microsoft.Data.SqlClient` lo activa al conectar, así
> que el servicio funciona sin hacer nada. Pero para consultas sueltas **desde
> `sqlcmd`**, que lo trae en OFF, hay que abrir con `SET QUOTED_IDENTIFIER ON;`
> o llega `Msg 1934`. SSMS no tiene ese problema.

---

## 3. Contrato gRPC

`AccountsAndMovements.Api/Protos/accounts_movements.proto`. Decimales como
**string** para no perder precisión, fechas **ISO 8601**, `""` = sin valor.

No hay endpoint genérico con discriminador de titular: cliente y comercio tienen
reglas distintas y cada uno tiene el suyo. Los 🔑 sacan el cliente del claim
`client_id` del JWT y **no lo reciben como parámetro**.

| RPC | Para qué |
|---|---|
| `CreateClientAccount` · `CreateCommerceAccount` | Altas. La de comercio no recibe moneda: siempre BOB |
| `GetAccount` · `GetMyAccount` 🔑 · `GetMyAccounts` 🔑 | Consulta por número, por moneda, o todas |
| `GetCommerceAccount` · `GetCommerceAccounts` | Cuentas de un comercio |
| `ApplyMovement` | Crédito/débito suelto, idempotente |
| **`ExecuteQrPayment`** 🔑 | **Pago del QR: la operación central** |
| `GetPayment` | Operación por código o por idempotency key |
| `GetMovements` · `GetMyHistory` 🔑 · `GetCommerceHistory` | Extracto e historial con filtros |

Campos, reglas, respuestas y errores de cada uno:
[`docs/ENDPOINTS.md`](../../docs/ENDPOINTS.md).

```json
// ExecuteQrPayment — el cliente sale del token, no del cuerpo
{ "qr_code": "QR-BO-0001", "amount": "20", "currency_code": "USD",
  "idempotency_key": "TX-2026-000001", "description": "Pago en Café Central" }

// 20 USD × 6.96 = 139.20 BOB
{ "status_code": "SUC000", "data": {
    "transaction_code": "d3f1b6f0-...", "movement_id": 900,
    "client_account_number": "QR0000000001", "commerce_account_number": "QR0000000002",
    "original_amount": "20", "original_currency": "USD", "exchange_rate": "6.96",
    "converted_amount": "139.20", "target_currency": "BOB",
    "client_balance": "480", "status": "COMPLETED", "is_duplicate": false } }
```

Reenviando **la misma** `idempotency_key` se responde lo mismo con
`"is_duplicate": true`, y al cliente **no** se le cobra de nuevo.

---

## 4. Flujo del pago

```
ExecuteQrPayment
      │
      ├─ 0. ¿La idempotency key ya se usó? ── sí, mismos datos ─→ devuelve el resultado original
      │                                    └─ sí, otros datos ─→ DUPLICATE_TRANSACTION
      ├─ 1. Validar cliente ......................... Clientes  → CUSTOMER_NOT_FOUND / CUSTOMER_INACTIVE
      ├─ 2. Validar QR .............................. QR        → QR_NOT_FOUND / QR_ALREADY_USED / QR_EXPIRED / QR_INACTIVE
      ├─ 3. Validar comercio ........................ Comercio  → COMMERCE_NOT_FOUND / COMMERCE_INACTIVE
      ├─ 4. Validar moneda .......................... Monedas   → CURRENCY_NOT_SUPPORTED
      ├─ 5. Obtener tipo de cambio .................. Monedas   → EXCHANGE_RATE_NOT_FOUND
      ├─ 6. Convertir  (20 USD × 6.96 = 139.20 BOB)
      ├─ 7. ¿El monto convertido cubre el QR? ..................→ INVALID_AMOUNT
      ├─ 8. Resolver las dos cuentas ..........................→ ACCOUNT_NOT_FOUND / COMMERCE_ACCOUNT_NOT_FOUND
      │
      ├─ 9. commerce.EJECUTAR_PAGO_QR  ◄── una sola transacción
      │        ├─ bloquea las dos cuentas
      │        ├─ valida saldo ...............................→ INSUFFICIENT_FUNDS
      │        ├─ valida que el QR no esté cobrado ...........→ QR_ALREADY_USED
      │        ├─ asienta DEBITO al cliente y CREDITO al comercio
      │        └─ actualiza los dos saldos
      │
      ├─ 10. Marcar el QR como usado (best effort)
      └─ 11. Devolver la operación con su conversión
```

**El saldo se valida adentro del SP, no en el handler.** Una lectura desde C# no
sirve para decidir: entre esa lectura y el pago puede entrar otra operación del
mismo cliente. La única validación confiable es la que ocurre con la fila
bloqueada, dentro de la transacción del motor.

---

## 5. Concurrencia

> Requisito del reto: *"La estrategia utilizada deberá estar documentada."*

**Bloqueo pesimista de fila, dentro de la transacción del pago.**
`commerce.EJECUTAR_PAGO_QR` lee las dos cuentas con `WITH (UPDLOCK, HOLDLOCK)`:
`UPDLOCK` impide que dos pagos simultáneos del mismo cliente lean ambos el saldo
viejo, y `HOLDLOCK` sostiene el bloqueo hasta el `COMMIT`, no hasta el fin del
`SELECT`.

Las dos cuentas se leen en **una sola sentencia** (`WHERE numero IN (origen,
destino)`): el motor toma los bloqueos en el orden del índice, idéntico para
todas las sesiones, y no puede darse el abrazo mortal de A-luego-B contra
B-luego-A.

```
Saldo del cliente: 100 USD.  Llegan a la vez A = 80 y B = 50.
A entra, bloquea la fila, ve 100, descuenta 80, COMMIT.
B espera el bloqueo, entra, ve 20, no le alcanza → INSUFFICIENT_FUNDS.
Se aprueba una sola. Saldo final 20. Nunca negativo.
```

Tres redes, de la más específica a la más general:

1. La validación de saldo con la fila bloqueada — es la que decide.
2. `UQ_COMMERCE_MOVIMIENTO_QR` y `UQ_COMMERCE_MOVIMIENTO_IDEMPOTENCIA` resuelven
   las carreras que se cuelan entre la lectura y el `INSERT`; el `CATCH` traduce
   la violación al resultado correcto en vez de devolver un error.
3. `CK_COMMERCE_CUENTA_SALDO >= 0`: si algo se escapara, el motor rechaza antes
   que dejar saldo negativo.

**El QR de un solo uso lo garantiza el índice único de este servicio**, no la
llamada a `MarkQrAsUsed`. Dos clientes distintos que escaneen el mismo QR a la
vez pasan las validaciones previas —son cuentas distintas, no comparten
bloqueo—; es el índice el que deja pasar un solo débito.

---

## 6. Idempotencia

Cada pago lleva una `idempotencyKey` del llamador (ej. `TX-2026-000001`).

**El alcance es global, no por cuenta**: identifica una operación del sistema
entero. Con alcance por cuenta, dos clientes que mandaran la misma clave a la vez
pasarían los dos, pero uno detrás del otro el segundo sería rechazado — el mismo
caso daría resultados distintos según el momento. El asiento del comercio lleva
la clave con sufijo `-IN`, así el par débito/crédito no choca entre sí; por eso
el validator la limita a 60 sobre una columna de 64.

Tres capas:

1. **Antes de validar nada**, el handler busca la clave. Si existe y los datos
   coinciden, responde el resultado original con `is_duplicate: true` y el saldo
   que quedó *entonces*.
2. Si existe **con otros datos**, no es un reintento sino una clave reusada:
   `DUPLICATE_TRANSACTION`. Devolver el resultado anterior le mostraría a un
   cliente el pago de otro, o daría por pagado un QR que nunca se cobró.
3. Dentro del SP se repite la búsqueda con la fila bloqueada, y
   `UQ_COMMERCE_MOVIMIENTO_IDEMPOTENCIA` cierra la carrera que quede, traducida
   en el `CATCH` a `-9`.

---

## 7. Errores

Los doce que exige el reto están cubiertos: `CUSTOMER_NOT_FOUND` ·
`CUSTOMER_INACTIVE` · `COMMERCE_NOT_FOUND` · `COMMERCE_INACTIVE` ·
`QR_NOT_FOUND` · `QR_EXPIRED` · `QR_ALREADY_USED` · `QR_INACTIVE` ·
`INSUFFICIENT_FUNDS` · `CURRENCY_NOT_SUPPORTED` · `INVALID_AMOUNT` ·
`DUPLICATE_TRANSACTION`. Más los propios del servicio y el catálogo completo con
sus mensajes en [`docs/ENDPOINTS.md`](../../docs/ENDPOINTS.md).

Las validaciones de entrada salen como `VAL001` con detalle por campo, y las
excepciones no controladas como `ERR001`, ya registradas por el
`LoggerInterceptor`.

---

## 8. Lo que necesita de los demás

Los `.proto` que consume están en `AccountsAndMovements.Infrastructure/Protos/`.
**Son el contrato que se espera de cada microservicio del equipo**; si alguno
define algo distinto, se reemplaza ese archivo y se ajusta la clase en
`ExternalServices/`.

| Servicio | RPC | Datos | Puerto |
|---|---|---|---|
| Clientes | `GetClientById` | id, nombre, email, país, moneda, estado | 52140 |
| Comercio | `GetCommerceById` | id, nombre, NIT, `is_active`. La moneda no viaja: siempre BOB | 52142 |
| QR | `GetQr`, `MarkQrAsUsed` | código, comercio, monto, moneda, referencia, estado, expiración | 52144 |
| Monedas | `GetCurrency`, `GetExchangeRate` | id/código/estado y tipo de cambio origen→destino | 52146 |

Puertos configurables en `appsettings.json`, sección `Services`. Cada cliente
gRPC tiene retry (3 intentos con backoff) y circuit breaker. Mientras esos cuatro
no existan, `UseFakeExternalServices: true` los reemplaza con catálogos en
memoria y el servicio corre solo.

---

## 9. Ejecución y pruebas

```bash
dotnet run --project Microservices/AccountsAndMovements/AccountsAndMovements.Api
dotnet test Microservices/AccountsAndMovements/Tests/AccountsAndMovements.Unit.Tests
```

Desde Visual Studio, el perfil ya está en `YastaCoin.slnLaunch`. `https://localhost:52130/`
responde con el nombre del servicio y la hora.

69 pruebas unitarias acá, 100 en la solución contando Auth. Cubren la lista
obligatoria del reto:

| Caso del reto | Prueba |
|---|---|
| Cliente existente / inexistente / inactivo | `Handle_WhenEverythingIsValid…`, `…ClientDoesNotExist…`, `…ClientIsInactive…` |
| QR válido / inexistente / expirado / usado / inactivo | `…QrDoesNotExist…`, `…QrStatusIsExpired…`, `…QrExpirationHasPassed…`, `…QrIsAlreadyUsed…`, `…QrIsCancelled…` |
| Saldo suficiente / insuficiente | `…SendsTheFrozenConversionToTheDatabase`, `…InsufficientFunds…` |
| Moneda soportada / no soportada | `…CurrencyIsNotInTheCatalog…`, `…CurrencyIsInactive…` |
| Transferencia exitosa / duplicada | `…CompletesThePaymentWithTheConversion`, `…KeyWasAlreadyUsedWithTheSameData…` |
| Solicitudes concurrentes | `…TwoPaymentsRaceOverTheSameBalance_OnlyOneIsApproved` |
| Cálculo de conversión | `CurrencyConverterTests` |

### Validación contra SQL Server real

Los SP se ejecutaron contra una instancia real (SQL Server Express) y se
verificó: alta de cuentas cliente y comercio; cuenta duplicada del mismo titular
y moneda → `-1`; pago de 20 USD × 6.96 → cliente 480, comercio 139.20, dos
asientos con la misma transacción; mismo `idempotencyKey` → mismo id sin mover
saldo; mismo QR con otra clave → `-6`; saldo insuficiente → `-3`; moneda distinta
→ `-7`; conversión incoherente → `-8`; cuenta inexistente → `-1` / `-4`; y que el
saldo es reconstruible desde la suma de los asientos en las dos cuentas.

Alcance global de la clave, también contra el motor: cliente B con la clave de A
→ `-9`; A reintentando con el mismo QR → el mismo id sin cobrar; A con esa clave
en otro QR → `-9`; recarga con una clave ya gastada por el pago → `-9`.

**Concurrencia real:** dos sesiones simultáneas de `sqlcmd` sincronizadas con
`WAITFOR TIME`, pagando desde una cuenta con 100 BOB, una por 80 y otra por 50.
Una aprobada, la otra `-3`, saldo final 50, ninguna cuenta en negativo. Es el
escenario del punto 13 del reto.

---

## 10. Decisiones y supuestos

- **`DB_QRBolivia` / esquema `commerce`**, separado del `coin` del proyecto base
  para no chocar con sus tablas `CUENTA` y `MOVIMIENTO`, que tienen otra forma.
  Cambiar de esquema es reemplazar el prefijo en los cinco scripts y en
  `Persistence/Queries/`.
- **Sin FK hacia CLIENTE, COMERCIO o MONEDA.** Son de otros microservicios; esa
  integridad se valida por gRPC antes de crear la cuenta.
- **El comercio cobra en BOB.** No es un parámetro del alta, así que una cuenta
  de comercio en otra moneda no puede llegar a pedirse.
- **Monto del QR.** Con monto fijo, el convertido debe coincidir exactamente;
  monto `0` es QR abierto y acepta cualquier importe.
- **Redondeo** a 2 decimales `AwayFromZero`, el criterio contable. El valor
  redondeado es el que se acredita y el que se graba.
- **`MarkQrAsUsed` es best effort.** Se llama con el dinero ya movido y no puede
  deshacerlo; si falla queda un warning y el pago sigue siendo válido, porque el
  índice único ya impide cobrar ese QR otra vez.
- **No se persisten intentos fallidos** como movimientos: el libro mayor guarda
  dinero real, los rechazos quedan en el log. Por eso una operación nunca queda
  en `PENDING` —débito y crédito ocurren en la misma transacción—, aunque la
  columna de estado y el filtro del historial soportan los cuatro estados.
- **Sin Entity Framework**, como exige el reto.
