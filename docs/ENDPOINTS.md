# Documentación de endpoints gRPC

Contratos de los dos microservicios que existen hoy: **Auth** y **AccountsAndMovements**.

| Servicio | Paquete proto | Puerto local | Puerto Docker | Contrato |
|---|---|---|---|---|
| Auth | `auth` | 52127 (http) · 52126 (https) | 52127 | [`auth.proto`](../Microservices/Auth/Auth.Api/Protos/auth.proto) |
| AccountsAndMovements | `accounts_movements` | 52131 (http) · 52130 (https) | 52131 | [`accounts_movements.proto`](../Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos/accounts_movements.proto) |

Ambos protos hacen `import "base_response.proto"`, que vive en
[`YastaCoin/Shared/Core.Domain/Grpc/Protobuf/`](../YastaCoin/Shared/Core.Domain/Grpc/Protobuf/base_response.proto).
Cualquier cliente que los compile necesita esa carpeta como *import path*.

---

## 1. Cómo se lee una respuesta

Todas las respuestas comparten el mismo sobre. Nunca hay un `data` "suelto":

```proto
message XxxBaseResponsePb {
    string                          message     = 1;
    <TipoDeDato>                    data        = 2;
    repeated base_response.ErrorPb  errors      = 3;
    base_response.ExceptionDetailPb exception   = 4;
    string                          status_code = 5;
}
```

| Campo | Cuándo viene | Qué contiene |
|---|---|---|
| `status_code` | siempre | `SUC000` si salió bien; si no, el código del error |
| `message` | siempre | Texto en español, listo para mostrarle a una persona |
| `data` | solo con `SUC000` | El resultado. Su tipo cambia por endpoint |
| `errors` | solo con `VAL001` | Un elemento por campo inválido: `{ field, message }` |
| `exception` | solo con `ERR001` | Detalle de la excepción, para diagnóstico |

**La regla de lectura es una sola: mirar `status_code`, no el canal de transporte.**
Un error de negocio —saldo insuficiente, QR vencido— viaja como una respuesta
gRPC `OK` con el `status_code` correspondiente. Eso es deliberado: son
resultados esperados de la operación, no fallas de comunicación. Los `StatusCode`
de gRPC quedan reservados para lo que sí es de transporte (§6).

### Convenciones del contrato

| Convención | Motivo |
|---|---|
| Los decimales viajan como **string** (`"139.20"`) | Un `double` redondearía montos y tipos de cambio, y esa diferencia terminaría grabada en el asiento contable |
| Las fechas viajan como **string ISO 8601** | proto3 no tiene `null` para escalares |
| `""` significa **sin valor** | Misma razón: un string vacío es el "no hay dato" del contrato |
| `0` en un `int64` opcional significa **sin valor** | Ej. `merchant_id: 0` en un movimiento que no es de un pago QR |

### Autenticación

Todos los RPC exigen la metadata `authorization` con formato `Bearer <jwt>`.
El interceptor corta antes de llegar al handler si falta.

Este microservicio **lee** el token pero **no verifica su firma**: eso es trabajo
del Gateway, único punto de entrada. Lo que sí resuelve acá es la **identidad**,
que no puede delegarse — un token auténtico igual podría traer en el cuerpo el id
de otro cliente. Los endpoints marcados con 🔑 sacan el cliente del claim
`client_id` y no lo reciben como parámetro.

Claims que emite Auth: `sub` (user_id), `client_id`, `username`, `role`,
`permissions`, `jti`, `exp`.

---

## 2. Auth

### `Login`

Único endpoint anónimo del sistema. Entrega el JWT que usan todos los demás.

**Request** `LoginRequestPb`

| Campo | Tipo | Reglas |
|---|---|---|
| `username` | string | requerido |
| `password` | string | requerido |

**Response** `data` = `LoginDataPb`: `access_token`, `expires_at`, `expires_in`
(segundos), `user_id`, `client_id` (0 si es un operador interno), `username`,
`full_name`, `role`.

