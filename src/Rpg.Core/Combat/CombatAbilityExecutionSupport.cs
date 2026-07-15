using RpgGame.Core.Content.Definitions;

namespace RpgGame.Core.Combat;

/// <summary>
/// Single description of the ability contract the current action resolver can execute.
/// </summary>
/// <remarks>
/// Both command execution and enemy planning need this answer. Keeping it here prevents the AI
/// from offering an ability that <see cref="CombatResolver"/> will reject. This is deliberately
/// not a registry or generic effect engine; it describes only the one implemented physical
/// contract and should grow only alongside real resolver behavior and focused tests.
/// </remarks>
internal static class CombatAbilityExecutionSupport
{
    public static bool HasSupportedCost(AbilityDefinition ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        return ability.CostStatisticId is null && ability.CostAmount == 0;
    }

    public static bool HasSupportedContract(AbilityDefinition ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        return string.Equals(
                ability.TargetingId,
                AbilityTargetingIds.SingleEnemy,
                StringComparison.Ordinal)
            && string.Equals(
                ability.RulesetId,
                AbilityRulesetIds.PhysicalDamage,
                StringComparison.Ordinal);
    }

    public static bool IsCurrentlyUsable(AbilityDefinition ability) =>
        HasSupportedCost(ability) && HasSupportedContract(ability);
}
