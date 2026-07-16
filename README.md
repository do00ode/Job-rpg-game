# RpgGame

Start here:

- `CONTENT_AUTHORING_GUIDE.md`: add items, equipment, maps, enemies, and loot tables.
- `CONTENT_SCHEMA.md`: complete JSON field and validation reference.
- `ARCHITECTURE.md`: ownership and dependency boundaries.
- `CURRENT_PROJECT_HANDOFF.md`: current implementation state.
- `ROADMAP.md`: milestones and deferred work.

Before sharing content changes, run:

```powershell
dotnet test tests/RpgGame.Core.Tests/RpgGame.Core.Tests.csproj
dotnet run --project tools/content-validation/RpgGame.ContentValidation.csproj -- game/content
```
