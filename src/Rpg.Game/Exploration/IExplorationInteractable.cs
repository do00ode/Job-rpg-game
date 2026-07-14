using Godot;
using RpgGame.Core.State;

namespace RpgGame.Exploration;

/// <summary>
/// Small presentation-boundary contract for something occupying an exploration tile.
/// </summary>
/// <remarks>
/// The interface lives in the Godot assembly because tile occupancy uses <see cref="Vector2I"/>
/// and belongs to the current map. The returned IDs and state mutation remain explicit, so
/// the controller does not need an untyped event bus or knowledge of NPC implementation details.
/// </remarks>
public interface IExplorationInteractable
{
    /// <summary>Tile that must be directly in front of the player.</summary>
    Vector2I TilePosition { get; }

    /// <summary>Performs the interaction and identifies the dialogue to present.</summary>
    ExplorationInteractionResult Interact(IGameSession session);

    /// <summary>Rebuilds transient presentation from authoritative campaign state.</summary>
    void RefreshFromState(IGameSession session);
}

/// <summary>Typed outcome consumed by the owning exploration controller.</summary>
public sealed record ExplorationInteractionResult(string DialogueId);
