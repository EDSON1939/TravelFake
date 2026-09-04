# Diagramas

Ocho archivos `.drawio` que draw.io abre directamente. Antes eran `.txt` con el
XML precedido de texto explicativo: había que seleccionar a mano el bloque
correcto y pegarlo en *Edit Diagram*, y si sobraba una línea de prosa la
importación fallaba. La explicación de cada uno está acá abajo; el archivo es
solo el diagrama.

## Cómo abrirlos

En [app.diagrams.net](https://app.diagrams.net) o en el escritorio:
**File → Open From → Device…**, o arrastrar el archivo a la ventana.

Para exportar: **File → Export as → PNG**, con *Transparent Background*
desactivado y zoom 200% para que se lea impreso.

## El conjunto

| | Diagrama | Responde a |
|---|---|---|
| 01 | [Pago QR](01-flujo-pago-qr.drawio) | ¿Por dónde pasa un pago y dónde se rechaza? |
| 02 | [Arquitectura](02-arquitectura.drawio) | ¿Qué capas hay y quién habla con quién? |
| 03 | [Modelo de datos](03-modelo-datos.drawio) | ¿Qué tablas y qué restricciones? |
| 04 | [Concurrencia](04-concurrencia.drawio) | ¿Por qué dos pagos simultáneos no se aprueban los dos? |
| 05 | [Pipeline gRPC](05-pipeline-grpc.drawio) | ¿Qué atraviesa una llamada antes de llegar al handler? |
| 06 | [Alta de cuenta](06-alta-de-cuenta.drawio) | ¿Qué se valida antes de crear una cuenta? |
| 07 | [ApplyMovement](07-aplicar-movimiento.drawio) | ¿Cómo cambia un saldo fuera del pago QR? |
| 08 | [Auditoría](08-auditoria.drawio) | ¿Qué queda registrado y dónde? |

## Qué muestra cada uno

### [`01-flujo-pago-qr.drawio`](01-flujo-pago-qr.drawio)

Los 15 pasos del flujo del reto. La columna central es el camino feliz; la
derecha, cada punto donde la operación se rechaza y con qué código. El recuadro
punteado marca lo que ocurre **dentro** de la transacción del motor: es la
frontera que explica por qué el saldo no se valida en el handler.

### [`02-arquitectura.drawio`](02-arquitectura.drawio)

Las capas de un microservicio y quién habla con quién. Los servicios de la
derecha con borde punteado se resuelven con catálogos en memoria
(`ExternalServices/Fakes`) mientras no estén desplegados; cada uno se activa por
separado con `Services:<Nombre>:UseFake`.

### [`03-modelo-datos.drawio`](03-modelo-datos.drawio)

Las cinco tablas y sus restricciones, repartidas en las dos bases: la de pagos
(`DB_QRBolivia`) y la de Auth (`DB_YastaCoinE`). Las restricciones están
dibujadas a propósito: son las que sostienen las garantías del reto, no
comentarios.

| Restricción | Qué garantiza |
|---|---|
| `UQ_COMMERCE_MOVIMIENTO_IDEMPOTENCIA` | No cobrar dos veces la misma operación |
| `UQ_COMMERCE_MOVIMIENTO_QR` | Un QR se paga una sola vez |
| `CK_COMMERCE_CUENTA_SALDO >= 0` | Nunca saldo negativo |
| `UQ_COMMERCE_CUENTA_TITULAR` | Una cuenta por titular y moneda |
| `UQ_COMMERCE_USUARIO_USERNAME` | Un username entre los vigentes |
| `UQ_COMMERCE_USUARIO_CLIENTE` | Un cliente, una sola credencial |

Los dos últimos son índices **filtrados** (`WHERE no eliminado`): la baja es
lógica, y con una restricción plana un usuario dado de baja bloquearía su nombre
para siempre.

`commerce.CUENTA` no tiene FK hacia `CLIENTE` ni `COMERCIO`, y
`commerce.USUARIO` tampoco hacia `CLIENTE`: son tablas de otros microservicios y
esa integridad se valida por gRPC antes de escribir.

### [`04-concurrencia.drawio`](04-concurrencia.drawio)

El escenario exacto del reto: saldo 100, llegan 80 y 50 a la vez, se aprueba una
sola. La línea gruesa marca el tiempo que B pasa **esperando** el bloqueo que
tomó A: ahí está la respuesta a "por qué no se aprueban las dos".

### [`05-pipeline-grpc.drawio`](05-pipeline-grpc.drawio)

Todo lo que atraviesa una llamada antes de llegar al handler, en el orden real
de ejecución: los cuatro interceptores (validación, log, JWT, auditoría), el
borde gRPC, MediatR con su `ValidationBehavior`, el handler, el repositorio y la
bifurcación entre Dapper para leer y stored procedures para escribir. A la
derecha, por dónde sale cada rechazo.

Dos cosas que solo se ven puestas en fila: el `ValidationInterceptor` hoy no
corta nada, porque los validadores son de los comandos de MediatR y no de los
mensajes Pb; y el auditor es el último de los cuatro, así que **una llamada que
el JWT rechaza no deja fila en `aud.BITACORA`**.

### [`06-alta-de-cuenta.drawio`](06-alta-de-cuenta.drawio)

Los dos RPC de alta —cliente extranjero y comercio boliviano— y dónde
convergen. Qué se valida contra qué microservicio, con qué código se rechaza
cada caso, y qué hace `commerce.INSERT_CUENTA` por dentro: el número derivado
del ID, y el saldo inicial asentado como un **crédito de apertura** en
`commerce.MOVIMIENTO` dentro de la misma transacción, para que el saldo nunca
exista sin respaldo en el libro mayor.

### [`07-aplicar-movimiento.drawio`](07-aplicar-movimiento.drawio)

El otro camino por el que cambia un saldo: recargas, ajustes y reversas. Muestra
`commerce.APLICAR_MOVIMIENTO` completo —bloqueo de fila, idempotencia, validación
de fondos, asiento, auditoría y commit— con sus cuatro códigos de retorno y cómo
los traduce el handler.

Lo interesante está en la idempotencia: la clave se busca **sin filtrar por
cuenta** porque es única en todo el sistema, y solo cuenta como reintento si el
asiento hallado coincide en cuenta, tipo y monto. Reusar la clave de una recarga
para un débito devolvería éxito con el id de la recarga y el débito no se
aplicaría nunca. El bloque del `CATCH` cubre la carrera contra el índice único.

### [`08-auditoria.drawio`](08-auditoria.drawio)

Por qué hay dos tablas de auditoría y no una. `aud.BITACORA` registra la
**llamada** y la escribe un interceptor **fuera** de la transacción: si falla,
un warning y la respuesta sigue. `aud.AUDITORIA` registra el **dato** —el antes
y el después en JSON— y la escriben los stored procedures **dentro** de la
transacción, así que comparte su destino: si la operación hace rollback, la
auditoría se va con ella.

El `TraceId` une las dos: de una fila de bitácora se llega a todos los cambios
de dato que produjo esa llamada.
