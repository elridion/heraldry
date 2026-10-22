using GUIFramework;
using HarmonyLib;

namespace Heraldry
{
    [HarmonyPatch(typeof(TextInput))]
    internal static class TextInputPatches
    {
        // The vanilla text box only reports its text on Enter; feed what is typed so far to the banner
        // it was opened for, which previews it. Once the box closes, end any preview right away. Cheap
        // check first: the box is usually closed, and EndPreview returns at once when nothing previews.
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        private static void Update_Postfix(TextReceiver ___m_queuedSign, GuiInputField ___m_inputField)
        {
            if (!TextInput.IsVisible())
            {
                EmblemBanner.EndPreview();
                return;
            }
            if (!(___m_queuedSign is EmblemBanner banner) || !banner || !___m_inputField) return;
            banner.SetPreview(___m_inputField.text);
        }
    }
}
