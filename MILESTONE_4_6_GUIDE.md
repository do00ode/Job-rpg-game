# Milestone 4.6 - First real class kits

## Purpose

This milestone makes the selected starting class observable in battle without adding a new
combat ruleset. The existing Skill/Magic availability projection, single-target physical damage
formula, typed affinity calculation, data-driven menu, and current-MP payment all participate:

```text
Selected class -> learned abilities and discipline access -> battle menu -> CombatCommand
-> immutable combat result and events
```

## Starter kits

| Starter presentation | Stable class ID | Level-one grants |
|---|---|---|
| Knight | `class.martial.knight` | Direct Skill `ability.knight.power-strike` |
| Black Mage | `class.magic.black-mage` | Black Magic access plus Fire, Ice, and Lightning |
| White Mage | `class.magic.white-mage` | White Magic access; Cure arrives in Milestone 4.7 |

`class.martial.knight` is the canonical Knight content ID. Save format 2 replaces the former
Vanguard ID with this new ID during load, so existing format-1 campaigns remain playable and
re-save in the canonical form. The base starting-class pool still contains exactly these three
records.

Power Strike, Fire, Ice, and Lightning are `target.enemy.single` abilities using the existing
`rules.damage.physical` formula. Power Strike is Slash. The three spells explicitly select
Fire, Ice, and Lightning damage types and spend current MP through `stat.max-mp` cost content.
A dedicated magical-stat formula, random variance, critical hits, animation, and area targets
remain outside this milestone.

Black Magic is a container, not an executable ability. The Black Mage receives both the
discipline unlock and individual spell unlocks; either fact alone is insufficient to make an
unlearned or inaccessible spell executable. Milestone 4.7 extends White Mage with Cure through
the same learned-spell plus discipline-access boundary.

## Presentation

The existing 4.4 menu shows Knight's direct Power Strike and Black Mage's learned Black Magic
spells. Milestone 4.7 makes White Magic present Cure through the same menu projection. Names
remain stable-ID-derived placeholders until localization presentation exists.

## Compatibility and deferrals

The content schema and mod data-API version do not change. Save format 2 migrates format-1
campaigns that use the retired Vanguard class ID. Content mods may add compatible class grants,
disciplines, and spells using the established JSON-only contracts.

Additional classes, equipment, item commands, status effects, magic-specific statistics,
multi-target abilities, MP recovery, class progression beyond authored level-one grants, and
class-change systems remain deferred.
