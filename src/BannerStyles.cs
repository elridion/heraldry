using System.Collections.Generic;
using UnityEngine;

namespace Heraldry
{
    /// <summary>
    /// How the symbol looks on each vanilla banner: its colour (white, gold or black, picked per
    /// banner) and width. Banners with embroidered trim along the edges keep the symbol narrow enough to
    /// clear it; the striped banners have no trim and take a larger symbol. Their cloth is part off-white,
    /// part dark red or blue stripes, so no single colour reads on both: the symbol is counterchanged,
    /// in the stripe colour over the off-white and off-white over the stripes.
    /// </summary>
    internal static class BannerStyles
    {
        // Symbol width as a share of the whole cloth width.
        private const float NormalWidth = 0.68f;
        // The gold scrollwork of the blue and green banners reaches further in than the other trims.
        private const float ScrollworkWidth = 0.58f;
        // Widths are of the symbol image, whose shape spans at most 80% of it (tools/make_symbols.ps1),
        // so the image may be wider than the cloth: at 1.05 the widest shapes still clear the edges.
        private const float LargeWidth = 1.05f;

        // Edges of the striped banners' stripes, measured from the cloth's centre line as a share of half
        // the cloth width, read from their textures. All three share the layout: an off-white middle, a
        // red or blue stripe, then an outer band (off-white, or blue on banner 06).
        private const float StripeInner = 0.26f;
        private const float StripeOuter = 0.635f;

        // Flat symbols in bright colours glare on the cloth, so white and black are toned down (to 70%
        // of the colour first picked).
        private static readonly Color s_white = new Color32(169, 168, 164, 255);
        // The gold scrollwork of the blue and of the green banner, read from their cloth textures. The two
        // differ, and any shared or dimmed gold looked olive green next to the embroidery in game.
        private static readonly Color s_blueBannerGold = new Color32(245, 168, 16, 255);
        private static readonly Color s_greenBannerGold = new Color32(189, 125, 48, 255);
        // Dark charcoal rather than black: pure black read as a hole in the cloth.
        private static readonly Color s_black = new Color32(45, 41, 38, 255);
        // The striped banners' own off-white (the median of each texture's white) and stripe colours, so
        // the counterchanged symbol looks cut from the field. Each banner's off-white differs a little.
        private static readonly Color s_redStripedWhite = new Color32(179, 162, 161, 255);
        private static readonly Color s_tricolourWhite = new Color32(178, 159, 157, 255);
        private static readonly Color s_blueStripedWhite = new Color32(162, 146, 146, 255);
        private static readonly Color s_stripeRed = new Color32(161, 12, 2, 255);
        private static readonly Color s_stripeBlue = new Color32(1, 66, 143, 255);

        // Keyed by vanilla prefab name.
        private static readonly Dictionary<string, BannerStyle> s_styles = new Dictionary<string, BannerStyle>
        {
            { "piece_banner01", new BannerStyle(s_white, NormalWidth) }, // black, white knotwork
            { "piece_banner02", new BannerStyle(s_blueBannerGold, ScrollworkWidth) }, // blue, gold scrollwork
            { "piece_banner03", new BannerStyle(s_stripeRed, LargeWidth, // white with red stripes
                new Band(StripeInner, s_redStripedWhite), new Band(StripeOuter, s_stripeRed)) },
            { "piece_banner04", new BannerStyle(s_white, NormalWidth) }, // dark red, white chain trim
            { "piece_banner05", new BannerStyle(s_greenBannerGold, ScrollworkWidth) }, // green, gold scrollwork
            { "piece_banner06", new BannerStyle(s_stripeRed, LargeWidth, // blue, red and white stripes
                new Band(StripeInner, s_tricolourWhite)) },
            { "piece_banner07", new BannerStyle(s_stripeBlue, LargeWidth, // white with light-blue stripes
                new Band(StripeInner, s_blueStripedWhite), new Band(StripeOuter, s_stripeBlue)) },
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
        // Colour at the centre line; Bands change it further out, innermost first.
        public readonly Color Color;
        public readonly float Width;
        public readonly Band[] Bands;

        public BannerStyle(Color color, float width, params Band[] bands)
        {
            Color = color;
            Width = width;
            Bands = bands;
        }

        // fromCentre is the distance from the cloth's centre line as a share of half the cloth width.
        public Color ColorAt(float fromCentre)
        {
            var color = Color;
            foreach (var band in Bands)
            {
                if (fromCentre >= band.From) color = band.Color;
            }
            return color;
        }
    }

    // From its inner edge outward, the symbol takes this colour (see BannerStyle.ColorAt).
    internal readonly struct Band
    {
        public readonly float From;
        public readonly Color Color;

        public Band(float from, Color color)
        {
            From = from;
            Color = color;
        }
    }
}
