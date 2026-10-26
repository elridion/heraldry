---
paths:
  - "src/**/*.cs"
---

# Heraldry code specifics

Adds to the workspace-wide `code-style.md` and `valheim-modding.md` in `../.claude/rules/`.

## Registering

- `BannerPieces.Register` adds `EmblemBanner` to the vanilla banner prefabs, skips one that already
  has it, and sets each banner up in its own try/catch. Everything needed (cloth, UVs) is checked
  before the component is added, so a failure leaves the vanilla prefab untouched.
- Localization keys are `heraldry_*`.

## Painting

- Vanilla cloth maps only the left half of its texture and mirrors it at the centre line, like a
  tabard. `EmblemPainter` paints the left half of each image, centred on the design's right edge at
  the image's own mirror axis. Asymmetric art comes out symmetric; that is accepted.
- The symbol is an extra alpha-clipped material (`new Material(vanilla)` copy, needs the cloth
  shader's `_Cutoff`) appended to the vanilla array; the instance keeps the vanilla array to restore
  when the emblem is cleared. Never re-bake the vanilla cloth texture (tried in 0.1.0: the GPU
  readback is reduced by the texture quality setting).
- Overlays are shared per (banner, cloth renderer, symbol) and reference-counted in `EmblemPainter`:
  every `Acquire` that returns materials needs a `Release` with the same key, or textures leak.
  Released materials are destroyed.
- The preview acquires one overlay per symbol typed, only after `EmblemBanner.PreviewDelay` of no
  typing, since each one builds and uploads a texture on the main thread.
- The cloth renderer is identified by its first material's texture name containing "banner", and
  only single-material cloth is used (the overlay is drawn over the last submesh but placed by the
  whole mesh's UVs). Identify it before the overlay is appended.
- `EmblemPainter.CanPaint` gates all texture work (dedicated servers have no GPU); the ZDO text
  still syncs.

## Networking and patches

- Like vanilla `Sign.SetText`, the writer claims ownership and sets `heraldry_emblem` directly; other
  clients poll in `EmblemBanner.Refresh`, which re-reads the text only when `ZDO.DataRevision` or
  the preview changed.
- `TextInputPatches` feeds the open text box to the banner for the preview. `TextInput.Update` runs
  every frame: bail out on `TextInput.IsVisible()` first.
