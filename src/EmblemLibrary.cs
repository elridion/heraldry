using System.Collections.Generic;
using UnityEngine;

namespace Heraldry
{
    /// <summary>
    /// The embedded emblem art: symbols by id, decoded on first use. Banner cloth only shows the left
    /// half of its texture, mirrored, so each image also records the column it is mirrored at.
    /// </summary>
    internal static class EmblemLibrary
    {
        private const string SymbolPrefix = "Heraldry.symbol.";
        private const string ResourceSuffix = ".png";
        // Alpha above this counts as part of the shape when looking for the mirror axis.
        private const byte ShapeAlpha = 30;
        // The art is centred by eye, within a few pixels of the image middle; search that band only.
        private const float AxisSearchRange = 4f;

        // Id to resource name. The file names are zero-padded (emblem_07.png), the ids players type aren't.
        private static readonly Dictionary<int, string> s_symbols = new Dictionary<int, string>();
        private static readonly Dictionary<string, EmblemArt> s_art = new Dictionary<string, EmblemArt>();

        public static int MaxSymbol { get; private set; } = -1;

        public static bool HasSymbol(int id) => s_symbols.ContainsKey(id);
        public static EmblemArt GetSymbol(int id) => s_symbols.TryGetValue(id, out var name) ? Load(name) : null;

        // Collects the ids from the resource names, so adding art needs no code change.
        public static void Index()
        {
            foreach (var name in typeof(EmblemLibrary).Assembly.GetManifestResourceNames())
            {
                if (!TryParseId(name, SymbolPrefix, out int symbol)) continue;
                s_symbols[symbol] = name;
                MaxSymbol = Mathf.Max(MaxSymbol, symbol);
            }
            if (s_symbols.Count == 0) Jotunn.Logger.LogWarning("No embedded emblem symbols found.");
        }

        private static bool TryParseId(string name, string prefix, out int id)
        {
            id = -1;
            if (!name.StartsWith(prefix) || !name.EndsWith(ResourceSuffix)) return false;
            return int.TryParse(name.Substring(prefix.Length, name.Length - prefix.Length - ResourceSuffix.Length), out id);
        }

        private static EmblemArt Load(string resourceName)
        {
            if (s_art.TryGetValue(resourceName, out var cached)) return cached;
            var art = Decode(resourceName);
            s_art[resourceName] = art;
            return art;
        }

        private static EmblemArt Decode(string resourceName)
        {
            using (var stream = typeof(EmblemLibrary).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Jotunn.Logger.LogWarning($"Embedded emblem '{resourceName}' not found.");
                    return null;
                }

                // Stream.Read may return fewer bytes than asked for; read until the buffer is full.
                var bytes = new byte[stream.Length];
                int read = 0, count;
                while (read < bytes.Length && (count = stream.Read(bytes, read, bytes.Length - read)) > 0) read += count;

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                try
                {
                    if (!Jotunn.Utils.AssetUtils.LoadImage(texture, bytes))
                    {
                        Jotunn.Logger.LogWarning($"Embedded emblem '{resourceName}' failed to decode.");
                        return null;
                    }
                    var art = new EmblemArt(texture.width, texture.height, texture.GetPixels32());
                    art.Axis = FindMirrorAxis(art);
                    return art;
                }
                finally
                {
                    Object.Destroy(texture);
                }
            }
        }

        // The column the shape is most nearly mirror-symmetric about, in pixel-centre coordinates
        // (63.5 lies between columns 63 and 64). Mirroring about the image middle instead would leave a
        // gap or a doubled stroke at the banner's centre line for art drawn a pixel or two off-centre.
        private static float FindMirrorAxis(EmblemArt art)
        {
            float middle = (art.Width - 1) * 0.5f;
            float best = middle, bestScore = float.MaxValue;
            for (float axis = middle - AxisSearchRange; axis <= middle + AxisSearchRange; axis += 0.5f)
            {
                long score = 0;
                for (int y = 0; y < art.Height; y++)
                {
                    for (int x = 0; x < art.Width; x++)
                    {
                        bool inside = art.Pixels[y * art.Width + x].a > ShapeAlpha;
                        int mx = Mathf.RoundToInt(2f * axis - x);
                        bool mirrored = mx >= 0 && mx < art.Width && art.Pixels[y * art.Width + mx].a > ShapeAlpha;
                        if (inside != mirrored) score++;
                    }
                }
                if (score < bestScore)
                {
                    bestScore = score;
                    best = axis;
                }
            }
            return best;
        }
    }

    /// <summary>
    /// One decoded emblem image. Pixels are stored bottom row first, as Unity returns them.
    /// </summary>
    internal sealed class EmblemArt
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Color32[] Pixels;
        public float Axis;

        public EmblemArt(int width, int height, Color32[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        // Bilinear sample at pixel-centre coordinates, with y counted from the top. Outside is transparent.
        public Color Sample(float x, float yFromTop)
        {
            float y = Height - 1 - yFromTop;
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            var a = Get(x0, y0);
            var b = Get(x0 + 1, y0);
            var c = Get(x0, y0 + 1);
            var d = Get(x0 + 1, y0 + 1);
            return Color.Lerp(Color.Lerp(a, b, fx), Color.Lerp(c, d, fx), fy);
        }

        private Color Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return Color.clear;
            return Pixels[y * Width + x];
        }
    }
}
