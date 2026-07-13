namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines the inventory and shop-facing identity shared by consumables, key items,
/// materials, and equipment items.
/// </summary>
public sealed record ItemDefinition : ContentDefinition
{
    /// <summary>Localization key for the short player-facing item name.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>Localization key for explanatory text shown in item views.</summary>
    public required string DescriptionKey { get; init; }

    /// <summary>Base amount a shop charges before any future modifiers.</summary>
    public int BuyPrice { get; init; }

    /// <summary>Base amount a shop pays before any future modifiers.</summary>
    public int SellPrice { get; init; }

    /// <summary>Maximum quantity represented by one inventory stack.</summary>
    public int MaxStack { get; init; } = 99;
}
