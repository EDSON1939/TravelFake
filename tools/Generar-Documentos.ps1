# Genera los tres documentos Word de la entrega.
. "$PSScriptRoot\Build-Docx.ps1"

$repo   = Split-Path $PSScriptRoot -Parent
$salida = Join-Path $repo "docs\word"
if (-not (Test-Path $salida)) { New-Item -ItemType Directory -Path $salida -Force | Out-Null }

$comun = @{
    Proyecto = "YastaCoin - QR Bolivia"
    Version  = "1.0"
    Fecha    = "03/09/2026"
    FechaIso = "2026-09-03T00:00:00Z"
    Estado   = "Aprobado para entrega"
    Reto     = "RETO 1 - QR Bolivia"
    Equipo   = "Edson . Mariel . Rene"
    Autor    = "Edson"
}

$docs = @(
    @{
        Archivo   = "01 - Documentacion de Endpoints gRPC.docx"
        Titulo    = "Documentacion de Endpoints gRPC"
        Subtitulo = "Contratos, atributos, respuestas y diccionario de errores de los microservicios Auth y AccountsAndMovements."
        Fuente    = "docs\ENDPOINTS.md"
        Historial = @(
            @("1.0", "03/09/2026", "Edson", "Version inicial. Cubre los 16 RPC de los dos microservicios, con ejemplos de peticion y respuesta y el catalogo completo de codigos.")
        )
    },
    @{
        Archivo   = "02 - Manual de Instalacion y Ejecucion.docx"
        Titulo    = "Manual de Instalacion y Ejecucion"
        Subtitulo = "Puesta en marcha con Docker o en local, configuracion por entorno, ejecucion de pruebas y resolucion de problemas frecuentes."
        Fuente    = "docs\EJECUCION.md"
        Historial = @(
            @("1.0", "03/09/2026", "Edson", "Version inicial. Incluye los dos caminos de despliegue, la instalacion del esquema de base de datos y la validacion contra el motor.")
        )
    },
    @{
        Archivo   = "03 - Documento Tecnico.docx"
        Titulo    = "Documento Tecnico"
        Subtitulo = "Arquitectura, modelo de datos y las decisiones de diseno que sostienen la consistencia del saldo, la idempotencia y el control de concurrencia."
        Fuente    = "Microservices\AccountsAndMovements\README.md"
        Historial = @(
            @("1.0", "03/09/2026", "Edson", "Version inicial. Documenta la estrategia de concurrencia y de idempotencia que exige el reto, el modelo de datos y las decisiones tomadas.")
        )
    }
)

foreach ($d in $docs) {
    $meta = $comun.Clone()
    $meta.Titulo    = $d.Titulo
    $meta.Subtitulo = $d.Subtitulo
    $meta.Historial = $d.Historial

    $fuente  = Join-Path $repo $d.Fuente
    $destino = Join-Path $salida $d.Archivo

    if (-not (Test-Path $fuente)) { Write-Host "   FALTA la fuente $fuente"; continue }

    New-Documento $meta $fuente $destino | Out-Null
    $kb = [math]::Round((Get-Item $destino).Length / 1KB, 1)
    Write-Host "   OK  $($d.Archivo)  ($kb KB)"
}
