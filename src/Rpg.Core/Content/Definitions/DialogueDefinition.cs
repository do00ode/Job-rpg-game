namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines one short, linear conversation displayed by an exploration interaction.
/// </summary>
/// <remarks>
/// Milestone 5.3A deliberately supports only a speaker key followed by ordered text keys.
/// Choices, conditions, commands, portraits, and cutscene instructions remain deferred.
/// </remarks>
public sealed record DialogueDefinition : ContentDefinition
{
    /// <summary>Localization key for the speaker label.</summary>
    public required string SpeakerNameKey { get; init; }

    /// <summary>Ordered localization keys advanced one at a time by the player.</summary>
    public List<string> LineTextKeys { get; init; } = [];
}
