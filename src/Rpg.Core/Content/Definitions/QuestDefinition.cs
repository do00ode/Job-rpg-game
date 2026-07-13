namespace RpgGame.Core.Content.Definitions;

/// <summary>
/// Defines the authored objectives and rewards for one quest.
/// </summary>
/// <remarks>
/// Whether the quest is inactive, active, or complete is playthrough state and will be
/// stored separately. This definition is shared by every save file.
/// </remarks>
public sealed record QuestDefinition : ContentDefinition
{
    /// <summary>Localization key for the journal's quest title.</summary>
    public required string DisplayNameKey { get; init; }

    /// <summary>Localization key for the journal's quest description.</summary>
    public required string DescriptionKey { get; init; }

    /// <summary>Conditions that must be satisfied for completion.</summary>
    public List<QuestObjectiveDefinition> Objectives { get; init; } = [];

    /// <summary>Items awarded when completion is accepted.</summary>
    public List<QuestRewardDefinition> Rewards { get; init; } = [];

    /// <summary>
    /// Optional persistent event flag set on completion so maps/dialogue can react without
    /// directly depending on the quest implementation.
    /// </summary>
    public string? CompletionFlagId { get; init; }
}

/// <summary>
/// One addressable objective embedded in a quest. Objective kinds remain a small,
/// code-owned registry rather than arbitrary scripts.
/// </summary>
public sealed record QuestObjectiveDefinition
{
    /// <summary>
    /// Stable within its parent quest because save data may refer to it.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>Stable rule key such as <c>objective.defeat</c> or <c>objective.reach</c>.</summary>
    public required string Kind { get; init; }

    /// <summary>ID of the enemy, item, map marker, or other subject interpreted by Kind.</summary>
    public required string TargetId { get; init; }

    /// <summary>Number of matching actions required; one is appropriate for reach/talk.</summary>
    public int RequiredCount { get; init; } = 1;
}

/// <summary>One item stack granted as part of quest completion.</summary>
public sealed record QuestRewardDefinition
{
    /// <summary>Stable ID of the awarded item.</summary>
    public required string ItemId { get; init; }

    /// <summary>Positive number of copies awarded.</summary>
    public int Quantity { get; init; } = 1;
}
