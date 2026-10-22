# Emblem art

World of Warcraft guild tabard art, © Blizzard Entertainment. Not covered by this repository's MIT
license.

Downloaded on 2026-10-21 from Blizzard's public render server:

- `symbols/emblem_NN.png`: `https://render.worldofwarcraft.com/us/guild/tabards/emblem_NN.png` (00-195, 125 x 125)

Ids are zero-padded to two digits up to 99 and three digits after that, as on the server.

The images are grey with a baked-in metallic bevel. The mod uses only their alpha and fills the
shape with the banner's flat symbol colour. The file
number is the id players type, so never renumber files. Add new art under new numbers.

The server also has the tabard borders (`border_NN.png`); they don't suit the banner cloth and are
not used.
