# Heraldry

Valheim mod (BepInEx + Jotunn) adding **heraldry** to the vanilla banners: each one (no new pieces)
can show a "somewhat familiar" guild emblem symbol painted over the vanilla cloth design. Players set it like writing on
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
| `src/BannerStyles.cs` | Symbol colour (white/gold/black) and width per vanilla banner, chosen by the user |
| `src/TextInputPatches.cs` | Feeds the open text box's text to the banner for the live preview |
| `assets/emblems/` | Emblem art: `symbols/emblem_NN.png` (source in its README) |
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
- **Emblem ids are the art's file numbers** (`emblem_42.png` is symbol 42). Players' banners store
  these ids, so never renumber or reorder the art; add new art under new numbers.
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
- **Docs follow code.** Recipes, codes and ranges described in `package/README.md` stay in sync with
  `BannerPieces` and the art in the same change.

## Art and licensing

The emblem art is Blizzard's WoW guild tabard art, downloaded from Blizzard's public render server
(see `assets/emblems/README.md`). The user decided to publish with it knowing the takedown risk; do
not reopen that decision. In player-facing text (mod page, changelog, release notes, commit
messages) the symbols are called "somewhat familiar", never WoW or Blizzard; the mod page only says
the art isn't MIT and its rights stay with their owners.

## Commits

Plain imperative, sentence-case subjects, no type prefix, no trailing period; a wrapped body that
explains why. User-visible changes update `CHANGELOG.md` (and `package/README.md` where relevant) in
the same commit. Do not commit `bin/`, `obj/` or `dist/`.

## Detailed rules

Path-scoped rules in `.claude/rules/` load when matching files are read or edited:

- `code-style.md` - C# conventions for `src/`
- `valheim-modding.md` - Unity/Valheim/Jotunn/Harmony patterns for `src/`
- `release.md` - versioning, packaging, README and CHANGELOG
