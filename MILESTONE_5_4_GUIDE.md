# Milestone 5.4 - Combat Damage Variance and Weapon Personality

Damage variance is an inclusive percentage roll applied after the authored damage formula and
damage-type affinity multiplier. The result is floored once and clamped to the target's remaining
HP. Randomness is supplied through `IRandomSource`, so combat tests and replays can use scripted
rolls.

## Authoring

Weapon-family records live in `game/content/weapon-families/` and use IDs beginning with
`weapon-family.`. Each record supplies `displayNameKey`, a default `weaponDamagePercentages`
profile, and `damageVariance` bounds. The five
vanilla families are sword (95-105 Slash), hammer (90-110 Blunt), axe (75-125 50/50 Blunt/Slash),
bow (85-115 Pierce), and gun (90-110 Pierce).

Weapon equipment may reference `weaponFamilyId`; its nonempty `weaponDamagePercentages` map
overrides the family profile, which allows elemental or mixed-damage weapons. An empty map uses
the family default. Equipment may also provide a `damageVariance` override.
Abilities may also provide a `damageVariance` override. Bounds are inclusive, nonnegative, ordered,
and capped at 500 percent by content validation.

## Precedence

Ability override, equipped weapon override, weapon-family default, ruleset fallback, then the safe
95-105 fallback. Magic abilities use an 80-120 fallback when they have no authored override.
Unassigned or enemy physical attacks use the safe physical fallback.

The resolver reports the selected roll in `DamageApplied.VariancePercent`; it does not mutate
inventory, campaign state, or content definitions.

## Deferred

This milestone does not add critical hits, accuracy, elemental status effects, dual-wield rules,
weapon-family UI, or a new damage ruleset. Existing content may continue to use the historical
physical ruleset ID; `damageTypeId` remains the field that selects affinity behavior.
