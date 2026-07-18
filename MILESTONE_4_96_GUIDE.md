# Milestone 4.96 - Pixel-perfect 4:3 presentation

The game uses a fixed `320x240` logical gameplay canvas. This is the complete authored frame:
twenty 16x16 map tiles wide by fifteen tiles high. Godot uses `canvas_items` stretching with
integer scale mode so pixel-art CanvasItems keep nearest-neighbor presentation while dynamic UI
fonts rasterize at output resolution. Widescreen output uses pillarbox bars; it never stretches
the frame or changes the map coordinate system.

Maps and exploration sprites are authored in native pixels. A map larger than the native frame
scrolls through a `Camera2D` that follows the player, clamps to map bounds, centers smaller maps,
uses no zoom or smoothing, and snaps its final position to whole native pixels.

Exploration menus, equipment, controls, battle, and victory rewards must fit wholly inside the
authored canvas. Presentation uses full-viewport anchors, container size flags, bounded margins,
and compact control heights. It must not depend on a centered fixed rectangle wider or taller
than the reference viewport.

The equipment screen keeps the actor and equipped Weapon, Body, and Accessory rows at the top.
Its lower choices and statistics panes divide the available width rather than reserving desktop
minimum widths. Its comparison uses paired compact rows for HP/MP, Strength/Intelligence,
Defense/Spirit, and Speed/Weapon Attack, so all resolved values fit without pushing equipped
slots above the viewport. Item details are intentionally hidden in this compact pass so detail
text cannot move controls below the viewport. Game Menu and Controls are opaque full-viewport screens, so
exploration does not remain visible behind a menu.

The Game Menu includes a Display entry backed by `DisplaySettingsService`. It immediately applies
one supported windowed resolution: 640x480, 800x600, 1024x768, 1280x720, 1366x768, or 1920x1080.
It also offers explicit 2x through 5x integer pixel-scale presets (640x480 through 1600x1200)
for players who want a smaller or larger crisp output without changing the internal canvas.
The project launches at 960x720 by default. Resolution selection is currently session-local;
persistent display preferences are deferred. The internal viewport remains 320x240 at every size.

Pixel-art CanvasItems use the project default nearest texture filter. UI must use the shared scene
theme rather than a machine-specific `SystemFont` or intentionally disabled font rasterization.
When a redistributable UI TTF/OTF is added under `game/assets/fonts/`, that theme is the single
place to assign it; until then Godot's built-in dynamic fallback font is used deliberately.

`BattleFormationView` owns only adaptable rendering geometry. It calculates cell widths, cell
heights, grid positions, and presentation labels from its allocated `Control.Size`, then rebuilds
those labels when resized. Formation rules, placements, command resolution, and `GameState` stay
unchanged.

Supported manual output checks:

- 640x480, 800x600, and 1024x768 for 4:3 output scaling;
- 1280x720 and 1920x1080 for 16:9;
- 1280x800 and 1920x1200 for 16:10.

At each size, verify the equipment, Game Menu, Controls, dialogue, battle, and reward summary
remain inside the 4:3 frame and that widescreen output has pillarboxes rather than distortion.
This milestone does not add mobile layouts, safe-area handling, controller-specific navigation,
or localization expansion.
