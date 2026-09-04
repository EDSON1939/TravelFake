# =============================================================================
#  Build-Docx.ps1 — genera documentos Word profesionales desde Markdown.
#
#  Un .docx es un ZIP de XML (OOXML). No hay node ni LibreOffice en esta
#  maquina, asi que se arma el paquete a mano: mas control y cero dependencias.
#
#  Cada documento lleva portada, control de versiones, indice automatico,
#  encabezado, pie con numero de pagina y estilos propios.
# =============================================================================

Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression

$Utf8 = New-Object System.Text.UTF8Encoding($false)

# ── Paleta y tipografia ──────────────────────────────────────────────────────
$Acento   = "0C5C52"   # verde petroleo
$AcentoOsc= "0A423B"
$Tinta    = "1A2422"
$Gris     = "5C6B67"
$FondoTab = "DFEEEA"
$FondoCod = "F2F5F4"
$Linea    = "C3D2CE"

function Esc([string]$s) {
    if ($null -eq $s) { return "" }
    $s = $s -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
    return $s
}

# ── Runs con formato inline: **negrita**, `codigo`, *cursiva* ────────────────
function ConvertTo-Runs([string]$texto, [string]$colorBase = $Tinta, [int]$tam = 21) {
    # Los enlaces markdown se reducen a su texto: en papel una URL relativa no sirve.
    $texto = [regex]::Replace($texto, '\[([^\]]+)\]\(([^)]+)\)', '$1')
    # Emoji fuera: no todas las fuentes de Word los tienen.
    $texto = $texto -replace '[\uD800-\uDBFF][\uDC00-\uDFFF]', '' -replace '[\u2190-\u21FF\u2600-\u27BF\uFE0F]', ''

    $sb = New-Object System.Text.StringBuilder
    $patron = '(\*\*[^*]+\*\*|`[^`]+`|\*[^*]+\*)'
    $partes = [regex]::Split($texto, $patron)

    foreach ($p in $partes) {
        if ([string]::IsNullOrEmpty($p)) { continue }

        if ($p -match '^\*\*(.+)\*\*$') {
            [void]$sb.Append("<w:r><w:rPr><w:b/><w:color w:val=""$colorBase""/><w:sz w:val=""$tam""/></w:rPr><w:t xml:space=""preserve"">$(Esc $matches[1])</w:t></w:r>")
        }
        elseif ($p -match '^`(.+)`$') {
            [void]$sb.Append("<w:r><w:rPr><w:rFonts w:ascii=""Consolas"" w:hAnsi=""Consolas""/><w:color w:val=""$AcentoOsc""/><w:sz w:val=""$($tam - 3)""/><w:shd w:val=""clear"" w:color=""auto"" w:fill=""$FondoCod""/></w:rPr><w:t xml:space=""preserve"">$(Esc $matches[1])</w:t></w:r>")
        }
        elseif ($p -match '^\*(.+)\*$') {
            [void]$sb.Append("<w:r><w:rPr><w:i/><w:color w:val=""$colorBase""/><w:sz w:val=""$tam""/></w:rPr><w:t xml:space=""preserve"">$(Esc $matches[1])</w:t></w:r>")
        }
        else {
            [void]$sb.Append("<w:r><w:rPr><w:color w:val=""$colorBase""/><w:sz w:val=""$tam""/></w:rPr><w:t xml:space=""preserve"">$(Esc $p)</w:t></w:r>")
        }
    }
    return $sb.ToString()
}

function New-Parrafo([string]$texto, [string]$estilo = "", [string]$extraPpr = "") {
    $ppr = ""
    if ($estilo) { $ppr += "<w:pStyle w:val=""$estilo""/>" }
    $ppr += $extraPpr
    $p = "<w:p>"
    if ($ppr) { $p += "<w:pPr>$ppr</w:pPr>" }
    $p += (ConvertTo-Runs $texto)
    $p += "</w:p>"
    return $p
}

function New-Titulo([string]$texto, [int]$nivel) {
    $limpio = [regex]::Replace($texto, '\[([^\]]+)\]\(([^)]+)\)', '$1')
    $limpio = $limpio -replace '[\uD800-\uDBFF][\uDC00-\uDFFF]', '' -replace '`',''
    $limpio = $limpio.Trim()
    return "<w:p><w:pPr><w:pStyle w:val=""Heading$nivel""/></w:pPr><w:r><w:t xml:space=""preserve"">$(Esc $limpio)</w:t></w:r></w:p>"
}

