namespace RpgGame.Core.State;

/// <summary>
/// Narrow application service that owns the active state across scene changes.
/// </summary>
/// <remarks>
/// The future implementation may live for the application's lifetime, but consumers only
/// receive this small interface rather than a global bag of managers. Feature use cases
/// will perform validated state transitions; this bootstrap contract currently demonstrates
/// ownership and restore/new-game replacement without implementing gameplay.
/// </remarks>
public interface IGameSession
{
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
}
