# Milestone 5.1 - Status Effect Foundation

## Authored definitions

`StatusEffectDefinition` is a registered `status-effects/` content category. It contains a
stable ID, display key, optional description key, `stackingRuleId`, positive `defaultDuration`,
`durationUnitId`, and closed `effectKindIds`. The only executable kind currently supported is
`status-effect.modify-speed-percent`; its explicit `speedPercentModifier` is bounded and is
intended as a foundation hook, not a scripting language. No production status records were added.

## Active combat state

`CombatantSnapshot.ActiveStatusEffects` is an immutable list of `ActiveStatusEffect` values.
Each value stores the status ID, optional source combatant ID, application timeline time,
positive duration, and stack count. Statuses are transient encounter state and are not part of
`GameState` or normal saves. Defeated combatants retain status values for inspection, but defeated
targets cannot receive new statuses and statuses do not affect initiative selection.

## Duration and stacking

5.1 supports only `timeline-time`. A status expires when
`CurrentTimelineTime >= AppliedTimelineTime + Duration`. `CombatStatusService.ExpireStatuses`
removes expired values in combatant order and emits `StatusExpired`. The timeline resolver runs
expiration cleanup before selecting an actor; preview projection ignores statuses already expired
at the forecast time.

Reapplying an existing status is keyed by `StatusEffectId` on the target:

- `refresh-duration` creates a new application at the current timeline time with the authored
  duration and emits `StatusRefreshed`.
- `ignore-if-present` returns the same snapshot and emits `StatusIgnored`.
- `replace` replaces the active value and emits `StatusRefreshed`.

Explicit removal is a deterministic no-op when the status is absent and emits `StatusRemoved`
when a value is removed.

## Timeline hook

`CombatStatusService.ResolveEffectiveSpeed` is the single status-aware speed path. Without active
speed modifiers it returns resolved Speed clamped to one. A test-only speed-percent status changes
that value, and both timeline scheduling and turn-order preview consume the resolver. Future
Haste/Slow/Stop/Stun/Quick/Delay Strike work should extend this closed hook boundary rather than
moving status logic into Godot or duplicating speed formulas.

## Explicit deferrals

Production status spells and abilities, status resistance/immunity, random application chance,
cleansing, revive/death statuses, poison/regen ticks, stat and damage mitigation hooks, full
status icons, animations, sound, boss scripting, and status-duration gameplay beyond the
timeline-time foundation are deferred.

## Validation

```text
dotnet test tests/RpgGame.Core.Tests/RpgGame.Core.Tests.csproj --no-restore
Passed! - Failed: 0, Passed: 381, Skipped: 0, Total: 381

dotnet run --project tools/content-validation/RpgGame.ContentValidation.csproj -- game/content
Content validation passed: 41 definitions loaded

dotnet run --project tools/content-validation/RpgGame.ContentValidation.csproj -- game/content examples/mods
Content validation passed: 44 definitions loaded

dotnet build RpgGame.sln --no-restore
Build succeeded. 0 Warning(s), 0 Error(s).

D:\Godot\Godot_v4.7-stable_mono_win64.exe --headless --editor --path . --quit
Exit code: 0
```

No interactive Godot playtest or screenshot capture was run.
