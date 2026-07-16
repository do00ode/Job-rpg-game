# Milestone 4.4 - Data-driven battle command menu

## Purpose

The battle screen projects the acting party combatant's structured
`PartyAbilityAvailability` instead of assuming `ability.command.attack`. Direct Skills appear
at the top level. Every unlocked magic discipline appears as a deterministic submenu, whose
learned spells remain in authored order.

## Ownership

- `BattleCommandAvailabilityResolver` in `Rpg.Core` converts a party snapshot into immutable,
  currently usable direct-skill and magic-discipline command groups.
- `BattleController` in `Rpg.Game` creates buttons, owns focus and submenu navigation, resolves
  stable ability IDs through the injected catalog, and submits ordinary `CombatCommand` values.
- `CombatResolver` remains the sole owner of command legality and effects.

The availability resolver filters contracts the current resolver cannot execute and abilities
the actor cannot currently afford. It preserves discipline containers even when all their
spells are unavailable, so presentation can show a disabled submenu rather than inventing
content state.

## Targeting

`target.enemy.single` opens the living-enemy selector. `target.ally.single` opens the living
ally selector. `target.combatant.single` opens a selector for every living combatant, including
the acting combatant. `target.self` is routed immediately to the acting combatant and never opens
selection. Guard remains deliberately deferred, so no self command is currently selectable.

Menu/Cancel returns from a magic submenu to the top-level menu, or from target selection to
the menu that selected the ability. Movement cycles command buttons in insertion order and
target buttons in snapshot order; remappable input actions remain unchanged.

Milestone 4.6 supplies the first authored kits that use this projection: Knight Power Strike
and Black Mage's Black Magic spell list. Names remain temporary stable-ID-derived placeholders.
Localized display names, MP affordability explanations, command icons, multi-actor queues,
items, and new targeting/effect contracts remain outside this milestone.

## Compatibility

No content schema or save schema changes are introduced. Existing omitted `abilityKindId`
continues to mean Skill. The UI reads the authoritative availability already present in each
party combatant snapshot; it does not persist menu state.
