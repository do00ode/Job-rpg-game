using RpgGame.Core.Content;
using RpgGame.Core.Content.Definitions;

namespace RpgGame.Core.State;

/// <summary>
/// Creates a valid initial campaign snapshot from immutable actor/class content.
/// </summary>
public sealed class NewGameFactory
{
    private static readonly HashSet<string> ValidFacings =
        new(StringComparer.Ordinal) { "north", "east", "south", "west" };

    private readonly IContentCatalog _content;

    public NewGameFactory(IContentCatalog content)
    {
        _content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Builds fresh actor progress, active-party order, location, and empty event flags.
    /// Invalid setup is rejected here rather than becoming a corrupt save later.
    /// </summary>
    public GameState Create(NewGameRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SaveId);

        if (!ContentId.IsValid(request.StartingMapId)
            || !request.StartingMapId.StartsWith("map.", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Starting map '{request.StartingMapId}' must be a canonical map.* ID.",
                nameof(request));
        }

        if (!ValidFacings.Contains(request.StartingFacing))
        {
            throw new ArgumentException(
                $"Starting facing '{request.StartingFacing}' is not north/east/south/west.",
                nameof(request));
        }

        PartyRules.ValidateMemberCount(request.StartingPartyMembers.Count, nameof(request));

        IReadOnlyList<string> selectableStartingClasses = StartingClassPool.Resolve(_content);
        if (selectableStartingClasses.Count == 0)
        {
            throw new InvalidOperationException(
                "Content does not provide any selectable starting classes.");
        }

        var selectableClassIds = selectableStartingClasses.ToHashSet(StringComparer.Ordinal);

        var progress = new Dictionary<string, ActorProgressState>(StringComparer.Ordinal);
        foreach (StartingPartyMemberRequest member in request.StartingPartyMembers)
        {
            if (member is null)
            {
                throw new ArgumentException(
                    "Starting party entries cannot be null.",
                    nameof(request));
            }

            if (member.Level < 1)
            {
                throw new ArgumentException(
                    $"Starting level for actor '{member.ActorId}' must be at least 1.",
                    nameof(request));
            }

            if (progress.ContainsKey(member.ActorId))
            {
                throw new ArgumentException(
                    $"Starting actor '{member.ActorId}' is listed more than once.",
                    nameof(request));
            }

            ActorDefinition actor = _content.GetRequired<ActorDefinition>(member.ActorId);
            ClassDefinition selectedClass = _content.GetRequired<ClassDefinition>(member.ClassId);

            if (!selectableClassIds.Contains(selectedClass.Id))
            {
                throw new ArgumentException(
                    $"Class '{selectedClass.Id}' is not available during new-game selection.",
                    nameof(request));
            }

            progress.Add(actor.Id, new ActorProgressState
            {
                ActorId = actor.Id,
                ClassId = selectedClass.Id,
                Level = member.Level,
                Experience = 0,
            });
        }

        return new GameState
        {
            SaveId = request.SaveId,
            Location = new MapLocationState
            {
                MapId = request.StartingMapId,
                X = request.StartingX,
                Y = request.StartingY,
                Facing = request.StartingFacing,
            },
            ActivePartyActorIds = request.StartingPartyMembers
                .Select(member => member.ActorId)
                .ToList(),
            ActorProgress = progress,
            EventFlags = new Dictionary<string, bool>(StringComparer.Ordinal),
        };
    }
}
