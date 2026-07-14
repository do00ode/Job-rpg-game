namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines one short, linear conversation displayed by an exploration interaction.
/// </summary>
/// <remarks>
/// Milestone 2 deliberately supports only a speaker label followed by ordered text lines.
/// Choices, conditions, commands, portraits, and cutscene instructions require real future
/// gameplay cases before they earn fields here. The strings are placeholder authored text;
/// a localization contract is deferred rather than hidden behind an unfinished abstraction.
/// </remarks>
public sealed record DialogueDefinition : ContentDefinition
{
    /// <summary>Temporary player-facing speaker text for the minimal dialogue panel.</summary>
    public required string SpeakerName { get; init; }

    /// <summary>Ordered nonblank lines advanced one at a time by the player.</summary>
    public List<string> Lines { get; init; } = [];
}
