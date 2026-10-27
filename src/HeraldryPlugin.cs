using System.IO;
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
        public const string PluginVersion = "0.3.4";

        private const string TranslationPrefix = "Heraldry.translations.";

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
