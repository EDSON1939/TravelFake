# AccountsAndMovements — núcleo de pagos QR Bolivia

Microservicio dueño de **cuentas** (saldos) y **movimientos** (libro mayor) del
reto *QR Bolivia*. Es el único punto por el que la plata cambia de manos: valida
la operación contra los demás microservicios, convierte la moneda y ejecuta el
pago de forma atómica.

| | |
|---|---|
| Responsable | Edson |
| Reto | RETO 1 — QR Bolivia (Edson + Mariel + Rene) |
| Puertos | `https://localhost:52130` · `http://localhost:52131` |
| Base de datos | `DB_QRBolivia`, esquema `pay` |
| Stack | C# · ASP.NET Core · gRPC · Protocol Buffers · SQL (Dapper + ADO.NET, **sin EF Core**) |

---

## 1. Alcance

Este servicio hace, y solo hace:

- Abrir y consultar cuentas de **clientes extranjeros** (en su moneda) y de
  **comercios bolivianos** (en BOB).
- Acreditar y debitar saldo (recargas, ajustes) de forma idempotente.
- **Ejecutar el pago QR**: validar, convertir, debitar al cliente y acreditar al
  comercio en una sola transacción.
- Consultar una operación y el historial de movimientos con filtros.

Lo que **no** hace, porque es de otros microservicios del equipo: catálogo de
clientes, países, monedas y tipos de cambio, comercios, y generación/estado de
los códigos QR.

```
                    ┌──────────────────────────────┐
   Clientes ────────┤                              │
   Comercio ────────┤   AccountsAndMovements       ├──── pay.CUENTA
   QR       ────────┤   (núcleo de pagos)          ├──── pay.MOVIMIENTO
   Monedas  ────────┤                              │
                    └──────────────────────────────┘
                        valida por gRPC        es dueño del saldo
```

---

## 2. Estructura

Respeta la arquitectura del proyecto base (Clean Architecture + CQRS con
MediatR, validación con FluentValidation, respuestas `BaseResponse<T>`):

```
AccountsAndMovements/
├── AccountsAndMovements.Domain/          Entidades, códigos de error, contratos
│   ├── Entities/                         AccountEntity, MovementEntity, PaymentEntity, estados
│   ├── Errors/                           ErrorCode / ErrorMessage
│   ├── ExternalServices/                 IClientService, IMerchantService, IQrService, ICurrencyService
│   ├── Repositories/                     IAccountRepository, IMovementRepository
│   └── Services/                         CurrencyConverter (regla de conversión)
├── AccountsAndMovements.Infrastructure/  SQL y clientes gRPC
│   ├── Persistence/Commands/             Llamadas a stored procedures
│   ├── Persistence/Queries/              SQL directo con Dapper
│   ├── ExternalServices/                 Clientes gRPC de los otros microservicios
│   └── Protos/                           Contratos consumidos
├── AccountsAndMovements.Application/     Casos de uso
│   └── Features/{Accounts,Payments,Movements}/
├── AccountsAndMovements.Api/             Borde gRPC
│   ├── Protos/accounts_movements.proto   Contrato publicado
│   └── Services/                         Traduce mensaje → MediatR
└── Tests/AccountsAndMovements.Unit.Tests/
```

---

## 3. Base de datos

Sin Entity Framework: stored procedures para todo lo que escribe, Dapper para lo
que lee. Scripts en `Database/AccountsAndMovements/`.

### Tablas

**`pay.CUENTA`** — saldo por titular y moneda. El titular es `CLIENTE` o
`COMERCIO`; una sola tabla para los dos lados del pago.

| Restricción | Para qué |
|---|---|
| `CK_PAY_CUENTA_SALDO` (`saldo >= 0`) | Garantía final de que no existe saldo negativo |
| `UQ_PAY_CUENTA_TITULAR` | Un titular, una cuenta por moneda |

**`pay.MOVIMIENTO`** — libro mayor. Cada fila es un asiento inmutable con saldo
anterior y posterior. Un pago escribe **dos** filas con el mismo
`MOVI_TRANSACCION_VC`: el DEBITO del cliente y el CREDITO del comercio.

