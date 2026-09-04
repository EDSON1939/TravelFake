# Endpoints gRPC

| Servicio | Paquete | Puerto local | Docker | Contrato |
|---|---|---|---|---|
| Auth | `auth` | 52127 http · 52126 https | 52127 | [`auth.proto`](../Microservices/Auth/Auth.Api/Protos/auth.proto) |
| AccountsAndMovements | `accounts_movements` | 52131 http · 52130 https | 52131 | [`accounts_movements.proto`](../Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos/accounts_movements.proto) |

Los dos importan [`base_response.proto`](../YastaCoin/Shared/Core.Domain/Grpc/Protobuf/base_response.proto):
esa carpeta es *import path* obligatorio para compilarlos.

---

## 1. El sobre de respuesta

Todas las respuestas comparten la misma forma. Nunca hay un `data` suelto.

| Campo | Cuándo viene | Contiene |
|---|---|---|
| `status_code` | siempre | `SUC000` si salió bien; si no, el código del error |
| `message` | siempre | Texto en español, listo para mostrar |
| `data` | solo con `SUC000` | El resultado; su tipo cambia por endpoint |
| `errors` | solo con `VAL001` | Un elemento por campo inválido: `{ field, message }` |
| `exception` | solo con `ERR001` | Detalle de la excepción |

**Se lee el `status_code`, no el canal de transporte.** Un error de negocio
—saldo insuficiente, QR vencido— viaja como respuesta gRPC `OK` con su código:
son resultados esperados, no fallas de comunicación. Los `StatusCode` de gRPC
quedan para lo que sí es transporte (§5).

| Convención | Motivo |
|---|---|
| Decimales como **string** (`"139.20"`) | Un `double` redondearía montos y tipos de cambio, y eso quedaría grabado en el asiento |
| Fechas como **string ISO 8601** | proto3 no tiene `null` para escalares |
| `""` y `0` significan **sin valor** | Misma razón |

**Autenticación** · todos los RPC exigen metadata `authorization: Bearer <jwt>`.
El servicio **lee** el token pero no verifica su firma —eso es del Gateway—; lo
que sí resuelve es la **identidad**, que no se delega: un token auténtico podría
traer en el cuerpo el id de otro cliente. Los endpoints 🔑 sacan el cliente del
claim `client_id` y no lo reciben como parámetro.

Claims que emite Auth: `sub`, `client_id`, `username`, `role`, `permissions`,
`jti`, `exp`.

---

## 2. Auth

| RPC | Request | Response `data` | Errores |
|---|---|---|---|
| `Login` | `username`, `password` | `access_token`, `expires_at`, `expires_in`, `user_id`, `client_id` (0 = operador interno), `username`, `full_name`, `role` | `INVALID_CREDENTIALS`, `USER_INACTIVE`, `USER_LOCKED` |
| `CreateUser` | `username` (único), `password`, `document` (vacío = ADMIN interno), `full_name`, `role`: `CLIENTE`·`AGENTE`·`ADMIN` | `int64` user_id | `USER_DUPLICATE`, `CLIENT_NOT_FOUND`, `CLIENT_INACTIVE`, `CLIENT_ALREADY_HAS_USER`, `INSERT_FAILED` |
| `DeleteUser` | `user_id` | `int64` id eliminado | `USER_NOT_FOUND`, `DELETE_FAILED` |

`Login` es el único endpoint anónimo. Usuario inexistente y contraseña incorrecta
devuelven **el mismo error**: distinguirlos le diría a un atacante qué usuarios
existen. Cada fallo suma al contador; al llegar a `Jwt:MaxFailedAttempts` el
usuario queda bloqueado `Jwt:LockMinutes`.

La contraseña se guarda como PBKDF2 `iteraciones.salt.hash` y nunca sale de Auth
—tampoco a la auditoría—. `DeleteUser` es baja **lógica**: un libro de auditoría
sin el actor no sirve.

---

## 3. Cuentas

No hay un endpoint genérico con discriminador de titular: cliente y comercio
tienen reglas distintas y cada uno tiene su operación, así el contrato **no
permite expresar** combinaciones inválidas en vez de rechazarlas en ejecución.

