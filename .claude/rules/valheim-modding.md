---
paths:
  - "src/**/*.cs"
---

# Valheim, Jotunn and Harmony patterns

`../warden/.claude/rules/valheim-modding.md` has the full set of lessons from the sibling mod. These
are the ones this mod depends on.

## Registering content

- `BannerPieces.Register`, subscribed to `PrefabManager.OnVanillaPrefabsAvailable`, adds
  `EmblemBanner` to the vanilla banner prefabs; it unsubscribes itself so it runs once, and skips a
  prefab that already has the component.
- Look vanilla prefabs up with `PrefabManager.Cache.GetPrefab<GameObject>(name)` and skip with a
  warning when missing. Check everything needed (cloth, UVs) before adding the component, so a
  failure leaves the vanilla prefab untouched.
- New player-facing strings go into `HeraldryPlugin.AddLocalizations` (English only). Reuse vanilla
  tokens where the meaning is identical (`$piece_noaccess`, `$KEY_Use`).

## Textures and materials

- **Never mutate shared vanilla assets.** The overlay is a `new Material(vanilla)` copy appended to
  the vanilla array; the instance keeps the vanilla array to restore when the emblem is cleared.
- Symbol overlays are shared per (banner, cloth renderer, symbol) and reference-counted in
  `EmblemPainter`; every `Acquire` that returns materials needs a `Release` with the same key, or
  textures leak. The preview acquires one per symbol typed, after `EmblemBanner.PreviewDelay` of no
  typing, since each one builds and uploads a texture on the main thread.
- Never re-bake the vanilla cloth texture. Reading it back from the GPU returns the copy reduced by
  the player's texture quality setting, so the banner visibly drops resolution (tried in 0.1.0
  development). The symbol is an extra alpha-clipped material over the vanilla one instead; it needs
  the cloth shader's `_Cutoff`.
- If you ever do change another texture slot, set explicit `Texture2D.blackTexture` rather than
  clearing it, and check `material.HasProperty` before every `Set*`.
- The cloth renderer is identified by its first material's texture name containing "banner", and only
  single-material cloth is used: the overlay is drawn over the last submesh but placed by the UVs of
  the whole mesh. Identify the cloth before the overlay is appended.

## Components and networking

- Persistent state lives in the ZDO (`heraldry_emblem`, a `GetStableHashCode()` hash in a static
  field). Like vanilla `Sign.SetText`, the writer claims ownership and sets it directly; other clients
  pick it up by polling (`EmblemBanner.Refresh`), which like vanilla `Sign` only re-reads the text
  when `ZDO.DataRevision` changes or the preview changed.
- A component whose `ZNetView` is missing or invalid is the placement ghost: it does nothing.
- Dedicated servers have no graphics device: anything visual is gated by `EmblemPainter.CanPaint`.

## Harmony patches

- Patches are `internal static class ...Patches` with `[HarmonyPatch]` attributes, applied by
  `PatchAll`. Target private methods by string name, read private fields through triple-underscore
  parameters (`___m_queuedSign`). Don't access private vanilla members directly even though the
  publicized assemblies would compile it.
- `TextInput.Update` runs every frame: bail out on `TextInput.IsVisible()` first.