function New-Codigo([string[]]$lineas) {
    $sb = New-Object System.Text.StringBuilder
    $n = $lineas.Count
    for ($i = 0; $i -lt $n; $i++) {
        $bordes = ""
        if ($i -eq 0)      { $bordes += "<w:top w:val=""single"" w:sz=""4"" w:color=""$Linea""/>" }
        if ($i -eq $n - 1) { $bordes += "<w:bottom w:val=""single"" w:sz=""4"" w:color=""$Linea""/>" }
        $bordes += "<w:left w:val=""single"" w:sz=""18"" w:color=""$Acento""/><w:right w:val=""single"" w:sz=""4"" w:color=""$Linea""/>"
        [void]$sb.Append("<w:p><w:pPr><w:pStyle w:val=""Codigo""/><w:pBdr>$bordes</w:pBdr></w:pPr><w:r><w:t xml:space=""preserve"">$(Esc $lineas[$i])</w:t></w:r></w:p>")
    }
    return $sb.ToString()
}

function New-Vineta([string]$texto) {
    return "<w:p><w:pPr><w:pStyle w:val=""Vineta""/><w:numPr><w:ilvl w:val=""0""/><w:numId w:val=""1""/></w:numPr></w:pPr>$(ConvertTo-Runs $texto)</w:p>"
}

function New-Nota([string]$texto) {
    $ppr = "<w:pStyle w:val=""Nota""/><w:pBdr><w:left w:val=""single"" w:sz=""18"" w:color=""B8860B""/></w:pBdr><w:shd w:val=""clear"" w:color=""auto"" w:fill=""FBF4E4""/><w:ind w:left=""200""/><w:spacing w:before=""120"" w:after=""120""/>"
    return "<w:p><w:pPr>$ppr</w:pPr>$(ConvertTo-Runs $texto)</w:p>"
}

