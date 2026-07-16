using RpgGame.Core.Combat;
using RpgGame.Core.Content;
using RpgGame.Core.Content.Definitions;
using RpgGame.Core.State;
using Xunit;

namespace RpgGame.Core.Tests.Combat;

public sealed class FirstClassKitsTests
{
    private const string KnightId = "class.martial.vanguard";
    private const string BlackMageId = "class.magic.black-mage";
    private const string WhiteMageId = "class.magic.white-mage";
    private const string PowerStrikeId = "ability.knight.power-strike";
    private const string FireId = "ability.black-magic.fire";
    private const string IceId = "ability.black-magic.ice";
    private const string LightningId = "ability.black-magic.lightning";
    private const string BlackMagicId = "magic-discipline.black-magic";
    private const string WhiteMagicId = "magic-discipline.white-magic";

    [Fact]
    public void ResolvePartyActor_KnightAddsPowerStrikeAsDirectSkill()
    {
        ContentCatalog content = TestContent.LoadCatalog();

        PartyAbilityAvailability availability = ResolveAvailability(content, KnightId);

        Assert.Equal([CombatTestFixture.AttackId, PowerStrikeId], availability.DirectSkillIds);
        Assert.Empty(availability.MagicDisciplines);
    }

    [Fact]
    public void ResolvePartyActor_BlackMageRequiresAndGrantsBlackMagicSpellbook()
    {
        ContentCatalog content = TestContent.LoadCatalog();

        PartyAbilityAvailability availability = ResolveAvailability(content, BlackMageId);

        Assert.Equal([CombatTestFixture.AttackId], availability.DirectSkillIds);
        MagicDisciplineAvailability discipline = Assert.Single(availability.MagicDisciplines);
        Assert.Equal(BlackMagicId, discipline.MagicDisciplineId);
        Assert.Equal([FireId, IceId, LightningId], discipline.SpellAbilityIds);
        Assert.Equal(
            [CombatTestFixture.AttackId, FireId, IceId, LightningId],
            availability.ExecutableAbilityIds);
    }

    [Fact]
    public void ResolvePartyActor_WhiteMageHasNoBrokenUnlearnedSpellCommands()
    {
        ContentCatalog content = TestContent.LoadCatalog();

        PartyAbilityAvailability availability = ResolveAvailability(content, WhiteMageId);

        Assert.Equal([CombatTestFixture.AttackId], availability.ExecutableAbilityIds);
        MagicDisciplineAvailability discipline = Assert.Single(availability.MagicDisciplines);
        Assert.Equal(WhiteMagicId, discipline.MagicDisciplineId);
        Assert.Empty(discipline.SpellAbilityIds);
    }

    [Theory]
    [InlineData(FireId, DamageTypeIds.Fire)]
    [InlineData(IceId, DamageTypeIds.Ice)]
    [InlineData(LightningId, DamageTypeIds.Lightning)]
    public void Resolve_BlackMageSpellEmitsItsAuthoredDamageType(
        string abilityId,
        string expectedDamageTypeId)
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle(BlackMageId);

        CombatResolution resolution = new CombatResolver(battle.Content).Resolve(
            battle.Snapshot,
            new CombatCommand("party-0", abilityId, ["enemy-0"]));

        Assert.IsType<ResourceSpent>(resolution.Events[0]);
        DamageApplied damage = Assert.IsType<DamageApplied>(resolution.Events[1]);
        Assert.Equal(expectedDamageTypeId, damage.DamageTypeId);
    }

    [Fact]
    public void Resolve_FireSpendsMpExactlyOnce()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle(BlackMageId);
        CombatantSnapshot originalParty = battle.Snapshot.GetRequiredCombatant("party-0");

        CombatResolution resolution = new CombatResolver(battle.Content).Resolve(
            battle.Snapshot,
            new CombatCommand("party-0", FireId, ["enemy-0"]));

        CombatantSnapshot updatedParty = resolution.Next.GetRequiredCombatant("party-0");
        ResourceSpent spent = Assert.IsType<ResourceSpent>(resolution.Events[0]);
        Assert.Equal(FireId, spent.AbilityId);
        Assert.Equal(3, spent.Amount);
        Assert.Equal(originalParty.CurrentMp, spent.PreviousValue);
        Assert.Equal(originalParty.CurrentMp - spent.Amount, spent.CurrentValue);
        Assert.Equal(spent.CurrentValue, updatedParty.CurrentMp);
    }

    [Fact]
    public void Resolve_InsufficientFireMpRejectsWithoutChangingHpOrMp()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle(BlackMageId);
        CombatSnapshot insufficientMp = ReplaceCombatant(
            battle.Snapshot,
            "party-0",
            combatant => combatant.WithCurrentMp(2));

        CombatCommandValidationException exception = Assert.Throws<CombatCommandValidationException>(
            () => new CombatResolver(battle.Content).Resolve(
                insufficientMp,
                new CombatCommand("party-0", FireId, ["enemy-0"])));

        Assert.Equal(CombatCommandProblemCodes.AbilityResourceInsufficient, exception.ProblemCode);
        Assert.Equal(2, insufficientMp.GetRequiredCombatant("party-0").CurrentMp);
        Assert.Equal(22, insufficientMp.GetRequiredCombatant("enemy-0").CurrentHp);
    }

    [Fact]
    public void LegacyVanguardId_ResolvesTheKnightKitForExistingSaveProgress()
    {
        ContentCatalog content = TestContent.LoadCatalog();

        PartyAbilityAvailability availability = ResolveAvailability(content, "class.martial.vanguard");

        Assert.Equal(
            [CombatTestFixture.AttackId, PowerStrikeId],
            availability.ExecutableAbilityIds);
    }

    private static PartyAbilityAvailability ResolveAvailability(ContentCatalog content, string classId) =>
        new AbilityAvailabilityResolver(content).ResolvePartyActor(new ActorProgressState
        {
            ActorId = CombatTestFixture.JamesId,
            ClassId = classId,
            Level = 1,
        });

    private static CombatSnapshot ReplaceCombatant(
        CombatSnapshot snapshot,
        string instanceId,
        Func<CombatantSnapshot, CombatantSnapshot> replacement)
    {
        CombatantSnapshot[] combatants = snapshot.Combatants
            .Select(combatant => string.Equals(combatant.InstanceId, instanceId, StringComparison.Ordinal)
                ? replacement(combatant)
                : combatant)
            .ToArray();
        return new CombatSnapshot(snapshot.Round, combatants);
    }
}
