# Instrucciones de ejecución

Dos caminos. **Docker** levanta todo con un comando y es el recomendado para
revisar el proyecto; **local** hace falta para depurar desde Visual Studio.

| | Docker | Local |
|---|---|---|
| Requisitos | Docker Desktop | .NET 8 SDK · SQL Server (Express sirve) · sqlcmd |
| Base de datos | La crea y la puebla el compose | Hay que crearla a mano |
| Puertos | 52127 · 52131 · 1433 | 52127 · 52131 |
| Sirve para | Demo, revisión, entrega | Depurar, correr tests |

---

## Camino A — Docker

```bash
cp .env.example .env
```

Editar `.env`: `MSSQL_SA_PASSWORD` necesita 8+ caracteres con mayúsculas,
minúsculas, números y símbolos o la imagen de SQL Server no arranca;
`JWT_TOKEN_SECRET` necesita 32+ o Auth falla al emitir el primer token.
`.gitignore` ya excluye `*.env`.

```bash
docker compose up -d --build
```

La primera vez tarda: descarga las imágenes y compila los dos servicios. El
orden lo maneja el compose — `db-init` espera a que SQL Server responda una
consulta real, no solo a que abra el puerto.

```bash
docker compose ps
```

Los tres contenedores en `running` y `yastacoin-db-init` en `exited (0)`. Si
quedó en otro código, los scripts fallaron: `docker compose logs db-init`.

```bash
curl http://localhost:52131/
```

Devuelve `AccountsAndMovements gRPC Service — <fecha>`.

Para apagar, `docker compose down`. Los datos sobreviven en el volumen
`mssql-data`; `docker compose down -v` los borra también.

---

## Camino B — Local

### 1. Crear las bases

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "IF DB_ID('DB_QRBolivia') IS NULL CREATE DATABASE DB_QRBolivia; IF DB_ID('DB_YastaCoinE') IS NULL CREATE DATABASE DB_YastaCoinE;"
```

### 2. Esquema de QR Bolivia

En `DB_QRBolivia`. El orden importa: `CUENTA` antes que `MOVIMIENTO` por la FK,
y las tablas de `aud` antes que los procedimientos de `commerce`, porque estos
llaman a `aud.REGISTRAR_AUDITORIA`.

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d DB_QRBolivia -b -i "Database\Core\Tables\aud.BITACORA.sql,Database\Core\Tables\aud.AUDITORIA.sql,Database\Core\StoredProcedures\aud.INSERT_BITACORA.sql,Database\Core\StoredProcedures\aud.REGISTRAR_AUDITORIA.sql,Database\AccountsAndMovements\Tables\commerce.CUENTA.sql,Database\AccountsAndMovements\Tables\commerce.MOVIMIENTO.sql,Database\AccountsAndMovements\StoredProcedures\commerce.INSERT_CUENTA.sql,Database\AccountsAndMovements\StoredProcedures\commerce.APLICAR_MOVIMIENTO.sql,Database\AccountsAndMovements\StoredProcedures\commerce.EJECUTAR_PAGO_QR.sql"
```

### 3. Esquema de Auth

En `DB_YastaCoinE`. Necesita las dos cosas: la auditoría de `Core` y su tabla de
credenciales. Cada base lleva su propio `aud`.

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d DB_YastaCoinE -b -i "Database\Core\Tables\aud.BITACORA.sql,Database\Core\Tables\aud.AUDITORIA.sql,Database\Core\StoredProcedures\aud.INSERT_BITACORA.sql,Database\Core\StoredProcedures\aud.REGISTRAR_AUDITORIA.sql,Database\Auth\Tables\commerce.USUARIO.sql,Database\Auth\StoredProcedures\commerce.INSERT_USUARIO.sql,Database\Auth\StoredProcedures\commerce.DELETE_USUARIO.sql,Database\Auth\StoredProcedures\commerce.REGISTRAR_INTENTO_LOGIN.sql"
```

Los scripts son **idempotentes**: se pueden volver a ejecutar sin efecto
adicional y sin perder datos.

### 4. Levantar

```bash
dotnet run --project Microservices\AccountsAndMovements\AccountsAndMovements.Api
```

Queda escuchando en `http://localhost:52131` —texto plano, el que usa Postman— y
`https://localhost:52130`. Desde Visual Studio: abrir `YastaCoin.sln` y usar el
perfil de inicio múltiple de `YastaCoin.slnLaunch`.

---

## Configuración

Todo se sobreescribe por variable de entorno con el separador `__`:
`Database__ConnectionString`, `DevAuth__ClientId`, etc.

