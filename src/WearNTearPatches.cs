using HarmonyLib;

namespace Heraldry
{
    [HarmonyPatch(typeof(WearNTear))]
    internal static class WearNTearPatches
    {
        // A torn-down piece breaks into fragments that copy its renderers' materials and fall for a few
        // seconds after the piece is gone. Runs on every client, since each builds its own fragments.
        [HarmonyPrefix]
        [HarmonyPatch("RPC_CreateFragments")]
        private static void RPC_CreateFragments_Prefix(WearNTear __instance)
        {
            var banner = __instance.GetComponent<EmblemBanner>();
            if (banner) banner.RemoveEmblem();
        }
    }
}
