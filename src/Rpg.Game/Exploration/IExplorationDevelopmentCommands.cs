namespace RpgGame.Exploration;

/// <summary>
/// Narrow persistence commands exposed to the exploration menu.
/// </summary>
/// <remarks>
/// The exploration scene needs a way to request save/load, but it must not learn where files
/// live or reach through the scene tree for <c>GameRoot</c>.
/// </remarks>
public interface IExplorationDevelopmentCommands
{
    /// <summary>Saves the current authoritative campaign to a logical slot.</summary>
    Task SaveSlotAsync(string slotId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a logical slot into the authoritative session, returning false if it is unused.
    /// </summary>
    Task<bool> LoadSlotAsync(string slotId, CancellationToken cancellationToken = default);
}
