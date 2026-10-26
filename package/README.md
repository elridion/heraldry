# Heraldry

Emblems for vanilla banners, as easy as editing a sign.

## Features

- Works on the vanilla banners themselves: no new pieces, and banners you have already built can
  carry emblems too. Without the mod, banners simply look vanilla again.
- Interact with a banner and type a symbol number from 0 to 159, e.g. `42`.
- The banner previews the symbol while you type. Enter saves it, Escape keeps the old one.
- The symbol is embroidered over the vanilla banner's design, in a colour picked for each banner.
- Leave the number empty for a plain vanilla banner.
- Anyone with ward access can change a banner's emblem, like a sign.

## Symbols

There are 160 heraldic symbols to choose from: beasts, skulls, crowns, castles, helms, weapons,
shields, heraldic charges, gems, plants, stars, ships and more. The banner is mirrored down the
middle, so every symbol is symmetric; side-on creatures such as ravens, dragons and horses come as
a pair facing each other.

![All symbols with their numbers](https://raw.githubusercontent.com/elridion/heraldry/main/screenshots/symbols.png)

| Banner | Symbol |
| --- | --- |
| Black, Red, Purple, Orange | White |
| Blue, Green | Gold |
| Yellow, White | Black |
| Red-striped, blue-striped, blue-red-white-striped | Counterchanged: stripe colour on the white, white on the stripes; larger (no edge trim) |

## Languages

The banner texts follow the game's language setting, in every language Valheim offers except
Abenaki, which shows English. The translations have not been checked by native speakers yet;
corrections are welcome on GitHub (see below).

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

- Symbols: icons from [game-icons.net](https://game-icons.net) by Lorc, Delapouite, Sbed, Caro
  Asercion, Cathelineau, DarkZaitzev, Kier Heyl, Pepijn Poolman, Skoll, Sparker and various
  artists, licensed under [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/), and by Viscious
  Speed under CC0. They are cropped, resized, paired and recoloured for the banners.
- Mod icon: "Knight banner" by Delapouite from [game-icons.net](https://game-icons.net), licensed
  under [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). Recoloured and outlined.

## Source and issues

Bug reports and feedback: the [#heraldry channel on Discord](https://discord.gg/GUTMJQa6CQ).

Source code (MIT license): [github.com/elridion/heraldry](https://github.com/elridion/heraldry).
