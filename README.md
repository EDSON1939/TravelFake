# YastaCoin — RETO 1 · QR Bolivia

Backend que permite a un **cliente extranjero pagar un QR boliviano** con fondos
en su propia moneda. El comercio cobra en BOB; la conversión, la validación de
saldo y el asiento contable ocurren en una sola operación atómica.

| | |
|---|---|
| Reto | RETO 1 — QR Bolivia |
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
correr en local, depurar o usar Postman: **[`docs/EJECUCION.md`](docs/EJECUCION.md)**.

---

## Entregables

| Entregable | Dónde |
|---|---|
| **Código fuente** | [`Microservices/`](Microservices) · [`YastaCoin/Shared/`](YastaCoin/Shared) |
| **`.proto`** | [`accounts_movements.proto`](Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos/accounts_movements.proto) · [`auth.proto`](Microservices/Auth/Auth.Api/Protos/auth.proto) · [`base_response.proto`](YastaCoin/Shared/Core.Domain/Grpc/Protobuf/base_response.proto) |
| **Scripts SQL / estructura de BD** | [`Database/`](Database) |
| **Tests** | [`Tests/`](Microservices/AccountsAndMovements/Tests) · [`docs/validacion.sql`](docs/validacion.sql) |
| **README** | este archivo · [del microservicio](Microservices/AccountsAndMovements/README.md) |
| **Diagrama de flujo** | [`docs/diagramas/`](docs/diagramas) — XML para draw.io |
| **Documentación de endpoints gRPC** | [`docs/word/`](docs/word) (Word) · [`docs/ENDPOINTS.md`](docs/ENDPOINTS.md) (fuente) |
| **Instrucciones de ejecución** | [`docs/word/`](docs/word) (Word) · [`docs/EJECUCION.md`](docs/EJECUCION.md) (fuente) |
| **Ejemplos de requests/responses** | [`docs/ENDPOINTS.md`](docs/ENDPOINTS.md) — uno por endpoint |

### Documentos Word

Los tres documentos formales de la entrega, con portada, control de versiones,
índice automático, encabezado y pie numerado.

| Documento | Contenido |
|---|---|
| `01 - Documentación de Endpoints gRPC.docx` | Los 16 RPC con atributos, respuestas, ejemplos y el diccionario completo de errores |
| `02 - Manual de Instalación y Ejecución.docx` | Despliegue con Docker y en local, configuración, pruebas y problemas frecuentes |
| `03 - Documento Técnico.docx` | Arquitectura, modelo de datos y las decisiones de concurrencia e idempotencia |

Se generan desde los Markdown de `docs/`, así que no se desincronizan:

```bash
powershell -ExecutionPolicy Bypass -File tools\Generar-Documentos.ps1
```

Al abrirlos, Word ofrece actualizar el índice. Si no lo pide, seleccionarlo y
pulsar **F9**.

### Diagramas

Cada archivo trae el XML listo para pegar en draw.io (**Extras → Edit
Diagram…**) y exportar como imagen. Las instrucciones están en la cabecera de
cada uno.

| Archivo | Qué muestra |
|---|---|
| [`01-flujo-pago-qr`](docs/diagramas/01-flujo-pago-qr.drawio.txt) | Los 15 pasos del pago, con cada punto de rechazo y su código de error |
| [`02-arquitectura`](docs/diagramas/02-arquitectura.drawio.txt) | Capas del microservicio y sus dependencias |
| [`03-modelo-datos`](docs/diagramas/03-modelo-datos.drawio.txt) | Las cuatro tablas y las restricciones que sostienen las garantías |
| [`04-concurrencia`](docs/diagramas/04-concurrencia.drawio.txt) | Dos pagos simultáneos sobre el mismo saldo |

---

## Microservicios

