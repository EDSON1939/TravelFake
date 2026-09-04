# YastaCoin — RETO 1 · QR Bolivia

Backend que permite a un **cliente extranjero pagar un QR boliviano** con fondos
en su propia moneda. El comercio cobra en BOB; la conversión, la validación de
saldo y el asiento contable ocurren en una sola operación atómica.

| | |
|---|---|
| Equipo | Edson · Mariel · Rene |
| Stack | C# · ASP.NET Core · gRPC · Protocol Buffers · SQL Server |
| Persistencia | Dapper para leer, stored procedures para escribir. **Sin Entity Framework** |
| Tests | 100 pruebas unitarias + 19 validaciones contra el motor |

```
Cliente extranjero  →  escanea QR  →  gRPC  →  valida  →  convierte  →  debita
                                                              ↓
                                                     acredita al comercio
                                                              ↓
                                                     historial + auditoría
```

---

## Empezar

```bash
cp .env.example .env
```

```bash
docker compose up -d --build
```

Levanta SQL Server, crea los esquemas y arranca los dos microservicios. Para
correr en local, depurar o usar Postman:
**[`docs/EJECUCION.md`](docs/EJECUCION.md)**.

| Servicio | Puerto | Base | Estado |
|---|---|---|---|
| [AccountsAndMovements](Microservices/AccountsAndMovements) | 52131 | `DB_QRBolivia` | Completo |
| [Auth](Microservices/Auth) | 52127 | `DB_YastaCoinE` | Completo |
| Clientes · Comercios · QR · Monedas | — | — | No existen todavía |

Mientras esos cuatro no estén, `UseFakeExternalServices: true` los resuelve con
catálogos en memoria —
[los de AccountsAndMovements](Microservices/AccountsAndMovements/AccountsAndMovements.Infrastructure/ExternalServices/Fakes)
y [el de Auth](Microservices/Auth/Auth.Infrastructure/ExternalServices/Fakes)— y
cada servicio corre solo. Apagar la bandera devuelve el cableado gRPC real sin
tocar código.

---

## Entregables

| Entregable | Dónde |
|---|---|
| Código fuente | [`Microservices/`](Microservices) · [`YastaCoin/Shared/`](YastaCoin/Shared) |
| `.proto` | [`accounts_movements`](Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos/accounts_movements.proto) · [`auth`](Microservices/Auth/Auth.Api/Protos/auth.proto) · [`base_response`](YastaCoin/Shared/Core.Domain/Grpc/Protobuf/base_response.proto) |
| Scripts SQL y estructura de BD | [`Database/`](Database) |
| Tests | [`Tests/`](Microservices/AccountsAndMovements/Tests) · [`docs/validacion.sql`](docs/validacion.sql) |
| Endpoints gRPC + ejemplos | [`docs/ENDPOINTS.md`](docs/ENDPOINTS.md) · Word en [`docs/word/`](docs/word) |
| Instrucciones de ejecución | [`docs/EJECUCION.md`](docs/EJECUCION.md) · Word en [`docs/word/`](docs/word) |
| Diagramas | [`docs/diagramas/`](docs/diagramas) — archivos `.drawio` |

**Word** — los tres documentos formales, con portada, control de versiones,
índice automático y pie numerado: `01` los 16 RPC con atributos, respuestas,
ejemplos y diccionario de errores; `02` despliegue, configuración y problemas
frecuentes; `03` arquitectura, modelo de datos y las decisiones de concurrencia
e idempotencia. Se generan desde los Markdown de `docs/`, así que no se
desincronizan:

```bash
powershell -ExecutionPolicy Bypass -File tools\Generar-Documentos.ps1
```

Al abrirlos, Word ofrece actualizar el índice; si no lo pide, seleccionarlo y
pulsar **F9**.

