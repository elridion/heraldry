# Builds screenshots/symbols.png, the symbol overview shown on the mod page: every symbol as the
# banner paints it (left half mirrored at the image's own mirror axis, like EmblemLibrary), with its
# number. Run after make_symbols.ps1 whenever symbols are added.
#
# Usage (from the repo root, Windows PowerShell): powershell -File tools/make_symbol_sheet.ps1

param(
    [int]$Columns = 12,
    [int]$Tile = 80
)

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)

Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System; using System.Drawing; using System.Drawing.Drawing2D; using System.Drawing.Imaging; using System.Drawing.Text;
public static class SymbolSheet {
    // Same values as EmblemLibrary.
    const int ShapeAlpha = 30;
    const float AxisSearchRange = 4f;
    static readonly Color Ground = Color.FromArgb(46, 26, 22), Cloth = Color.FromArgb(122, 30, 28), Label = Color.FromArgb(232, 196, 120);

    public static void Make(string[] paths, int[] ids, string outPath, int columns, int tile) {
        int label = tile / 4, rows = (paths.Length + columns - 1) / columns, pad = tile / 16;
        using (var sheet = new Bitmap(columns * tile, rows * (tile + label), PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(sheet))
        using (var font = new Font("Segoe UI", label * 0.62f, FontStyle.Bold, GraphicsUnit.Pixel))
        using (var cloth = new SolidBrush(Cloth))
        using (var text = new SolidBrush(Label))
        using (var centred = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) {
            g.Clear(Ground);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            for (int i = 0; i < paths.Length; i++) {
                int x = (i % columns) * tile, y = (i / columns) * (tile + label);
                g.FillRectangle(cloth, x + pad, y + pad, tile - 2 * pad, tile - 2 * pad);
                using (var art = Mirrored(paths[i]))
                    g.DrawImage(art, x + 2 * pad, y + 2 * pad, tile - 4 * pad, tile - 4 * pad);
                g.DrawString(ids[i].ToString(), font, text, new RectangleF(x, y + tile - pad / 2, tile, label), centred);
            }
            sheet.Save(outPath, ImageFormat.Png);
        }
    }

    // The banner view: the left half of the image mirrored at its mirror axis.
    static Bitmap Mirrored(string path) {
        using (var src = new Bitmap(path)) {
            int w = src.Width, h = src.Height;
            var data = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var px = new int[w * h];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, px, 0, px.Length);
            src.UnlockBits(data);
            float middle = (w - 1) * 0.5f, axis = middle; long best = long.MaxValue;
            for (float a = middle - AxisSearchRange; a <= middle + AxisSearchRange; a += 0.5f) {
                long score = 0;
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++) {
                        bool inside = ((px[y * w + x] >> 24) & 255) > ShapeAlpha;
                        int mx = (int)Math.Round(2f * a - x);
                        bool mirrored = mx >= 0 && mx < w && ((px[y * w + mx] >> 24) & 255) > ShapeAlpha;
                        if (inside != mirrored) score++;
                    }
                if (score < best) { best = score; axis = a; }
            }
            var outPx = new int[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) {
                    int sx = x <= axis ? x : (int)Math.Round(2f * axis - x);
                    // White shape, keeping the art's alpha: the mod only uses alpha too.
                    outPx[y * w + x] = sx >= 0 && sx < w ? (px[y * w + sx] & unchecked((int)0xFF000000)) | 0xFFFFFF : 0;
                }
            var dst = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var d = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(outPx, 0, d.Scan0, outPx.Length);
            dst.UnlockBits(d);
            return dst;
        }
    }
}
'@

$files = Get-ChildItem 'assets/emblems/symbols/emblem_*.png' |
    ForEach-Object { [pscustomobject]@{ Id = [int]($_.BaseName -replace '^emblem_', ''); Path = $_.FullName } } |
    Sort-Object Id
New-Item -ItemType Directory -Force 'screenshots' | Out-Null
$out = Join-Path (Resolve-Path 'screenshots') 'symbols.png'
[SymbolSheet]::Make([string[]]$files.Path, [int[]]$files.Id, $out, $Columns, $Tile)
Write-Host "Wrote $($files.Count) symbols to $out"