```json
// request
{ "username": "carlos", "password": "Secreta123!" }

// response
{
  "status_code": "SUC000",
  "message": "Operación realizada con éxito.",
  "data": {
    "access_token": "eyJhbGciOiJIUzI1NiIs...",
    "expires_at": "2026-09-03T22:35:00Z",
    "expires_in": 1800,
    "user_id": 4,
    "client_id": 1,
    "username": "carlos",
    "full_name": "Carlos Pérez",
    "role": "CLIENTE"
  }
}
```

**Errores**

| Código | Cuándo |
|---|---|
| `INVALID_CREDENTIALS` | Usuario inexistente o contraseña incorrecta. **Mismo error para los dos casos**: distinguirlos le diría a un atacante qué usuarios existen |
| `USER_INACTIVE` | El usuario está dado de baja |
| `USER_LOCKED` | Bloqueado por intentos fallidos. Se libera solo tras `Jwt:LockMinutes` |

Cada intento fallido incrementa un contador; al llegar a `Jwt:MaxFailedAttempts`
el usuario queda bloqueado. Es lo que frena la fuerza bruta.

---

### `CreateUser`

Requiere rol **ADMIN**.

**Request** `CreateUserRequestPb`

| Campo | Tipo | Reglas |
|---|---|---|
| `username` | string | requerido, único |
| `password` | string | requerido |
| `document` | string | Documento del cliente del negocio. Vacío solo para un ADMIN interno |
| `full_name` | string | requerido |
| `role` | string | `CLIENTE` · `AGENTE` · `ADMIN` |

**Response** `data` = `int64` con el `user_id` creado.

**Errores**: `USER_DUPLICATE`, `CLIENT_NOT_FOUND`, `CLIENT_INACTIVE`,
`CLIENT_ALREADY_HAS_USER`, `INSERT_FAILED`.

La contraseña se guarda como PBKDF2 en formato `iteraciones.salt.hash` y nunca
sale de Auth.

---

### `DeleteUser`

Baja **lógica**: sella `deleted_at`. El usuario deja de poder autenticarse pero se
conserva el rastro de quién era — un libro de auditoría sin el actor no sirve.

**Request** `DeleteUserRequestPb`: `user_id` (int64).
**Response** `data` = `int64` con el id eliminado.
**Errores**: `USER_NOT_FOUND`, `DELETE_FAILED`.

---

## 3. AccountsAndMovements — Cuentas

No existe un endpoint genérico con un discriminador de titular. Cliente y
comercio tienen reglas distintas y cada uno tiene su operación: así el contrato
**no permite expresar** combinaciones inválidas en vez de rechazarlas en tiempo
de ejecución.

### `CreateClientAccount`

Alta de la cuenta de un cliente extranjero, en la moneda de su país.

> **Endpoint de operador.** `initial_balance` acredita saldo real, así que
> debería exigir rol `AGENTE` o `ADMIN`. Hoy no hay control de rol — está
> anotado como pendiente.

**Request** `CreateClientAccountRequestPb`

| Campo | Tipo | Reglas |
|---|---|---|
| `client_id` | int64 | `> 0` |
| `coin_code` | string | requerido, máx. 10, `^[A-Za-z0-9]+$` |
| `initial_balance` | string (decimal) | `>= 0`. `0` = sin apertura |

**Response** `data` = `int64`, el id de la cuenta creada. El número de cuenta se
deriva de ese id: `QR` + 10 dígitos → `QR0000000001`.

```json
// request
{ "client_id": 1, "coin_code": "USD", "initial_balance": "500" }

// response
{ "status_code": "SUC000", "message": "Operación realizada con éxito.", "data": 1 }
```

**Errores**: `CUSTOMER_NOT_FOUND`, `CUSTOMER_INACTIVE`, `CURRENCY_NOT_SUPPORTED`,
`ACCOUNT_DUPLICATE`, `INSERT_FAILED`.

El saldo inicial no se escribe directo en la cuenta: entra como un asiento
`CREDITO` de apertura, para que el libro mayor explique el saldo desde el
principio.

