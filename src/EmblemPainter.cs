using System.Collections.Generic;
using UnityEngine;

namespace Heraldry
{
    /// <summary>
    /// Builds and shares the symbol overlay: an extra, alpha-clipped copy of the cloth material whose
    /// texture holds only the symbol. Unity draws a renderer's surplus materials over its last submesh,
    /// so the vanilla cloth keeps its own texture at full resolution and the symbol is drawn on top
    /// through the same UVs. Banners showing the same symbol on the same base banner share one overlay;
    /// it is destroyed when the last banner releases it, so typing in the emblem box (a new symbol per
    /// keystroke) doesn't pile up textures.
    /// </summary>
    internal static class EmblemPainter
    {
        // Symbol centre as a share of the cloth height from the top. The top ~8% is behind the crossbar,
        // and the black and white banners carry ornaments near the top; this keeps the symbol below them.
        // Width, colour and brightness come from BannerStyles.
        private const float SymbolCenterFromTop = 0.4f;
        // Width of the overlay's half-cloth design in pixels, about 3x the symbol art's own resolution there,
        // so the clipped edge follows the art's smooth outline instead of its pixel steps.
        private const int DesignWidth = 256;
        private const float AlphaCutoff = 0.5f;

        private static readonly Dictionary<string, Overlay> s_overlays = new Dictionary<string, Overlay>();
        private static bool s_warnedNoCutoff;

        private sealed class Overlay
        {
            public Material[] Materials;
            public Material Material;
            public Texture2D Texture;
            public int Users;
        }

        public static bool CanPaint => SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;

        public static string Key(BannerInfo info, int cloth, EmblemCode code) => info.PrefabName + "#" + cloth + "#" + code;

        // The cloth's vanilla material plus the symbol overlay, or null when there is nothing to paint
        // (or no graphics device, as on a dedicated server). Pair every non-null result with Release(key).
        // BannerPieces.FindCloth only hands over single-material cloth.
        public static Material[] Acquire(string key, BannerInfo info, int cloth, Material[] vanilla, EmblemCode code)
        {
            if (code.IsEmpty || !CanPaint || vanilla.Length != 1 || !vanilla[0] || !vanilla[0].mainTexture) return null;
            if (s_overlays.TryGetValue(key, out var overlay))
            {
                overlay.Users++;
                return overlay.Materials;
            }

            // The overlay only works if the cloth shader can clip by alpha; otherwise it would cover the
            // whole cloth.
            var source = vanilla[0];
            if (!source.HasProperty("_Cutoff"))
            {
                if (!s_warnedNoCutoff) Jotunn.Logger.LogWarning("Banner cloth shader has no alpha cutoff; emblems are disabled.");
                s_warnedNoCutoff = true;
                return null;
            }

            var symbol = EmblemLibrary.GetSymbol(code.Symbol);
            if (symbol == null) return null;
            var texture = Paint(symbol, BannerStyles.For(info.PrefabName), info.UvRects[cloth], source.mainTexture);
            var material = MakeOverlayMaterial(source, texture);

            var materials = new[] { vanilla[0], material };
            s_overlays[key] = new Overlay { Materials = materials, Material = material, Texture = texture, Users = 1 };
            return materials;
        }

        public static void Release(string key)
        {
            if (key == null || !s_overlays.TryGetValue(key, out var overlay)) return;
            if (--overlay.Users > 0) return;
            s_overlays.Remove(key);
            Object.Destroy(overlay.Material);
            Object.Destroy(overlay.Texture);
        }

        // The half-cloth design takes the proportions of the vanilla texture's UV region, so the symbol
        // keeps the same aspect on the cloth as the vanilla design does.
        private static Texture2D Paint(EmblemArt symbol, BannerStyle style, Rect uvRect, Texture vanilla)
        {
            var tint = style.Color * style.Brightness;
            tint.a = 1f;
            float regionAspect = vanilla.height * uvRect.height / Mathf.Max(1f, vanilla.width * uvRect.width);
            int width = DesignWidth;
            int height = Mathf.Clamp(Mathf.RoundToInt(width * regionAspect), 1, 4096);

            // Transparent pixels still carry the tint, so filtering at the clipped edge doesn't darken it.
            var clear = (Color32)new Color(tint.r, tint.g, tint.b, 0f);
            var design = new Color32[width * height];
            for (int i = 0; i < design.Length; i++) design[i] = clear;

            float scale = style.Width * width * 2f / symbol.Width;
            Stamp(design, width, height, symbol, scale, SymbolCenterFromTop * height, tint);
            return BuildAtlas(design, width, height, uvRect);
        }

