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

### 1. Secretos

```bash
cp .env.example .env
```

Editar `.env` y cambiar los dos valores. `MSSQL_SA_PASSWORD` necesita al menos 8
caracteres con mayúsculas, minúsculas, números y símbolos, o la imagen de SQL
Server se niega a arrancar. `JWT_TOKEN_SECRET` necesita 32 caracteres como
mínimo, o Auth falla al emitir el primer token.

`.gitignore` ya excluye `*.env`: el archivo real nunca se sube.

### 2. Levantar

```bash
docker compose up -d --build
```

La primera vez tarda: descarga las imágenes de SQL Server y del SDK de .NET, y
compila los dos servicios. El orden lo maneja el compose — `db-init` espera a
que SQL Server responda una consulta real, no solo a que abra el puerto, y los
servicios esperan a que `db-init` termine.

### 3. Comprobar

```bash
docker compose ps
```

Los tres contenedores tienen que estar `running` y `yastacoin-db-init` en
`exited (0)`. Si quedó en otro código, los scripts fallaron:

```bash
docker compose logs db-init
```

Una prueba rápida de que el servicio responde:

```bash
curl http://localhost:52131/
```

Devuelve `AccountsAndMovements gRPC Service — <fecha>`.

### 4. Apagar

```bash
docker compose down
```

Los datos sobreviven en el volumen `mssql-data`. Para borrarlos también:
`docker compose down -v`.

---

## Camino B — Local

### 1. Crear las bases

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "IF DB_ID('DB_QRBolivia') IS NULL CREATE DATABASE DB_QRBolivia; IF DB_ID('DB_YastaCoinE') IS NULL CREATE DATABASE DB_YastaCoinE;"
```

### 2. Instalar el esquema de QR Bolivia

El orden importa: `pay.CUENTA` antes que `pay.MOVIMIENTO` por la FK, y las
tablas de `aud` antes que los procedimientos de `pay`, porque estos llaman a
`aud.REGISTRAR_AUDITORIA`.

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d DB_QRBolivia -b -i "Database\Core\Tables\aud.BITACORA.sql,Database\Core\Tables\aud.AUDITORIA.sql,Database\Core\StoredProcedures\aud.INSERT_BITACORA.sql,Database\Core\StoredProcedures\aud.REGISTRAR_AUDITORIA.sql,Database\AccountsAndMovements\Tables\pay.CUENTA.sql,Database\AccountsAndMovements\Tables\pay.MOVIMIENTO.sql,Database\AccountsAndMovements\StoredProcedures\pay.INSERT_CUENTA.sql,Database\AccountsAndMovements\StoredProcedures\pay.APLICAR_MOVIMIENTO.sql,Database\AccountsAndMovements\StoredProcedures\pay.EJECUTAR_PAGO_QR.sql"
```

Los scripts son **idempotentes**: se pueden volver a ejecutar sin efecto
adicional y sin perder datos.

### 3. Instalar la auditoría en la base de Auth

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -d DB_YastaCoinE -b -i "Database\Core\Tables\aud.BITACORA.sql,Database\Core\Tables\aud.AUDITORIA.sql,Database\Core\StoredProcedures\aud.INSERT_BITACORA.sql,Database\Core\StoredProcedures\aud.REGISTRAR_AUDITORIA.sql"
```

### 4. Levantar el servicio

```bash
dotnet run --project Microservices\AccountsAndMovements\AccountsAndMovements.Api
```

Queda escuchando en `http://localhost:52131` (texto plano, el que usa Postman) y
`https://localhost:52130`.

Desde Visual Studio: abrir `YastaCoin.sln` y usar el perfil de inicio múltiple
de `YastaCoin.slnLaunch`.

---

## Configuración

Todo se puede sobreescribir por variable de entorno con el separador `__`:
`Database__ConnectionString`, `DevAuth__ClientId`, etc.

