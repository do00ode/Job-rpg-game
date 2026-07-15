namespace RpgGame.Core.Combat;

/// <summary>Stable identifiers for invalid complete-round command collections.</summary>
/// <remarks>
/// These failures concern collection completeness and round ownership. Once an ordered action
/// begins, ordinary <see cref="CombatCommandValidationException"/> rules still validate the
/// selected ability and target against the latest snapshot.
/// </remarks>
public static class CombatRoundProblemCodes
{
    public const string BattleAlreadyEnded = "combat.round.battle-already-ended";
    public const string ActorMissing = "combat.round.actor-missing";
    public const string ActorDefeated = "combat.round.actor-defeated";
    public const string CommandDuplicate = "combat.round.command-duplicate";
    public const string CommandMissing = "combat.round.command-missing";
}

/// <summary>Indicates that commands cannot form one legal complete combat round.</summary>
public sealed class CombatRoundValidationException : InvalidOperationException
{
    public CombatRoundValidationException(string problemCode, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(problemCode);
        ProblemCode = problemCode;
    }

    public string ProblemCode { get; }
}
