# Builds assets/emblems/symbols/emblem_NN.png and assets/emblems/symbols.csv from the game-icons.net
# PNG archive (white on transparent, 512 x 512, CC BY 3.0).
#
# Each icon is cropped to its shape, centred, and scaled so its larger side spans $Fill of a square
# canvas. The banner sizes symbols by image width, so a uniform margin keeps all symbols the same
# visual size. The order of $Symbols is the symbol id players type: never reorder or remove entries
# once released, only append.
#
# Usage (from the repo root, Windows PowerShell): powershell -File tools/make_symbols.ps1

param(
    [string]$Archive = 'assets/emblems/source/game-icons.net.png.zip',
    [int]$Size = 256,
    [double]$Fill = 0.8
)

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)

# Picked by eye from the icons that are close to mirror-symmetric (the banner shows the left half
# twice), grouped by theme. A name several authors use is written 'author/name'.
$Symbols = @(
    # Beasts
    'delapouite/bat', 'batwing-emblem', 'bear-face', 'badger', 'boar-tusks', 'fox-head', 'lynx-head',
    'walrus-head', 'bull', 'bull-horns', 'aries', 'minotaur', 'deer-head', 'stag-head', 'owl',
    'eagle-emblem', 'condor-emblem', 'hawk-emblem', 'freedom-dove', 'angular-spider', 'flying-beetle',
    'turtle', 'squid-head', 'horned-reptile', 'gargoyle', 'devil-mask', 'flat-paw-print',
    # Skulls
    'death-skull', 'skull-crossed-bones', 'horned-skull', 'crowned-skull', 'daemon-skull',
    'skull-sabertooth', 'desert-skull', 'animal-skull',
    # Crowns and regalia
    'imperial-crown', 'jewel-crown', 'queen-crown', 'crenel-crown', 'laurel-crown', 'sharp-crown',
    'crowned-heart', 'winged-scepter', 'jeweled-chalice', 'chalice-drops',
    # Folk
    'old-king', 'dwarf-face', 'beard',
    # Strongholds
    'delapouite/castle', 'elven-castle', 'guarded-tower', 'white-tower', 'locked-fortress', 'medieval-pavilion',
    'evil-tower', 'viking-church', 'viking-longhouse',
    # Helms
    'viking-helmet', 'horned-helm', 'elf-helmet', 'black-knight-helm', 'crested-helmet', 'visored-helm',
    'heavy-helm', 'dwarf-helmet', 'gauls-helm', 'centurion-helmet', 'warlord-helmet', 'overlord-helm',
    # Arms
    'crossed-axes', 'crossed-sabres', 'crossed-swords', 'swords-emblem', 'winged-sword', 'sword-altar',
    'daggers', 'crossed-bones',
    # Shields
    'viking-shield', 'templar-shield', 'heart-shield', 'winged-shield', 'cross-shield', 'rosa-shield',
    'trident-shield', 'black-hand-shield', 'layered-armor',
    # Heraldic charges
    'fleur-de-lys', 'trefoil-lily', 'ermine', 'scallop', 'iron-cross', 'hell-crosses', 'jerusalem-cross',
    'camargue-cross', 'gothic-cross', 'sbed/clover', 'clover-spiked', 'clubs',
    # Plants
    'flower-emblem', 'fire-flower', 'jasmine', 'oat', 'sprout',
    # Stars, suns and moons
    'polar-star', 'seven-pointed-star', 'heptagram', 'justice-star', 'spikes', 'spikes-init',
    'spikes-full', 'azul-flake', 'sunbeams', 'lorc/sunrise', 'nested-eclipses',
    # Land and sea
    'valley', 'zigzag-hieroglyph', 'sail', 'paddles', 'ship-wheel',
    # Hearth and craft
    'lorc/cauldron', 'fire-bowl', 'anvil', 'lyre', 'horseshoe', 'hourglass', 'scales', 'fire',
    # Signs
    'fireflake', 'tension-snowflake', 'bowen-knot', 'triquetra', 'totem-head', 'hearts', 'evil-love',
    # Wings
    'angel-wings', 'winged-emblem', 'steelwing-emblem',
    # Banners
    'knight-banner'
)

if (-not (Test-Path $Archive)) {
    New-Item -ItemType Directory -Force (Split-Path $Archive) | Out-Null
    Invoke-WebRequest 'https://game-icons.net/archives/png/zip/ffffff/transparent/game-icons.net.png.zip' -UseBasicParsing -OutFile $Archive
}

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System; using System.Drawing; using System.Drawing.Drawing2D; using System.Drawing.Imaging; using System.IO;
public static class SymbolPrep {
    // Crops to pixels with alpha, then draws the crop centred on a transparent square canvas with its
    // larger side spanning fill of the canvas.
    public static void Make(Stream png, string outPath, int size, double fill) {
        using (var src = new Bitmap(png)) {
            int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
            var data = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var px = new int[src.Width * src.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);
            src.UnlockBits(data);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    if (((px[y * src.Width + x] >> 24) & 255) > 0) {
                        if (x < minX) minX = x; if (x > maxX) maxX = x;
                        if (y < minY) minY = y; if (y > maxY) maxY = y;
                    }
            if (maxX < 0) throw new Exception("empty icon");
            int w = maxX - minX + 1, h = maxY - minY + 1;
            double scale = fill * size / Math.Max(w, h);
            float dw = (float)(w * scale), dh = (float)(h * scale);
            using (var dst = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(dst)) {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(src, new RectangleF((size - dw) / 2f, (size - dh) / 2f, dw, dh),
                    new RectangleF(minX, minY, w, h), GraphicsUnit.Pixel);
                dst.Save(outPath, ImageFormat.Png);
            }
        }
    }
}
'@

$outDir = 'assets/emblems/symbols'
if (Test-Path $outDir) { Remove-Item "$outDir/*.png" }
New-Item -ItemType Directory -Force $outDir | Out-Null

$zip = [IO.Compression.ZipFile]::OpenRead((Resolve-Path $Archive))
try {
    # Entries are icons/ffffff/transparent/1x1/<author>/<name>.png.
    $entries = @{}
    foreach ($e in $zip.Entries) {
        if ($e.FullName -notmatch '/([^/]+)/([^/]+)\.png$') { continue }
        $item = @{ Entry = $e; Author = $Matches[1]; Name = $Matches[2] }
        $entries["$($item.Author)/$($item.Name)"] = @($item)
        $entries[$item.Name] = @($entries[$item.Name]) + $item | Where-Object { $_ }
    }

    $rows = @('id,name,author')
    for ($id = 0; $id -lt $Symbols.Count; $id++) {
        $spec = $Symbols[$id]
        $found = @($entries[$spec])
        if ($found.Count -eq 0) { throw "Icon '$spec' not in the archive" }
        if ($found.Count -gt 1) { throw "Icon '$spec' exists from several authors ($(($found | ForEach-Object { $_.Author }) -join ', ')); write it as 'author/name'" }
        $item = $found[0]
        $name = $item.Name
        $stream = $item.Entry.Open()
        try {
            [SymbolPrep]::Make($stream, (Join-Path (Resolve-Path $outDir) ('emblem_{0:D2}.png' -f $id)), $Size, $Fill)
        } finally { $stream.Dispose() }
        $rows += "$id,$name,$($item.Author)"
    }
    Set-Content 'assets/emblems/symbols.csv' $rows -Encoding ascii
    Write-Host "Wrote $($Symbols.Count) symbols to $outDir"
} finally {
    $zip.Dispose()
}
