# Builds package/icon.png (256 x 256, the Thunderstore/Hexium mod icon): a red banner hanging from a
# wooden crossbar, with one of the mod's own symbols on it.
#
# Usage (from the repo root, Windows PowerShell): powershell -File tools/make_icon.ps1 [-Symbol 57]

param(
    [int]$Symbol = 57,
    [string]$Out = 'package/icon.png'
)

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)
Add-Type -AssemblyName System.Drawing

$size = 256
$icon = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($icon)
$g.SmoothingMode = 'AntiAlias'
$g.InterpolationMode = 'HighQualityBicubic'
$g.PixelOffsetMode = 'HighQuality'
$g.Clear([System.Drawing.Color]::Transparent)

# Cloth: a tall strip ending in a point, like the vanilla banners.
$left = 58; $right = 198; $top = 26; $shoulder = 206; $tip = 246
$cloth = New-Object System.Drawing.Drawing2D.GraphicsPath
$cloth.AddPolygon([System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF $left, $top),
    (New-Object System.Drawing.PointF $right, $top),
    (New-Object System.Drawing.PointF $right, $shoulder),
    (New-Object System.Drawing.PointF (($left + $right) / 2), $tip),
    (New-Object System.Drawing.PointF $left, $shoulder)))
$clothBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush (
    (New-Object System.Drawing.Point $left, 0), (New-Object System.Drawing.Point $right, 0),
    [System.Drawing.Color]::FromArgb(255, 112, 22, 22), [System.Drawing.Color]::FromArgb(255, 112, 22, 22))
$blend = New-Object System.Drawing.Drawing2D.ColorBlend 3
$blend.Colors = @([System.Drawing.Color]::FromArgb(255, 96, 18, 18), [System.Drawing.Color]::FromArgb(255, 150, 32, 30), [System.Drawing.Color]::FromArgb(255, 96, 18, 18))
$blend.Positions = @(0.0, 0.5, 1.0)
$clothBrush.InterpolationColors = $blend
$g.FillPath($clothBrush, $cloth)

# Trim: a thin light line inset along the cloth's edge, as on the trimmed vanilla banners.
$inset = 9
$trim = New-Object System.Drawing.Drawing2D.GraphicsPath
$trim.AddPolygon([System.Drawing.PointF[]]@(
    (New-Object System.Drawing.PointF ($left + $inset), ($top + 14)),
    (New-Object System.Drawing.PointF ($right - $inset), ($top + 14)),
    (New-Object System.Drawing.PointF ($right - $inset), ($shoulder - 3)),
    (New-Object System.Drawing.PointF (($left + $right) / 2), ($tip - 13)),
    (New-Object System.Drawing.PointF ($left + $inset), ($shoulder - 3))))
$g.DrawPath((New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(200, 230, 222, 205)), 2.5), $trim)

# Symbol, tinted off-white like the in-game white symbols.
$art = [System.Drawing.Image]::FromFile((Resolve-Path ('assets/emblems/symbols/emblem_{0:D2}.png' -f $Symbol)))
$matrix = New-Object System.Drawing.Imaging.ColorMatrix
$matrix.Matrix00 = 236 / 255; $matrix.Matrix11 = 230 / 255; $matrix.Matrix22 = 214 / 255
$attributes = New-Object System.Drawing.Imaging.ImageAttributes
$attributes.SetColorMatrix($matrix)
$symbolSize = 118
$sx = [int](($left + $right - $symbolSize) / 2); $sy = 62
$g.DrawImage($art, (New-Object System.Drawing.Rectangle $sx, $sy, $symbolSize, $symbolSize), 0, 0, $art.Width, $art.Height, 'Pixel', $attributes)
$art.Dispose()

# Crossbar with darker end caps.
$wood = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 110, 76, 46))
$woodDark = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 62, 42, 26))
$g.FillRectangle($wood, 34, 14, 188, 16)
$g.FillRectangle($woodDark, 34, 26, 188, 4)
$g.FillEllipse($woodDark, 26, 12, 20, 20)
$g.FillEllipse($woodDark, 210, 12, 20, 20)

$g.Dispose()
$icon.Save((Join-Path (Get-Location) $Out), [System.Drawing.Imaging.ImageFormat]::Png)
$icon.Dispose()
Write-Host "Wrote $Out with symbol $Symbol"
