using RpgGame.Core.Combat;
using RpgGame.Core.Content.Definitions;
using Xunit;

namespace RpgGame.Core.Tests.Combat;

public sealed class CombatStatusTests
{
    [Fact]
    public void ApplyStatus_AddsImmutableStateAndTypedEvent()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        ContentStatusCatalog content = CreateStatusCatalog(
            Status("status.test.focus", StatusStackingRuleIds.RefreshDuration, duration: 40));
        CombatStatusService service = new(content.Catalog);

        StatusResolution result = service.ApplyStatus(
            battle.Snapshot,
            "party-0",
            "party-0",
            "status.test.focus");

        Assert.Empty(battle.Snapshot.GetRequiredCombatant("party-0").ActiveStatusEffects);
        ActiveStatusEffect active = Assert.Single(
            result.Next.GetRequiredCombatant("party-0").ActiveStatusEffects);
        Assert.Equal("status.test.focus", active.StatusEffectId);
        Assert.Equal(40, active.Duration);
        Assert.Equal(0, active.AppliedTimelineTime);
        Assert.IsType<StatusApplied>(Assert.Single(result.Events));
    }

    [Fact]
    public void ApplyStatus_RefreshDurationReplacesTimingWithoutIncreasingStacks()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        ContentStatusCatalog content = CreateStatusCatalog(
            Status("status.test.focus", StatusStackingRuleIds.RefreshDuration, duration: 40));
        CombatStatusService service = new(content.Catalog);
        StatusResolution first = service.ApplyStatus(
            battle.Snapshot,
            null,
            "party-0",
            "status.test.focus");
        CombatSnapshot later = AtTime(first.Next, 90);

        StatusResolution refreshed = service.ApplyStatus(
            later,
            null,
            "party-0",
            "status.test.focus");

        ActiveStatusEffect active = Assert.Single(
            refreshed.Next.GetRequiredCombatant("party-0").ActiveStatusEffects);
        Assert.Equal(90, active.AppliedTimelineTime);
        Assert.Equal(1, active.StackCount);
        Assert.IsType<StatusRefreshed>(Assert.Single(refreshed.Events));
    }

    [Fact]
    public void ApplyStatus_IgnoreIfPresentLeavesExistingStateUnchanged()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        ContentStatusCatalog content = CreateStatusCatalog(
            Status("status.test.focus", StatusStackingRuleIds.IgnoreIfPresent, duration: 40));
        CombatStatusService service = new(content.Catalog);
        StatusResolution first = service.ApplyStatus(
            battle.Snapshot,
            null,
            "party-0",
            "status.test.focus");

        CombatSnapshot later = AtTime(first.Next, 90);
        StatusResolution ignored = service.ApplyStatus(
            later,
            null,
            "party-0",
            "status.test.focus");

        Assert.Same(later, ignored.Next);
        Assert.IsType<StatusIgnored>(Assert.Single(ignored.Events));
    }

    [Fact]
    public void ApplyStatus_ReplaceUsesTheNewDefinitionDuration()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        ContentStatusCatalog content = CreateStatusCatalog(
            Status("status.test.focus", StatusStackingRuleIds.Replace, duration: 12));
        CombatStatusService service = new(content.Catalog);
        StatusResolution first = service.ApplyStatus(
            battle.Snapshot,
            null,
            "party-0",
            "status.test.focus");

        StatusResolution replaced = service.ApplyStatus(
            AtTime(first.Next, 90),
            null,
            "party-0",
            "status.test.focus");

        ActiveStatusEffect active = Assert.Single(
            replaced.Next.GetRequiredCombatant("party-0").ActiveStatusEffects);
        Assert.Equal(12, active.Duration);
        Assert.IsType<StatusRefreshed>(Assert.Single(replaced.Events));
    }

    [Fact]
    public void ExpireStatuses_RemovesAtBoundaryAndEmitsEvent()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        ContentStatusCatalog content = CreateStatusCatalog(
            Status("status.test.focus", StatusStackingRuleIds.RefreshDuration, duration: 20));
        CombatStatusService service = new(content.Catalog);
        StatusResolution applied = service.ApplyStatus(
            battle.Snapshot,
            null,
            "party-0",
            "status.test.focus");

        StatusResolution expired = service.ExpireStatuses(AtTime(applied.Next, 90));

        Assert.Empty(expired.Next.GetRequiredCombatant("party-0").ActiveStatusEffects);
        Assert.IsType<StatusExpired>(Assert.Single(expired.Events));
        Assert.Single(applied.Next.GetRequiredCombatant("party-0").ActiveStatusEffects);
    }

    [Fact]
    public void ApplyStatus_RejectsDefeatedTarget()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        ContentStatusCatalog content = CreateStatusCatalog(
            Status("status.test.focus", StatusStackingRuleIds.RefreshDuration, duration: 20));
        CombatStatusService service = new(content.Catalog);

        StatusValidationException exception = Assert.Throws<StatusValidationException>(() =>
            service.ApplyStatus(
                Replace(battle.Snapshot, battle.Snapshot.GetRequiredCombatant("enemy-0").WithCurrentHp(0)),
                null,
                "enemy-0",
                "status.test.focus"));

        Assert.Equal(StatusProblemCodes.TargetDefeated, exception.ProblemCode);
    }

    [Fact]
    public void EffectiveSpeed_UsesStatusModifierAndPreviewRemainsImmutable()
    {
        FixedBattle battle = CombatTestFixture.CreateFixedBattle();
        ContentStatusCatalog content = CreateStatusCatalog(
            Status(
                "status.test.haste",
                StatusStackingRuleIds.RefreshDuration,
                duration: 100,
                effectKindId: StatusEffectKindIds.ModifySpeedPercent,
                speedPercentModifier: 100));
        CombatStatusService service = new(content.Catalog);
        StatusResolution applied = service.ApplyStatus(
            battle.Snapshot,
            null,
            "party-0",
            "status.test.haste");

        CombatantSnapshot party = applied.Next.GetRequiredCombatant("party-0");
        Assert.Equal(12, CombatStatusService.ResolveEffectiveSpeed(
            applied.Next,
            party,
            content.Catalog));
        Assert.Equal(62, CombatTimeline.CalculateActionDelay(
            applied.Next,
            party,
            content.Catalog));

        TurnOrderPreview before = new TurnOrderPreviewService(content.Catalog)
            .Create(applied.Next, 8);
        Assert.Single(applied.Next.GetRequiredCombatant("party-0").ActiveStatusEffects);
        Assert.Equal(12, CombatStatusService.ResolveEffectiveSpeed(
            applied.Next,
            party,
            content.Catalog));
        Assert.NotEmpty(before.Entries);
    }

    private static StatusEffectDefinition Status(
        string id,
        string stackingRule,
        long duration,
        string? effectKindId = null,
        int speedPercentModifier = 0) => new()
        {
            Id = id,
            DisplayNameKey = $"{id}.name",
            StackingRuleId = stackingRule,
            DefaultDuration = duration,
            EffectKindIds = effectKindId is null ? [] : [effectKindId],
            SpeedPercentModifier = speedPercentModifier,
        };

    private static CombatSnapshot AtTime(CombatSnapshot snapshot, long timelineTime) =>
        new(snapshot.Round, timelineTime, snapshot.Combatants);

    private static CombatSnapshot Replace(
        CombatSnapshot source,
        CombatantSnapshot replacement)
    {
        CombatantSnapshot[] combatants = source.Combatants.ToArray();
        int index = Array.FindIndex(
            combatants,
            combatant => combatant.InstanceId == replacement.InstanceId);
        combatants[index] = replacement;
        return new CombatSnapshot(source.Round, source.TimelineTime, combatants);
    }

    private static ContentStatusCatalog CreateStatusCatalog(StatusEffectDefinition status) =>
        new(new TestCatalog(status));

    private sealed record ContentStatusCatalog(TestCatalog Catalog);
}