| RPC | Request | Response `data` | Errores |
|---|---|---|---|
| `CreateClientAccount` | `client_id` >0, `coin_code` (máx 10, alfanum.), `initial_balance` >=0 | `int64` id | `CUSTOMER_NOT_FOUND`, `CUSTOMER_INACTIVE`, `CURRENCY_NOT_SUPPORTED`, `ACCOUNT_DUPLICATE`, `INSERT_FAILED` |
| `CreateCommerceAccount` | `commerce_id` >0, `initial_balance` >=0 | `int64` id | `COMMERCE_NOT_FOUND`, `COMMERCE_INACTIVE`, `CURRENCY_NOT_SUPPORTED`, `ACCOUNT_DUPLICATE`, `INSERT_FAILED` |
| `GetAccount` | `number` (máx 20) | `AccountDataPb` | `ACCOUNT_NOT_FOUND` |
| `GetMyAccount` 🔑 | `coin_code` | `AccountDataPb` | `ACCOUNT_NOT_FOUND` |
| `GetMyAccounts` 🔑 | `only_active` | `repeated AccountDataPb` | — |
| `GetCommerceAccount` | `commerce_id` >0 | `AccountDataPb` | `ACCOUNT_NOT_FOUND` |
| `GetCommerceAccounts` | `commerce_id` >0, `only_active` | `repeated AccountDataPb` | — |

`CreateCommerceAccount` **no recibe moneda**: el comercio boliviano cobra en BOB
por regla del negocio, y al no ser parámetro `COMMERCE_CURRENCY_INVALID` dejó de
poder ocurrir.

El número de cuenta se deriva del id: `QR` + 10 dígitos → `QR0000000001`. El
saldo inicial no se escribe directo en la cuenta: entra como asiento `CREDITO` de
apertura, para que el libro mayor explique el saldo desde el principio.

Una lista vacía es `SUC000`, no un error: "no tenés cuentas" es una respuesta
válida.

> **Pendiente**: `initial_balance` acredita saldo real, así que las dos altas
> deberían exigir rol `AGENTE` o `ADMIN`. Hoy no hay control de rol.

`AccountDataPb`: `account_id`, `number`, `account_type` (`CLIENT`·`COMMERCE`),
`holder_id`, `coin_id`, `coin_code`, `balance`, `is_active`, `created_at`,
`updated_at`, `deleted_at`.

---

## 4. Movimientos y pagos

| RPC | Request | Response `data` |
|---|---|---|
| `ApplyMovement` | `account_number`, `type`: `CREDITO`·`DEBITO`, `amount` >0, `reference` (64), `idempotency_key` (60), `description` (250) | `movement_id`, `account_number`, `balance` |
| `ExecuteQrPayment` 🔑 | `qr_code` (64), `amount` >0, `currency_code`, `idempotency_key` (60), `description` | `PaymentDataPb` |
| `GetPayment` | `transaction_code` (36) **o** `idempotency_key` (64) | `PaymentDataPb` |
| `GetMovements` | `account_number`, `page_number` >0, `page_size` 1-100 | `repeated MovementDataPb` |
| `GetMyHistory` 🔑 | `date_from`, `date_to` (inclusive, >= from), `status`, `page_number`, `page_size` | `repeated MovementDataPb` |
| `GetCommerceHistory` | igual + `commerce_id` >0 | `repeated MovementDataPb` |

`ApplyMovement` es para créditos y débitos sueltos —recargas, ajustes, reversas—.
**El pago QR no pasa por acá**: tiene su propia operación atómica de dos patas.
Errores: `ACCOUNT_NOT_FOUND`, `ACCOUNT_INACTIVE`, `INSUFFICIENT_FUNDS`,
`DUPLICATE_TRANSACTION`, `MOVEMENT_FAILED`.

`GetMyHistory` es del **titular**: si tiene cuentas en varias monedas salen todas
juntas. `GetPayment` se resuelve solo con el libro mayor —no llama a Comercios ni
a Monedas—, así una consulta de algo ya ocurrido no se cae porque otro servicio
esté abajo; la contrapartida es que devuelve `commerce_name` vacío.

### Idempotencia

