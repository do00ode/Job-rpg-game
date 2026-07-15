using RpgGame.Core.Combat;
using RpgGame.Core.Content.Definitions;
using Xunit;

namespace RpgGame.Core.Tests.Combat;

public sealed class CombatResolverTests
{
    [Fact]
    public void Resolve_Attack_ReturnsNewSnapshotAndDamageEventWithoutMutatingInput()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        var resolver = new CombatResolver(battle.Content);
        CombatantSnapshot originalTarget = battle.Snapshot.GetRequiredCombatant("enemy-0");

        CombatResolution resolution = resolver.Resolve(
            battle.Snapshot,
            Attack("party-0", "enemy-0"));

        Assert.NotSame(battle.Snapshot, resolution.Next);
        Assert.Equal(22, originalTarget.CurrentHp);
        CombatantSnapshot updatedTarget = resolution.Next.GetRequiredCombatant("enemy-0");
        Assert.Equal(11, updatedTarget.CurrentHp);
        Assert.Equal(originalTarget.Placement, updatedTarget.Placement);
        Assert.Equal(originalTarget.Statistics.ToArray(), updatedTarget.Statistics.ToArray());
        Assert.Equal(originalTarget.AbilityIds, updatedTarget.AbilityIds);
        Assert.Equal(
            battle.Snapshot.Combatants.Select(combatant => combatant.InstanceId),
            resolution.Next.Combatants.Select(combatant => combatant.InstanceId));
        Assert.Same(
            battle.Snapshot.GetRequiredCombatant("party-0"),
            resolution.Next.GetRequiredCombatant("party-0"));
        Assert.Same(
            battle.Snapshot.GetRequiredCombatant("enemy-1"),
            resolution.Next.GetRequiredCombatant("enemy-1"));
        Assert.Equal(battle.Snapshot.Round, resolution.Next.Round);

