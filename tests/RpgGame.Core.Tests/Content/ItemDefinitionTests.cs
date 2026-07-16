using RpgGame.Core.Content.Definitions;
using Xunit;

namespace RpgGame.Core.Tests.Content;

public sealed class ItemDefinitionTests
{
    [Fact]
    public void OrdinaryItem_DefaultsToStackOfNinetyNine()
    {
        ItemDefinition item = CreateItem();

        Assert.False(item.Unique);
        Assert.Equal(99, item.MaxStack);
    }

    [Fact]
    public void UniqueItem_UsesEffectiveStackOfOneWithoutAuthoredMaxStack()
    {
        ItemDefinition item = CreateItem() with { Unique = true };

        Assert.True(item.Unique);
        Assert.Equal(1, item.MaxStack);
    }

    [Fact]
    public void NonUniqueItemMayUseAnExplicitStackLimit()
    {
        ItemDefinition item = CreateItem() with { MaxStack = 7 };

        Assert.Equal(7, item.MaxStack);
    }

    private static ItemDefinition CreateItem() => new()
    {
        Id = "item.test.stack-default",
        DisplayNameKey = "item.test.stack-default.name",
        DescriptionKey = "item.test.stack-default.description",
    };
}
