# Heraldry

Valheim mod (BepInEx + Jotunn) adding **heraldry** to the vanilla banners: each one (no new pieces)
can show a heraldic symbol painted over the vanilla cloth design. Players set it like writing on
a sign, by typing the symbol number (`42`); the cloth previews it while they type. Player-facing description lives in
`package/README.md` (the mod page shipped in the package); the root `README.md` is the GitHub page for
developers; release history in `CHANGELOG.md`.

Sibling project `../warden` uses the same stack, build and release setup, and its `.claude/rules/`
hold more Valheim lessons; read it when a pattern here is missing.

## Stack

- C# targeting `net462`, `LangVersion` latest, nullable reference types disabled.
- `JotunnLib` 2.30.2 from NuGet. Jotunn's prebuild generates publicized Valheim assemblies in
  `Valheim_Data/Managed/publicized_assemblies`; Valheim is located through the Steam registry entry
  (override with `-p:VALHEIM_INSTALL="<path>"`).
- Harmony (via BepInEx) for patches. No Unity project and no asset bundles: textures and materials
  are built at runtime from the embedded emblem PNGs.

## Layout

| Path | Purpose |
| --- | --- |
| `src/HeraldryPlugin.cs` | BepInEx entry point: GUID/version, English localization, Harmony `PatchAll` |
| `src/BannerPieces.cs` | Adds `EmblemBanner` to each vanilla `piece_bannerNN`; finds the cloth and its UV rect |
| `src/EmblemBanner.cs` | Banner component: emblem text in the ZDO, sign-style interact/hover, live preview, material swap |
| `src/EmblemPainter.cs` | Builds the symbol overlay material drawn over the vanilla cloth; shared, reference-counted cache |
| `src/EmblemLibrary.cs` | Embedded symbol art by id, decoded lazily, with each image's mirror axis |
| `src/EmblemCode.cs` | Parses and normalises the symbol text |
| `src/BannerStyles.cs` | Hard-coded symbol colour (white/gold/black) and width per vanilla banner; golds read from the vanilla cloth textures, the rest picked by eye in game |
| `src/TextInputPatches.cs` | Feeds the open text box's text to the banner for the live preview |
| `assets/emblems/` | Emblem art: `symbols/emblem_NN.png` and `symbols.csv` (ids, icon names, authors) |
| `tools/make_symbols.ps1` | Generates the symbols from the game-icons.net archive; its list order is the symbol ids |
| `tools/make_icon.ps1` | Generates `package/icon.png` from game-icons.net "knight-banner" (same archive) |
| `tools/make_symbol_sheet.ps1` | Generates `screenshots/symbols.png`, the numbered symbol overview on the mod page; rerun after adding symbols |
| `package/README.md`, `package/icon.png` | Mod page and icon shipped in the package |
| `.github/workflows/build.yml` | CI: builds the package on push/PR, GitHub release and Hexium upload on `v*` tags |

## Commands

Run from the repo root (Windows, PowerShell).

```
dotnet build -c Release                           # build bin/Release/Heraldry.dll, touches nothing outside the repo
dotnet build -c Release -t:Package                # also writes dist/Heraldry-<version>.zip
```

- The build never copies anything into the game. The user tests by importing the packaged zip into
  their mod manager (Gale), which installs its own copy; never copy the DLL into a game or Gale
  profile by hand, since the mod manager would overwrite it again.
- A clean build prints one known warning, `MSB3245` for `UnityEngine.ProfilerModule`. Any other is new.

## Verification

There is no test project. "Done" means: `dotnet build -c Release` succeeds with no
new warnings, and behaviour that needs the game (placement, painting, preview, multiplayer sync) is
called out as untested-in-game unless the user has confirmed it in Valheim. Never claim in-game
behaviour works on the strength of a successful build.

## Invariants that must not break

- **Saved-world compatibility.** Persisted or networked identifiers: the ZDO key `heraldry_emblem`
  and the plugin GUID `elridion.heraldry`. Renaming one breaks existing banners. The mod adds no
  prefabs, so removing it leaves vanilla banners; keep it that way.
- **Emblem ids are the art's file numbers** (`emblem_42.png` is symbol 42), which follow the order of
  the list in `tools/make_symbols.ps1`. Players' banners store these ids, so never reorder or remove
  entries in that list; append new icons at the end.
- **Mirrored cloth.** Vanilla banner cloth maps only the left half of its texture and mirrors it at
  the centre line, like a tabard. `EmblemPainter` paints the left half of each image, centred on
  the design's right edge at the image's own mirror axis. Asymmetric art comes out symmetric; that is
  accepted. Never edit the cloth mesh or UVs (MagicaCloth simulates the original mesh).
- **Vanilla assets are shared.** The only change to vanilla banner prefabs is the added
  `EmblemBanner` component. Never modify their materials, meshes or existing components; painted
  materials are per-instance copies, and released ones are destroyed. Other mods that also change
  vanilla banners (e.g. Balrond BannerColorizer) may conflict.
- **Every client and the server need the mod** (`NetworkCompatibility(EveryoneMustHaveMod, VersionStrictness.Minor)`).
- **Dedicated servers have no graphics device.** `EmblemPainter.CanPaint` gates all texture work; the
  ZDO text still syncs.
- **Docs follow code.** The symbol range and the per-banner symbol table in `package/README.md` stay
  in sync with the art and `BannerStyles` in the same change, and `screenshots/symbols.png` is
  regenerated whenever symbols change. The mod page loads it from `main` on
  raw.githubusercontent.com, so it only shows new symbols once they are merged.

## Art and licensing

The symbols are game-icons.net icons under CC BY 3.0, generated by `tools/make_symbols.ps1` (see
`assets/emblems/README.md`). CC BY requires credit: keep the author list and license link in the
Credits of `package/README.md` and in `assets/emblems/README.md` in sync with `symbols.csv` when
icons are added. Only add icons that are close to mirror-symmetric, or side-on icons as a
`pair:` entry (the script mirrors them into two facing figures). Never bring back art the mod
has no licence to ship (0.1.x shipped WoW guild tabard art; 0.2.0 replaced it for that reason).

## Commits

Plain imperative, sentence-case subjects, no type prefix, no trailing period; a wrapped body that
explains why. User-visible changes update `CHANGELOG.md` (and `package/README.md` where relevant) in
the same commit. Do not commit `bin/`, `obj/` or `dist/`.

## Detailed rules

Path-scoped rules in `.claude/rules/` load when matching files are read or edited:

- `code-style.md` - C# conventions for `src/`
- `valheim-modding.md` - Unity/Valheim/Jotunn/Harmony patterns for `src/`
- `release.md` - versioning, packaging, README and CHANGELOG
