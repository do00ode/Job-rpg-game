namespace RpgGame.Core.Combat;

/// <summary>
/// Reports the authoritative HP change produced by one accepted damage ability.
/// </summary>
/// <remarks>
/// Presentation can animate from <see cref="PreviousHp"/> to <see cref="CurrentHp"/> without
/// repeating the damage formula. <see cref="Amount"/> is applied damage after remaining-HP
/// clamping, so it never reports more damage than the target actually lost.
/// </remarks>
public sealed record DamageApplied(
    string ActingCombatantId,
    string TargetCombatantId,
    string AbilityId,
    int Amount,
    int PreviousHp,
    int CurrentHp) : CombatEvent;

/// <summary>Reports that one combatant reached zero HP during the resolved action.</summary>
/// <remarks>
/// This is a fact about battle-local state only. It does not declare victory, defeat, rewards,
/// encounter clearing, or any campaign-state change.
/// </remarks>
public sealed record CombatantDefeated(string CombatantId) : CombatEvent;