Las dos operaciones son idempotentes por `idempotency_key`. Repetir la llamada
**con los mismos datos** devuelve el mismo resultado sin volver a mover saldo:
`ExecuteQrPayment` responde `SUC000` con **`is_duplicate: true`** y el saldo que
quedó *entonces*, no el de hoy.

Cuenta como reintento solo si coinciden cuenta, tipo y monto. Reusar una clave
para otra operación devuelve `DUPLICATE_TRANSACTION`, no un éxito que ocultaría
que el movimiento nuevo nunca se aplicó.

> El límite de 60 sobre una columna de 64 no es arbitrario: el asiento del
> comercio reutiliza la clave con el sufijo `-IN`.

### `ExecuteQrPayment` — ejemplo

```json
// request  ·  metadata: authorization: Bearer <jwt del cliente 1>
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
    "commerce_id": 55, "commerce_name": "Café Central",
    "commerce_account_number": "QR0000000002", "qr_code": "QR-BO-0001",
    "original_amount": "20", "original_currency": "USD", "exchange_rate": "6.96",
    "converted_amount": "139.20", "target_currency": "BOB",
    "client_balance": "480.00", "status": "COMPLETED",
    "idempotency_key": "TX-2026-000001", "is_duplicate": false
  }
}
```

**Errores, en el orden en que se evalúan**

| Código | Cuándo |
|---|---|
| `DUPLICATE_TRANSACTION` | La clave ya existe con otros datos |
| `CUSTOMER_NOT_FOUND` · `CUSTOMER_INACTIVE` | Cliente |
| `QR_NOT_FOUND` · `QR_ALREADY_USED` · `QR_EXPIRED` · `QR_INACTIVE` | QR |
| `COMMERCE_NOT_FOUND` · `COMMERCE_INACTIVE` | Comercio emisor |
| `CURRENCY_NOT_SUPPORTED` | Moneda fuera del catálogo o inactiva |
| `CURRENCY_MISMATCH` | La moneda del QR no coincide con la del comercio |
| `EXCHANGE_RATE_NOT_FOUND` | Sin cotización vigente para el par |
| `INVALID_AMOUNT` | El QR tiene monto fijo y el convertido no coincide. Monto `0` = QR abierto |
| `ACCOUNT_NOT_FOUND` · `COMMERCE_ACCOUNT_NOT_FOUND` | Falta la cuenta de algún lado |
| `ACCOUNT_INACTIVE` · `COMMERCE_ACCOUNT_INACTIVE` | Cuenta dada de baja |
| `INSUFFICIENT_FUNDS` | Lo decide el motor con la fila bloqueada, no el handler |
| `PAYMENT_FAILED` | Cualquier otro fallo del procedimiento |

---

## 5. Códigos

**Transversales** · `SUC000` éxito · `VAL001` validación, detalle en `errors[]` ·
`ERR001` error no controlado, detalle en `exception` · `AUT001` token ausente o
inválido · `AUT002` firma o vigencia · `AUT003` sin permiso.

**Los doce que exige el reto**

| Código | Mensaje |
|---|---|
| `CUSTOMER_NOT_FOUND` | El cliente indicado no existe. |
| `CUSTOMER_INACTIVE` | El cliente indicado se encuentra inactivo. |
| `COMMERCE_NOT_FOUND` | El comercio indicado no existe. |
| `COMMERCE_INACTIVE` | El comercio indicado se encuentra inactivo. |
| `QR_NOT_FOUND` | El código QR no existe. |
| `QR_EXPIRED` | El código QR expiró. |
| `QR_ALREADY_USED` | El código QR ya fue utilizado. |
| `QR_INACTIVE` | El código QR no se encuentra activo. |
| `INSUFFICIENT_FUNDS` | El saldo de la cuenta es insuficiente. |
| `CURRENCY_NOT_SUPPORTED` | La moneda indicada no está soportada. |
| `INVALID_AMOUNT` | El monto de la operación no es válido. |
| `DUPLICATE_TRANSACTION` | La clave de idempotencia ya fue usada con datos distintos. |

