namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Adds equippable behavior to an ordinary item without duplicating its name, price,
/// description, or inventory properties.
/// </summary>
public sealed record EquipmentDefinition : ContentDefinition
{
    /// <summary>Stable ID of the corresponding inventory item.</summary>
    public required string ItemId { get; init; }

    /// <summary>Stable game-owned slot ID, such as <c>slot.weapon.main-hand</c>.</summary>
    public required string SlotId { get; init; }

    /// <summary>Additive equipped bonuses keyed by statistic definition ID.</summary>
    public Dictionary<string, int> StatisticModifiers { get; init; } = [];

    /// <summary>
    /// Weapon damage composition keyed by code-owned damage type ID. Values are whole
    /// percentages and a nonempty profile must total exactly 100.
    /// </summary>
    public Dictionary<string, int> WeaponDamagePercentages { get; init; } = [];

    /// <summary>
    /// Direct offensive value used by the intrinsic basic Attack when this is an equipped weapon.
    /// This is intentionally separate from statistic modifiers such as Strength.
    /// </summary>
    public int Attack { get; init; }

    public string? WeaponFamilyId { get; init; }

    public DamageVarianceDefinition? DamageVariance { get; init; }

    /// <summary>Abilities available only while this equipment is active.</summary>
    public List<string> GrantedAbilityIds { get; init; } = [];

    /// <summary>
    /// Reserved code-owned effect IDs for future equipment mechanics and presentation.
    /// These IDs describe no behavior until a later rules milestone explicitly supports them.
    /// </summary>
    public List<string> SpecialEffectIds { get; init; } = [];
}
