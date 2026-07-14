namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines a unique, party-capable story actor and the values used to create new progress.
/// </summary>
/// <remarks>
/// This describes the actor's identity and intrinsic values, not one campaign's class or
/// level. Mutable progress belongs in <c>ActorProgressState</c> so it survives save/load.
/// </remarks>
public sealed record ActorDefinition : ContentDefinition
{
    /// <summary>Localization key used to obtain the actor's player-facing name.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>
    /// Actor-specific base values keyed by statistic ID. These are inputs to future
    /// progression formulas rather than a snapshot of current battle statistics.
    /// </summary>
    public Dictionary<string, int> BaseStatistics { get; init; } = [];

    /// <summary>Abilities granted independently of the actor's starting class.</summary>
    public List<string> StartingAbilityIds { get; init; } = [];
}
