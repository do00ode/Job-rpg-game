using RpgGame.Core.State;
using Xunit;

namespace RpgGame.Core.Tests.State;

/// <summary>Tests the scene-independent mutations used by the first exploration room.</summary>
public sealed class GameSessionExplorationTests
{
    [Fact]
    public void UpdateLocation_ValidChange_ReplacesSnapshotAndNotifiesOnce()
    {
        var session = CreateSession();
        GameState previous = session.Current;
        int notifications = 0;
        session.StateChanged += (_, _) => notifications++;

        session.UpdateLocation(previous.Location with
        {
            X = 6,
            Y = 4,
            Facing = "east",
        });

        Assert.NotSame(previous, session.Current);
        Assert.Equal(6, session.Current.Location.X);
        Assert.Equal(4, session.Current.Location.Y);
        Assert.Equal("east", session.Current.Location.Facing);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void SetEventFlag_FirstInteraction_ClonesFlagsAndNotifiesOnce()
    {
        var session = CreateSession();
        GameState previous = session.Current;
        int notifications = 0;
        session.StateChanged += (_, _) => notifications++;

        session.SetEventFlag("flag.test-room.npc-spoken-to");

        Assert.False(previous.EventFlags.ContainsKey("flag.test-room.npc-spoken-to"));
        Assert.True(session.GetEventFlag("flag.test-room.npc-spoken-to"));
        Assert.Equal(1, notifications);

        // Repeating the same fact is a no-op, preventing needless UI reconstruction.
        session.SetEventFlag("flag.test-room.npc-spoken-to");
        Assert.Equal(1, notifications);
    }

    [Theory]
    [InlineData("not-a-map", 1, 1, "south")]
    [InlineData("map.prologue.test-room", -1, 1, "south")]
    [InlineData("map.prologue.test-room", 1, 1, "sideways")]
    public void UpdateLocation_InvalidPersistentLocation_IsRejected(
        string mapId,
        int x,
        int y,
        string facing)
    {
        var session = CreateSession();
        var invalid = new MapLocationState
        {
            MapId = mapId,
            X = x,
            Y = y,
            Facing = facing,
        };

        Assert.ThrowsAny<ArgumentException>(() => session.UpdateLocation(invalid));
    }

    [Theory]
    [InlineData("npc-spoken-to")]
    [InlineData("map.test-room.npc-spoken-to")]
    public void SetEventFlag_NonFlagId_IsRejected(string flagId)
    {
        var session = CreateSession();

        Assert.Throws<ArgumentException>(() => session.SetEventFlag(flagId));
    }

    private static GameSession CreateSession()
    {
        var session = new GameSession();
        session.ReplaceState(new GameState
        {
            SaveId = "exploration-session-test",
            Location = new MapLocationState
            {
                MapId = "map.prologue.test-room",
                X = 4,
                Y = 4,
                Facing = "south",
            },
        });
        return session;
    }
}