---

### `CreateMerchantAccount`

**No recibe moneda.** El comercio boliviano cobra en BOB por regla del negocio;
al no ser un parámetro, `MERCHANT_CURRENCY_INVALID` dejó de poder ocurrir.

**Request** `CreateMerchantAccountRequestPb`

| Campo | Tipo | Reglas |
|---|---|---|
| `merchant_id` | int64 | `> 0` |
| `initial_balance` | string (decimal) | `>= 0` |

**Response** `data` = `int64`.
**Errores**: `MERCHANT_NOT_FOUND`, `MERCHANT_INACTIVE`, `CURRENCY_NOT_SUPPORTED`,
`ACCOUNT_DUPLICATE`, `INSERT_FAILED`.

---

### `GetAccount`

**Request**: `number` (string, requerido, máx. 20).
**Response** `data` = `AccountDataPb`.
**Errores**: `ACCOUNT_NOT_FOUND`.

```json
{
  "status_code": "SUC000",
  "data": {
    "account_id": 1, "number": "QR0000000001",
    "owner_type": "CLIENTE", "owner_id": 1,
    "coin_id": 2, "coin_code": "USD",
    "balance": "480.00", "is_active": true,
    "created_at": "2026-09-03T21:40:11", "updated_at": "2026-09-03T21:52:03", "deleted_at": ""
  }
}
```

---

### `GetMyAccount` 🔑

Mi cuenta en una moneda. El cliente sale del token.

**Request**: `coin_code` (string, requerido, máx. 10).
**Response** `data` = `AccountDataPb`.
**Errores**: `ACCOUNT_NOT_FOUND`.

---

### `GetMyAccounts` 🔑

**Request**: `only_active` (bool).
**Response** `data` = `repeated AccountDataPb`. Una lista vacía es `SUC000`, no
un error: "no tenés cuentas" es una respuesta válida.

---

### `GetMerchantAccount`

**Request**: `merchant_id` (int64, `> 0`). Sin moneda: siempre BOB.
**Response** `data` = `AccountDataPb`.
**Errores**: `ACCOUNT_NOT_FOUND`.

---

### `GetMerchantAccounts`

**Request**: `merchant_id` (int64, `> 0`), `only_active` (bool).
**Response** `data` = `repeated AccountDataPb`.

---

### `ApplyMovement`

Crédito o débito suelto sobre una cuenta: recargas, ajustes, reversas. **El pago
QR no pasa por acá** — tiene su propia operación atómica de dos patas.

**Request** `ApplyMovementRequestPb`

| Campo | Tipo | Reglas |
|---|---|---|
| `account_number` | string | requerido, máx. 20 |
| `type` | string | `CREDITO` · `DEBITO` |
| `amount` | string (decimal) | `> 0` |
| `reference` | string | máx. 64 |
| `idempotency_key` | string | requerido, máx. 60 |
| `description` | string | máx. 250 |

**Response** `data` = `MovementAppliedDataPb`: `movement_id`, `account_number`,
`balance`.

```json
// request
{
  "account_number": "QR0000000001", "type": "CREDITO", "amount": "100",
  "reference": "RECARGA", "idempotency_key": "TOPUP-001", "description": "Recarga de saldo"
}

// response
{ "status_code": "SUC000", "data": { "movement_id": 3, "account_number": "QR0000000001", "balance": "580.00" } }
```

**Errores**: `ACCOUNT_NOT_FOUND`, `ACCOUNT_INACTIVE`, `INSUFFICIENT_FUNDS`,
`DUPLICATE_TRANSACTION`, `MOVEMENT_FAILED`.

Es idempotente por `idempotency_key`: repetir la llamada **con los mismos
datos** devuelve el mismo `movement_id` y no vuelve a acreditar.

Cuenta como reintento solo si el asiento hallado es de la misma cuenta y además
del mismo tipo y monto. Reusar una clave para otra operación —la clave de una
recarga en un débito, por ejemplo— devuelve `DUPLICATE_TRANSACTION`, no un éxito
que ocultaría que el movimiento nuevo nunca se aplicó. Es el mismo criterio que
usa el pago QR.

