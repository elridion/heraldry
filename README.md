# Heraldry

Valheim mod (BepInEx + Jotunn) that lets every vanilla banner carry a guild emblem, chosen like
writing on a sign. It adds no new pieces.

Players: install it from a mod manager. Features and emblem codes are on the mod page, whose source
is [`package/README.md`](package/README.md).

## Building

Requires the .NET SDK (8+) and a Valheim install. The build locates Valheim through the Steam
registry entry; override with `-p:VALHEIM_INSTALL="<path>"`.

```
dotnet build -c Release                           # build bin/Release/Heraldry.dll
dotnet build -c Release -t:Package                # also write dist/Heraldry-<version>.zip
```

The first build generates publicized Valheim assemblies in `Valheim_Data/Managed/publicized_assemblies`.

The build touches nothing outside the repo. To test in game, install the zip with a mod manager
(e.g. import it into a [Gale](https://github.com/Kesomannen/gale) profile).

## Project layout

| Path | Purpose |
| --- | --- |
| `src/` | Plugin source: banner pieces, banner component, emblem painting, text box patch |
| `assets/emblems/` | Emblem symbol images, embedded in the DLL |
| `package/` | Mod page README and icon shipped in the package |
| `tools/` | Generators for the symbols, the mod icon and the symbol overview image |
| `screenshots/symbols.png` | Numbered symbol overview shown on the mod page |

No Unity project or asset bundles: banner textures are painted at runtime.

## Releases

CI (`.github/workflows/build.yml`) builds the package on every push and pull request. Pushing a
`vX.Y.Z` tag checks that the tag matches the version in `Heraldry.csproj` and `src/HeraldryPlugin.cs`
and has a `CHANGELOG.md` section, then creates a GitHub release and publishes to Hexium.

## License

Code: [MIT](LICENSE). The symbols in `assets/emblems/` are icons from
[game-icons.net](https://game-icons.net) by Lorc, Delapouite and others, licensed under
[CC BY 3.0](https://creativecommons.org/licenses/by/3.0/); see `assets/emblems/README.md`.