**Diagramas** — ocho archivos `.drawio` que se abren directo en draw.io (*File →
Open From → Device…*, o arrastrarlos a la ventana):
[`01`](docs/diagramas/01-flujo-pago-qr.drawio) los 15 pasos del pago con cada
punto de rechazo y su código,
[`02`](docs/diagramas/02-arquitectura.drawio) capas y dependencias,
[`03`](docs/diagramas/03-modelo-datos.drawio) las cinco tablas de las dos bases
con las restricciones que sostienen las garantías,
[`04`](docs/diagramas/04-concurrencia.drawio) dos pagos simultáneos sobre el
mismo saldo,
[`05`](docs/diagramas/05-pipeline-grpc.drawio) lo que atraviesa una llamada
antes de llegar al handler,
[`06`](docs/diagramas/06-alta-de-cuenta.drawio) el alta de cuenta de cliente y
de comercio,
[`07`](docs/diagramas/07-aplicar-movimiento.drawio) `ApplyMovement` y el SP que
mueve el saldo, y
[`08`](docs/diagramas/08-auditoria.drawio) por qué hay dos tablas de auditoría.
La explicación de cada uno está en
[`docs/diagramas/README.md`](docs/diagramas/README.md).

---

## Las tres garantías del reto

**Consistencia del saldo — nunca negativo.** Tres redes, de la más específica a
la más general: la validación de saldo **con la fila bloqueada** (`UPDLOCK,
HOLDLOCK`) dentro de la transacción del pago, que es la que decide; los índices
únicos `UQ_COMMERCE_MOVIMIENTO_QR` y `UQ_COMMERCE_MOVIMIENTO_IDEMPOTENCIA`, que
resuelven las carreras entre la lectura y el `INSERT`; y
`CK_COMMERCE_CUENTA_SALDO >= 0`, que rechaza antes que dejar saldo negativo. Por
eso **el saldo no se valida en el handler**: una lectura desde C# no sirve para
decidir, porque entre esa lectura y el pago puede entrar otra operación.

**Idempotencia — no cobrar dos veces.** La `idempotencyKey` identifica una
operación **del sistema entero**, no de una cuenta. Repetirla con los mismos
datos devuelve el resultado original con `is_duplicate: true`, incluido el saldo
de entonces; repetirla con datos distintos es una clave reusada y se rechaza con
`DUPLICATE_TRANSACTION` — devolver el resultado anterior le mostraría a un
cliente el pago de otro.

**Identidad — nadie opera sobre la cuenta ajena.** El `client_id` **no viaja en
el request**: sale del claim del JWT. Si viajara en el cuerpo, cualquiera con un
token válido podría pagar desde la cuenta de otro.

---

## Auditoría

| | `aud.BITACORA` | `aud.AUDITORIA` |
|---|---|---|
| Responde | ¿Quién llamó a qué, cuándo, desde dónde? | ¿Cómo llegó este dato a este valor? |
| Registra | Toda llamada, consultas incluidas | Solo lo que cambió, con el JSON de antes y después |
| La escribe | El interceptor gRPC, al terminar | El stored procedure, **dentro de su transacción** |

Se enlazan por **TraceId**, el mismo que imprime Serilog y usa OpenTelemetry. Una
operación que hace rollback no deja auditoría: no hay cambio sin rastro ni rastro
sin cambio. Detalle en [`docs/ENDPOINTS.md §6`](docs/ENDPOINTS.md).

---

## Estructura

```
├── Database/
│   ├── AccountsAndMovements/   commerce.CUENTA · commerce.MOVIMIENTO · 3 SP
│   ├── Auth/                   commerce.USUARIO · 3 SP
│   └── Core/                   aud.BITACORA · aud.AUDITORIA, para todos los servicios
├── Microservices/
│   ├── AccountsAndMovements/   Domain · Application · Infrastructure · Api · Tests
│   └── Auth/                   misma estructura
├── YastaCoin/Shared/           Core.Domain · Core.Infrastructure · Core.ShareKernel · Core.AuditTrail
├── docs/                       Endpoints · ejecución · diagramas · validación SQL
├── docker-compose.yml
└── .env.example
```

Clean Architecture + CQRS con MediatR, validación con FluentValidation y
respuestas `BaseResponse<T>`, tal como define el proyecto base.

---

## Verificación

```bash
dotnet test YastaCoin.sln
```

100 pruebas, sin necesidad de base de datos. Lo que vive en SQL —bloqueos,
transacciones, restricciones, auditoría— no lo cubren: para eso está
[`docs/validacion.sql`](docs/validacion.sql), que comprueba 19 puntos contra una
base temporal, incluido que **todo saldo sea reconstruible sumando sus
asientos**. El procedimiento está en
[`docs/EJECUCION.md`](docs/EJECUCION.md#tests).