| Restricción | Para qué |
|---|---|
| `UQ_PAY_MOVIMIENTO_IDEMPOTENCIA` (clave, **global**) | Imposible cobrar dos veces la misma operación |
| `UQ_PAY_MOVIMIENTO_QR` (filtrado, solo DEBITO) | Un QR se paga una sola vez, aunque dos clientes lo escaneen a la vez |

Los campos de conversión (`MONTO_ORIGEN`, `MONEDA_ORIGEN`, `TIPO_CAMBIO`,
`MONTO_DESTINO`, `MONEDA_DESTINO`) se graban en el asiento y nunca se
recalculan: **la operación histórica no cambia si mañana cambia el tipo de
cambio**.

### Stored procedures

| SP | Qué resuelve |
|---|---|
| `pay.INSERT_CUENTA` | Crea la cuenta y, si trae saldo inicial, su asiento de apertura |
| `pay.APLICAR_MOVIMIENTO` | Crédito o débito sobre una cuenta, con bloqueo de fila e idempotencia |
| `pay.EJECUTAR_PAGO_QR` | Pago completo: débito + crédito + dos asientos, todo o nada |

### Instalación

```bash
sqlcmd -S localhost\SQLEXPRESS -Q "IF DB_ID('DB_QRBolivia') IS NULL CREATE DATABASE DB_QRBolivia"
```

Luego, en ese orden:

```bash
sqlcmd -S localhost\SQLEXPRESS -d DB_QRBolivia -i Database/AccountsAndMovements/Tables/pay.CUENTA.sql
sqlcmd -S localhost\SQLEXPRESS -d DB_QRBolivia -i Database/AccountsAndMovements/Tables/pay.MOVIMIENTO.sql
sqlcmd -S localhost\SQLEXPRESS -d DB_QRBolivia -i Database/AccountsAndMovements/StoredProcedures/pay.INSERT_CUENTA.sql
sqlcmd -S localhost\SQLEXPRESS -d DB_QRBolivia -i Database/AccountsAndMovements/StoredProcedures/pay.APLICAR_MOVIMIENTO.sql
sqlcmd -S localhost\SQLEXPRESS -d DB_QRBolivia -i Database/AccountsAndMovements/StoredProcedures/pay.EJECUTAR_PAGO_QR.sql
```

Los scripts son idempotentes: se pueden volver a ejecutar sin efecto adicional.

> **`QUOTED_IDENTIFIER` y el índice filtrado.** `UQ_PAY_MOVIMIENTO_QR` es un
> índice filtrado, y SQL Server exige `QUOTED_IDENTIFIER ON` para cualquier
> `INSERT`/`UPDATE`/`DELETE` sobre `pay.MOVIMIENTO`. Los tres SP ya lo fijan
> antes del `CREATE PROCEDURE`, así que el servicio funciona sin hacer nada más
> (`Microsoft.Data.SqlClient` también lo activa al conectarse). Pero si se
> ejecutan consultas sueltas **desde `sqlcmd`**, que lo trae en OFF, hay que
> abrir con `SET QUOTED_IDENTIFIER ON;` o se recibe el error `Msg 1934`. SSMS
> no tiene ese problema.

---

## 4. Contrato gRPC

`AccountsAndMovements.Api/Protos/accounts_movements.proto`. Los decimales viajan
como **string** para no perder precisión; las fechas como **ISO 8601**, y `""`
significa "sin valor" porque proto3 no tiene null para escalares.

| RPC | Para qué |
|---|---|
| `CreateAccount` | Abre cuenta de cliente o comercio, con saldo inicial opcional |
| `GetAccount` | Cuenta por número |
| `GetAccountByOwner` | Resuelve (titular, moneda) → cuenta |
| `GetAccountsByOwner` | Todas las cuentas de un titular |
| `ApplyMovement` | Crédito/débito suelto, idempotente |
| **`ExecuteQrPayment`** | **Pago del QR: la operación central** |
| `GetPayment` | Consulta una operación por código o por idempotency key |
| `GetMovements` | Extracto de una cuenta |
| `GetHistory` | Historial del titular filtrando por fechas y estado |

### Ejemplo — `ExecuteQrPayment`

Request:

```json
{
  "client_id": 7,
  "qr_code": "QR-BO-0001",
  "amount": "20",
  "currency_code": "USD",
  "idempotency_key": "TX-2026-000001",
  "description": "Pago en Café Central"
}
```

