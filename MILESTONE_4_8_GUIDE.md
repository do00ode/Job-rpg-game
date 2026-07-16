# Milestone 4.8 - Equipment ownership, equipped slots, and weapon Attack

## Persistent ownership

`GameState.Inventory` remains the campaign's sole item ownership record. Each
`ActorProgressState.EquippedItems` entry maps a stable slot ID to an owned inventory item ID;
equipping neither consumes nor duplicates a stack. Omitted maps deserialize as empty for old
saves. The starter slice supports only `slot.weapon.main-hand`.

`EquipmentService.EquipItem(actorId, equipmentItemId, slotId)` resolves the item and its unique
equipment definition, requires positive inventory ownership, checks the authored slot, and
publishes a replacement actor-progress state through `IGameSession`. Re-equipping replaces the
slot. `UnequipItem` clears it; an already-empty slot is a no-op. Multi-actor ownership conflicts
are intentionally deferred while the campaign has one actor.

## Weapon Attack

Equipment `attack` is a nonnegative weapon-only offensive value, not a character statistic.
Strength continues to come from actor/class statistics. Iron Sword uses `attack: 4` and no
`stat.strength` modifier. Statistic modifiers remain reserved for future conventional gear.

At battle construction, the equipped main-hand weapon is projected into the immutable combatant
snapshot. Intrinsic `ability.command.attack` uses:

```text
max(1, Strength + WeaponAttack + AbilityPower - Defense)
```

Only basic Attack gets WeaponAttack for this milestone. Power Strike, enemy Tackle, Fire, Ice,
Lightning, Cure, and other future abilities keep their existing authored contracts. A single
100% weapon profile supplies basic Attack's damage type; no weapon leaves its authored/legacy
type in place. Mixed profiles are valid content but cannot start a battle while equipped.

## Temporary starter content and deferrals

Until Milestone 4.9 adds an equipment menu, bootstrap new games grant and equip James's Iron
Sword through the core inventory/equipment services. There is no Godot equipment state.

Deferred: equipment UI, shops, item use in battle, multi-component damage, weapon-based skill
rulesets beyond Attack, dual wielding, two-handed weapons, armor elemental resistance, special
accessories, per-instance uniqueness, upgrades, affixes, ATB, status effects, and hybrid classes.
