namespace RpgGame.Core.Combat;

/// <summary>Stable identifiers for failures to produce one deterministic enemy command.</summary>
public static class EnemyCommandPlanningProblemCodes
{
    public const string ActorMissing = "combat.enemy-plan.actor-missing";
    public const string ActorNotEnemy = "combat.enemy-plan.actor-not-enemy";
    public const string ActorDefeated = "combat.enemy-plan.actor-defeated";
    public const string TargetUnavailable = "combat.enemy-plan.target-unavailable";
    public const string AbilityUnavailable = "combat.enemy-plan.ability-unavailable";
}

/// <summary>
/// Indicates that the basic enemy policy cannot create a legal command from the supplied state.
/// </summary>
public sealed class EnemyCommandPlanningException : InvalidOperationException
{
    public EnemyCommandPlanningException(string problemCode, string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(problemCode);
        ProblemCode = problemCode;
    }

    public string ProblemCode { get; }
}