| Clave | Qué hace | Dónde |
|---|---|---|
| `Database:ConnectionString` | Cadena de conexión. **Cada servicio a su base**: AccountsAndMovements a `DB_QRBolivia`, Auth a `DB_YastaCoinE` | Los dos |
| `UseFakeExternalServices` | Valor **por defecto** de los cuatro servicios externos: `true` los resuelve con catálogos en memoria en vez de gRPC | Los dos |
| `Services:<Nombre>:UseFake` | Pisa lo anterior para **un** servicio (`Client`, `Commerce`, `Qr`, `Currency`). Es lo que permite tener Comercios real y los otros tres quemados | AccountsAndMovements |
| `Services:<Nombre>:BaseAddress` | Host y puerto del microservicio destino. `http://` si no tiene TLS; un certificado autofirmado se acepta igual | AccountsAndMovements |
| `DevAuth:Enabled` · `DevAuth:ClientId` | Completa el claim `client_id` cuando el token no lo trae. **Solo en Development y Docker** | AccountsAndMovements |
| `Jwt:TokenSecret` | Firma de los JWT. Nunca en el repositorio | Auth |
| `Serilog:*` · `OpenTelemetry:Enabled` | Logs, trazas y métricas | Los dos |

| `ASPNETCORE_ENVIRONMENT` | Archivo | Notas |
|---|---|---|
| `Development` | `appsettings.Development.json` | Rutas de log de Windows. DevAuth encendido. Fakes encendidos salvo Comercios, que va contra el microservicio real |
| `Docker` | `appsettings.Docker.json` | Rutas de Linux, sin Grafana Loki |
| *(producción)* | `appsettings.json` | Fakes y DevAuth apagados por omisión |

---

## Tests

```bash
dotnet test YastaCoin.sln
```

**100 pruebas**: 69 de AccountsAndMovements y 31 de Auth. No necesitan base de
datos — repositorios y servicios externos van sustituidos con NSubstitute.

### Validación contra el motor

Las unitarias no cubren lo que vive en SQL: bloqueos, transacciones,
restricciones y auditoría. Para eso se crea una base temporal, se le instala el
esquema con el comando del paso 2 cambiando el `-d`, y se corre
`docs/validacion.sql`:

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "IF DB_ID('DB_VALIDA_TMP') IS NOT NULL BEGIN ALTER DATABASE DB_VALIDA_TMP SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE DB_VALIDA_TMP; END; CREATE DATABASE DB_VALIDA_TMP;"
```

Verifica 19 puntos: saldos, idempotencia por los dos caminos, rechazo por saldo
insuficiente, que una operación rechazada no deje auditoría, el enlace bitácora ↔
auditoría, y que todo saldo sea reconstruible sumando sus asientos.

### Concurrencia

No se prueba desde Postman, que manda las peticiones en serie: se prueba con dos
sesiones simultáneas de `sqlcmd` sincronizadas con `WAITFOR TIME`, o con `ghz`
contra el endpoint gRPC. El escenario está en
[`04-concurrencia`](diagramas/04-concurrencia.drawio).

---

## Probar con Postman

1. **New → gRPC**, Server URL `localhost:52131`, TLS **apagado**.
2. Método → **Import a .proto file** →
   `Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos/accounts_movements.proto`
3. En el mismo diálogo, agregar como *import path* la carpeta
   `YastaCoin/Shared/Core.Domain/Grpc/Protobuf/`. Sin eso falla la importación:
   el proto hace `import "base_response.proto"`.
4. Pestaña **Metadata**, en todas las llamadas: `authorization` = `Bearer dev`.

No hay *server reflection*: el `.proto` es obligatorio. Los bodies de ejemplo
están en [`ENDPOINTS.md`](ENDPOINTS.md).

---

## Problemas frecuentes

| Síntoma | Causa | Solución |
|---|---|---|
| `Invalid object name 'commerce.X'` | El servicio apunta a una base donde no se corrieron los scripts | Comparar el `Database` de la cadena con la base donde están los objetos. Cada servicio va a la suya |
| `Could not find stored procedure 'aud.INSERT_BITACORA'` | Falta el `aud` de esa base | Los scripts de `Core` van en **las dos** bases, no en una |
| `Unavailable` hacia otro microservicio | Clientes, Comercios, QR o Monedas no están levantados | `UseFakeExternalServices: true`, o `Services:<Nombre>:UseFake: true` para volver a quemar solo ese |
| `COMMERCE_NOT_FOUND` con un comercio que sí existe | Comercios real respondió, pero su `status_code` no es `SUC000` o su contrato no coincide con `commerce.proto` | Comparar el `.proto` con el del servicio real |
| Postman no conecta | TLS encendido, o el servicio en HTTPS | `localhost:52131` sin TLS |
| Falla al importar el `.proto` | Falta el import path de `base_response.proto` | Paso 3 de arriba |
| `Msg 1934` en un DML o un SP | `QUOTED_IDENTIFIER OFF`; los índices filtrados lo exigen | En `sqlcmd`, abrir con `SET QUOTED_IDENTIFIER ON;`. La app no se ve afectada |
| `PermissionDenied: El token no identifica a un cliente` | Endpoint 🔑 y el token no trae el claim | Un JWT con `client_id`, o encender `DevAuth` |
| El build falla con `MSB3027` | La app está corriendo y bloquea los DLL | Pararla antes de compilar |
| La app no arranca, error de JSON | Un `appsettings.*.json` con `\` sin escapar en una ruta de Windows | Usar `\\` |
| `db-init` termina en error | Los scripts no encontraron la base | `docker compose logs db-init` |
