# Starting heroes and class choices

This guide explains the small distinction that keeps James replayable and makes the
new-game class list moddable without turning the project into a general-purpose engine.

## The three different facts

| Fact | Example | Stored in | Why |
|---|---|---|---|
| Actor identity | `actor.hero.james` | `ActorDefinition` JSON | James remains the same story person. |
| Legal new-game choices | Vanguard, Black Mage, White Mage | `StartingClassRuleDefinition` JSON | Base content and mods compose one pool. |
| This save's choice | James chose White Mage at level 1 | `ActorProgressState` | Different campaigns can make different choices. |

Do not put James's current class in `james.json`. That would make content identity and saved
progress the same thing. It would also make replaying as a different class require changing a
definition that every save shares.

## What happens during new game

1. `JsonContentLoader` reads actors, classes, and `starting-class-rules/` records.
2. `ContentValidator` proves every rule references an existing class and the final pool is
   nonempty.
3. `StartingClassPool.Resolve` unions all includes, removes all excludes, then sorts IDs.
4. The future title screen will show those resolved class definitions to the player.
5. `StartingPartyMemberRequest` carries the selected actor ID, class ID, and level.
6. `NewGameFactory` rechecks the choice and creates `ActorProgressState` in `GameState`.
7. Save/load preserves that selected `ClassId`; it does not reread a default from James.

The current project has no title screen. `GameRoot.StartNewGame()` therefore chooses the
first stable ID in the resolved pool only to exercise startup. A future controller will call
`GameRoot.StartNewGame(selectedClassId)` after the player confirms a choice.

## Editing James

`game/content/actors/james.json` owns James's stable ID, name localization key, intrinsic
base statistics, and actor-specific abilities. `displayNameKey` is not the displayed text;
it is a stable lookup key. A later localization file can map `actor.james.name` to `James`
without making display text part of identity.

`startingAbilityIds` should contain only abilities James receives regardless of class.
Vanguard's Guard belongs in `classes/vanguard.json`, not on James, because a Black Mage or
White Mage James should not inherit Vanguard-only training.

Milestone 3.10 adds `ability.command.attack` to James's `startingAbilityIds` for the opposite
reason: Attack is the universal basic command James should retain with every starting class.
That intrinsic grant does not make James a Vanguard and does not change which classes can be
selected.

## Adding a vanilla starting class

1. Add one class JSON file under `game/content/classes/` with a permanent `class.*` ID.
2. Add that ID to `includeClassIds` in
   `game/content/starting-class-rules/default.json`.
3. Run content validation and the core tests.

Removing an ID from the base include list stops new unmodded campaigns from choosing it. It
does not erase the class or rewrite saves that already selected it.

## Changing choices from a data mod

A mod adds its own namespaced rule rather than editing a vanilla file:

```json
{
  "schemaVersion": 1,
  "id": "newgame.class-rule.example.my-mod.options",
  "includeClassIds": ["class.example.my-mod.new-class"],
  "excludeClassIds": ["class.magic.black-mage"]
}
```

The record above adds the mod's class and removes vanilla Black Mage. Exclusion wins across
all installed rules. This is intentionally simple and deterministic: there is no priority,
load-order override, or last-file-wins behavior for authors to debug.

## Future randomizer boundary

A seeded randomizer should receive the already resolved class IDs plus an injected
`IRandomSource`, then return a selected ID. It should not rewrite content, depend on directory
order, or use `System.Random` invisibly. That work waits until a real randomizer/new-game UI
milestone so the project does not gain abstractions without a gameplay use case.

## Current limits

- There is one class per actor in `ActorProgressState`; multiclassing is not implemented.
- Later class unlocks and class-changing are not implemented.
- Class names and descriptions have no UI yet.
- Black Mage and White Mage are schema-valid placeholder definitions, not final balance.
- The campaign supports one to four total heroes; mods cannot increase that limit.
