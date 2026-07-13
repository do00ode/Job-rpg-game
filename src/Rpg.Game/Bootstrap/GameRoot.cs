using Godot;

namespace RpgGame.Bootstrap;

/// <summary>
/// Godot composition root for scene-facing adapters and narrowly scoped application services.
/// </summary>
/// <remarks>
/// Godot creates this Node from <c>game/scenes/bootstrap/GameRoot.tscn</c>, which is the
/// project's configured main scene. Future startup code will construct long-lived services
/// such as the content catalog, game session, save coordinator, and scene navigator here,
/// then inject narrow interfaces into scene controllers.
///
/// Gameplay behavior does not belong in this node. Keeping the root intentionally boring
/// prevents it from becoming an unrestricted global GameManager over time.
///
/// Godot requires scripts deriving from GodotObject to be <c>partial</c>; its source
/// generators supply the engine-facing portion of the class during compilation.
/// </remarks>
public partial class GameRoot : Node
{
    // Empty by design for Milestone 0. Startup behavior is added only when the first
    // application services have real implementations and tests.
}
