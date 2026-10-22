using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace Heraldry
{
    /// <summary>
    /// Gives every vanilla banner an <see cref="EmblemBanner"/>, so banners already built in a world gain
    /// the emblem too. Nothing else on the vanilla prefab changes: without the mod, the stored symbol is
    /// ignored and the banner looks vanilla again.
    /// </summary>
    internal static class BannerPieces
    {
        // Vanilla names its banners piece_banner01, piece_banner02, ...; probe further than exist today so
        // banners a game update adds are picked up.
        private const string VanillaPrefix = "piece_banner";
        private const int MaxVanillaBanners = 30;

        private static readonly Dictionary<string, BannerInfo> s_banners = new Dictionary<string, BannerInfo>();

        public static BannerInfo GetInfo(string prefabName) => s_banners.TryGetValue(prefabName, out var info) ? info : null;

        public static void Register()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= Register;

            int added = 0;
            for (int i = 1; i <= MaxVanillaBanners; i++)
            {
                string name = VanillaPrefix + i.ToString("D2");
                var banner = PrefabManager.Cache.GetPrefab<GameObject>(name);
                if (!banner) continue;
                try
                {
                    if (AddEmblem(banner)) added++;
                }
                catch (System.Exception e)
                {
                    Jotunn.Logger.LogError($"Failed to add an emblem to {name}: {e}");
                }
            }
            if (added == 0) Jotunn.Logger.LogWarning($"No vanilla '{VanillaPrefix}NN' prefabs found; no banners carry emblems.");
        }

        private static bool AddEmblem(GameObject banner)
        {
            if (banner.GetComponent<EmblemBanner>()) return true;

            var info = new BannerInfo(banner.name);
            foreach (var renderer in banner.GetComponentsInChildren<Renderer>(true))
            {
                if (LooksLikeCloth(renderer) && renderer.sharedMaterials.Length != 1)
                    Jotunn.Logger.LogWarning($"Banner cloth '{renderer.name}' on {banner.name} has {renderer.sharedMaterials.Length} materials; it gets no emblem.");
            }
            foreach (var renderer in FindCloth(banner))
            {
                if (!TryGetUvRect(renderer, out var uvRect))
                {
                    Jotunn.Logger.LogWarning($"Banner cloth '{renderer.name}' on {banner.name} has no readable UVs; painting the whole texture.");
                    uvRect = new Rect(0f, 0f, 1f, 1f);
                }
                info.UvRects.Add(uvRect);
            }
            if (info.UvRects.Count == 0)
            {
                Jotunn.Logger.LogWarning($"No banner cloth found on '{banner.name}'; it gets no emblem.");
                return false;
            }

            s_banners[banner.name] = info;
            banner.AddComponent<EmblemBanner>();
            return true;
        }

        // The cloth is the renderer whose first material's texture is a banner texture. The pole and
        // crossbar use wood materials. Only single-material cloth is used: the overlay is drawn over the
        // last submesh and placed by the UVs of the whole mesh, which only agree when there is one.
        // Call this before the overlay is added, which appends a material.
        public static List<Renderer> FindCloth(GameObject root)
        {
            var result = new List<Renderer>();
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                if (LooksLikeCloth(r) && r.sharedMaterials.Length == 1) result.Add(r);
            return result;
        }

        private static bool LooksLikeCloth(Renderer renderer)
        {
            var materials = renderer.sharedMaterials;
            return materials.Length > 0 && materials[0] && materials[0].mainTexture &&
                materials[0].mainTexture.name.IndexOf("banner", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // The strip of UV space the cloth samples. Vanilla cloth maps only the left half of the banner
        // and mirrors it, so this is where the half design goes.
        private static bool TryGetUvRect(Renderer renderer, out Rect rect)
        {
            rect = default;
            Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
            if (!mesh || !mesh.isReadable) return false;

            var uvs = mesh.uv;
            if (uvs.Length == 0) return false;
            Vector2 min = uvs[0], max = uvs[0];
            foreach (var uv in uvs)
            {
                min = Vector2.Min(min, uv);
                max = Vector2.Max(max, uv);
            }
            if (max.x <= min.x || max.y <= min.y) return false;

            rect = Rect.MinMaxRect(Mathf.Clamp01(min.x), Mathf.Clamp01(min.y), Mathf.Clamp01(max.x), Mathf.Clamp01(max.y));
            return true;
        }
    }

    /// <summary>
    /// What painting needs to know about one banner prefab: the UV rect of each cloth renderer, in the
    /// order <see cref="BannerPieces.FindCloth"/> returns them.
    /// </summary>
    internal sealed class BannerInfo
    {
        public readonly string PrefabName;
        public readonly List<Rect> UvRects = new List<Rect>();

        public BannerInfo(string prefabName)
        {
            PrefabName = prefabName;
        }
    }
}
