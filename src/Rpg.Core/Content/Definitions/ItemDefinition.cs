namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines the inventory and shop-facing identity shared by consumables, key items,
/// materials, and equipment items.
/// </summary>
public sealed record ItemDefinition : ContentDefinition
{
    private int _maxStack = 99;

    /// <summary>Localization key for the short player-facing item name.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>Localization key for explanatory text shown in item views.</summary>
    public required string DescriptionKey { get; init; }

    /// <summary>Base amount a shop charges before any future modifiers.</summary>
    public int BuyPrice { get; init; }

    /// <summary>Base amount a shop pays before any future modifiers.</summary>
    public int SellPrice { get; init; }

    /// <summary>Maximum quantity represented by one inventory stack.</summary>
    public int MaxStack
    {
        get => Unique ? 1 : _maxStack;
        init => _maxStack = value;
    }

    /// <summary>
    /// Marks story, quest, or other one-of-a-kind items. Unique items always use a stack
    /// limit of one; ordinary items default to ninety-nine.
    /// </summary>
    public bool Unique { get; init; }

    /// <summary>
    /// Whether an owned stack can be selected from the battle Item command.
    /// Battle-use items always select one living combatant from either side.
    /// </summary>
    public bool BattleUse { get; init; }

    /// <summary>
    /// The code-owned ability contract executed when this item is used in battle.
    /// Required only when <see cref="BattleUse"/> is true.
    /// </summary>
    public string? BattleAbilityId { get; init; }
}
