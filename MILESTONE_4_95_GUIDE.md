# Milestone 4.95 - Character equipment screen with immediate stat comparison

The remappable `game.equipment` action opens a character-focused screen directly; its default
key is `I`, and the existing Menu entry remains available. The equipment screen fills the
viewport with an opaque blue surface, so exploration is not visible behind it.

The permanent top section shows actor, class, level, and the current Weapon, Off Hand, Body, Feet, Helm,
Accessory 1, and Accessory 2 slots. Confirming a slot opens compatible owned choices in the lower-left pane while the current
equipment stays visible above. The lower-right pane shows current and preview Max HP, Max MP,
Strength, Intelligence, Defense, Spirit, Speed, Weapon Attack, and item details. Movement changes focus, Interact
confirms, Menu backs out one level, and Equipment closes the screen directly.

Each preview value explicitly shows the signed change from the current loadout. Gains are green,
losses are red, and unchanged values use a neutral color. Equipment can also author optional
`specialEffectIds`; these are displayed as item effects, but remain inert until a later
code-owned rules milestone assigns their behavior.

`EquipmentScreenProjectionResolver` is a Godot-free read model. A highlighted item or Unequip
creates only a copied `ActorProgressState.EquippedItems` map, resolves statistics through
`CombatStatisticResolver`, and returns current and projected values. It never changes
`GameState`. Confirm remains the responsibility of `EquipmentService`; session notification then
rebuilds the screen from authoritative state.

Weapon Attack is separate from Strength. Iron Sword previews Weapon Attack +4 and Strength +0.
Equipment statistic modifiers are applied by the shared statistic resolver, so future simple
armor/accessory modifiers preview identically to confirmed combat stats. Intelligence and Spirit
are authored, resolved, and presented attributes ready for later explicit magic-damage, healing,
or magical-resistance rules; this screen does not assign them combat behavior.

The starter content includes Wooden Shield (Off Hand, Defense +2), Leather Armor (Body, Defense +2), Leather Boots (Feet, Speed +1),
Leather Helm (Helm, Defense +1), Power Ring (Accessory 1, Strength +1), and Spirit Charm
(Accessory 2, Spirit +1). They enter a new campaign as ordinary inventory stacks and are not
auto-equipped; Iron Sword remains the one equipped starter item. Existing saves remain valid:
they simply have no entries for newly available slots until the player equips something.

Only `slot.weapon.main-hand` currently projects Weapon Attack and its damage profile into battle.
The Off Hand is ready for shields and future off-hand equipment, but dual wielding and a
two-handed/bow occupancy rule are intentionally deferred until a concrete weapon requires them.

Deferred: full inventory UI, shops, best equipment, multi-actor selector, per-instance gear,
executable special effects, advanced armor/accessory behavior, ATB, status effects, and hybrid
classes.
