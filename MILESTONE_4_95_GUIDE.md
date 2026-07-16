# Milestone 4.95 - Character equipment screen with immediate stat comparison

The remappable `game.equipment` action opens a character-focused screen directly; its default
key is `I`, and the existing Menu entry remains available. The equipment screen fills the
viewport with an opaque blue surface, so exploration is not visible behind it.

The permanent top section shows actor, class, level, and the current Weapon, Body, and Accessory
slots. Confirming a slot opens compatible owned choices in the lower-left pane while the current
equipment stays visible above. The lower-right pane shows current and preview Max HP, Max MP,
Strength, Intelligence, Defense, Spirit, Speed, Weapon Attack, and item details. Movement changes focus, Interact
confirms, Menu backs out one level, and Equipment closes the screen directly.

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

Deferred: full inventory UI, shops, best equipment, multi-actor selector, per-instance gear,
special effects, advanced armor/accessory behavior, ATB, status effects, and hybrid classes.