# ── Tablas ───────────────────────────────────────────────────────────────────
function New-Tabla([string[]]$filas) {
    $celdas = @()
    foreach ($f in $filas) {
        $t = $f.Trim().Trim('|')
        $celdas += ,($t -split '\s*\|\s*' | ForEach-Object { $_.Trim() })
    }
    if ($celdas.Count -lt 2) { return "" }

    $encabezado = $celdas[0]
    $cuerpo = @()
    for ($i = 2; $i -lt $celdas.Count; $i++) { $cuerpo += ,$celdas[$i] }

    $nCol = $encabezado.Count
    $anchoTotal = 9360

    # Ancho proporcional al contenido, con piso y techo para que ninguna
    # columna quede impracticable.
    $pesos = @()
    for ($c = 0; $c -lt $nCol; $c++) {
        $max = $encabezado[$c].Length
        foreach ($fila in $cuerpo) {
            if ($c -lt $fila.Count -and $fila[$c].Length -gt $max) { $max = $fila[$c].Length }
        }
        if ($max -lt 8)  { $max = 8 }
        if ($max -gt 60) { $max = 60 }
        $pesos += $max
    }
    $suma = ($pesos | Measure-Object -Sum).Sum
    $anchos = @()
    $acum = 0
    for ($c = 0; $c -lt $nCol; $c++) {
        if ($c -eq $nCol - 1) { $anchos += ($anchoTotal - $acum) }
        else {
            $w = [int]($anchoTotal * $pesos[$c] / $suma)
            $anchos += $w; $acum += $w
        }
    }

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append("<w:tbl><w:tblPr><w:tblW w:w=""$anchoTotal"" w:type=""dxa""/><w:tblLayout w:type=""fixed""/><w:tblBorders>")
    [void]$sb.Append("<w:top w:val=""single"" w:sz=""4"" w:color=""$Linea""/><w:bottom w:val=""single"" w:sz=""4"" w:color=""$Linea""/>")
    [void]$sb.Append("<w:insideH w:val=""single"" w:sz=""4"" w:color=""$Linea""/><w:left w:val=""nil""/><w:right w:val=""nil""/><w:insideV w:val=""nil""/>")
    [void]$sb.Append("</w:tblBorders><w:tblCellMar><w:top w:w=""60"" w:type=""dxa""/><w:bottom w:w=""60"" w:type=""dxa""/><w:left w:w=""100"" w:type=""dxa""/><w:right w:w=""100"" w:type=""dxa""/></w:tblCellMar></w:tblPr>")

    [void]$sb.Append("<w:tblGrid>")
    foreach ($a in $anchos) { [void]$sb.Append("<w:gridCol w:w=""$a""/>") }
    [void]$sb.Append("</w:tblGrid>")

    # Encabezado, repetido en cada pagina.
    [void]$sb.Append("<w:tr><w:trPr><w:tblHeader/></w:trPr>")
    for ($c = 0; $c -lt $nCol; $c++) {
        $txt = ""
        if ($c -lt $encabezado.Count) { $txt = $encabezado[$c] }
        [void]$sb.Append("<w:tc><w:tcPr><w:tcW w:w=""$($anchos[$c])"" w:type=""dxa""/><w:shd w:val=""clear"" w:color=""auto"" w:fill=""$FondoTab""/></w:tcPr>")
        [void]$sb.Append("<w:p><w:pPr><w:pStyle w:val=""CeldaTitulo""/></w:pPr>$(ConvertTo-Runs $txt $AcentoOsc 18)</w:p></w:tc>")
    }
    [void]$sb.Append("</w:tr>")

    foreach ($fila in $cuerpo) {
        [void]$sb.Append("<w:tr>")
        for ($c = 0; $c -lt $nCol; $c++) {
            $txt = ""
            if ($c -lt $fila.Count) { $txt = $fila[$c] }
            [void]$sb.Append("<w:tc><w:tcPr><w:tcW w:w=""$($anchos[$c])"" w:type=""dxa""/></w:tcPr>")
            [void]$sb.Append("<w:p><w:pPr><w:pStyle w:val=""Celda""/></w:pPr>$(ConvertTo-Runs $txt $Tinta 18)</w:p></w:tc>")
        }
        [void]$sb.Append("</w:tr>")
    }
    [void]$sb.Append("</w:tbl><w:p><w:pPr><w:spacing w:after=""0"" w:line=""120"" w:lineRule=""exact""/></w:pPr></w:p>")
    return $sb.ToString()
}

