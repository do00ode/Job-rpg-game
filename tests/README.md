# Automated tests

`RpgGame.Core.Tests` is a fast, headless xUnit suite for rules, state transitions,
content validation, deterministic combat, quest logic, and save migrations. Godot
scene smoke tests will live in a separate project when scene behavior exists; they
must not replace unit tests for nonvisual logic.

Run the current suite from the repository root:

```sh
dotnet test tests/RpgGame.Core.Tests/RpgGame.Core.Tests.csproj
```

The Milestone 1 integration test uses a unique directory beneath the operating system's
temporary folder and removes it afterward. It does not touch a developer's actual Godot
`user://saves` directory.

Milestone 1.5 tests also load the checked-in example data mod, enforce its namespace,
exercise dependency sorting/missing/cycle failures, and verify that saves reject missing or
version-mismatched required mods. Temporary manifest installations are deleted after each test.
The suite also proves all three vanilla starting classes can create James and that the example
mod can add Chronoguard while excluding vanilla Black Mage from the resolved pool.

Milestone 2 tests validate the linear dialogue record, exercise location and event-flag
mutations through `GameSession`, and prove the moved location plus
`flag.test-room.npc-spoken-to` survive the real filesystem save/load round trip. Visual input
and layout remain manual/Godot smoke-test concerns rather than being duplicated in Core tests.
