namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines a reusable enemy species/template from which encounter combatants are created.
/// </summary>
/// <remarks>
/// Per-battle instance data such as current HP, status effects, and chosen actions belongs
/// in combat state. This record only holds stable authored defaults.
/// </remarks>
public sealed record EnemyDefinition : ContentDefinition
{
    /// <summary>Localization key for the enemy name shown to the player.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>Authored level used by future scaling and reward calculations.</summary>
    public int Level { get; init; } = 1;

    /// <summary>Base values keyed by statistic definition ID.</summary>
    public Dictionary<string, int> Statistics { get; init; } = [];

    /// <summary>Abilities available to this enemy's future AI.</summary>
    public List<string> AbilityIds { get; init; } = [];

    /// <summary>Independent item-drop possibilities evaluated after victory.</summary>
    public List<LootEntryDefinition> Loot { get; init; } = [];
}

/// <summary>Embedded description of one possible item drop.</summary>
public sealed record LootEntryDefinition
{
    /// <summary>Stable ID of the item that may drop.</summary>
    public required string ItemId { get; init; }

    /// <summary>Probability from 0 through 1; for example, 0.125 means 12.5%.</summary>
    public decimal Chance { get; init; }

    /// <summary>Smallest quantity awarded when this entry succeeds.</summary>
    public int MinQuantity { get; init; } = 1;

    /// <summary>Largest quantity awarded when this entry succeeds.</summary>
    public int MaxQuantity { get; init; } = 1;
}
