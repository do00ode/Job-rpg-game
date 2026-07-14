namespace RpgGame.Core.State;

/// <summary>
/// Game-specific starting choices supplied to the reusable new-game factory.
/// </summary>
public sealed record NewGameRequest
{
    /// <summary>Unique identity assigned to this new playthrough.</summary>
    public required string SaveId { get; init; }

    /// <summary>Stable initial map ID.</summary>
    public required string StartingMapId { get; init; }

    /// <summary>Starting horizontal tile coordinate.</summary>
    public int StartingX { get; init; }

    /// <summary>Starting vertical tile coordinate.</summary>
    public int StartingY { get; init; }

    /// <summary>Initial logical facing direction.</summary>
    public string StartingFacing { get; init; } = "south";

    /// <summary>Actors, selected classes, and levels in initial party order.</summary>
    public List<StartingPartyMemberRequest> StartingPartyMembers { get; init; } = [];
}

/// <summary>
/// One explicit actor/class choice used to construct a new campaign.
/// </summary>
/// <remarks>
/// Keeping this choice outside ActorDefinition lets separate runs—and future randomizers—
/// start the same story actor in different valid classes without rewriting actor content.
/// </remarks>
public sealed record StartingPartyMemberRequest
{
    /// <summary>Stable actor definition selected for this party position.</summary>
    public required string ActorId { get; init; }

    /// <summary>Stable class definition selected from the resolved starting-class pool.</summary>
    public required string ClassId { get; init; }

    /// <summary>Initial campaign level for this actor.</summary>
    public int Level { get; init; } = 1;
}
