using RpgGame.Core.State;
using Xunit;

namespace RpgGame.Core.Tests.State;

/// <summary>Tests the boundary between immutable actor content and per-save progress.</summary>
public sealed class NewGameFactoryTests
{
    [Fact]
    public void Create_BuildsIndependentInitialCampaignState()
    {
        var request = new NewGameRequest
        {
            SaveId = "test-campaign",
            StartingMapId = "map.prologue.test-room",
            StartingX = 4,
            StartingY = 7,
            StartingFacing = "south",
            StartingPartyMembers =
            [
                new StartingPartyMemberRequest
                {
                    ActorId = "actor.hero.james",
                    ClassId = "class.martial.vanguard",
                },
            ],
        };

        GameState state = new NewGameFactory(TestContent.LoadCatalog()).Create(request);

        Assert.Equal("test-campaign", state.SaveId);
        Assert.Equal("map.prologue.test-room", state.Location.MapId);
        Assert.Equal(4, state.Location.X);
        Assert.Equal(7, state.Location.Y);
        Assert.Equal("south", state.Location.Facing);
        Assert.Equal(["actor.hero.james"], state.ActivePartyActorIds);
        Assert.Empty(state.Inventory);
        Assert.Empty(state.EventFlags);

        ActorProgressState progress = state.ActorProgress["actor.hero.james"];
        Assert.Equal("class.martial.vanguard", progress.ClassId);
        Assert.Equal(1, progress.Level);
        Assert.Equal(0, progress.Experience);
    }

    [Fact]
    public void PartyRule_AllowsFourHeroesAndRejectsFive()
    {
        PartyRules.ValidateMemberCount(4);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => PartyRules.ValidateMemberCount(5));
    }

    [Fact]
    public void Create_StartingPartyAboveFour_IsRejectedBeforeContentLookup()
    {
        var request = new NewGameRequest
        {
            SaveId = "invalid-party-test",
            StartingMapId = "map.prologue.test-room",
            StartingPartyMembers =
            [
                Member("actor.hero.james"),
                Member("actor.hero.james"),
                Member("actor.hero.james"),
                Member("actor.hero.james"),
                Member("actor.hero.james"),
            ],
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new NewGameFactory(TestContent.LoadCatalog()).Create(request));
    }

    [Fact]
    public void ReplaceState_NotifiesSessionObservers()
    {
        var session = new GameSession();
        int notificationCount = 0;
        session.StateChanged += (_, _) => notificationCount++;

        GameState state = new NewGameFactory(TestContent.LoadCatalog()).Create(
            new NewGameRequest
            {
                SaveId = "event-test",
                StartingMapId = "map.prologue.test-room",
                StartingPartyMembers = [Member("actor.hero.james")],
            });

        session.ReplaceState(state);

        Assert.True(session.HasActiveGame);
        Assert.Same(state, session.Current);
        Assert.Equal(1, notificationCount);
    }

    [Theory]
    [InlineData("class.martial.vanguard")]
    [InlineData("class.magic.black-mage")]
    [InlineData("class.magic.white-mage")]
    public void Create_AllThreeVanillaStartingClassesAreSelectable(string classId)
    {
        var request = new NewGameRequest
        {
            SaveId = "class-choice-test",
            StartingMapId = "map.prologue.test-room",
            StartingPartyMembers = [Member("actor.hero.james", classId)],
        };

        GameState state = new NewGameFactory(TestContent.LoadCatalog()).Create(request);

        Assert.Equal(classId, state.ActorProgress["actor.hero.james"].ClassId);
    }

    private static StartingPartyMemberRequest Member(
        string actorId,
        string classId = "class.martial.vanguard") => new()
        {
            ActorId = actorId,
            ClassId = classId,
        };
}
