namespace RpgGame.Core.Content.Definitions;

public sealed record WeaponFamilyDefinition : ContentDefinition
{
    public required string DisplayNameKey { get; init; }
    public required DamageVarianceDefinition DamageVariance { get; init; }
    public Dictionary<string, int> WeaponDamagePercentages { get; init; } = [];
}
