# Example data mod

`mod.example.starter-pack` is a deliberately tiny authoring fixture. It proves that a mod can
add a class and ability, reference base-game statistics, and validate without adding scripts,
assemblies, scenes, or gameplay code. Its starting-class rule adds Chronoguard to the
new-game pool and removes the vanilla Black Mage choice, demonstrating both supported
directions without overwriting a vanilla record.

Validate the base pack and this example together from the repository root:

```powershell
dotnet run --project tools/content-validation/RpgGame.ContentValidation.csproj -- game/content examples/mods
```

Successful combined validation loads 21 definitions. With this example enabled, the final
starting pool is Chronoguard, White Mage, and Vanguard.

To try discovery in a development build, copy the entire `mod.example.starter-pack` folder into
the Godot project's `user://mods` folder. The startup output will report one enabled data mod.
