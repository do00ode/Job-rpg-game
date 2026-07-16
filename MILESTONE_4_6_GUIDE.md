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
| Knight | `class.martial.vanguard` | Direct Skill `ability.knight.power-strike` |
| Black Mage | `class.magic.black-mage` | Black Magic access plus Fire, Ice, and Lightning |
| White Mage | `class.magic.white-mage` | White Magic access only |

The pre-existing `class.martial.vanguard` ID now presents the Knight kit. The permanent ID is
retained because existing saves store it in `ActorProgressState.ClassId`; it must not be renamed
or removed. The base starting-class pool still contains exactly these three records.

Power Strike, Fire, Ice, and Lightning are `target.enemy.single` abilities using the existing
`rules.damage.physical` formula. Power Strike is Slash. The three spells explicitly select
Fire, Ice, and Lightning damage types and spend current MP through `stat.max-mp` cost content.
A dedicated magical-stat formula, random variance, critical hits, animation, and area targets
remain outside this milestone.

Black Magic is a container, not an executable ability. The Black Mage receives both the
discipline unlock and individual spell unlocks; either fact alone is insufficient to make an
unlearned or inaccessible spell executable. White Mage similarly receives White Magic access,
but no Cure record is authored because the current resolver has no honest healing effect.

## Presentation

The existing 4.4 menu shows Knight's direct Power Strike and Black Mage's learned Black Magic
spells. White Magic is an empty disabled container rather than a fake damage or broken Cure
command. Names remain stable-ID-derived placeholders until localization presentation exists.

## Compatibility and deferrals

No save field, content schema version, or mod data-API version changes. Existing saves using
`class.martial.vanguard` continue to resolve the Knight kit. Content mods may add compatible
class grants, disciplines, and spells using the established JSON-only contracts.

Cure and all healing are deferred to Milestone 4.7. Also deferred: additional classes,
equipment, item commands, status effects, magic-specific statistics, multi-target abilities,
MP recovery, class progression beyond authored level-one grants, and class-change systems.
