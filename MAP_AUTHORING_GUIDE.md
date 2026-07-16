# Map Authoring Guide

Maps use an ASCII gameplay logic layer. The engine does not know whether a blocked tile is a
tree, cliff, building, mountain, or stream; it only knows whether the tile is passable.

## Symbols

| Symbol | Meaning |
|---|---|
| `#` | Impassable |
| `.` | Passable |
| `E` | Passable encounter marker tile |
| `T` | Passable transition marker tile |

Rows use `Rows[y][x]`: X increases left to right, Y increases top to bottom, and the top-left
tile is `[0, 0]`. Every row must have exactly `width` characters, and the row count must equal
`height`.

```text
############
#..........#
#..E.......#
#.......T..#
############
```

Map records live in `game/content/maps/`. They contain `width`, `height`, `rows`, named `spawns`,
and map-owned encounter markers. A spawn contains an ID, tile X/Y, and facing. Encounter markers
contain an ID, tile X/Y, encounter definition ID, and cleared flag ID. Transition definitions live
in `game/content/map-transitions/`; their source cell must be authored over `T` and must point to a
valid destination map and spawn.

Visual drawing is intentionally separate. The current placeholder views draw colored rectangles
from the logic symbols. Later TileMaps or sprites may paint over the same logic without changing
collision or trigger behavior.

Random encounters, terrain types, movement costs, map editors, final art, and larger dungeon
authoring remain deferred.