---

## 4. AccountsAndMovements — Pagos

### `ExecuteQrPayment` 🔑

La operación central. Valida, convierte, debita al cliente y acredita al
comercio **en una sola transacción**.

**Request** `ExecuteQrPaymentRequestPb`

| Campo | Tipo | Reglas |
|---|---|---|
| ~~`client_id`~~ | — | **Reservado.** Sale del claim `client_id` del JWT |
| `qr_code` | string | requerido, máx. 64 |
| `amount` | string (decimal) | `> 0`, en la moneda del cliente |
| `currency_code` | string | requerido, máx. 10, `^[A-Za-z]{3,10}$` |
| `idempotency_key` | string | requerido, máx. 60 |
| `description` | string | máx. 250 |

> El límite de 60 sobre una columna de 64 no es arbitrario: el asiento del
> comercio reutiliza la clave con el sufijo `-IN`.

**Response** `data` = `PaymentDataPb`. El campo que hay que mirar además del
`status_code` es **`is_duplicate`**: `true` significa que la clave ya se había
usado y esta es la respuesta de la operación original — **al cliente no se le
cobró de nuevo**.

```json
// request  (metadata: authorization: Bearer <jwt del cliente 1>)
{
  "qr_code": "QR-BO-0001", "amount": "20", "currency_code": "USD",
  "idempotency_key": "TX-2026-000001", "description": "Almuerzo en Café Central"
}

// response
{
  "status_code": "SUC000",
  "message": "Operación realizada con éxito.",
  "data": {
    "transaction_code": "8f3c1e0a-...", "movement_id": 2,
    "client_id": 1, "client_account_number": "QR0000000001",
    "merchant_id": 55, "merchant_name": "Café Central", "merchant_account_number": "QR0000000002",
    "qr_code": "QR-BO-0001",
    "original_amount": "20", "original_currency": "USD",
    "exchange_rate": "6.96",
    "converted_amount": "139.20", "target_currency": "BOB",
    "client_balance": "480.00",
    "status": "COMPLETED", "reference": "REF-CAFE-001",
    "idempotency_key": "TX-2026-000001", "description": "Almuerzo en Café Central",
    "created_at": "2026-09-03T21:52:03", "is_duplicate": false
  }
}
```

**Errores**, en el orden en que se evalúan:

| Código | Cuándo |
|---|---|
| `DUPLICATE_TRANSACTION` | La clave ya existe **con otros datos** — otro cliente, otro QR, otro monto, o se gastó en una recarga |
| `CUSTOMER_NOT_FOUND` / `CUSTOMER_INACTIVE` | Cliente |
| `QR_NOT_FOUND` / `QR_ALREADY_USED` / `QR_EXPIRED` / `QR_INACTIVE` | QR |
| `MERCHANT_NOT_FOUND` / `MERCHANT_INACTIVE` | Comercio emisor del QR |
| `CURRENCY_NOT_SUPPORTED` | La moneda del cliente no está en el catálogo o está inactiva |
| `CURRENCY_MISMATCH` | La moneda del QR no coincide con la del comercio: catálogo inconsistente |
| `EXCHANGE_RATE_NOT_FOUND` | No hay cotización vigente para el par |
| `INVALID_AMOUNT` | El QR tiene monto fijo y el convertido no coincide. Monto `0` = QR abierto, acepta cualquiera |
| `ACCOUNT_NOT_FOUND` / `MERCHANT_ACCOUNT_NOT_FOUND` | Falta la cuenta de alguno de los dos lados |
| `ACCOUNT_INACTIVE` / `MERCHANT_ACCOUNT_INACTIVE` | Cuenta dada de baja |
| `INSUFFICIENT_FUNDS` | Lo decide el motor con la fila bloqueada, no el handler |
| `PAYMENT_FAILED` | Cualquier otro fallo del procedimiento |

