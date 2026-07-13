namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines a unique, party-capable story actor and the values used to create new progress.
/// </summary>
/// <remarks>
/// This is a starting template, not the actor's current level or class. Mutable progress
/// belongs in <c>ActorProgressState</c> so it survives scenes and save/load.
/// </remarks>
public sealed record ActorDefinition : ContentDefinition
{
    /// <summary>Localization key used to obtain the actor's player-facing name.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>Class definition assigned when a brand-new game creates this actor.</summary>
    public required string StartingClassId { get; init; }

    /// <summary>Initial level before any experience is earned.</summary>
    public int StartingLevel { get; init; } = 1;

    /// <summary>
    /// Actor-specific base values keyed by statistic ID. These are inputs to future
    /// progression formulas rather than a snapshot of current battle statistics.
    /// </summary>
    public Dictionary<string, int> BaseStatistics { get; init; } = [];

    /// <summary>Abilities granted independently of the actor's starting class.</summary>
    public List<string> StartingAbilityIds { get; init; } = [];
}
