namespace RpgGame.Exploration;

/// <summary>
/// Narrow development-only commands exposed to the Milestone 2 test room.
/// </summary>
/// <remarks>
/// The exploration scene needs a way to manually prove save/load, but it must not learn
/// where files live or reach through the scene tree for <c>GameRoot</c>. A future title or
/// pause menu will replace these shortcuts with its own presentation while continuing to
/// call the same application-level save operations.
/// </remarks>
public interface IExplorationDevelopmentCommands
{
    /// <summary>The logical slot used by the temporary K/L developer shortcuts.</summary>
    string QuickSlotId { get; }

    /// <summary>Saves the current authoritative campaign to the quick slot.</summary>
    Task SaveQuickSlotAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the quick slot into the authoritative session, returning false if it is unused.
    /// </summary>
    Task<bool> LoadQuickSlotAsync(CancellationToken cancellationToken = default);
}
