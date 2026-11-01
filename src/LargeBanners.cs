using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Heraldry
{
    /// <summary>
    /// Large banners: a clone of each vanilla banner at twice the size, for twice its materials. The
    /// clones are always registered, so placed ones stay in the world; the <c>LargeBanners</c> setting
    /// only decides whether the hammer offers them (<see cref="Piece.m_enabled"/>).
    /// </summary>
    internal static class LargeBanners
    {
        // Players' worlds store placed large banners by these prefab names: never rename them.
        private const string PrefabPrefix = "heraldry_";
        private const string PrefabSuffix = "_large";
        private const string NamePrefix = "$heraldry_large ";

        // Twice the size and twice the materials of the vanilla banner.
        private const float Scale = 2f;
        private const int CostFactor = 2;

        private static readonly List<Piece> s_pieces = new List<Piece>();

        // Called by BannerPieces.Register for each vanilla banner, after it got its emblem.
        public static void Add(GameObject vanilla)
        {
            var vanillaPiece = vanilla.GetComponent<Piece>();
            if (!vanillaPiece)
            {
                Jotunn.Logger.LogWarning($"{vanilla.name} has no Piece; it gets no large banner.");
                return;
            }

            var requirements = new List<RequirementConfig>();
            foreach (var requirement in vanillaPiece.m_resources)
            {
                if (!requirement.m_resItem) continue;
                requirements.Add(new RequirementConfig(requirement.m_resItem.name, requirement.m_amount * CostFactor, 0, requirement.m_recover));
            }

            // The clone keeps the vanilla banner's hammer tab, crafting station, icon and wear.
            var config = new PieceConfig
            {
                Name = NamePrefix + vanillaPiece.m_name,
                Description = vanillaPiece.m_description,
                PieceTable = PieceTables.Hammer,
                Enabled = HeraldryPlugin.LargeBanners.Value,
                Requirements = requirements.ToArray(),
            };
            string name = PrefabPrefix + vanilla.name + PrefabSuffix;
            var custom = new CustomPiece(name, vanilla.name, config);
            var prefab = custom.PiecePrefab;
            prefab.transform.localScale = Vector3.one * Scale;

            // Same cloth and UVs as the vanilla banner, so it paints and shares overlays the same way.
            BannerPieces.Alias(prefab.name, vanilla.name);
            PieceManager.Instance.AddPiece(custom);
            s_pieces.Add(prefab.GetComponent<Piece>());
        }

        // The setting is synced from the server and can change while playing. The hammer menu picks the
        // change up when the player next rebuilds its piece list (equipping the hammer, inventory changes).
        public static void UpdateEnabled()
        {
            bool enabled = HeraldryPlugin.LargeBanners.Value;
            foreach (var piece in s_pieces)
            {
                if (piece) piece.m_enabled = enabled;
            }
        }
    }
}
