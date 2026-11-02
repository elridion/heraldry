using System.IO;
using BepInEx;
using BepInEx.Configuration;
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
        public const string PluginVersion = "0.4.0";

        private const string TranslationPrefix = "Heraldry.translations.";

        public static ConfigEntry<bool> LargeBanners;

        private Harmony _harmony;

        private void Awake()
        {
            LargeBanners = Config.Bind("Pieces", "LargeBanners", false,
                new ConfigDescription("Offer a large banner, twice the size for twice the materials, next to each vanilla banner in the hammer. " +
                    "Turning it off only hides them from the hammer; large banners already built stay.",
                    null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
            LargeBanners.SettingChanged += (_, __) => Heraldry.LargeBanners.UpdateEnabled();

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

        // Each embedded assets/translations/<Language>.json is named after a Valheim language. Jotunn
        // falls back to English for languages and keys without a translation.
        private static void AddLocalizations()
        {
            var localization = LocalizationManager.Instance.GetLocalization();
            var assembly = typeof(HeraldryPlugin).Assembly;
            foreach (string resource in assembly.GetManifestResourceNames())
            {
                if (!resource.StartsWith(TranslationPrefix) || !resource.EndsWith(".json")) continue;
                string language = resource.Substring(TranslationPrefix.Length, resource.Length - TranslationPrefix.Length - ".json".Length);
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(resource)))
                    localization.AddJsonFile(language, reader.ReadToEnd());
            }
        }
    }
}
