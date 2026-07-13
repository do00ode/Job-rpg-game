namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines a named numeric statistic and its content-level legal range.
/// </summary>
/// <remarks>
/// Examples include maximum HP, strength, magic, or speed. Defining statistics as data
/// lets content validation understand dictionaries that are keyed by statistic ID.
/// </remarks>
public sealed record StatisticDefinition : ContentDefinition
{
    /// <summary>Localization key for menus and status screens.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>Smallest legal authored/base value, inclusive.</summary>
    public int MinimumValue { get; init; }

    /// <summary>Largest legal authored/base value, inclusive.</summary>
    public int MaximumValue { get; init; }

    /// <summary>Fallback value when a source does not explicitly supply this statistic.</summary>
    public int DefaultValue { get; init; }
}
