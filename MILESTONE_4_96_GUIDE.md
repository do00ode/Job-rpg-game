# Milestone 4.96 - Resolution-safe presentation

The game retains its authored `1280x720` logical viewport because the fixed exploration room and
its HUD already own geometry in that coordinate space. Godot uses `canvas_items` stretching, so
smaller windowed resolutions scale the complete authored canvas instead of changing map geometry
or reflowing the room into a different coordinate system.

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
The project launches at 1280x720 by default. Resolution selection is currently session-local;
persistent display preferences are deferred.

`BattleFormationView` owns only adaptable rendering geometry. It calculates cell widths, cell
heights, grid positions, and presentation labels from its allocated `Control.Size`, then rebuilds
those labels when resized. Formation rules, placements, command resolution, and `GameState` stay
unchanged.

Supported manual viewport checks:

- 640x480, 800x600, and 1024x768 for 4:3 output scaling;
- 1280x720 and 1920x1080 for 16:9;
- 1280x800 and 1920x1200 for 16:10.

At each size, verify the equipment, Game Menu, Controls, dialogue, battle, and reward summary
have no clipped selectable control or inaccessible keyboard focus. This milestone does not add
mobile layouts, safe-area handling, controller-specific navigation, localization expansion, or
dynamic font scaling.
