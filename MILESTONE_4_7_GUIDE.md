# Milestone 4.7 - Healing and ally targeting

## Purpose

Milestone 4.7 adds the first restorative combat effect without generalizing combat into an
effect framework. White Mage learns Cure, selects one living ally, pays MP through the existing
resource path, and receives an authoritative `HealingApplied` event.

## Closed contracts

| Contract | Meaning |
|---|---|
| `target.ally.single` | Exactly one living combatant on the acting combatant's side. |
| `rules.healing.flat` | Restore the positive whole-number authored `power` amount. |

Flat healing is deterministic:

```text
nextHp = min(target MaximumHp, target CurrentHp + authored power)
appliedHealing = nextHp - target CurrentHp
```

The event reports `appliedHealing`, not the authored power, so a near-full target correctly
reports only the HP actually restored. A successful MP-costing Cure emits `ResourceSpent` before
`HealingApplied`. Rejected commands change neither HP nor MP.

## Targeting and presentation

The core requires same-side, living targets for `target.ally.single`; this is side-neutral so a
future enemy healer could use the contract, but `EnemyCommandPlanner` deliberately continues to
plan only damage actions. The battle controller uses its existing target row for both living
enemies and living allies, enabling only the side selected by the authored ability.

White Mage receives both `magic-discipline.white-magic` and
`ability.white-magic.cure`. Discipline access does not grant an unlearned spell, and a learned
spell without a matching unlocked discipline remains unavailable.

## Compatibility and deferrals

No save field, migration, schema-version, or mod data-API change is required. Current HP and
MP remain battle-local snapshot state.

Deferred: revive, defeated targeting, area healing, enemy healer AI, healing items, regeneration,
status cleansing, overheal/shields, magical-stat scaling, random variance, animation, and sound.
