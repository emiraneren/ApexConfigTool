Add-Type -AssemblyName System.Drawing

function New-IconFrame([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    # Koyu graphite arka plan, yuvarlatilmis kare
    $bgColor = [System.Drawing.Color]::FromArgb(255, 11, 13, 16)
    $bgBrush = New-Object System.Drawing.SolidBrush $bgColor
    $radius = [Math]::Max(2, [int]($size * 0.22))
    $rect = New-Object System.Drawing.Rectangle 0, 0, $size, $size

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $radius * 2
    $path.AddArc(0, 0, $d, $d, 180, 90)
    $path.AddArc($size - $d, 0, $d, $d, 270, 90)
    $path.AddArc($size - $d, $size - $d, $d, $d, 0, 90)
    $path.AddArc(0, $size - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    $g.FillPath($bgBrush, $path)

    # Amber vurgu karesi (uygulamadaki logo rozetiyle ayni)
    $accentColor = [System.Drawing.Color]::FromArgb(255, 255, 145, 56)
    $accentBrush = New-Object System.Drawing.SolidBrush $accentColor
    $inset = [int]($size * 0.16)
    $accentSize = $size - ($inset * 2)
    $accentRadius = [Math]::Max(2, [int]($accentSize * 0.28))
    $ad = $accentRadius * 2

    $accentPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $accentPath.AddArc($inset, $inset, $ad, $ad, 180, 90)
    $accentPath.AddArc($inset + $accentSize - $ad, $inset, $ad, $ad, 270, 90)
    $accentPath.AddArc($inset + $accentSize - $ad, $inset + $accentSize - $ad, $ad, $ad, 0, 90)
    $accentPath.AddArc($inset, $inset + $accentSize - $ad, $ad, $ad, 90, 90)
    $accentPath.CloseFigure()
    $g.FillPath($accentBrush, $accentPath)

    # "A" harfi
    $fontSize = [float]($size * 0.52)
    $font = New-Object System.Drawing.Font "Segoe UI", $fontSize, ([System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 26, 18, 6))
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $g.DrawString("A", $font, $textBrush, [float]($size / 2), [float]($size / 2 + $size * 0.02), $format)

    $g.Dispose()
    return $bmp
}

function ConvertTo-PngBytes([System.Drawing.Bitmap]$bmp) {
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    # Unary comma: PowerShell dizileri pipeline'da otomatik duzlestirir;
    # bu, byte[] butunlugunu koruyup tek nesne olarak donmesini saglar.
    return ,$ms.ToArray()
}

$sizes = @(16, 32, 48, 64, 128, 256)
$images = @()

foreach ($s in $sizes) {
    $bmp = New-IconFrame $s
    $png = ConvertTo-PngBytes $bmp
    $images += [PSCustomObject]@{ Size = $s; Png = $png }
    $bmp.Dispose()
}

$outPath = Join-Path $PSScriptRoot "..\AppIcon.ico"
$fs = [System.IO.File]::Open($outPath, [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter $fs

# ICONDIR
$bw.Write([UInt16]0)       # reserved
$bw.Write([UInt16]1)       # type = icon
$bw.Write([UInt16]$images.Count)

$headerSize = 6 + (16 * $images.Count)
$offset = $headerSize

foreach ($img in $images) {
    $w = if ($img.Size -ge 256) { 0 } else { $img.Size }
    $h = $w
    $bw.Write([Byte]$w)
    $bw.Write([Byte]$h)
    $bw.Write([Byte]0)     # color count
    $bw.Write([Byte]0)     # reserved
    $bw.Write([UInt16]1)   # planes
    $bw.Write([UInt16]32)  # bit count
    $bw.Write([UInt32]$img.Png.Length)
    $bw.Write([UInt32]$offset)
    $offset += $img.Png.Length
}

foreach ($img in $images) {
    $bw.Write([byte[]]$img.Png)
}

$bw.Flush()
$bw.Close()
$fs.Close()

Write-Output "Icon written: $outPath"