Si la clave se repite **con los mismos datos** (mismo cliente, QR, moneda y
monto), la respuesta es `SUC000` con `is_duplicate: true` y el saldo que quedó
*entonces*, no el de hoy.

---

### `GetPayment`

Se resuelve solo con el libro mayor: no llama a Comercios ni a Monedas, así una
consulta de algo ya ocurrido no se cae porque otro servicio esté abajo.

**Request** `GetPaymentRequestPb` — uno de los dos. Si vienen ambos, manda
`transaction_code`.

| Campo | Tipo | Reglas |
|---|---|---|
| `transaction_code` | string | máx. 36 |
| `idempotency_key` | string | máx. 64 |

**Response** `data` = `PaymentDataPb`, con `merchant_name` vacío (esa es la
contrapartida de no llamar al servicio de Comercios) e `is_duplicate: false`.

**Errores**: `PAYMENT_NOT_FOUND`.

---

## 5. AccountsAndMovements — Movimientos

### `GetMovements`

Extracto de **una cuenta**, más reciente primero.

| Campo | Tipo | Reglas |
|---|---|---|
| `account_number` | string | requerido, máx. 20 |
| `page_number` | int32 | `> 0` |
| `page_size` | int32 | entre 1 y 100 |

**Response** `data` = `repeated MovementDataPb`.

---

### `GetMyHistory` 🔑

Historial **del titular**: si el cliente tiene cuentas en varias monedas, salen
todas juntas.

| Campo | Tipo | Reglas |
|---|---|---|
| `date_from` | string ISO | vacío = sin filtro |
| `date_to` | string ISO | vacío = sin filtro. Se toma **inclusive** · debe ser `>= date_from` |
| `status` | string | `PENDING` · `COMPLETED` · `FAILED` · `CANCELLED`. Vacío = todos |
| `page_number` | int32 | `> 0` |
| `page_size` | int32 | entre 1 y 100 |

**Response** `data` = `repeated MovementDataPb`.

```json
// request
{ "date_from": "2026-09-01", "date_to": "2026-09-30", "status": "COMPLETED", "page_number": 1, "page_size": 10 }
```

---

### `GetMerchantHistory`

Igual que el anterior, con `merchant_id` (int64, `> 0`) como primer campo.

---

## 6. Catálogo de códigos

### Códigos transversales

| Código | Significado |
|---|---|
| `SUC000` | Operación exitosa |
| `VAL001` | Falló la validación de entrada. El detalle por campo viene en `errors[]` |
| `ERR001` | Error no controlado. El detalle viene en `exception` |
| `AUT001` | Token ausente o inválido |
| `AUT002` | Token que no pasa la validación de firma o vigencia |
| `AUT003` | El token es válido pero no habilita esa operación |

### Errores de negocio — AccountsAndMovements

Los doce que exige el reto:

| Código | Mensaje |
|---|---|
| `CUSTOMER_NOT_FOUND` | El cliente indicado no existe. |
| `CUSTOMER_INACTIVE` | El cliente indicado se encuentra inactivo. |
| `MERCHANT_NOT_FOUND` | El comercio indicado no existe. |
| `MERCHANT_INACTIVE` | El comercio indicado se encuentra inactivo. |
| `QR_NOT_FOUND` | El código QR no existe. |
| `QR_EXPIRED` | El código QR expiró. |
| `QR_ALREADY_USED` | El código QR ya fue utilizado. |
| `QR_INACTIVE` | El código QR no se encuentra activo. |
| `INSUFFICIENT_FUNDS` | El saldo de la cuenta es insuficiente. |
| `CURRENCY_NOT_SUPPORTED` | La moneda indicada no está soportada. |
| `INVALID_AMOUNT` | El monto de la operación no es válido. |
| `DUPLICATE_TRANSACTION` | La clave de idempotencia ya fue usada con datos distintos. |

Y los propios del servicio:

