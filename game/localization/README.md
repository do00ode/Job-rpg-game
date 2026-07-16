# Base Text Catalog

`en.json` maps stable content keys to player-facing English text. To change an item's
description, edit the value matching its `descriptionKey`; do not change the key itself.

Example:

```json
"item.leather-boots.description": "Scuffed leather boots. They know exactly how to leave a room dramatically."
```

Missing keys fall back to showing the key during development, making unfinished text visible.