        // Draws the left half of the art centred on the design's right edge, so the mirrored cloth shows
        // it whole. Asymmetric art comes out symmetric: its left half twice. Only the art's alpha is used:
        // its grey shading is a baked-in bevel that made the symbols look like metal plates, and the
        // symbol should read as flat embroidery in the banner's colour.
        private static void Stamp(Color32[] design, int width, int height, EmblemArt art, float scale, float centerRow, Color tint)
        {
            float halfHeight = art.Height * scale * 0.5f;
            int rowMin = Mathf.Max(0, Mathf.FloorToInt(centerRow - halfHeight));
            int rowMax = Mathf.Min(height - 1, Mathf.CeilToInt(centerRow + halfHeight));
            int xMin = Mathf.Max(0, Mathf.FloorToInt(width - (art.Axis + 1f) * scale));

            for (int row = rowMin; row <= rowMax; row++)
            {
                float sy = (row + 0.5f - centerRow) / scale + (art.Height - 1) * 0.5f;
                int y = height - 1 - row;
                for (int x = xMin; x < width; x++)
                {
                    float fromCentre = width - (x + 0.5f);
                    var source = art.Sample(art.Axis - fromCentre / scale, sy);
                    if (source.a <= 0f) continue;

                    var ink = tint;
                    ink.a = source.a;
                    design[y * width + x] = ink;
                }
            }
        }

        // Places the design at the cloth's UV rect, sized so the design keeps its resolution there.
        // Outside the rect the design's edge texels are repeated. Transparent texels there would halve the
        // filtered alpha at the rect's edge, which is the mirror line through the symbol's middle, right at
        // the clip threshold, and could open a hairline gap down the centre.
        private static Texture2D BuildAtlas(Color32[] design, int designWidth, int designHeight, Rect uvRect)
        {
            int width = Mathf.Clamp(Mathf.RoundToInt(designWidth / uvRect.width), designWidth, 4096);
            int height = Mathf.Clamp(Mathf.RoundToInt(designHeight / uvRect.height), designHeight, 4096);
            int x0 = Mathf.FloorToInt(uvRect.xMin * width), x1 = Mathf.CeilToInt(uvRect.xMax * width);
            int y0 = Mathf.FloorToInt(uvRect.yMin * height), y1 = Mathf.CeilToInt(uvRect.yMax * height);

            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                int dy = Mathf.Clamp(Mathf.FloorToInt((y + 0.5f - y0) / (y1 - y0) * designHeight), 0, designHeight - 1);
                for (int x = 0; x < width; x++)
                {
                    int dx = Mathf.Clamp(Mathf.FloorToInt((x + 0.5f - x0) / (x1 - x0) * designWidth), 0, designWidth - 1);
                    pixels[y * width + x] = design[dy * designWidth + dx];
                }
            }

            var atlas = new Texture2D(width, height, TextureFormat.RGBA32, true, false)
            {
                name = "heraldry_symbol",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4,
            };
            atlas.SetPixels32(pixels);
            // Not readable afterwards: frees the CPU copy.
            atlas.Apply(true, true);
            return atlas;
        }

        // A copy of the cloth material that draws only the symbol: alpha-clipped, untinted (the tint is in
        // the texture), and drawn after the opaque cloth. The vanilla normal map stays, so the symbol
        // follows the cloth's weave like embroidery.
        private static Material MakeOverlayMaterial(Material source, Texture2D texture)
        {
            var material = new Material(source) { name = source.name + "_heraldry", mainTexture = texture };
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            material.SetFloat("_Cutoff", AlphaCutoff);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            return material;
        }
    }
}
