using RpgGame.Core.State;
using Xunit;

namespace RpgGame.Core.Tests.State;

/// <summary>Tests the session boundary used by content-aware inventory use cases.</summary>
public sealed class GameSessionInventoryTests
{
    private const string PotionId = "item.consumable.potion";
    private const string SwordId = "item.equipment.iron-sword";

    [Fact]
    public void UpdateInventory_CopiesSourceAndPublishesOrdinalReplacementOnce()
    {
        GameSession session = CreateSession();
        GameState previous = session.Current;
        var source = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [PotionId] = 1,
        };
        int notifications = 0;
        session.StateChanged += (_, _) => notifications++;

        session.UpdateInventory(source);
        source[PotionId] = 7;
        source[SwordId] = 1;

        Assert.NotSame(previous, session.Current);
        Assert.Empty(previous.Inventory);
        Assert.Single(session.Current.Inventory);
        Assert.Equal(1, session.Current.Inventory[PotionId]);
        Assert.False(session.Current.Inventory.ContainsKey(SwordId));
        Assert.Same(StringComparer.Ordinal, session.Current.Inventory.Comparer);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void UpdateInventory_LogicallyIdenticalPairs_DoNotPublish()
    {
        GameSession session = CreateSession(new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [PotionId] = 2,
            [SwordId] = 1,
        });
        GameState previous = session.Current;
        var samePairsDifferentOrder = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [SwordId] = 1,
            [PotionId] = 2,
        };
        int notifications = 0;
        session.StateChanged += (_, _) => notifications++;

        session.UpdateInventory(samePairsDifferentOrder);

        Assert.Same(previous, session.Current);
        Assert.Equal(0, notifications);
    }

    [Fact]
    public void UpdateInventory_ComparisonUsesOrdinalItemIds()
    {
        GameSession session = CreateSession(new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["item.test.Potion"] = 1,
        });
        int notifications = 0;
        session.StateChanged += (_, _) => notifications++;

        session.UpdateInventory(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["item.test.potion"] = 1,
        });

        Assert.False(session.Current.Inventory.ContainsKey("item.test.Potion"));
        Assert.True(session.Current.Inventory.ContainsKey("item.test.potion"));
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void UpdateInventory_NullInput_IsRejected()
    {
        GameSession session = CreateSession();
        GameState previous = session.Current;

        Assert.Throws<ArgumentNullException>(() => session.UpdateInventory(null!));

        Assert.Same(previous, session.Current);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData(" ", 1)]
    [InlineData(PotionId, 0)]
    [InlineData(PotionId, -1)]
    public void UpdateInventory_InvalidPair_IsRejectedWithoutPublishing(
        string itemId,
        int quantity)
    {
        GameSession session = CreateSession();
        GameState previous = session.Current;
        int notifications = 0;
        session.StateChanged += (_, _) => notifications++;
        var source = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [itemId] = quantity,
        };

        Assert.Throws<ArgumentException>(() => session.UpdateInventory(source));

        Assert.Same(previous, session.Current);
        Assert.Equal(0, notifications);
    }

    private static GameSession CreateSession(Dictionary<string, int>? inventory = null)
    {
        var session = new GameSession();
        session.ReplaceState(new GameState
        {
            SaveId = "inventory-session-test",
            Inventory = inventory ?? new Dictionary<string, int>(StringComparer.Ordinal),
        });
        return session;
    }
}