# ── Markdown → cuerpo OOXML ──────────────────────────────────────────────────
function ConvertFrom-Markdown([string]$ruta) {
    $lineas = [System.IO.File]::ReadAllLines($ruta)
    $sb = New-Object System.Text.StringBuilder
    $i = 0
    $primerH1 = $true

    while ($i -lt $lineas.Count) {
        $l = $lineas[$i]

        # Bloque de codigo
        if ($l -match '^\s*```') {
            $buffer = @()
            $i++
            while ($i -lt $lineas.Count -and $lineas[$i] -notmatch '^\s*```') {
                $buffer += $lineas[$i]; $i++
            }
            $i++
            if ($buffer.Count -gt 0) { [void]$sb.Append((New-Codigo $buffer)) }
            continue
        }

        # Tabla
        if ($l -match '^\s*\|' -and ($i + 1) -lt $lineas.Count -and $lineas[$i+1] -match '^\s*\|[\s:\-|]+\|\s*$') {
            $buffer = @()
            while ($i -lt $lineas.Count -and $lineas[$i] -match '^\s*\|') {
                $buffer += $lineas[$i]; $i++
            }
            [void]$sb.Append((New-Tabla $buffer))
            continue
        }

        # Titulos
        # Los niveles suben uno: el H1 del markdown es el titulo del documento y
        # ya vive en la portada, asi que "##" pasa a ser la seccion de primer
        # nivel, como corresponde en un documento formal.
        if ($l -match '^####\s+(.*)') { [void]$sb.Append((New-Titulo $matches[1] 3)); $i++; continue }
        if ($l -match '^###\s+(.*)')  { [void]$sb.Append((New-Titulo $matches[1] 2)); $i++; continue }
        if ($l -match '^##\s+(.*)')   { [void]$sb.Append((New-Titulo $matches[1] 1)); $i++; continue }
        if ($l -match '^#\s+(.*)') {
            # El H1 del markdown ya esta en la portada.
            if ($primerH1) { $primerH1 = $false; $i++; continue }
            [void]$sb.Append((New-Titulo $matches[1] 1)); $i++; continue
        }

        # Separador
        if ($l -match '^\s*---\s*$') {
            [void]$sb.Append("<w:p><w:pPr><w:pBdr><w:bottom w:val=""single"" w:sz=""6"" w:color=""$Linea""/></w:pBdr><w:spacing w:before=""120"" w:after=""180""/></w:pPr></w:p>")
            $i++; continue
        }

        # Cita / nota
        if ($l -match '^>\s?(.*)') {
            $buffer = @($matches[1])
            $i++
            while ($i -lt $lineas.Count -and $lineas[$i] -match '^>\s?(.*)') { $buffer += $matches[1]; $i++ }
            [void]$sb.Append((New-Nota ($buffer -join ' ')))
            continue
        }

        # Vineta
        if ($l -match '^\s*[-*]\s+(.*)') {
            $texto = $matches[1]
            $i++
            while ($i -lt $lineas.Count -and $lineas[$i] -match '^\s{2,}\S' -and $lineas[$i] -notmatch '^\s*[-*]\s') {
                $texto += " " + $lineas[$i].Trim(); $i++
            }
            [void]$sb.Append((New-Vineta $texto))
            continue
        }

        # Lista numerada -> vineta simple
        if ($l -match '^\s*\d+\.\s+(.*)') {
            [void]$sb.Append((New-Vineta $matches[1])); $i++; continue
        }

        # Parrafo: junta lineas hasta un blanco
        if ($l.Trim().Length -gt 0) {
            $texto = $l.Trim()
            $i++
            while ($i -lt $lineas.Count -and $lineas[$i].Trim().Length -gt 0 `
                   -and $lineas[$i] -notmatch '^\s*(#|\||```|>|[-*]\s|\d+\.\s|---\s*$)') {
                $texto += " " + $lineas[$i].Trim(); $i++
            }
            [void]$sb.Append((New-Parrafo $texto))
            continue
        }

        $i++
    }
    return $sb.ToString()
}

# ── Portada y control de versiones ───────────────────────────────────────────
function New-Portada($meta) {
    $sb = New-Object System.Text.StringBuilder

    [void]$sb.Append("<w:p><w:pPr><w:spacing w:before=""2400"" w:after=""0""/></w:pPr></w:p>")
    [void]$sb.Append("<w:p><w:pPr><w:spacing w:after=""60""/></w:pPr><w:r><w:rPr><w:rFonts w:ascii=""Consolas"" w:hAnsi=""Consolas""/><w:color w:val=""$Acento""/><w:sz w:val=""20""/><w:spacing w:val=""60""/></w:rPr><w:t>$(Esc $meta.Proyecto.ToUpper())</w:t></w:r></w:p>")
    [void]$sb.Append("<w:p><w:pPr><w:pStyle w:val=""Portada""/></w:pPr><w:r><w:t>$(Esc $meta.Titulo)</w:t></w:r></w:p>")
    [void]$sb.Append("<w:p><w:pPr><w:pStyle w:val=""PortadaSub""/></w:pPr><w:r><w:t>$(Esc $meta.Subtitulo)</w:t></w:r></w:p>")

    [void]$sb.Append("<w:p><w:pPr><w:pBdr><w:bottom w:val=""single"" w:sz=""18"" w:color=""$Acento""/></w:pBdr><w:spacing w:before=""240"" w:after=""360""/><w:ind w:right=""5400""/></w:pPr></w:p>")

    $datos = @(
        @("Documento",  $meta.Titulo),
        @("Version",    $meta.Version),
        @("Fecha",      $meta.Fecha),
        @("Estado",     $meta.Estado),
        @("Reto",       $meta.Reto),
        @("Equipo",     $meta.Equipo),
        @("Autor",      $meta.Autor),
        @("Clasificacion", "Uso interno")
    )
    [void]$sb.Append("<w:tbl><w:tblPr><w:tblW w:w=""6000"" w:type=""dxa""/><w:tblLayout w:type=""fixed""/><w:tblBorders><w:top w:val=""nil""/><w:bottom w:val=""nil""/><w:left w:val=""nil""/><w:right w:val=""nil""/><w:insideH w:val=""nil""/><w:insideV w:val=""nil""/></w:tblBorders><w:tblCellMar><w:top w:w=""40"" w:type=""dxa""/><w:bottom w:w=""40"" w:type=""dxa""/><w:left w:w=""0"" w:type=""dxa""/><w:right w:w=""80"" w:type=""dxa""/></w:tblCellMar></w:tblPr><w:tblGrid><w:gridCol w:w=""1900""/><w:gridCol w:w=""4100""/></w:tblGrid>")
    foreach ($d in $datos) {
        [void]$sb.Append("<w:tr><w:tc><w:tcPr><w:tcW w:w=""1900"" w:type=""dxa""/></w:tcPr><w:p><w:pPr><w:spacing w:after=""0"" w:line=""260"" w:lineRule=""auto""/></w:pPr><w:r><w:rPr><w:rFonts w:ascii=""Consolas"" w:hAnsi=""Consolas""/><w:color w:val=""$Gris""/><w:sz w:val=""17""/></w:rPr><w:t>$(Esc $d[0])</w:t></w:r></w:p></w:tc>")
        [void]$sb.Append("<w:tc><w:tcPr><w:tcW w:w=""4100"" w:type=""dxa""/></w:tcPr><w:p><w:pPr><w:spacing w:after=""0"" w:line=""260"" w:lineRule=""auto""/></w:pPr><w:r><w:rPr><w:b/><w:color w:val=""$Tinta""/><w:sz w:val=""19""/></w:rPr><w:t>$(Esc $d[1])</w:t></w:r></w:p></w:tc></w:tr>")
    }
    [void]$sb.Append("</w:tbl>")

    [void]$sb.Append("<w:p><w:r><w:br w:type=""page""/></w:r></w:p>")
    return $sb.ToString()
}

