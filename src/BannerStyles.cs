using System.Collections.Generic;
using UnityEngine;

namespace Heraldry
{
    /// <summary>
    /// How the symbol looks on each vanilla banner: its colour (white, gold or black, picked by eye per
    /// banner), brightness and width. Banners with embroidered trim along the edges keep the symbol narrow
    /// enough to clear it; the striped banners have no trim and take a larger symbol.
    /// </summary>
    internal static class BannerStyles
    {
        // Symbol width as a share of the whole cloth width.
        private const float NormalWidth = 0.68f;
        // Widths are of the symbol image, whose shape spans at most 80% of it (tools/make_symbols.ps1),
        // so the image may be wider than the cloth: at 1.05 the widest shapes still clear the edges.
        private const float LargeWidth = 1.05f;
        // Flat symbols at full colour glare on the cloth, so the colour is scaled down; black stays black.
        // The blue banner's darker cloth swallows a dimmed gold, so it gets a brighter symbol; on the
        // green banner gold looks dull at normal brightness but glares at the blue banner's.
        public const float NormalBrightness = 0.7f;
        private const float MediumBrightness = 0.85f;
        private const float BrightBrightness = 0.95f;

        private static readonly Color s_white = new Color32(242, 240, 234, 255);
        private static readonly Color s_gold = new Color32(214, 170, 74, 255);
        // Dark charcoal rather than black: brightness scales it down further, and pure black read as a
        // hole in the cloth.
        private static readonly Color s_black = new Color32(64, 58, 54, 255);

        // Keyed by vanilla prefab name.
        private static readonly Dictionary<string, BannerStyle> s_styles = new Dictionary<string, BannerStyle>
        {
            { "piece_banner01", new BannerStyle(s_white, NormalWidth) }, // black, white knotwork
            { "piece_banner02", new BannerStyle(s_gold, NormalWidth, BrightBrightness) }, // blue, gold scrollwork
            { "piece_banner03", new BannerStyle(s_black, LargeWidth) },  // white with red stripes
            { "piece_banner04", new BannerStyle(s_white, NormalWidth) }, // dark red, white chain trim
            { "piece_banner05", new BannerStyle(s_gold, NormalWidth, MediumBrightness) }, // green, gold scrollwork
            { "piece_banner06", new BannerStyle(s_black, LargeWidth) },  // blue, red and white stripes
            { "piece_banner07", new BannerStyle(s_black, LargeWidth) },  // white with light-blue stripes
            { "piece_banner08", new BannerStyle(s_black, NormalWidth) }, // yellow, dark gold trim
            { "piece_banner09", new BannerStyle(s_white, NormalWidth) }, // purple, white wavy trim
            { "piece_banner10", new BannerStyle(s_white, NormalWidth) }, // orange, light gold wavy trim
            { "piece_banner11", new BannerStyle(s_black, NormalWidth) }, // off-white, grey knotwork
        };

        // Banners a game update adds aren't in the table yet; assume they have trim.
        private static readonly BannerStyle s_fallback = new BannerStyle(s_white, NormalWidth);

        public static BannerStyle For(string prefabName) => s_styles.TryGetValue(prefabName, out var style) ? style : s_fallback;
    }

    internal readonly struct BannerStyle
    {
        public readonly Color Color;
        public readonly float Width;
        public readonly float Brightness;

        public BannerStyle(Color color, float width, float brightness = BannerStyles.NormalBrightness)
        {
            Color = color;
            Width = width;
            Brightness = brightness;
        }
    }
}