| Servicio | Puerto | Base | Estado |
|---|---|---|---|
| [AccountsAndMovements](Microservices/AccountsAndMovements) | 52131 | `DB_QRBolivia` | Completo |
| [Auth](Microservices/Auth) | 52127 | `DB_YastaCoinE` | Emite el JWT. Le faltan sus scripts SQL |
| Clientes · Comercios · QR · Monedas | — | — | No existen todavía. Se sustituyen con catálogos en memoria |

Mientras esos cuatro no estén, `UseFakeExternalServices` los resuelve con
[catálogos en memoria](Microservices/AccountsAndMovements/AccountsAndMovements.Infrastructure/ExternalServices/Fakes).
Apagar la bandera devuelve el cableado gRPC real sin tocar código.

---

## Las tres garantías del reto

### Consistencia del saldo — nunca negativo

Tres redes, de la más específica a la más general:

1. La validación de saldo **con la fila de la cuenta bloqueada** (`UPDLOCK,
   HOLDLOCK`), dentro de la transacción del pago. Es la que decide.
2. Los índices únicos `UQ_PAY_MOVIMIENTO_QR` y `UQ_PAY_MOVIMIENTO_IDEMPOTENCIA`,
   que resuelven las carreras que se cuelan entre la lectura y el `INSERT`.
3. `CK_PAY_CUENTA_SALDO >= 0`: si algo se escapara de todo lo anterior, el motor
   rechaza antes que dejar saldo negativo.

Por eso **el saldo no se valida en el handler**: una lectura desde C# no sirve
para decidir, porque entre esa lectura y el pago puede entrar otra operación.

### Idempotencia — no cobrar dos veces

La `idempotencyKey` identifica una operación **del sistema entero**, no de una
cuenta: el alcance es global. Repetirla con los mismos datos devuelve el
resultado original con `is_duplicate: true` —incluido el saldo de entonces—;
repetirla con datos distintos es una clave reusada y se rechaza con
`DUPLICATE_TRANSACTION`. Devolver el resultado anterior en ese caso le mostraría
a un cliente el pago de otro.

### Identidad — nadie opera sobre la cuenta ajena

El `client_id` **no viaja en el request**: sale del claim del JWT. Si viajara en
el cuerpo, cualquiera con un token válido podría pagar desde la cuenta de otro.

---

## Auditoría

Dos tablas que responden preguntas distintas, y no son lo mismo:

| | `aud.BITACORA` | `aud.AUDITORIA` |
|---|---|---|
| Responde | ¿Quién llamó a qué, cuándo, desde dónde? | ¿Cómo llegó este dato a tener este valor? |
| Registra | Toda llamada, consultas incluidas | Solo lo que cambió, con el JSON de antes y después |
| La escribe | El interceptor gRPC, al terminar | El stored procedure, **dentro de su transacción** |

Se enlazan por **TraceId**, el mismo que imprime Serilog y usa OpenTelemetry.
Una operación que hace rollback no deja auditoría: no hay cambio sin rastro ni
rastro sin cambio. Detalle en [`docs/ENDPOINTS.md §8`](docs/ENDPOINTS.md).

---

## Estructura

```
├── Database/
│   ├── AccountsAndMovements/     pay.CUENTA · pay.MOVIMIENTO · 3 stored procedures
│   └── Core/                     aud.BITACORA · aud.AUDITORIA, para todos los servicios
├── Microservices/
│   ├── AccountsAndMovements/     Domain · Application · Infrastructure · Api · Tests
│   └── Auth/                     misma estructura
├── YastaCoin/Shared/             Core.Domain · Core.Infrastructure · Core.ShareKernel · Core.AuditTrail
├── docs/                         Endpoints · ejecución · diagramas · validación SQL
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

100 pruebas, sin necesidad de base de datos.

Lo que vive en SQL —bloqueos, transacciones, restricciones, auditoría— no lo
cubren las pruebas unitarias. Para eso está [`docs/validacion.sql`](docs/validacion.sql),
que corre contra una base temporal y comprueba 19 puntos, incluido que **todo
saldo sea reconstruible sumando sus asientos**. El procedimiento está en
[`docs/EJECUCION.md`](docs/EJECUCION.md#tests).
