namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Contributes additions and removals to the new-game class-selection pool.
/// </summary>
/// <remarks>
/// The base game supplies one rule containing its vanilla choices. Mods add their own rule
/// records instead of overwriting vanilla class files. The final pool is the union of every
/// included ID minus the union of every excluded ID, so exclusions deterministically win
/// without depending on filesystem or mod load order.
/// </remarks>
public sealed record StartingClassRuleDefinition : ContentDefinition
{
    /// <summary>Classes this record contributes to the selectable starting pool.</summary>
    public List<string> IncludeClassIds { get; init; } = [];

    /// <summary>Classes this record removes from the final selectable starting pool.</summary>
    public List<string> ExcludeClassIds { get; init; } = [];
}
