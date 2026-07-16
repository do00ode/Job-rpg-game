namespace RpgGame.Core.State;

/// <summary>
/// Narrow application service that owns the active state across scene changes.
/// </summary>
/// <remarks>
/// The implementation lives for the application's lifetime, but consumers receive this
/// small interface rather than a global bag of managers. Feature use cases will perform
/// validated state transitions. The contract exposes only the concrete campaign mutations
/// currently required by new-game, save/load, and the first exploration slice.
/// </remarks>
public interface IGameSession
{
    /// <summary>Whether a new or loaded campaign is currently active.</summary>
    bool HasActiveGame { get; }

    /// <summary>Current authoritative campaign snapshot.</summary>
    GameState Current { get; }

    /// <summary>
    /// Raised after authoritative session state changes. Scene/UI subscribers should
    /// re-read <see cref="Current"/> rather than treating an individual Node as truth.
    /// </summary>
    event EventHandler? StateChanged;

    /// <summary>
    /// Replaces the whole campaign snapshot when starting or restoring a game.
    /// Normal feature mutations will use narrower use cases rather than this method.
    /// </summary>
    void ReplaceState(GameState state);

    /// <summary>
    /// Replaces the persistent tile location after the exploration scene accepts a move.
    /// Collision remains scene-owned because Core has no Godot map or physics dependency.
    /// </summary>
    void UpdateLocation(MapLocationState location);

    /// <summary>
    /// Replaces campaign inventory after a content-aware use case validates item stacks.
    /// </summary>
    void UpdateInventory(IReadOnlyDictionary<string, int> inventory);

    /// <summary>
    /// Replaces one actor's persistent progress after a feature-specific use case validates it.
    /// </summary>
    void UpdateActorProgress(string actorId, ActorProgressState progress);

    /// <summary>Returns a persistent flag, treating an absent key as false.</summary>
    bool GetEventFlag(string flagId);

    /// <summary>Sets one stable persistent fact and notifies state observers.</summary>
    void SetEventFlag(string flagId, bool value = true);
}