Response:

```json
{
  "message": "OK.",
  "status_code": "SUC000",
  "data": {
    "transaction_code": "d3f1b6f0-3a41-4c2e-9f18-77c0a1b2c3d4",
    "movement_id": 900,
    "client_id": 7,
    "client_account_number": "QR0000000001",
    "merchant_id": 55,
    "merchant_name": "Café Central",
    "merchant_account_number": "QR0000000002",
    "qr_code": "QR-BO-0001",
    "original_amount": "20",
    "original_currency": "USD",
    "exchange_rate": "6.96",
    "converted_amount": "139.20",
    "target_currency": "BOB",
    "client_balance": "480",
    "status": "COMPLETED",
    "is_duplicate": false
  }
}
```

Reenviando **la misma** `idempotency_key` se responde exactamente lo mismo con
`"is_duplicate": true`, y al cliente **no** se le cobra de nuevo.

Error de negocio (saldo insuficiente):

```json
{
  "message": "El saldo de la cuenta es insuficiente.",
  "status_code": "INSUFFICIENT_FUNDS",
  "data": null
}
```

### Ejemplo — `CreateAccount`

```json
{ "owner_type": "CLIENTE", "owner_id": 7, "coin_code": "USD", "initial_balance": "500" }
```

```json
{ "message": "OK.", "status_code": "SUC000", "data": 1 }
```

### Ejemplo — `GetHistory`

```json
{
  "owner_type": "CLIENTE", "owner_id": 7,
  "date_from": "2026-09-01", "date_to": "2026-09-30",
  "status": "COMPLETED", "page_number": 1, "page_size": 20
}
```

La fecha final se toma **inclusive**: si viene sin hora, el rango se corre al día
siguiente para no dejar fuera lo del último día.

---

## 5. Flujo del pago

```
ExecuteQrPayment
      │
      ├─ 0. ¿La idempotency key ya se usó? ── sí, mismos datos ─→ devuelve el resultado original
      │                                    └─ sí, otros datos ─→ DUPLICATE_TRANSACTION
      ├─ 1. Validar cliente ......................... Clientes  → CUSTOMER_NOT_FOUND / CUSTOMER_INACTIVE
      ├─ 2. Validar QR .............................. QR        → QR_NOT_FOUND / QR_ALREADY_USED / QR_EXPIRED / QR_INACTIVE
      ├─ 3. Validar comercio ........................ Comercio  → MERCHANT_NOT_FOUND / MERCHANT_INACTIVE
      ├─ 4. Validar moneda .......................... Monedas   → CURRENCY_NOT_SUPPORTED
      ├─ 5. Obtener tipo de cambio .................. Monedas   → EXCHANGE_RATE_NOT_FOUND
      ├─ 6. Convertir  (20 USD × 6.96 = 139.20 BOB)
      ├─ 7. ¿El monto convertido cubre el QR? ..................→ INVALID_AMOUNT
      ├─ 8. Resolver cuentas de cliente y comercio ............→ ACCOUNT_NOT_FOUND / MERCHANT_ACCOUNT_NOT_FOUND
      │
      ├─ 9. pay.EJECUTAR_PAGO_QR  ◄── una sola transacción
      │        ├─ bloquea las dos cuentas
      │        ├─ valida saldo ...............................→ INSUFFICIENT_FUNDS
      │        ├─ valida que el QR no esté cobrado ...........→ QR_ALREADY_USED
      │        ├─ asienta DEBITO al cliente
      │        ├─ asienta CREDITO al comercio
      │        └─ actualiza los dos saldos
      │
      ├─ 10. Marcar el QR como usado (best effort)
      └─ 11. Devolver la operación con su conversión
```

**Por qué el saldo se valida adentro del SP y no en el handler:** una lectura de
saldo desde C# no sirve para decidir. Entre esa lectura y el pago puede entrar
otra operación del mismo cliente. La única validación confiable es la que ocurre
con la fila de la cuenta bloqueada, y eso solo se sostiene dentro de la
transacción del motor.

---

## 6. Concurrencia — estrategia

> Requisito del reto: *"La estrategia utilizada deberá estar documentada."*

