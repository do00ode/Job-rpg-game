namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines the authored data for one action that combat or another rules system can use.
/// </summary>
/// <remarks>
/// Definitions choose from a small set of code-owned targeting and ruleset behaviors.
/// They supply tuning values but do not contain animation code or an open-ended scripting
/// language. Presentation assets will be mapped separately by stable ability ID.
/// </remarks>
public sealed record AbilityDefinition : ContentDefinition
{
    /// <summary>Localization key for the short ability name.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>Localization key for help text describing the ability.</summary>
    public required string DescriptionKey { get; init; }

    /// <summary>
    /// Stable key selecting a code-owned targeting rule, such as one enemy or all allies.
    /// </summary>
    public required string TargetingId { get; init; }

    /// <summary>
    /// Statistic/resource spent to use the ability, or null when the ability is free.
    /// </summary>
    public string? CostStatisticId { get; init; }

    /// <summary>Amount deducted from <see cref="CostStatisticId"/> when used.</summary>
    public int CostAmount { get; init; }

    /// <summary>
    /// Identifies a small, code-owned rules implementation; this is not a scripting DSL.
    /// </summary>
    public required string RulesetId { get; init; }

    /// <summary>
    /// Ruleset-specific tuning values, such as power or accuracy. Each known ruleset must
    /// validate its accepted keys so this dictionary does not become an accidental DSL.
    /// Decimal values make authored percentages/factors explicit and deterministic.
    /// </summary>
    public Dictionary<string, decimal> NumericParameters { get; init; } = [];
}
