namespace RpgGame.Core.Content.Definitions;

/// <summary>Stable IDs for the small status contracts understood by the current build.</summary>
public static class StatusEffectKindIds
{
    public const string ModifySpeedPercent = "status-effect.modify-speed-percent";

    public static bool IsSupported(string id) =>
        string.Equals(id, ModifySpeedPercent, StringComparison.Ordinal);
}

public static class StatusStackingRuleIds
{
    public const string RefreshDuration = "refresh-duration";
    public const string IgnoreIfPresent = "ignore-if-present";
    public const string Replace = "replace";

    public static bool IsSupported(string id) => id is
        RefreshDuration or IgnoreIfPresent or Replace;
}

public static class StatusDurationUnitIds
{
    public const string TimelineTime = "timeline-time";
}

/// <summary>
/// Declarative status content. It selects closed code-owned behavior and never executes code.
/// </summary>
public sealed record StatusEffectDefinition : ContentDefinition
{
    public required string DisplayNameKey { get; init; }

    public string? DescriptionKey { get; init; }

    public string StackingRuleId { get; init; } = StatusStackingRuleIds.RefreshDuration;

    public long DefaultDuration { get; init; }

    public string DurationUnitId { get; init; } = StatusDurationUnitIds.TimelineTime;

    public List<string> EffectKindIds { get; init; } = [];

    /// <summary>
    /// Closed test/future tuning value used only by ModifySpeedPercent.
    /// </summary>
    public int SpeedPercentModifier { get; init; }
}