| Clave | Qué hace | Dónde |
|---|---|---|
| `Database:ConnectionString` | Cadena de conexión | `appsettings.json` de cada servicio |
| `UseFakeExternalServices` | Resuelve Clientes, Comercios, QR y Monedas con catálogos en memoria en vez de gRPC | AccountsAndMovements |
| `DevAuth:Enabled` · `DevAuth:ClientId` | Completa el claim `client_id` cuando el token no lo trae. **Solo aplica en Development y Docker** | AccountsAndMovements |
| `Jwt:TokenSecret` | Firma de los JWT. Nunca en el repositorio | Auth · por entorno |
| `Serilog:*` | Nivel y destinos de log | Todos |
| `OpenTelemetry:Enabled` | Trazas y métricas | Todos |

### Los tres entornos

| `ASPNETCORE_ENVIRONMENT` | Archivo | Notas |
|---|---|---|
| `Development` | `appsettings.Development.json` | Rutas de log de Windows. Fakes y DevAuth encendidos |
| `Docker` | `appsettings.Docker.json` | Rutas de Linux, sin Grafana Loki |
| *(producción)* | `appsettings.json` | Fakes y DevAuth apagados por omisión |

---

## Tests

```bash
dotnet test YastaCoin.sln
```

**100 pruebas**: 69 de AccountsAndMovements y 31 de Auth. No necesitan base de
datos — los repositorios y los servicios externos están sustituidos con
NSubstitute.

### Validación contra el motor

Las pruebas unitarias no cubren lo que vive en SQL: bloqueos, transacciones,
restricciones y auditoría. Eso se valida con un script aparte, contra una base
temporal que se crea y se borra:

```bash
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "IF DB_ID('DB_VALIDA_TMP') IS NOT NULL BEGIN ALTER DATABASE DB_VALIDA_TMP SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE DB_VALIDA_TMP; END; CREATE DATABASE DB_VALIDA_TMP;"
```

Después se instala el esquema en `DB_VALIDA_TMP` con el mismo comando del paso
2 cambiando el `-d`, y se corre `docs/validacion.sql`. Verifica 19 puntos: los
saldos, la idempotencia por los dos caminos, el rechazo por saldo insuficiente,
que una operación rechazada no deje auditoría, el enlace bitácora ↔ auditoría, y
que todo saldo sea reconstruible sumando sus asientos.

### Concurrencia

No se prueba desde Postman: manda las peticiones en serie. Se prueba con dos
sesiones simultáneas de `sqlcmd` sincronizadas con `WAITFOR TIME`, o con `ghz`
contra el endpoint gRPC. El escenario está en
[`docs/diagramas/04-concurrencia.drawio.txt`](diagramas/04-concurrencia.drawio.txt).

---

## Probar con Postman

1. **New → gRPC**, Server URL `localhost:52131`, TLS **apagado**.
2. Método → **Import a .proto file** →
   `Microservices/AccountsAndMovements/AccountsAndMovements.Api/Protos/accounts_movements.proto`
3. En el mismo diálogo agregar como *import path* la carpeta
   `YastaCoin/Shared/Core.Domain/Grpc/Protobuf/`. Sin eso la importación falla:
   el proto hace `import "base_response.proto"`.
4. Pestaña **Metadata**, en todas las llamadas: `authorization` = `Bearer dev`.

No hay *server reflection*: el `.proto` es obligatorio.

Los bodies de ejemplo de cada endpoint están en
[`docs/ENDPOINTS.md`](ENDPOINTS.md).

---

## Problemas frecuentes

| Síntoma | Causa | Solución |
|---|---|---|
| La app no arranca y el error menciona JSON | Un `appsettings.*.json` con `\` sin escapar en una ruta de Windows | Usar `\\` |
| Postman no conecta | TLS encendido, o el servicio escuchando en HTTPS | Usar `localhost:52131` sin TLS |
| Falla al importar el `.proto` | Falta el import path de `base_response.proto` | Paso 3 de arriba |
| `Msg 1934` al ejecutar un SP | `QUOTED_IDENTIFIER OFF` al crearlo | Los scripts ya lo fuerzan; volver a ejecutarlos |
| `PermissionDenied: El token no identifica a un cliente` | Endpoint que saca el cliente del JWT y el token no trae el claim | Usar un JWT con `client_id`, o encender `DevAuth` |
| El build local falla con `MSB3027` | La app está corriendo y bloquea los DLL | Pararla antes de compilar |
| `db-init` termina en error | Los scripts no encontraron la base | `docker compose logs db-init` |