        DamageApplied damage = Assert.IsType<DamageApplied>(Assert.Single(resolution.Events));
        Assert.Equal("party-0", damage.ActingCombatantId);
        Assert.Equal("enemy-0", damage.TargetCombatantId);
        Assert.Equal(CombatTestFixture.AttackId, damage.AbilityId);
        Assert.Equal(11, damage.Amount);
        Assert.Equal(22, damage.PreviousHp);
        Assert.Equal(11, damage.CurrentHp);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CombatEvent>)resolution.Events).Add(
                new CombatantDefeated("enemy-0")));
    }

    [Fact]
    public void Resolve_LethalAttack_ClampsDamageAndEmitsDefeatAfterDamage()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        var resolver = new CombatResolver(battle.Content);
        CombatSnapshot lowHpTarget = ReplaceCombatant(
            battle.Snapshot,
            "enemy-0",
            combatant => combatant.WithCurrentHp(5));

        CombatResolution resolution = resolver.Resolve(
            lowHpTarget,
            Attack("party-0", "enemy-0"));

        CombatantSnapshot defeated = resolution.Next.GetRequiredCombatant("enemy-0");
        Assert.Equal(0, defeated.CurrentHp);
        Assert.True(defeated.IsDefeated);
        Assert.Collection(
            resolution.Events,
            combatEvent =>
            {
                DamageApplied damage = Assert.IsType<DamageApplied>(combatEvent);
                Assert.Equal(5, damage.Amount);
                Assert.Equal(5, damage.PreviousHp);
                Assert.Equal(0, damage.CurrentHp);
            },
            combatEvent => Assert.Equal(
                "enemy-0",
                Assert.IsType<CombatantDefeated>(combatEvent).CombatantId));
    }

    [Fact]
    public void Resolve_HighDefenseStillTakesMinimumOneDamage()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        CombatSnapshot highDefense = ReplaceCombatant(
            battle.Snapshot,
            "enemy-0",
            combatant => WithStatistic(combatant, CombatStatisticIds.Defense, 999));

        CombatResolution resolution = new CombatResolver(battle.Content).Resolve(
            highDefense,
            Attack("party-0", "enemy-0"));

        DamageApplied damage = Assert.IsType<DamageApplied>(Assert.Single(resolution.Events));
        Assert.Equal(1, damage.Amount);
        Assert.Equal(21, resolution.Next.GetRequiredCombatant("enemy-0").CurrentHp);
    }

    [Fact]
    public void Resolve_DecimalPowerRoundsFinalDamageDownDeterministically()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        AbilityDefinition attack = PhysicalAbility(CombatTestFixture.AttackId, 4.75m);

        CombatResolution resolution = new CombatResolver(new TestCatalog(attack)).Resolve(
            battle.Snapshot,
            Attack("party-0", "enemy-0"));

        DamageApplied damage = Assert.IsType<DamageApplied>(Assert.Single(resolution.Events));
        Assert.Equal(11, damage.Amount);
    }

    [Fact]
    public void Resolve_EnemyPhysicalAbility_CanTargetLivingPartyCombatant()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();

        CombatResolution resolution = new CombatResolver(battle.Content).Resolve(
            battle.Snapshot,
            new CombatCommand(
                "enemy-0",
                CombatTestFixture.TackleId,
                ["party-0"]));

        DamageApplied damage = Assert.IsType<DamageApplied>(Assert.Single(resolution.Events));
        Assert.Equal(4, damage.Amount);
        Assert.Equal(92, resolution.Next.GetRequiredCombatant("party-0").CurrentHp);
    }

    [Fact]
    public void Resolve_MissingActor_IsRejected()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();

        AssertRejected(
            battle,
            Attack("party-missing", "enemy-0"),
            CombatCommandProblemCodes.ActorMissing);
    }

    [Fact]
    public void Resolve_DefeatedActor_IsRejected()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        CombatSnapshot defeatedActor = ReplaceCombatant(
            battle.Snapshot,
            "party-0",
            combatant => combatant.WithCurrentHp(0));

        AssertRejected(
            battle,
            Attack("party-0", "enemy-0"),
            CombatCommandProblemCodes.ActorDefeated,
            defeatedActor);
    }

    [Fact]
    public void Resolve_AbilityNotOwnedByActor_IsRejected()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();

        AssertRejected(
            battle,
            new CombatCommand("party-0", CombatTestFixture.TackleId, ["enemy-0"]),
            CombatCommandProblemCodes.AbilityNotOwned);
    }

    [Fact]
    public void Resolve_OwnedAbilityMissingFromCatalog_IsRejected()
    {
        const string missingAbilityId = "ability.test.missing";
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        CombatSnapshot missingAbility = ReplaceCombatant(
            battle.Snapshot,
            "party-0",
            combatant => new CombatantSnapshot(
                combatant.Placement,
                combatant.Statistics,
                [missingAbilityId],
                combatant.CurrentHp));
        var resolver = new CombatResolver(new TestCatalog());

        CombatCommandValidationException exception = Assert.Throws<
            CombatCommandValidationException>(() => resolver.Resolve(
                missingAbility,
                new CombatCommand("party-0", missingAbilityId, ["enemy-0"])));

        Assert.Equal(CombatCommandProblemCodes.AbilityMissing, exception.ProblemCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void Resolve_TargetCountOtherThanOne_IsRejected(int targetCount)
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        string[] targetIds = targetCount == 0
            ? []
            : ["enemy-0", "enemy-1"];

        AssertRejected(
            battle,
            new CombatCommand("party-0", CombatTestFixture.AttackId, targetIds),
            CombatCommandProblemCodes.TargetCountInvalid);
    }

    [Fact]
    public void Resolve_MissingTarget_IsRejected()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();

        AssertRejected(
            battle,
            Attack("party-0", "enemy-missing"),
            CombatCommandProblemCodes.TargetMissing);
    }

    [Fact]
    public void Resolve_DefeatedTarget_IsRejected()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        CombatSnapshot defeatedTarget = ReplaceCombatant(
            battle.Snapshot,
            "enemy-0",
            combatant => combatant.WithCurrentHp(0));

        AssertRejected(
            battle,
            Attack("party-0", "enemy-0"),
            CombatCommandProblemCodes.TargetDefeated,
            defeatedTarget);
    }

    [Fact]
    public void Resolve_TargetOnActorsSide_IsRejected()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();

        AssertRejected(
            battle,
            Attack("party-0", "party-0"),
            CombatCommandProblemCodes.TargetSameSide);
    }

    [Fact]
    public void Resolve_NonphysicalAbilityContract_IsRejected()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();

        AssertRejected(
            battle,
            new CombatCommand("party-0", CombatTestFixture.GuardId, ["enemy-0"]),
            CombatCommandProblemCodes.AbilityContractUnsupported);
    }

    [Fact]
    public void Resolve_CostBearingPhysicalAbility_IsRejected()
    {
        const string abilityId = "ability.test.costly-strike";
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        CombatSnapshot ownedAbility = ReplaceCombatant(
            battle.Snapshot,
            "party-0",
            combatant => new CombatantSnapshot(
                combatant.Placement,
                combatant.Statistics,
                [abilityId],
                combatant.CurrentHp));
        AbilityDefinition costly = PhysicalAbility(abilityId, 4m) with
        {
            CostStatisticId = "stat.max-mp",
            CostAmount = 1,
        };
        var resolver = new CombatResolver(new TestCatalog(costly));

        CombatCommandValidationException exception = Assert.Throws<
            CombatCommandValidationException>(() => resolver.Resolve(
                ownedAbility,
                new CombatCommand("party-0", abilityId, ["enemy-0"])));

        Assert.Equal(CombatCommandProblemCodes.AbilityCostUnsupported, exception.ProblemCode);
    }

    [Fact]
    public void CombatCommand_CopiesCallerOwnedTargetCollection()
    {
        var targetIds = new List<string> { "enemy-0" };
        var command = new CombatCommand(
            "party-0",
            CombatTestFixture.AttackId,
            targetIds);

        targetIds[0] = "enemy-1";

        Assert.Equal(["enemy-0"], command.TargetCombatantIds);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<string>)command.TargetCombatantIds).Add("enemy-1"));
    }

    private static CombatCommand Attack(string actorId, string targetId) => new(
        actorId,
        CombatTestFixture.AttackId,
        [targetId]);

    private static AbilityDefinition PhysicalAbility(string id, decimal power) => new()
    {
        Id = id,
        DisplayNameKey = $"{id}.name",
        DescriptionKey = $"{id}.description",
        AbilityKindId = AbilityKindIds.Skill,
        TargetingId = AbilityTargetingIds.SingleEnemy,
        RulesetId = AbilityRulesetIds.PhysicalDamage,
        NumericParameters = new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            [AbilityNumericParameterIds.Power] = power,
        },
    };

    private static void AssertRejected(
        FixedBattle battle,
        CombatCommand command,
        string expectedProblemCode,
        CombatSnapshot? snapshot = null)
    {
        CombatCommandValidationException exception = Assert.Throws<
            CombatCommandValidationException>(() => new CombatResolver(battle.Content).Resolve(
                snapshot ?? battle.Snapshot,
                command));

        Assert.Equal(expectedProblemCode, exception.ProblemCode);
    }

    private static CombatSnapshot ReplaceCombatant(
        CombatSnapshot source,
        string instanceId,
        Func<CombatantSnapshot, CombatantSnapshot> replace)
    {
        CombatantSnapshot[] combatants = source.Combatants.ToArray();
        int index = Array.FindIndex(
            combatants,
            combatant => string.Equals(
                combatant.InstanceId,
                instanceId,
                StringComparison.Ordinal));
        Assert.True(index >= 0, $"Fixture combatant '{instanceId}' was not found.");
        combatants[index] = replace(combatants[index]);
        return new CombatSnapshot(source.Round, combatants);
    }

    private static CombatantSnapshot WithStatistic(
        CombatantSnapshot source,
        string statisticId,
        int value)
    {
        var statistics = new Dictionary<string, int>(source.Statistics, StringComparer.Ordinal)
        {
            [statisticId] = value,
        };
        return source.PartyAbilityAvailability is null
            ? new CombatantSnapshot(
                source.Placement,
                statistics,
                source.AbilityIds,
                source.CurrentHp)
            : new CombatantSnapshot(
                source.Placement,
                statistics,
                source.PartyAbilityAvailability,
                source.CurrentHp);
    }
}