function New-ControlVersiones($meta) {
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append((New-Titulo "Control de versiones" 1))
    $filas = @("| Version | Fecha | Autor | Descripcion del cambio |", "|---|---|---|---|")
    foreach ($h in $meta.Historial) { $filas += "| $($h[0]) | $($h[1]) | $($h[2]) | $($h[3]) |" }
    [void]$sb.Append((New-Tabla $filas))

    [void]$sb.Append((New-Titulo "Indice" 1))
    [void]$sb.Append("<w:p><w:pPr><w:spacing w:after=""120""/></w:pPr><w:r><w:rPr><w:i/><w:color w:val=""$Gris""/><w:sz w:val=""17""/></w:rPr><w:t>Si el indice aparece vacio, seleccionarlo y pulsar F9 para actualizar los campos.</w:t></w:r></w:p>")
    [void]$sb.Append("<w:p><w:pPr><w:spacing w:after=""0""/></w:pPr><w:r><w:fldChar w:fldCharType=""begin"" w:dirty=""true""/></w:r><w:r><w:instrText xml:space=""preserve""> TOC \o &quot;1-3&quot; \h \z \u </w:instrText></w:r><w:r><w:fldChar w:fldCharType=""separate""/></w:r><w:r><w:rPr><w:color w:val=""$Gris""/><w:sz w:val=""19""/></w:rPr><w:t>Actualizar con F9.</w:t></w:r><w:r><w:fldChar w:fldCharType=""end""/></w:r></w:p>")
    [void]$sb.Append("<w:p><w:r><w:br w:type=""page""/></w:r></w:p>")
    return $sb.ToString()
}

