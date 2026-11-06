# Heraldry

Valheim mod adding **heraldry** to the vanilla banners: each one can show a heraldic symbol painted
over the vanilla cloth design. Players set it like writing on a sign, by typing the symbol number
(`42`); the cloth previews it while they type. Optionally (config `LargeBanners`, off by default)
the hammer also offers a "Large" clone of each banner at twice the size and cost.

Stack, build, verification and git workflow are shared by all mods: see `../CLAUDE.md` and
`../.claude/rules/`.

## Where things are

- `src/BannerPieces.cs` adds `EmblemBanner` to each vanilla `piece_bannerNN`; `src/LargeBanners.cs`
  clones each into `heraldry_piece_bannerNN_large`; `src/EmblemBanner.cs`
  is the banner component; `src/EmblemPainter.cs` builds the overlay materials; `src/EmblemLibrary.cs`
  holds the embedded art; `src/BannerStyles.cs` the per-banner symbol colour and width (golds read from
  the vanilla cloth textures, the rest picked by eye in game).
- `assets/emblems/symbols/emblem_NN.png` and `symbols.csv` (ids, icon names, authors).
- `tools/make_symbols.ps1` generates the symbols from the game-icons.net archive,
  `tools/make_symbol_sheet.ps1` the numbered overview `screenshots/symbols.png`, `tools/make_icon.ps1`
  the mod icon ("knight-banner").

## Invariants that must not break

- **Saved-world compatibility.** Persisted or networked identifiers: ZDO key `heraldry_emblem`,
  plugin GUID `elridion.heraldry` and the large banner prefab names `heraldry_piece_bannerNN_large`.
  The large banners are the only prefabs the mod adds, and they are opt-in; removing the mod leaves
  vanilla banners and makes placed large banners vanish (their ZDOs stay). Add no other prefabs.
- **Large banners stay registered.** The `LargeBanners` setting only toggles `Piece.m_enabled` (the
  hammer menu); the prefabs always exist, so switching it off never removes built ones.
- **Emblem ids are the art's file numbers** (`emblem_42.png` is symbol 42) and follow the order of
  the list in `tools/make_symbols.ps1`. Players' banners store them: never reorder or remove entries,
  append new icons at the end.
- **Vanilla banners get only the added `EmblemBanner` component.** Never modify their materials,
  meshes or existing components. Other mods that change vanilla banners (e.g. Balrond
  BannerColorizer) may conflict.
- **Docs follow code.** In the same change, `package/README.md` keeps the symbol count and range in
  sync with `assets/emblems/`, the features and how-to with `BannerPieces`, `LargeBanners` and
  `EmblemBanner`, and the config table with `HeraldryPlugin`; rerun `tools/make_symbol_sheet.ps1`
  whenever symbols change. The mod page loads that image from `main`, so new symbols show only
  once pushed. The store page stays short (the user's call): its first line matches
  `<PackageDescription>`, then short features, how to use, install notes and the settings;
  details such as the per-banner colours belong in this file, not the store page.

## Art and licensing

The symbols are game-icons.net icons under CC BY 3.0 (see `assets/emblems/README.md`). Keep the
author list and license link in the Credits of `package/README.md` and in `assets/emblems/README.md`
in sync with `symbols.csv`. Only add icons that are close to mirror-symmetric, or side-on icons as a
`pair:` entry (the script mirrors them into two facing figures). Never bring back art the mod has no
licence to ship (0.1.x shipped WoW guild tabard art; 0.2.0 replaced it for that reason).
