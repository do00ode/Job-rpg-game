namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines one reusable enemy formation plus presentation lookup keys for a battle.
/// </summary>
public sealed record EncounterDefinition : ContentDefinition
{
    /// <summary>Enemy templates and their unique positions in this formation.</summary>
    public List<EncounterEnemyDefinition> EnemyGroup { get; init; } = [];

    /// <summary>
    /// Optional stable presentation key for the battle background/arena. Core rules never
    /// turn this key into a Godot resource path.
    /// </summary>
    public string? BattlefieldId { get; init; }

    /// <summary>Optional stable presentation key for music selection.</summary>
    public string? MusicCueId { get; init; }
}

/// <summary>One enemy placement embedded in an encounter formation.</summary>
public sealed record EncounterEnemyDefinition
{
    /// <summary>Stable ID of the enemy template to instantiate.</summary>
    public required string EnemyId { get; init; }

    /// <summary>
    /// Unique formation position such as <c>formation.left</c>. This is an abstract slot,
    /// allowing presentation to decide its exact screen coordinates.
    /// </summary>
    public required string SlotId { get; init; }
}