# ── Partes fijas del paquete ─────────────────────────────────────────────────
function Get-Styles {
@"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:docDefaults><w:rPrDefault><w:rPr><w:rFonts w:ascii="Calibri" w:hAnsi="Calibri" w:eastAsia="Calibri" w:cs="Calibri"/><w:color w:val="$Tinta"/><w:sz w:val="21"/><w:szCs w:val="21"/><w:lang w:val="es-BO"/></w:rPr></w:rPrDefault>
  <w:pPrDefault><w:pPr><w:spacing w:after="140" w:line="276" w:lineRule="auto"/></w:pPr></w:pPrDefault></w:docDefaults>

  <w:style w:type="paragraph" w:default="1" w:styleId="Normal"><w:name w:val="Normal"/><w:qFormat/></w:style>

  <w:style w:type="paragraph" w:styleId="Portada"><w:name w:val="Portada"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:before="0" w:after="60" w:line="240" w:lineRule="auto"/></w:pPr>
    <w:rPr><w:rFonts w:ascii="Calibri Light" w:hAnsi="Calibri Light"/><w:b/><w:color w:val="$Tinta"/><w:sz w:val="64"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="PortadaSub"><w:name w:val="PortadaSub"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:before="0" w:after="0" w:line="264" w:lineRule="auto"/><w:ind w:right="2200"/></w:pPr>
    <w:rPr><w:color w:val="$Gris"/><w:sz w:val="26"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Heading1"><w:name w:val="heading 1"/><w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/>
    <w:pPr><w:keepNext/><w:outlineLvl w:val="0"/><w:spacing w:before="420" w:after="160"/>
      <w:pBdr><w:bottom w:val="single" w:sz="8" w:color="$Acento"/></w:pBdr></w:pPr>
    <w:rPr><w:rFonts w:ascii="Calibri Light" w:hAnsi="Calibri Light"/><w:b/><w:color w:val="$Acento"/><w:sz w:val="34"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Heading2"><w:name w:val="heading 2"/><w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/>
    <w:pPr><w:keepNext/><w:outlineLvl w:val="1"/><w:spacing w:before="300" w:after="120"/></w:pPr>
    <w:rPr><w:b/><w:color w:val="$AcentoOsc"/><w:sz w:val="26"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Heading3"><w:name w:val="heading 3"/><w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/>
    <w:pPr><w:keepNext/><w:outlineLvl w:val="2"/><w:spacing w:before="240" w:after="90"/></w:pPr>
    <w:rPr><w:b/><w:color w:val="$Tinta"/><w:sz w:val="22"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Heading4"><w:name w:val="heading 4"/><w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/>
    <w:pPr><w:keepNext/><w:outlineLvl w:val="3"/><w:spacing w:before="200" w:after="80"/></w:pPr>
    <w:rPr><w:b/><w:i/><w:color w:val="$Gris"/><w:sz w:val="21"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Codigo"><w:name w:val="Codigo"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:after="0" w:line="240" w:lineRule="auto"/><w:ind w:left="120"/>
      <w:shd w:val="clear" w:color="auto" w:fill="$FondoCod"/></w:pPr>
    <w:rPr><w:rFonts w:ascii="Consolas" w:hAnsi="Consolas"/><w:color w:val="$Tinta"/><w:sz w:val="17"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Celda"><w:name w:val="Celda"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:before="20" w:after="20" w:line="252" w:lineRule="auto"/></w:pPr>
    <w:rPr><w:sz w:val="18"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="CeldaTitulo"><w:name w:val="CeldaTitulo"/><w:basedOn w:val="Celda"/>
    <w:rPr><w:b/><w:color w:val="$AcentoOsc"/><w:sz w:val="18"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Vineta"><w:name w:val="Vineta"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:after="60"/><w:ind w:left="360" w:hanging="220"/></w:pPr></w:style>

  <w:style w:type="paragraph" w:styleId="Nota"><w:name w:val="Nota"/><w:basedOn w:val="Normal"/>
    <w:rPr><w:sz w:val="19"/><w:color w:val="4A3A14"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Encabezado"><w:name w:val="header"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:after="0"/></w:pPr><w:rPr><w:color w:val="$Gris"/><w:sz w:val="16"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="Pie"><w:name w:val="footer"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:after="0"/></w:pPr><w:rPr><w:color w:val="$Gris"/><w:sz w:val="16"/></w:rPr></w:style>

  <w:style w:type="paragraph" w:styleId="TOC1"><w:name w:val="toc 1"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:before="120" w:after="0"/><w:tabs><w:tab w:val="right" w:leader="dot" w:pos="9360"/></w:tabs></w:pPr>
    <w:rPr><w:b/><w:color w:val="$AcentoOsc"/><w:sz w:val="20"/></w:rPr></w:style>
  <w:style w:type="paragraph" w:styleId="TOC2"><w:name w:val="toc 2"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:after="0"/><w:ind w:left="240"/><w:tabs><w:tab w:val="right" w:leader="dot" w:pos="9360"/></w:tabs></w:pPr>
    <w:rPr><w:sz w:val="19"/></w:rPr></w:style>
  <w:style w:type="paragraph" w:styleId="TOC3"><w:name w:val="toc 3"/><w:basedOn w:val="Normal"/>
    <w:pPr><w:spacing w:after="0"/><w:ind w:left="480"/><w:tabs><w:tab w:val="right" w:leader="dot" w:pos="9360"/></w:tabs></w:pPr>
    <w:rPr><w:color w:val="$Gris"/><w:sz w:val="18"/></w:rPr></w:style>
</w:styles>
"@
}

