using System.Diagnostics.CodeAnalysis;
using RpgGame.Core.Combat.Formation;

namespace RpgGame.Core.Combat;

/// <summary>
/// Resolves one complete deterministic round from commands collected before any action runs.
/// </summary>
/// <remarks>
/// This class coordinates existing single-action rules; it does not duplicate damage math or
/// choose player/enemy intent. Commands act by descending Speed, with ordinal instance ID as an
/// explicit tie-breaker. An actor defeated by an earlier action loses its pending action. The
/// round stops immediately when either side has no living combatants.
/// </remarks>
public sealed class CombatRoundResolver : ICombatRoundResolver
{
    private readonly ICombatResolver _actionResolver;

    public CombatRoundResolver(ICombatResolver actionResolver)
    {
        _actionResolver = actionResolver
            ?? throw new ArgumentNullException(nameof(actionResolver));
    }

    /// <inheritdoc />
    public CombatResolution ResolveRound(
        CombatSnapshot current,
        IReadOnlyList<CombatCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(commands);

        BattleOutcome startingOutcome = current.Outcome;
        if (startingOutcome != BattleOutcome.InProgress)
        {
            Reject(
                CombatRoundProblemCodes.BattleAlreadyEnded,
                $"Round {current.Round} cannot begin because the battle outcome is "
                + $"'{startingOutcome}'.");
        }

        IReadOnlyDictionary<string, CombatantSnapshot> startingCombatants =
            IndexStartingCombatants(current);
        IReadOnlyDictionary<string, CombatCommand> commandsByActor =
            ValidateAndIndexCommands(commands, startingCombatants);

        foreach (CombatantSnapshot combatant in current.Combatants)
        {
            if (!combatant.IsDefeated && !commandsByActor.ContainsKey(combatant.InstanceId))
            {
                Reject(
                    CombatRoundProblemCodes.CommandMissing,
                    $"Living combatant '{combatant.InstanceId}' has no command for round "
                    + $"{current.Round}.");
            }
        }

        PlannedCommand[] orderedCommands = commandsByActor.Values
            .Select(command =>
            {
                CombatantSnapshot actor = startingCombatants[command.ActingCombatantId];
                return new PlannedCommand(command, RequireSpeed(actor));
            })
            .OrderByDescending(planned => planned.Speed)
            .ThenBy(
                planned => planned.Command.ActingCombatantId,
                StringComparer.Ordinal)
            .ToArray();

        CombatSnapshot next = current;
        var events = new List<CombatEvent>();
        foreach (PlannedCommand planned in orderedCommands)
        {
            if (next.Outcome != BattleOutcome.InProgress)
            {
                break;
            }

            // The command was legal to collect because its actor began alive. Re-read that
            // actor from the newest immutable snapshot: a faster action may have defeated it.
            CombatantSnapshot actingCombatant = next.GetRequiredCombatant(
                planned.Command.ActingCombatantId);
            if (actingCombatant.IsDefeated)
            {
                continue;
            }

            CombatResolution action = _actionResolver.Resolve(next, planned.Command);
            next = action.Next;
            events.AddRange(action.Events);
        }

        // Round means "the round accepting commands." Only create the next round number when
        // both sides survived and another round can actually begin.
        if (next.Outcome == BattleOutcome.InProgress)
        {
            if (next.Round == int.MaxValue)
            {
                throw new InvalidOperationException(
                    "Combat round cannot advance beyond the maximum 32-bit integer value.");
            }

            next = new CombatSnapshot(next.Round + 1, next.Combatants);
        }

        return new CombatResolution(next, events);
    }

    private static IReadOnlyDictionary<string, CombatantSnapshot> IndexStartingCombatants(
        CombatSnapshot current)
    {
        var result = new Dictionary<string, CombatantSnapshot>(StringComparer.Ordinal);
        foreach (CombatantSnapshot combatant in current.Combatants)
        {
            if (!result.TryAdd(combatant.InstanceId, combatant))
            {
                throw new InvalidDataException(
                    $"Combat snapshot contains duplicate battle-local instance ID "
                    + $"'{combatant.InstanceId}'.");
            }
        }

        return result;
    }

    private static IReadOnlyDictionary<string, CombatCommand> ValidateAndIndexCommands(
        IReadOnlyList<CombatCommand> commands,
        IReadOnlyDictionary<string, CombatantSnapshot> startingCombatants)
    {
        var result = new Dictionary<string, CombatCommand>(StringComparer.Ordinal);
        foreach (CombatCommand? command in commands)
        {
            if (command is null)
            {
                throw new ArgumentException(
                    "A combat round command collection cannot contain null.",
                    nameof(commands));
            }

            string? actorId = command.ActingCombatantId;
            if (string.IsNullOrWhiteSpace(actorId))
            {
                Reject(
                    CombatRoundProblemCodes.ActorMissing,
                    "Round command actor ID cannot be blank.");
            }

            if (!startingCombatants.TryGetValue(actorId, out CombatantSnapshot? actor))
            {
                Reject(
                    CombatRoundProblemCodes.ActorMissing,
                    $"Round command actor '{actorId}' does not exist in the starting "
                    + "snapshot.");
            }

            if (actor.IsDefeated)
            {
                Reject(
                    CombatRoundProblemCodes.ActorDefeated,
                    $"Defeated combatant '{actor.InstanceId}' must not submit a round "
                    + "command.");
            }

            if (!result.TryAdd(actor.InstanceId, command))
            {
                Reject(
                    CombatRoundProblemCodes.CommandDuplicate,
                    $"Living combatant '{actor.InstanceId}' submitted more than one command.");
            }
        }

        return result;
    }

    private static int RequireSpeed(CombatantSnapshot combatant)
    {
        if (!combatant.Statistics.TryGetValue(CombatStatisticIds.Speed, out int speed))
        {
            throw new InvalidDataException(
                $"Combatant '{combatant.InstanceId}' is missing required combat statistic "
                + $"'{CombatStatisticIds.Speed}'.");
        }

        return speed;
    }

    [DoesNotReturn]
    private static void Reject(string problemCode, string message) =>
        throw new CombatRoundValidationException(problemCode, message);

    private sealed record PlannedCommand(CombatCommand Command, int Speed);
}
