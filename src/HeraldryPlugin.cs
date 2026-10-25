using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Heraldry
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class HeraldryPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "elridion.heraldry";
        public const string PluginName = "Heraldry";
        public const string PluginVersion = "0.2.3";

        private Harmony _harmony;

        private void Awake()
        {
            EmblemLibrary.Index();
            AddLocalizations();
            PrefabManager.OnVanillaPrefabsAvailable += BannerPieces.Register;

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(HeraldryPlugin).Assembly);
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        private static void AddLocalizations()
        {
            LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string>
            {
                { "heraldry_set_emblem", "Set emblem" },
                { "heraldry_input_topic", "Emblem symbol, e.g. 42" },
                { "heraldry_symbol", "Symbol" },
                { "heraldry_no_emblem", "No emblem" },
                { "heraldry_invalid", "Unknown symbol. Symbols are 0-$1." },
            });
        }
    }
}