**Bloqueo pesimista de fila, dentro de la transacción del pago.**

`pay.EJECUTAR_PAGO_QR` lee las dos cuentas con `WITH (UPDLOCK, HOLDLOCK)`:

- `UPDLOCK` toma un bloqueo de actualización al leer, así dos pagos simultáneos
  del mismo cliente **no** pueden ambos leer el saldo viejo.
- `HOLDLOCK` lo mantiene hasta el `COMMIT`, no hasta el final del `SELECT`.

Las dos cuentas se leen en **una sola sentencia** con `WHERE numero IN (origen,
destino)`. Al ser una sola sentencia, el motor toma los bloqueos en el orden del
índice —idéntico para todas las sesiones— y no puede darse el abrazo mortal
clásico de bloquear A-luego-B en un hilo y B-luego-A en el otro.

Escenario del reto:

```
Saldo del cliente: 100 USD
Llegan a la vez:   Transferencia A = 80 USD
                   Transferencia B = 50 USD

A entra, bloquea la fila, ve 100, descuenta 80, hace COMMIT.
B espera el bloqueo, entra, ve 20, no le alcanza → INSUFFICIENT_FUNDS.

Resultado: se aprueba una sola. Saldo final = 20. Nunca negativo.
```

Tres redes, de la más específica a la más general:

1. La validación de saldo con la fila bloqueada (decide).
2. `UQ_PAY_MOVIMIENTO_QR` (un QR, un solo débito) y
   `UQ_PAY_MOVIMIENTO_IDEMPOTENCIA` (una clave, un solo asiento): resuelven las
   carreras que se cuelan entre la lectura y el `INSERT`; el bloque `CATCH`
   traduce la violación al resultado correcto en vez de devolver un error.
3. `CK_PAY_CUENTA_SALDO >= 0`: si algo se escapara de todo lo anterior, el motor
   rechaza la operación antes que dejar saldo negativo.

**Sobre el QR de un solo uso:** que un QR no se pague dos veces lo garantiza el
índice único de *este* servicio, no la llamada a `MarkQrAsUsed`. Si dos clientes
distintos escanean el mismo QR al mismo tiempo, los dos pasan las validaciones
previas —son cuentas distintas, no comparten bloqueo— y es el índice el que deja
pasar un solo débito.

---

## 7. Idempotencia

Cada pago lleva una `idempotencyKey` del llamador (ej: `TX-2026-000001`).

**El alcance de la clave es global**, no por cuenta: identifica una operación del
sistema entero. Es una decisión deliberada. Con alcance por cuenta, dos clientes
que mandaran la misma clave a la vez pasarían los dos, pero uno detrás del otro
el segundo sería rechazado: el mismo caso daría resultados distintos según el
momento. El asiento del comercio lleva la clave con el sufijo `-IN`, así que el
par débito/crédito no choca entre sí (por eso el validator la limita a 60
caracteres sobre una columna de 64).

Se protege en tres capas:

1. **Antes de validar nada**, el handler busca la clave. Si ya existe y los datos
   coinciden —mismo cliente, mismo QR, mismo monto y moneda—, responde el
   resultado original con `is_duplicate: true`, incluido el saldo que quedó
   entonces, no el de hoy.
2. Si la clave existe **con otros datos** —otro cliente, otro QR, o se gastó en
   una recarga— no es un reintento sino una clave reusada:
   `DUPLICATE_TRANSACTION`. Devolver el resultado anterior le mostraría a un
   cliente el pago de otro, o daría por pagado un QR que nunca se cobró.
3. Dentro del SP se repite la búsqueda con la fila ya bloqueada, comparando
   cuenta y QR; y `UQ_PAY_MOVIMIENTO_IDEMPOTENCIA` cierra la carrera que quede,
   traducida en el `CATCH` a `-9` → `DUPLICATE_TRANSACTION`.

---

## 8. Errores

Se devuelven en `status_code` dentro de `BaseResponse`, con el mensaje en
`message`. Los doce que exige el reto están cubiertos:

`CUSTOMER_NOT_FOUND` · `CUSTOMER_INACTIVE` · `MERCHANT_NOT_FOUND` ·
`MERCHANT_INACTIVE` · `QR_NOT_FOUND` · `QR_EXPIRED` · `QR_ALREADY_USED` ·
`QR_INACTIVE` · `INSUFFICIENT_FUNDS` · `CURRENCY_NOT_SUPPORTED` ·
`INVALID_AMOUNT` · `DUPLICATE_TRANSACTION`

Más los propios del servicio: `ACCOUNT_NOT_FOUND`, `ACCOUNT_INACTIVE`,
`ACCOUNT_DUPLICATE`, `MERCHANT_ACCOUNT_NOT_FOUND`, `MERCHANT_ACCOUNT_INACTIVE`,
`MERCHANT_CURRENCY_INVALID`, `CURRENCY_MISMATCH`, `EXCHANGE_RATE_NOT_FOUND`,
`INSERT_FAILED`, `MOVEMENT_FAILED`, `PAYMENT_FAILED`, `PAYMENT_NOT_FOUND`.

Los errores de validación de entrada salen como `VAL001` con el detalle por
campo (interceptor del proyecto base), y las excepciones no controladas como
`ERR001`, ya registradas por el `LoggerInterceptor`.

---

## 9. Lo que este servicio necesita de los demás

Los `.proto` que consume están en `AccountsAndMovements.Infrastructure/Protos/`.
**Son el contrato que se espera de cada microservicio del equipo**; si alguno
define algo distinto, se reemplaza ese archivo y se ajusta la clase equivalente
en `ExternalServices/`.

| Servicio | RPC que se consume | Datos que se necesitan |
|---|---|---|
| Clientes | `GetClientById` | id, nombre, email, país, moneda, estado (`ACTIVE`/`INACTIVE`) |
| Comercio | `GetMerchantById` | id, nombre, NIT, moneda (BOB), estado |
| QR | `GetQr`, `MarkQrAsUsed` | código, comercio, monto, moneda, referencia, estado, expiración |
| Monedas | `GetCurrency`, `GetExchangeRate` | id/código/estado de la moneda y el tipo de cambio origen→destino |

Puertos esperados (configurables en `appsettings.json`, sección `Services`):

| Servicio | Puerto |
|---|---|
| Clientes | `https://localhost:52140` |
| Comercio | `https://localhost:52142` |
| QR | `https://localhost:52144` |
| Monedas | `https://localhost:52146` |

Cada cliente gRPC tiene **retry** (3 intentos con backoff) y **circuit breaker**
configurados por sección en `appsettings.json`, usando el mecanismo del proyecto
base.

---

## 10. Ejecución

```bash
dotnet run --project Microservices/AccountsAndMovements/AccountsAndMovements.Api
```

Desde Visual Studio: el perfil `AccountsAndMovements.Api` ya está en
`YastaCoin.slnLaunch`, dentro de "Todos los microservicios".

Verificación rápida de que está arriba: `https://localhost:52130/` responde con
el nombre del servicio y la hora.

Para probar los RPC conviene un cliente gRPC (Postman, BloomRPC o `grpcurl`)
apuntando a `accounts_movements.proto`.

---

## 11. Pruebas

```bash
dotnet test Microservices/AccountsAndMovements/Tests/AccountsAndMovements.Unit.Tests
```

65 pruebas, que cubren la lista obligatoria del reto:

| Caso del reto | Prueba |
|---|---|
| Cliente existente / inexistente / inactivo | `Handle_WhenEverythingIsValid…`, `…ClientDoesNotExist…`, `…ClientIsInactive…` |
| QR válido / inexistente / expirado / utilizado / inactivo | `…QrDoesNotExist…`, `…QrStatusIsExpired…`, `…QrExpirationHasPassed…`, `…QrIsAlreadyUsed…`, `…QrIsCancelled…` |
| Saldo suficiente / insuficiente | `…SendsTheFrozenConversionToTheDatabase`, `…InsufficientFunds…` |
| Moneda soportada / no soportada | `…CurrencyIsNotInTheCatalog…`, `…CurrencyIsInactive…` |
| Transferencia exitosa / duplicada | `…CompletesThePaymentWithTheConversion`, `…KeyWasAlreadyUsedWithTheSameData…` |
| Solicitudes concurrentes | `…TwoPaymentsRaceOverTheSameBalance_OnlyOneIsApproved` |
| Cálculo de conversión | `CurrencyConverterTests` |