**Propios de AccountsAndMovements** · `ACCOUNT_NOT_FOUND` · `ACCOUNT_INACTIVE` ·
`ACCOUNT_DUPLICATE` (el titular ya tiene cuenta en esa moneda) ·
`COMMERCE_ACCOUNT_NOT_FOUND` · `COMMERCE_ACCOUNT_INACTIVE` · `CURRENCY_MISMATCH`
· `EXCHANGE_RATE_NOT_FOUND` · `INSERT_FAILED` · `MOVEMENT_FAILED` ·
`PAYMENT_FAILED` · `PAYMENT_NOT_FOUND`. Más `COMMERCE_CURRENCY_INVALID`,
inalcanzable desde que el alta de comercio no recibe moneda; se conserva por
compatibilidad.

**Propios de Auth** · `INVALID_CREDENTIALS` · `USER_INACTIVE` · `USER_LOCKED` ·
`USER_DUPLICATE` · `USER_NOT_FOUND` · `INSERT_FAILED` · `DELETE_FAILED` ·
`CLIENT_NOT_FOUND` · `CLIENT_INACTIVE` · `CLIENT_ALREADY_HAS_USER`.

**Transporte gRPC** — solo para lo que de verdad es transporte:
`UNAUTHENTICATED` (falta la metadata o no empieza con `Bearer `),
`PERMISSION_DENIED` (token sin cliente en un endpoint 🔑, o rol insuficiente),
`INTERNAL` (falla no controlada).

**Internos de los procedimientos** — un valor positivo es el id del asiento:

| Retorno | Se traduce a | Retorno | Se traduce a |
|---|---|---|---|
| `-1` | `ACCOUNT_NOT_FOUND` | `-6` | `QR_ALREADY_USED` |
| `-2` | `ACCOUNT_INACTIVE` | `-7` | `CURRENCY_MISMATCH` |
| `-3` | `INSUFFICIENT_FUNDS` | `-8` | `INVALID_AMOUNT` |
| `-4` | `COMMERCE_ACCOUNT_NOT_FOUND` | `-9` | `DUPLICATE_TRANSACTION` |
| `-5` | `COMMERCE_ACCOUNT_INACTIVE` | | |

`commerce.INSERT_CUENTA` devuelve `-1` si el titular ya tiene cuenta en esa
moneda → `ACCOUNT_DUPLICATE`.

---

## 6. Auditoría

Dos tablas que responden preguntas distintas.

| | `aud.BITACORA` | `aud.AUDITORIA` |
|---|---|---|
| Responde | ¿Quién llamó a qué, cuándo, desde dónde? | ¿Cómo llegó este dato a este valor? |
| Registra | Toda llamada, **consultas incluidas** | Solo lo que cambió |
| La escribe | `AuditInterceptor`, al terminar el RPC | El stored procedure, dentro de su transacción |
| Si la operación falla | Se escribe igual, con `ERROR` | No queda nada: el rollback se la lleva |
| Si escribirla falla | Warning, y la respuesta sigue | Se deshace la operación |

Se enlazan por **TraceId**, el mismo que imprime Serilog y usa OpenTelemetry:
desde una fila de auditoría se llega a la llamada, a sus logs y a su traza. Un
pago QR deja una fila de bitácora y cuatro de auditoría —dos asientos, dos
saldos—.

Los tres procedimientos reciben `@AUDITORIA_TRAZA_VC` y `@AUDITORIA_USUARIO_IT`
con default `NULL`; no afectan el resultado.

Contraseñas y tokens nunca llegan a la bitácora: el interceptor los enmascara
antes de escribir, y los SP de Auth enumeran columnas para dejar el hash fuera de
la auditoría. Las bajas se registran como `DELETE` aunque físicamente sean un
`UPDATE`: se guarda la intención, no la mecánica.

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

---

## 7. Consumir los contratos

No hay *server reflection*: el `.proto` es obligatorio para generar el cliente, y
en los tres casos hay que sumar `YastaCoin/Shared/Core.Domain/Grpc/Protobuf/`
como import path o falla el `import "base_response.proto"`.

**Postman** · New → gRPC, URL `localhost:52131`, TLS apagado.

**Flutter / Dart**

```bash
protoc --dart_out=grpc:lib/generated \
  -I Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos \
  -I YastaCoin/Shared/Core.Domain/Grpc/Protobuf \
  accounts_movements.proto base_response.proto
```

**C#** · el `.proto` como `<Protobuf ... GrpcServices="Client" />` con
`AdditionalImportDirs` a la misma carpeta.