| Código | Mensaje |
|---|---|
| `ACCOUNT_NOT_FOUND` | La cuenta solicitada no existe. |
| `ACCOUNT_INACTIVE` | La cuenta se encuentra inactiva. |
| `ACCOUNT_DUPLICATE` | El titular ya tiene una cuenta en esa moneda. |
| `MERCHANT_ACCOUNT_NOT_FOUND` | El comercio no tiene una cuenta habilitada para cobrar. |
| `MERCHANT_ACCOUNT_INACTIVE` | La cuenta del comercio se encuentra inactiva. |
| `MERCHANT_CURRENCY_INVALID` | Las cuentas de comercio solo pueden operar en BOB. *(inalcanzable desde que el alta de comercio no recibe moneda; se conserva por compatibilidad)* |
| `CURRENCY_MISMATCH` | La moneda enviada no corresponde a la cuenta del cliente. |
| `EXCHANGE_RATE_NOT_FOUND` | No existe un tipo de cambio vigente para el par de monedas. |
| `INSERT_FAILED` | No se pudo crear la cuenta. Intente nuevamente. |
| `MOVEMENT_FAILED` | No se pudo aplicar el movimiento. Intente nuevamente. |
| `PAYMENT_FAILED` | No se pudo ejecutar el pago. Intente nuevamente. |
| `PAYMENT_NOT_FOUND` | La operación solicitada no existe. |

### Errores de negocio — Auth

| Código | Mensaje |
|---|---|
| `INVALID_CREDENTIALS` | Usuario o contraseña incorrectos. |
| `USER_INACTIVE` | El usuario se encuentra inactivo. |
| `USER_LOCKED` | El usuario está bloqueado temporalmente por intentos fallidos. |
| `USER_DUPLICATE` | Ya existe un usuario con ese nombre. |
| `USER_NOT_FOUND` | El usuario solicitado no existe. |
| `INSERT_FAILED` | No se pudo crear el usuario. Intente nuevamente. |
| `DELETE_FAILED` | No se pudo eliminar el usuario: no existe o ya fue eliminado. |
| `CLIENT_NOT_FOUND` | El cliente indicado no existe. |
| `CLIENT_INACTIVE` | El cliente indicado se encuentra inactivo. |
| `CLIENT_ALREADY_HAS_USER` | El cliente ya tiene un usuario asignado. |

### Errores de validación

Cuando falla el validador, la respuesta es `VAL001` y el detalle viaja por campo:

```json
{
  "status_code": "VAL001",
  "message": "Se encontraron errores de validación.",
  "errors": [
    { "field": "Amount",         "message": "El monto debe ser mayor a cero." },
    { "field": "IdempotencyKey", "message": "La clave de idempotencia es requerida." }
  ]
}
```

### Códigos de transporte gRPC

Se usan solo para lo que de verdad es de transporte. Todo lo demás viaja como
`OK` con su `status_code`.

| `StatusCode` | Cuándo |
|---|---|
| `UNAUTHENTICATED` | Falta la metadata `authorization` o no empieza con `Bearer ` |
| `PERMISSION_DENIED` | El token no identifica a un cliente en un endpoint 🔑, o el rol no habilita la operación |
| `INTERNAL` | Falla no controlada del servidor |

---

## 7. Códigos internos de los procedimientos

No se ven desde el contrato, pero explican qué error devuelve el servicio. Los
retornan `pay.EJECUTAR_PAGO_QR` y `pay.APLICAR_MOVIMIENTO`; un valor **positivo**
es el id del asiento aplicado.

| Retorno | Constante | Se traduce a |
|---:|---|---|
| `-1` | `ACCOUNT_NOT_FOUND` | `ACCOUNT_NOT_FOUND` |
| `-2` | `ACCOUNT_INACTIVE` | `ACCOUNT_INACTIVE` |
| `-3` | `INSUFFICIENT_FUNDS` | `INSUFFICIENT_FUNDS` |
| `-4` | `MERCHANT_ACCOUNT_NOT_FOUND` | `MERCHANT_ACCOUNT_NOT_FOUND` |
| `-5` | `MERCHANT_ACCOUNT_INACTIVE` | `MERCHANT_ACCOUNT_INACTIVE` |
| `-6` | `QR_ALREADY_USED` | `QR_ALREADY_USED` |
| `-7` | `CURRENCY_MISMATCH` | `CURRENCY_MISMATCH` |
| `-8` | `CONVERSION_MISMATCH` | `INVALID_AMOUNT` |
| `-9` | `IDEMPOTENCY_CONFLICT` | `DUPLICATE_TRANSACTION` |

