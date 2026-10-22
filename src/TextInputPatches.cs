using GUIFramework;
using HarmonyLib;

namespace Heraldry
{
    [HarmonyPatch(typeof(TextInput))]
    internal static class TextInputPatches
    {
        // The vanilla text box only reports its text on Enter; feed what is typed so far to the banner
        // it was opened for, which previews it. Cheap check first: the box is usually closed.
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix(TextReceiver ___m_queuedSign, GuiInputField ___m_inputField)
        {
            if (!TextInput.IsVisible()) return;
            if (!(___m_queuedSign is EmblemBanner banner) || !banner || !___m_inputField) return;
            banner.SetPreview(___m_inputField.text);
        }
    }
}