### Validación contra SQL Server real

Los scripts se ejecutaron contra una instancia real (SQL Server 2025 Express) en
una base temporal, ya eliminada. Resultados verificados:

| Caso | Esperado | Obtenido |
|---|---|---|
| Crear cuentas cliente (500 USD) y comercio (0 BOB) | ids 1 y 2 | ✔ |
| Cuenta duplicada del mismo titular y moneda | `-1` | ✔ |
| Pago 20 USD × 6.96 | cliente 480 USD · comercio 139.20 BOB · 2 asientos | ✔ |
| Mismo `idempotencyKey` otra vez | mismo id, saldos sin cambio, sin asientos nuevos | ✔ |
| Mismo QR con otra clave | `-6` | ✔ |
| Saldo insuficiente | `-3` | ✔ |
| Moneda distinta a la de la cuenta | `-7` | ✔ |
| Conversión incoherente con el tipo de cambio | `-8` | ✔ |
| Cuenta de cliente / de comercio inexistente | `-1` / `-4` | ✔ |
| Recarga repetida con la misma clave | acredita una sola vez | ✔ |
| Saldo == suma de los asientos, en las dos cuentas | reconstruible | ✔ |

Alcance de la clave de idempotencia, también verificado contra el motor:

| Caso | Esperado | Obtenido |
|---|---|---|
| Cliente A paga con `TX-DUP` | id del asiento | ✔ |
| Cliente **B** usa la misma `TX-DUP` | `-9` → `DUPLICATE_TRANSACTION` | ✔ |
| Cliente A reintenta `TX-DUP` con el mismo QR | el mismo id, sin cobrar de nuevo | ✔ |
| Cliente A usa `TX-DUP` en **otro** QR | `-9` | ✔ |
| Recarga con `TX-DUP`, ya gastada por el pago | `-9` | ✔ |
| Saldos tras los cinco intentos | solo un cobro aplicado | ✔ |

**Concurrencia real:** dos sesiones simultáneas de `sqlcmd`, sincronizadas con
`WAITFOR TIME`, pagando desde una cuenta con 100 BOB: una por 80 y otra por 50.
Resultado: una aprobada (50), la otra rechazada con `-3`, saldo final 50 y
ninguna cuenta en negativo. Es exactamente el escenario del punto 13 del reto.

---

## 12. Decisiones y supuestos

- **Base de datos y esquema.** `DB_QRBolivia` / esquema `pay`, separado del
  `coin` del proyecto base para no chocar con sus tablas `CUENTA` y `MOVIMIENTO`,
  que tienen otra forma. Si el equipo acordó otro esquema, se reemplaza el
  prefijo `pay.` en los cinco scripts SQL y en las consultas de
  `Persistence/Queries/`.
- **Sin FK hacia CLIENTE, COMERCIO o MONEDA.** Son tablas de otros
  microservicios; esa integridad se valida por gRPC antes de crear la cuenta.
  Es el mismo criterio del proyecto base.
- **El comercio cobra en BOB.** Una cuenta de comercio en otra moneda se rechaza
  con `MERCHANT_CURRENCY_INVALID`.
- **Monto del QR.** Si el QR trae monto fijo, el convertido debe coincidir
  exactamente; monto `0` significa QR abierto y acepta cualquier importe.
- **Redondeo.** La conversión se redondea a 2 decimales `AwayFromZero`, el
  criterio contable. El valor redondeado es el que se acredita y el que se graba.
- **`MarkQrAsUsed` es best effort.** Se llama con el dinero ya movido y no puede
  deshacerlo; si falla se registra un warning y el pago sigue siendo válido,
  porque el índice único ya impide que ese QR se cobre otra vez.
- **No se persisten intentos fallidos** como movimientos. El libro mayor guarda
  movimientos de dinero reales; los rechazos quedan en el log. Por eso una
  operación nunca queda en `PENDING`: débito y crédito ocurren en la misma
  transacción, no hay ventana intermedia. La columna de estado y el filtro del
  historial soportan igual los cuatro estados del reto.
- **Sin Entity Framework**, como exige el reto: stored procedures para escribir y
  Dapper con SQL explícito para leer.
