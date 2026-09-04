Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression

$fallos = 0
function Chk([bool]$ok, [string]$msg) {
    if ($ok) { Write-Host "      OK   $msg" }
    else     { Write-Host "      FALLO $msg"; $script:fallos++ }
}

$esperadas = @(
  "[Content_Types].xml","_rels/.rels","word/document.xml","word/_rels/document.xml.rels",
  "word/styles.xml","word/numbering.xml","word/settings.xml","word/header1.xml",
  "word/footer1.xml","docProps/core.xml","docProps/app.xml"
)

Get-ChildItem (Join-Path (Split-Path $PSScriptRoot -Parent) "docs\word\*.docx") | ForEach-Object {
    Write-Host ""
    Write-Host "   === $($_.Name) ==="
    $zip = [System.IO.Compression.ZipFile]::OpenRead($_.FullName)
    if ($null -eq $zip) { Chk $false "el paquete se puede abrir"; return }
    try {
        $nombres = $zip.Entries | ForEach-Object { $_.FullName }

        foreach ($e in $esperadas) { Chk ($nombres -contains $e) "parte presente: $e" }

        $xmls = @{}
        $malformado = @()
        foreach ($entry in $zip.Entries) {
            if ($entry.FullName -notmatch '\.(xml|rels)$') { continue }
            $sr = New-Object System.IO.StreamReader($entry.Open())
            $txt = $sr.ReadToEnd(); $sr.Close()
            $xmls[$entry.FullName] = $txt
            try { [xml]$txt | Out-Null } catch { $malformado += $entry.FullName }
        }
        Chk ($malformado.Count -eq 0) "todas las partes XML estan bien formadas$(if($malformado.Count){' -> ' + ($malformado -join ', ')})"

        $doc  = $xmls["word/document.xml"]
        $rels = $xmls["word/_rels/document.xml.rels"]
        $sty  = $xmls["word/styles.xml"]
        $num  = $xmls["word/numbering.xml"]

        # Relaciones referenciadas vs declaradas
        $usadas = [regex]::Matches($doc, 'r:id="([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
        $decl   = [regex]::Matches($rels, 'Id="([^"]+)"')  | ForEach-Object { $_.Groups[1].Value }
        $huerf  = $usadas | Where-Object { $decl -notcontains $_ }
        Chk ($huerf.Count -eq 0) "las relaciones usadas estan declaradas$(if($huerf.Count){' -> ' + ($huerf -join ', ')})"

        # Estilos referenciados vs definidos
        $estUsados = [regex]::Matches($doc, '<w:pStyle w:val="([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
        $estDef    = [regex]::Matches($sty, 'w:styleId="([^"]+)"')      | ForEach-Object { $_.Groups[1].Value }
        $sinEstilo = $estUsados | Where-Object { $estDef -notcontains $_ }
        Chk ($sinEstilo.Count -eq 0) "los estilos usados estan definidos$(if($sinEstilo.Count){' -> ' + ($sinEstilo -join ', ')})"

        # Numeracion
        $numUsados = [regex]::Matches($doc, '<w:numId w:val="([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
        $numDef    = [regex]::Matches($num, '<w:num w:numId="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
        $sinNum    = $numUsados | Where-Object { $numDef -notcontains $_ }
        Chk ($sinNum.Count -eq 0) "las listas referencian numeraciones existentes"

        # Integridad de tablas: celdas por fila == columnas declaradas
        $x = [xml]$doc
        $ns = [System.Xml.XmlNamespaceManager]::new($x.NameTable)
        $ns.AddNamespace("w","http://schemas.openxmlformats.org/wordprocessingml/2006/main")
        $tablas = $x.SelectNodes("//w:tbl", $ns)
        $desalineadas = 0
        foreach ($t in $tablas) {
            $cols = $t.SelectNodes("w:tblGrid/w:gridCol", $ns).Count
            foreach ($tr in $t.SelectNodes("w:tr", $ns)) {
                if ($tr.SelectNodes("w:tc", $ns).Count -ne $cols) { $desalineadas++ }
            }
        }
        Chk ($desalineadas -eq 0) "todas las filas tienen tantas celdas como columnas ($($tablas.Count) tablas)"

        # Anchos de columna suman el ancho de tabla
        $malAncho = 0
        foreach ($t in $tablas) {
            $tw = [int]$t.SelectSingleNode("w:tblPr/w:tblW", $ns).GetAttribute("w","http://schemas.openxmlformats.org/wordprocessingml/2006/main")
            $suma = 0
            foreach ($g in $t.SelectNodes("w:tblGrid/w:gridCol", $ns)) {
                $suma += [int]$g.GetAttribute("w","http://schemas.openxmlformats.org/wordprocessingml/2006/main")
            }
            if ($suma -ne $tw) { $malAncho++ }
        }
        Chk ($malAncho -eq 0) "los anchos de columna suman el ancho de la tabla"

        # Toda celda tiene al menos un parrafo (Word lo exige)
        $celdasVacias = 0
        foreach ($tc in $x.SelectNodes("//w:tc", $ns)) {
            if ($tc.SelectNodes("w:p", $ns).Count -lt 1) { $celdasVacias++ }
        }
        Chk ($celdasVacias -eq 0) "toda celda contiene al menos un parrafo"

        # Elementos esperados del documento
        Chk ($doc -match 'TOC \\o') "el indice automatico esta presente"
        Chk ($xmls["word/footer1.xml"] -match 'PAGE' -and $xmls["word/footer1.xml"] -match 'NUMPAGES') "el pie numera las paginas"
        Chk ($doc -match 'w:pStyle w:val="Portada"') "la portada esta presente"
        Chk ($doc -match 'Control de versiones') "la tabla de control de versiones esta presente"
        Chk ($xmls["docProps/core.xml"] -match '<cp:revision>') "los metadatos llevan version"

        $h1 = ([regex]::Matches($doc, 'w:pStyle w:val="Heading1"')).Count
        $h2 = ([regex]::Matches($doc, 'w:pStyle w:val="Heading2"')).Count
        $h3 = ([regex]::Matches($doc, 'w:pStyle w:val="Heading3"')).Count
        $cod= ([regex]::Matches($doc, 'w:pStyle w:val="Codigo"')).Count
        Chk ($h1 -ge 2 -and $h2 -ge 1) "jerarquia de titulos: H1=$h1 H2=$h2 H3=$h3, tablas=$($tablas.Count), lineas de codigo=$cod"

        # Content types cubren todas las partes
        $ct = $xmls["[Content_Types].xml"]
        $faltaCT = @()
        foreach ($n in $nombres) {
            if ($n -match "\.rels$") { continue }
            if ($n -eq "[Content_Types].xml") { continue }
            if ($n -notmatch '\.xml$') { continue }
            if ($ct -notmatch [regex]::Escape("/$n")) { $faltaCT += $n }
        }
        Chk ($faltaCT.Count -eq 0) "[Content_Types].xml declara todas las partes$(if($faltaCT.Count){' -> ' + ($faltaCT -join ', ')})"
    }
    finally { $zip.Dispose() }
}

Write-Host ""
if ($fallos -eq 0) { Write-Host "   RESULTADO: los tres documentos pasan todas las comprobaciones." }
else { Write-Host "   RESULTADO: $fallos comprobacion(es) fallaron." }
