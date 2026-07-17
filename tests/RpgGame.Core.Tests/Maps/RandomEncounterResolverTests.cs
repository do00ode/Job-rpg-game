using RpgGame.Core.Combat;
using RpgGame.Core.Content.Definitions;
using RpgGame.Core.Maps;
using Xunit;

namespace RpgGame.Core.Tests.Maps;

public sealed class RandomEncounterResolverTests
{
    [Fact]
    public void Resolve_RollAtOrAboveRate_ReturnsNoEncounterAndConsumesNoSelectionRoll()
    {
        var random = new ScriptedRandomSource(8);

        string? result = new RandomEncounterResolver().Resolve(Table(rate: 8), random);

        Assert.Null(result);
        Assert.Equal(1, random.CallCount);
    }

    [Fact]
    public void Resolve_RollBelowRate_SelectsWeightedEntriesInAuthoredOrder()
    {
        var commonRandom = new ScriptedRandomSource(7, 89);
        var rareRandom = new ScriptedRandomSource(7, 90);
        RandomEncounterResolver resolver = new();

        Assert.Equal("encounter.test.common", resolver.Resolve(Table(rate: 8), commonRandom));
        Assert.Equal("encounter.test.rare", resolver.Resolve(Table(rate: 8), rareRandom));
    }

    [Fact]
    public void Resolve_ZeroRateNeverStartsAnEncounter()
    {
        var random = new ScriptedRandomSource(0);

        Assert.Null(new RandomEncounterResolver().Resolve(Table(rate: 0), random));
        Assert.Equal(1, random.CallCount);
    }

    [Fact]
    public void Resolve_NullTableReturnsNoEncounterWithoutRolling()
    {
        var random = new ScriptedRandomSource();

        Assert.Null(new RandomEncounterResolver().Resolve(null, random));
        Assert.Equal(0, random.CallCount);
    }

    private static MapRandomEncounterDefinition Table(int rate) => new()
    {
        Rate = rate,
        Entries =
        [
            new MapRandomEncounterEntryDefinition
            {
                EncounterId = "encounter.test.common",
                Weight = 90,
            },
            new MapRandomEncounterEntryDefinition
            {
                EncounterId = "encounter.test.rare",
                Weight = 10,
            },
        ],
    };

    private sealed class ScriptedRandomSource(params int[] values) : IRandomSource
    {
        private int _index;

        public int CallCount => _index;

        public int Next(int minInclusive, int maxExclusive)
        {
            if (_index >= values.Length)
            {
                throw new InvalidOperationException("The scripted random sequence is exhausted.");
            }

            int value = values[_index++];
            Assert.InRange(value, minInclusive, maxExclusive - 1);
            return value;
        }
    }
}