`pay.INSERT_CUENTA` devuelve `-1` cuando el titular ya tiene una cuenta en esa
moneda → `ACCOUNT_DUPLICATE`.

Los tres procedimientos reciben además `@AUDITORIA_TRAZA_VC` y
`@AUDITORIA_USUARIO_IT`, con default `NULL`. No afectan el resultado: son los
datos con los que dejan la fila de `aud.AUDITORIA` dentro de su misma
transacción.

---

## 8. Auditoría

Cada llamada deja rastro en dos tablas que responden preguntas distintas.

| | `aud.BITACORA` | `aud.AUDITORIA` |
|---|---|---|
| Responde | ¿Quién llamó a qué, cuándo, desde dónde? | ¿Cómo llegó este dato a tener este valor? |
| Registra | Toda llamada, **consultas incluidas** | Solo lo que cambió |
| La escribe | `AuditInterceptor`, al terminar el RPC | El stored procedure, dentro de su transacción |
| Si la operación falla | Se escribe igual, con `ERROR` | No queda nada: el rollback se la lleva |
| Si escribirla falla | Se registra un warning y la respuesta sigue | Se deshace la operación |

Se enlazan por **TraceId**, que es el mismo que imprime Serilog en cada línea de
log y el que usa OpenTelemetry. Desde una fila de auditoría se puede llegar a la
llamada que la causó, a sus logs y a su traza distribuida.

Un pago QR deja **una** fila de bitácora y **cuatro** de auditoría: los dos
asientos y los dos saldos.

```sql
-- Toda la historia de una cuenta, y quién la produjo
SELECT a.AUDI_FECHA_DT, a.AUDI_OPERACION_VC,
       JSON_VALUE(a.AUDI_DATO_ANTERIOR_NV, '$.CUEN_SALDO_DE') AS SaldoAntes,
       JSON_VALUE(a.AUDI_DATO_NUEVO_NV,    '$.CUEN_SALDO_DE') AS SaldoDespues,
       b.BITA_METODO_VC, b.BITA_USUARIO_VC
FROM   aud.AUDITORIA a
LEFT   JOIN aud.BITACORA b ON b.BITA_ID_IT = a.AUDI_BITACORA_ID_IT
WHERE  a.AUDI_TABLA_VC = 'CUENTA' AND a.AUDI_LLAVE_VC = '1'
ORDER  BY a.AUDI_FECHA_DT;
```

Las contraseñas y los tokens nunca llegan a `BITA_PETICION_NV`: el interceptor
los enmascara antes de escribir.

Las bajas se registran como `DELETE` aunque físicamente sean un `UPDATE` que
sella la fecha de eliminación. Lo que se guarda es la intención, no la mecánica.

---

## 9. Cómo consumir los contratos

**Postman** · New → gRPC, URL `localhost:52131`, TLS apagado. Importar el
`.proto` agregando `YastaCoin/Shared/Core.Domain/Grpc/Protobuf/` como *import
path*, o la importación falla por el `import "base_response.proto"`.

**Flutter / Dart**

```bash
protoc --dart_out=grpc:lib/generated \
  -I Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos \
  -I YastaCoin/Shared/Core.Domain/Grpc/Protobuf \
  accounts_movements.proto base_response.proto
```

**C#** · agregar el `.proto` como `<Protobuf ... GrpcServices="Client" />` con
`AdditionalImportDirs` apuntando a la misma carpeta.

No hay *server reflection* habilitada: el `.proto` es obligatorio para generar
el cliente.
