using System.Collections.Generic;
using UnityEngine;

namespace Heraldry
{
    /// <summary>
    /// A heraldic banner: stores its symbol code in the ZDO and paints it onto the cloth. Works like a
    /// vanilla sign, so the text box shows the code; while the box is open for this banner, the cloth
    /// previews what is being typed without saving it.
    /// </summary>
    public class EmblemBanner : MonoBehaviour, Hoverable, Interactable, TextReceiver
    {
        private const float RefreshInterval = 0.25f;
        // Symbol ids have at most three digits; one spare for a stray space.
        private const int MaxTextLength = 4;

        private static readonly int s_emblemKey = "heraldry_emblem".GetStableHashCode();
        // The banner whose text box is open, so a preview ends when the box moves to another receiver.
        private static EmblemBanner s_previewing;

        private ZNetView m_nview;
        private Piece m_piece;
        private BannerInfo m_info;
        private readonly List<Renderer> m_cloth = new List<Renderer>();
        private readonly List<Material[]> m_vanillaMaterials = new List<Material[]>();
        private readonly List<string> m_paintedKeys = new List<string>();
        private string m_shownText;
        private string m_previewText;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            m_piece = GetComponent<Piece>();
            // Without a valid ZNetView this is the placement ghost, which keeps the plain vanilla look.
            if (!m_nview || !m_nview.IsValid()) return;
            if (!EmblemPainter.CanPaint) return;

            m_info = BannerPieces.GetInfo(Utils.GetPrefabName(gameObject));
            if (m_info == null) return;
            foreach (var renderer in BannerPieces.FindCloth(gameObject))
            {
                m_cloth.Add(renderer);
                m_vanillaMaterials.Add(renderer.sharedMaterials);
                m_paintedKeys.Add(null);
            }
            if (m_cloth.Count != m_info.UvRects.Count)
            {
                Jotunn.Logger.LogWarning($"{name}: found {m_cloth.Count} cloth renderers, expected {m_info.UvRects.Count}; emblem disabled.");
                m_cloth.Clear();
                return;
            }

            InvokeRepeating(nameof(Refresh), 0f, RefreshInterval);
        }

        private void OnDestroy()
        {
            if (s_previewing == this) s_previewing = null;
            for (int i = 0; i < m_paintedKeys.Count; i++) EmblemPainter.Release(m_paintedKeys[i]);
        }

        // Called by the text box patch every frame while the box is open for this banner.
        public void SetPreview(string text)
        {
            if (s_previewing && s_previewing != this) s_previewing.m_previewText = null;
            s_previewing = this;
            if (text == m_previewText) return;
            m_previewText = text;
            Refresh();
        }

        private void Refresh()
        {
            if (m_cloth.Count == 0) return;
            if (m_previewText != null && (s_previewing != this || !TextInput.IsVisible()))
            {
                m_previewText = null;
                if (s_previewing == this) s_previewing = null;
            }

            // A half-typed preview that isn't a valid code yet keeps showing the saved emblem.
            string text = m_previewText != null && EmblemCode.TryParse(m_previewText, out _) ? m_previewText : GetText();
            if (text == m_shownText) return;
            m_shownText = text;

            EmblemCode.TryParse(text, out var code);
            for (int i = 0; i < m_cloth.Count; i++)
            {
                if (!m_cloth[i]) continue;
                string key = code.IsEmpty ? null : EmblemPainter.Key(m_info, i, code);
                if (key == m_paintedKeys[i]) continue;

                var painted = key == null ? null : EmblemPainter.Acquire(key, m_info, i, m_vanillaMaterials[i], code);
                m_cloth[i].sharedMaterials = painted ?? m_vanillaMaterials[i];
                EmblemPainter.Release(m_paintedKeys[i]);
                m_paintedKeys[i] = painted == null ? null : key;
            }
        }

        public string GetText() => m_nview && m_nview.IsValid() ? m_nview.GetZDO().GetString(s_emblemKey) : "";

        // Saves only valid codes, in normalised form; an invalid entry keeps the old emblem.
        public void SetText(string text)
        {
            m_previewText = null;
            if (!m_nview || !m_nview.IsValid()) return;
            if (!PrivateArea.CheckAccess(transform.position, 0f, true, false)) return;

            if (!EmblemCode.TryParse(text, out var code))
            {
                if (Player.m_localPlayer)
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                        Localization.instance.Localize("$heraldry_invalid", EmblemLibrary.MaxSymbol.ToString()));
                Refresh();
                return;
            }

            m_nview.ClaimOwnership();
            m_nview.GetZDO().Set(s_emblemKey, code.ToString());
            Refresh();
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold) return false;
            if (!PrivateArea.CheckAccess(transform.position, 0f, true, false)) return true;
            TextInput.instance.RequestText(this, "$heraldry_input_topic", MaxTextLength);
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

        public string GetHoverText()
        {
            string name = GetHoverName();
            if (!PrivateArea.CheckAccess(transform.position, 0f, false, false))
                return Localization.instance.Localize(name + "\n$piece_noaccess");

            EmblemCode.TryParse(GetText(), out var code);
            string emblem = code.IsEmpty ? "$heraldry_no_emblem" : $"$heraldry_symbol {code.Symbol}";
            return Localization.instance.Localize($"{name}\n{emblem}\n[<color=yellow><b>$KEY_Use</b></color>] $heraldry_set_emblem");
        }

        public string GetHoverName() => m_piece ? m_piece.m_name : "";

        public float GetHoverOffset() => 0f;
    }
}
