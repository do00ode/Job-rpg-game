# UI font asset

The exploration UI theme is prepared for a project-owned dynamic font. Add the selected
redistributable `.ttf` or `.otf` here, record its licence beside it, and assign it as the
`default_font` of `game/themes/UiTheme.tres`.

Do not replace this with a `SystemFont`: the game must not depend on a font installed on the
player's operating system. Until the asset is chosen, the theme deliberately uses Godot's built-in
dynamic fallback font, which is output-resolution rasterized and avoids the prior Windows-only
Consolas dependency.