function Get-Numbering {
@"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:numbering xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:abstractNum w:abstractNumId="0">
    <w:lvl w:ilvl="0"><w:start w:val="1"/><w:numFmt w:val="bullet"/><w:lvlText w:val="&#8226;"/><w:lvlJc w:val="left"/>
      <w:pPr><w:ind w:left="360" w:hanging="220"/></w:pPr>
      <w:rPr><w:rFonts w:ascii="Symbol" w:hAnsi="Symbol" w:hint="default"/><w:color w:val="$Acento"/></w:rPr></w:lvl>
  </w:abstractNum>
  <w:num w:numId="1"><w:abstractNumId w:val="0"/></w:num>
</w:numbering>
"@
}

function Get-Header($meta) {
@"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:hdr xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:p><w:pPr><w:pStyle w:val="Encabezado"/><w:tabs><w:tab w:val="right" w:pos="9360"/></w:tabs>
    <w:pBdr><w:bottom w:val="single" w:sz="4" w:color="$Linea"/></w:pBdr></w:pPr>
    <w:r><w:t xml:space="preserve">$(Esc $meta.Proyecto) &#8212; $(Esc $meta.Titulo)</w:t></w:r>
    <w:r><w:tab/><w:t>v$(Esc $meta.Version)</w:t></w:r></w:p>
</w:hdr>
"@
}

function Get-Footer($meta) {
@"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:ftr xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:p><w:pPr><w:pStyle w:val="Pie"/><w:tabs><w:tab w:val="center" w:pos="4680"/><w:tab w:val="right" w:pos="9360"/></w:tabs>
    <w:pBdr><w:top w:val="single" w:sz="4" w:color="$Linea"/></w:pBdr></w:pPr>
    <w:r><w:t xml:space="preserve">$(Esc $meta.Equipo)</w:t></w:r>
    <w:r><w:tab/><w:t xml:space="preserve">Pagina </w:t></w:r>
    <w:r><w:fldChar w:fldCharType="begin"/></w:r><w:r><w:instrText xml:space="preserve"> PAGE </w:instrText></w:r><w:r><w:fldChar w:fldCharType="separate"/></w:r><w:r><w:t>1</w:t></w:r><w:r><w:fldChar w:fldCharType="end"/></w:r>
    <w:r><w:t xml:space="preserve"> de </w:t></w:r>
    <w:r><w:fldChar w:fldCharType="begin"/></w:r><w:r><w:instrText xml:space="preserve"> NUMPAGES </w:instrText></w:r><w:r><w:fldChar w:fldCharType="separate"/></w:r><w:r><w:t>1</w:t></w:r><w:r><w:fldChar w:fldCharType="end"/></w:r>
    <w:r><w:tab/><w:t>$(Esc $meta.Fecha)</w:t></w:r></w:p>
</w:ftr>
"@
}

