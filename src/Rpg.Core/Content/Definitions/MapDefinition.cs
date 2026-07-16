namespace RpgGame.Core.Content.Definitions;

/// <summary>Defines an exploration map and its named logical spawn points.</summary>
public sealed record MapDefinition : ContentDefinition
{
    public required string DisplayNameKey { get; init; }
    public List<MapSpawnDefinition> Spawns { get; init; } = [];
}

/// <summary>A logical tile used when entering an exploration map.</summary>
public sealed record MapSpawnDefinition
{
    public required string Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public string Facing { get; init; } = "south";
}
