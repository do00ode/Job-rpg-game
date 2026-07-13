namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines one equippable/assignable RPG class and its level-based learning table.
/// </summary>
public sealed record ClassDefinition : ContentDefinition
{
    /// <summary>Localization key for the class name shown in menus.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>
    /// Additive adjustments keyed by statistic ID. The eventual statistic calculator
    /// combines these with actor, equipment, and status contributions.
    /// </summary>
    public Dictionary<string, int> BaseStatisticBonuses { get; init; } = [];

    /// <summary>Ordered or validated list of abilities learned at class levels.</summary>
    public List<AbilityUnlockDefinition> AbilityUnlocks { get; init; } = [];
}

/// <summary>
/// Embedded value object connecting a class level to an ability definition.
/// It is not independently addressable content, so it does not inherit ContentDefinition.
/// </summary>
public sealed record AbilityUnlockDefinition
{
    /// <summary>Class level at which the ability becomes available.</summary>
    public int Level { get; init; } = 1;

    /// <summary>Stable ID of the ability to unlock.</summary>
    public required string AbilityId { get; init; }
}
