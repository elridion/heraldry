# Heraldry

Emblems for vanilla banners, as easy as editing a sign.

## Features

- Works on the vanilla banners themselves: no new pieces, and banners you have already built can
  carry emblems too. Without the mod, banners simply look vanilla again.
- Interact with a banner and type a symbol number from 0 to 195, e.g. `42`.
- The banner previews the symbol while you type. Enter saves it, Escape keeps the old one.
- The symbol is embroidered over the vanilla banner's design, in a colour picked for each banner.
- Leave the number empty for a plain vanilla banner.
- Anyone with ward access can change a banner's emblem, like a sign.

## Symbols

There are 196 somewhat familiar symbols to choose from. The banner is mirrored down the middle, so
the symbol's left half is shown on both sides.

| Banner | Symbol |
| --- | --- |
| Black, Red, Purple, Orange | White |
| Blue, Green | Gold |
| Yellow, White | Black |
| Red-striped, blue-striped, blue-red-white-striped | Black, larger (no edge trim) |

## Requirements

- BepInEx for Valheim
- [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/)

Every client and the server need the mod installed, at the same minor version.

## Uninstalling

Remove the mod from your mod manager; nothing else needs cleaning up. The mod adds no new pieces,
so every banner stays in your world and simply looks vanilla again. Each banner keeps its symbol
number as a few bytes of extra data that the game ignores, so if you reinstall the mod, the
emblems come back.

In multiplayer, the server and all players have to remove it together, since everyone needs the
same version to connect.

## Credits

- The symbol art is not covered by this mod's MIT license; all rights remain with its respective
  owners. This is a fan project and is not affiliated with or endorsed by them.

## Source and issues

Source code (MIT license) and bug reports: [github.com/elridion/heraldry](https://github.com/elridion/heraldry).