# ── Armado del paquete ───────────────────────────────────────────────────────
# Escribe el paquete entrada por entrada. NO se usa ZipFile::CreateFromDirectory:
# en .NET Framework guarda los nombres con "\", y tanto la especificacion ZIP
# como OPC exigen "/". Word suele tolerarlo, pero otras herramientas no.
function Write-Paquete([hashtable]$partes, [string]$salida) {
    if (Test-Path $salida) { Remove-Item $salida -Force }
    $fs  = [System.IO.File]::Open($salida, [System.IO.FileMode]::CreateNew)
    $zip = New-Object System.IO.Compression.ZipArchive($fs, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        # [Content_Types].xml va primero: es lo que espera un lector OPC estricto.
        $orden = @("[Content_Types].xml") + ($partes.Keys | Where-Object { $_ -ne "[Content_Types].xml" } | Sort-Object)
        foreach ($nombre in $orden) {
            $entrada = $zip.CreateEntry($nombre, [System.IO.Compression.CompressionLevel]::Optimal)
            $sw = New-Object System.IO.StreamWriter($entrada.Open(), (New-Object System.Text.UTF8Encoding($false)))
            $sw.Write($partes[$nombre])
            $sw.Flush(); $sw.Dispose()
        }
    }
    finally { $zip.Dispose(); $fs.Dispose() }
}

function New-Documento($meta, [string]$markdown, [string]$salida) {
    $cuerpo  = (New-Portada $meta)
    $cuerpo += (New-ControlVersiones $meta)
    $cuerpo += (ConvertFrom-Markdown $markdown)

    $sectPr = "<w:sectPr><w:headerReference w:type=""default"" r:id=""rIdHdr""/><w:footerReference w:type=""default"" r:id=""rIdFtr""/><w:titlePg/><w:pgSz w:w=""12240"" w:h=""15840""/><w:pgMar w:top=""1418"" w:right=""1440"" w:bottom=""1418"" w:left=""1440"" w:header=""709"" w:footer=""709"" w:gutter=""0""/><w:cols w:space=""708""/></w:sectPr>"

    $doc = "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" +
           "<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">" +
           "<w:body>$cuerpo$sectPr</w:body></w:document>"

    $ct = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
  <Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>
  <Override PartName="/word/numbering.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"/>
  <Override PartName="/word/settings.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml"/>
  <Override PartName="/word/header1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml"/>
  <Override PartName="/word/footer1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"/>
  <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
  <Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
</Types>
"@

    $rels = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
</Relationships>
"@

    $docRels = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rIdSty" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
  <Relationship Id="rIdNum" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering" Target="numbering.xml"/>
  <Relationship Id="rIdSet" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings" Target="settings.xml"/>
  <Relationship Id="rIdHdr" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/header" Target="header1.xml"/>
  <Relationship Id="rIdFtr" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer" Target="footer1.xml"/>
</Relationships>
"@

    # updateFields hace que Word ofrezca actualizar el indice al abrir.
    $settings = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:settings xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:updateFields w:val="true"/>
  <w:defaultTabStop w:val="708"/>
</w:settings>
"@

    $core = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:dcterms="http://purl.org/dc/terms/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <dc:title>$(Esc $meta.Titulo)</dc:title>
  <dc:subject>$(Esc $meta.Subtitulo)</dc:subject>
  <dc:creator>$(Esc $meta.Autor)</dc:creator>
  <cp:lastModifiedBy>$(Esc $meta.Autor)</cp:lastModifiedBy>
  <cp:revision>$(Esc $meta.Version)</cp:revision>
  <cp:category>$(Esc $meta.Proyecto)</cp:category>
  <cp:contentStatus>$(Esc $meta.Estado)</cp:contentStatus>
  <dcterms:created xsi:type="dcterms:W3CDTF">$($meta.FechaIso)</dcterms:created>
  <dcterms:modified xsi:type="dcterms:W3CDTF">$($meta.FechaIso)</dcterms:modified>
</cp:coreProperties>
"@

    $app = @"
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
  <Application>YastaCoin DocGen</Application>
  <Company>$(Esc $meta.Equipo)</Company>
  <AppVersion>1.0000</AppVersion>
</Properties>
"@

    $partes = @{
        "[Content_Types].xml"           = $ct
        "_rels/.rels"                   = $rels
        "word/document.xml"             = $doc
        "word/_rels/document.xml.rels"  = $docRels
        "word/styles.xml"               = (Get-Styles)
        "word/numbering.xml"            = (Get-Numbering)
        "word/settings.xml"             = $settings
        "word/header1.xml"              = (Get-Header $meta)
        "word/footer1.xml"              = (Get-Footer $meta)
        "docProps/core.xml"             = $core
        "docProps/app.xml"              = $app
    }

    Write-Paquete $partes $salida
    return $salida
}
