# Milestone 4.96 - Resolution-safe presentation

The game uses a `640x480` logical viewport as its minimum supported presentation canvas. This
matches common 4:3 CRT resolutions and gives every interactive scene one concrete lower bound.
Godot uses `canvas_items` stretching with `expand` aspect handling: 4:3 screens retain the
reference composition, while wider displays gain horizontal space without changing UI ownership
or requiring a second scene layout.

Exploration menus, equipment, controls, battle, and victory rewards must fit wholly inside that
canvas. Presentation uses full-viewport anchors, container size flags, bounded margins, and
compact control heights. It must not depend on a centered fixed rectangle wider or taller than
the reference viewport.

The equipment screen keeps the actor and equipped Weapon, Body, and Accessory rows at the top.
Its lower choices and statistics panes divide the available width rather than reserving desktop
minimum widths. Item details are intentionally limited to one visible line so detail text cannot
move controls below the viewport. Game Menu and Controls are opaque full-viewport screens, so
exploration does not remain visible behind a menu.

`BattleFormationView` owns only adaptable rendering geometry. It calculates cell widths, cell
heights, grid positions, and presentation labels from its allocated `Control.Size`, then rebuilds
those labels when resized. Formation rules, placements, command resolution, and `GameState` stay
unchanged.

Supported manual viewport checks:

- 640x480, 800x600, and 1024x768 for 4:3;
- 1280x720 and 1920x1080 for 16:9;
- 1280x800 and 1920x1200 for 16:10.

At each size, verify the equipment, Game Menu, Controls, dialogue, battle, and reward summary
have no clipped selectable control or inaccessible keyboard focus. This milestone does not add
mobile layouts, safe-area handling, controller-specific navigation, localization expansion, or
dynamic font scaling.
