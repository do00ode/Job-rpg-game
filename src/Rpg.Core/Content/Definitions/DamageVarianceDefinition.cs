using System.Text.Json.Serialization;

namespace RpgGame.Core.Content.Definitions;

public sealed record DamageVarianceDefinition
{
    [JsonRequired]
    public int MinimumPercent { get; init; }

    [JsonRequired]
    public int MaximumPercent { get; init; }
}
