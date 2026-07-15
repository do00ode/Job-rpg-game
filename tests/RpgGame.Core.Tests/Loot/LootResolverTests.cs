using System.Diagnostics.CodeAnalysis;
using RpgGame.Core.Combat;
using RpgGame.Core.Content;
using RpgGame.Core.Content.Definitions;
using RpgGame.Core.Loot;
using Xunit;

namespace RpgGame.Core.Tests.Loot;

/// <summary>Tests deterministic, content-only loot resolution.</summary>
public sealed class LootResolverTests
{
    [Fact]
    public void Resolve_ChanceZero_NeverAwardsOrConsumesRandomness()
    {
        ItemDefinition item = Item("item.test.zero");
        EnemyDefinition enemy = Enemy("enemy.test.zero", "loot-table.test.zero");
        LootTableDefinition table = Table(
            "loot-table.test.zero",
            Entry(item.Id, chance: 0m, minimum: 1, maximum: 1));
        var random = new ScriptedRandomSource();

        LootResolution resolution = Resolve(random, item, enemy, table);

        Assert.Empty(resolution.Awards);
        Assert.Empty(random.Calls);
    }

    [Fact]
    public void Resolve_ChanceOne_AlwaysAwards()
    {
        ItemDefinition item = Item("item.test.guaranteed");
        EnemyDefinition enemy = Enemy("enemy.test.guaranteed", "loot-table.test.guaranteed");
        LootTableDefinition table = Table(
            "loot-table.test.guaranteed",
            Entry(item.Id, chance: 1m, minimum: 2, maximum: 2));
        var random = new ScriptedRandomSource();

        LootAward award = Assert.Single(Resolve(random, item, enemy, table).Awards);

        Assert.Equal(new LootAward(enemy.Id, table.Id, item.Id, 2), award);
        Assert.Empty(random.Calls);
    }

    [Fact]
    public void Resolve_IntermediateChances_UseScriptedRollsAtTheDocumentedThreshold()
    {
        ItemDefinition firstItem = Item("item.test.first");
        ItemDefinition secondItem = Item("item.test.second");
        EnemyDefinition enemy = Enemy("enemy.test.intermediate", "loot-table.test.intermediate");
        LootTableDefinition table = Table(
            "loot-table.test.intermediate",
            Entry(firstItem.Id, chance: 0.25m, minimum: 1, maximum: 1),
            Entry(secondItem.Id, chance: 0.25m, minimum: 1, maximum: 1));
        var random = new ScriptedRandomSource(249_999, 250_000);

        LootResolution resolution = Resolve(random, firstItem, secondItem, enemy, table);

        LootAward award = Assert.Single(resolution.Awards);
        Assert.Equal(firstItem.Id, award.ItemId);
        Assert.Equal(
        [
            new RandomCall(0, 1_000_000),
            new RandomCall(0, 1_000_000),
        ],
        random.Calls);
    }

    [Fact]
    public void Resolve_QuantityRange_IncludesBothMinimumAndMaximum()
    {
        ItemDefinition minimumItem = Item("item.test.minimum");
        ItemDefinition maximumItem = Item("item.test.maximum");
        EnemyDefinition enemy = Enemy("enemy.test.range", "loot-table.test.range");
        LootTableDefinition table = Table(
            "loot-table.test.range",
            Entry(minimumItem.Id, chance: 1m, minimum: 2, maximum: 4),
            Entry(maximumItem.Id, chance: 1m, minimum: 2, maximum: 4));
        var random = new ScriptedRandomSource(2, 4);

        LootResolution resolution = Resolve(random, minimumItem, maximumItem, enemy, table);

        Assert.Equal([2, 4], resolution.Awards.Select(award => award.Quantity));
        Assert.Equal(
        [
            new RandomCall(2, 5),
            new RandomCall(2, 5),
        ],
        random.Calls);
    }

    [Fact]
    public void Resolve_FixedQuantity_DoesNotConsumeAnUnnecessaryQuantityRoll()
    {
        ItemDefinition item = Item("item.test.fixed");
        EnemyDefinition enemy = Enemy("enemy.test.fixed", "loot-table.test.fixed");
        LootTableDefinition table = Table(
            "loot-table.test.fixed",
            Entry(item.Id, chance: 1m, minimum: 3, maximum: 3));
        var random = new ScriptedRandomSource();

        LootAward award = Assert.Single(Resolve(random, item, enemy, table).Awards);

        Assert.Equal(3, award.Quantity);
        Assert.Empty(random.Calls);
    }

    [Fact]
    public void Resolve_EmptyAndNullLootTables_ReturnNoAwards()
    {
        EnemyDefinition emptyEnemy = Enemy("enemy.test.empty", "loot-table.test.empty");
        LootTableDefinition emptyTable = Table("loot-table.test.empty");
        EnemyDefinition noLootEnemy = Enemy("enemy.test.none", lootTableId: null);
        var random = new ScriptedRandomSource();

        LootResolution resolution = new LootResolver(new TestCatalog(
            emptyEnemy,
            emptyTable,
            noLootEnemy)).Resolve([emptyEnemy.Id, noLootEnemy.Id], random);

        Assert.Empty(resolution.Awards);
        Assert.Empty(random.Calls);
    }

    [Fact]
    public void Resolve_IdenticalDefeatedEnemyDefinitions_AreEvaluatedIndependently()
    {
        ItemDefinition item = Item("item.test.repeat-enemy");
        EnemyDefinition enemy = Enemy("enemy.test.repeat-enemy", "loot-table.test.repeat-enemy");
        LootTableDefinition table = Table(
            "loot-table.test.repeat-enemy",
            Entry(item.Id, chance: 1m, minimum: 1, maximum: 1));
        var resolver = new LootResolver(new TestCatalog(item, enemy, table));

        LootResolution resolution = resolver.Resolve([enemy.Id, enemy.Id], new ScriptedRandomSource());

        Assert.Equal(2, resolution.Awards.Count);
        Assert.All(resolution.Awards, award =>
            Assert.Equal(new LootAward(enemy.Id, table.Id, item.Id, 1), award));
    }

    [Fact]
    public void Resolve_RepeatedItemEntries_RemainIndependentAwards()
    {
        ItemDefinition item = Item("item.test.repeat-entry");
        EnemyDefinition enemy = Enemy("enemy.test.repeat-entry", "loot-table.test.repeat-entry");
        LootTableDefinition table = Table(
            "loot-table.test.repeat-entry",
            Entry(item.Id, chance: 1m, minimum: 1, maximum: 1),
            Entry(item.Id, chance: 1m, minimum: 2, maximum: 2));

        LootResolution resolution = Resolve(new ScriptedRandomSource(), item, enemy, table);

        Assert.Equal([1, 2], resolution.Awards.Select(award => award.Quantity));
        Assert.Equal([item.Id, item.Id], resolution.Awards.Select(award => award.ItemId));
    }

    [Fact]
    public void Resolve_PreservesSuppliedEnemyAndAuthoredEntryOrder()
    {
        ItemDefinition firstItem = Item("item.test.first-order");
        ItemDefinition secondItem = Item("item.test.second-order");
        ItemDefinition thirdItem = Item("item.test.third-order");
        EnemyDefinition firstEnemy = Enemy("enemy.test.first-order", "loot-table.test.first-order");
        EnemyDefinition secondEnemy = Enemy("enemy.test.second-order", "loot-table.test.second-order");
        LootTableDefinition firstTable = Table(
            "loot-table.test.first-order",
            Entry(firstItem.Id, chance: 1m, minimum: 1, maximum: 1));
        LootTableDefinition secondTable = Table(
            "loot-table.test.second-order",
            Entry(secondItem.Id, chance: 1m, minimum: 1, maximum: 1),
            Entry(thirdItem.Id, chance: 1m, minimum: 1, maximum: 1));
        var resolver = new LootResolver(new TestCatalog(
            firstItem,
            secondItem,
            thirdItem,
            firstEnemy,
            secondEnemy,
            firstTable,
            secondTable));

        LootResolution resolution = resolver.Resolve(
            [secondEnemy.Id, firstEnemy.Id],
            new ScriptedRandomSource());

        Assert.Equal(
        [
            new LootAward(secondEnemy.Id, secondTable.Id, secondItem.Id, 1),
            new LootAward(secondEnemy.Id, secondTable.Id, thirdItem.Id, 1),
            new LootAward(firstEnemy.Id, firstTable.Id, firstItem.Id, 1),
        ],
        resolution.Awards);
    }

    [Fact]
    public void Resolve_MissingEnemyAndWrongCategoryReferences_FailClearly()
    {
        ItemDefinition item = Item("item.test.reference");
        EnemyDefinition enemyWithWrongTable = Enemy(
            "enemy.test.wrong-table",
            item.Id);
        EnemyDefinition wrongCategoryItem = Enemy("enemy.test.wrong-item", lootTableId: null);
        EnemyDefinition enemyWithWrongItem = Enemy(
            "enemy.test.entry-wrong-item",
            "loot-table.test.entry-wrong-item");
        LootTableDefinition tableWithWrongItem = Table(
            "loot-table.test.entry-wrong-item",
            Entry(wrongCategoryItem.Id, chance: 1m, minimum: 1, maximum: 1));
        var resolver = new LootResolver(new TestCatalog(
            item,
            enemyWithWrongTable,
            wrongCategoryItem,
            enemyWithWrongItem,
            tableWithWrongItem));

        Assert.Throws<KeyNotFoundException>(() => resolver.Resolve(
            ["enemy.test.missing"],
            new ScriptedRandomSource()));
        Assert.Throws<KeyNotFoundException>(() => resolver.Resolve(
            [enemyWithWrongTable.Id],
            new ScriptedRandomSource()));
        Assert.Throws<KeyNotFoundException>(() => resolver.Resolve(
            [enemyWithWrongItem.Id],
            new ScriptedRandomSource()));
    }

    [Fact]
    public void Resolve_DoesNotMutateSourceDefinitions()
    {
        ItemDefinition item = Item("item.test.immutable-source");
        var entries = new List<LootEntryDefinition>
        {
            Entry(item.Id, chance: 1m, minimum: 1, maximum: 1),
        };
        var table = new LootTableDefinition
        {
            Id = "loot-table.test.immutable-source",
            Entries = entries,
        };
        EnemyDefinition enemy = Enemy("enemy.test.immutable-source", table.Id);

        _ = Resolve(new ScriptedRandomSource(), item, enemy, table);

        Assert.Same(entries, table.Entries);
        LootEntryDefinition entry = Assert.Single(table.Entries);
        Assert.Equal(item.Id, entry.ItemId);
        Assert.Equal(1m, entry.Chance);
        Assert.Equal(1, entry.MinQuantity);
        Assert.Equal(1, entry.MaxQuantity);
    }

    [Fact]
    public void Resolve_ReturnsImmutableAwards()
    {
        ItemDefinition item = Item("item.test.immutable-result");
        EnemyDefinition enemy = Enemy("enemy.test.immutable-result", "loot-table.test.immutable-result");
        LootTableDefinition table = Table(
            "loot-table.test.immutable-result",
            Entry(item.Id, chance: 1m, minimum: 1, maximum: 1));

        LootResolution resolution = Resolve(new ScriptedRandomSource(), item, enemy, table);
        var awards = Assert.IsAssignableFrom<IList<LootAward>>(resolution.Awards);

        Assert.Throws<NotSupportedException>(() => awards[0] = new LootAward(
            enemy.Id,
            table.Id,
            item.Id,
            2));
    }

    [Fact]
    public void Resolve_IdenticalScriptedRandomSequences_ProduceIdenticalAwards()
    {
        ItemDefinition item = Item("item.test.deterministic");
        EnemyDefinition enemy = Enemy("enemy.test.deterministic", "loot-table.test.deterministic");
        LootTableDefinition table = Table(
            "loot-table.test.deterministic",
            Entry(item.Id, chance: 0.5m, minimum: 2, maximum: 4));
        var resolver = new LootResolver(new TestCatalog(item, enemy, table));

        LootResolution first = resolver.Resolve([enemy.Id], new ScriptedRandomSource(125_000, 4));
        LootResolution second = resolver.Resolve([enemy.Id], new ScriptedRandomSource(125_000, 4));

        Assert.Equal(first.Awards.ToArray(), second.Awards.ToArray());
    }

    private static LootResolution Resolve(
        IRandomSource random,
        params ContentDefinition[] definitions)
    {
        EnemyDefinition enemy = definitions.OfType<EnemyDefinition>().Single();
        return new LootResolver(new TestCatalog(definitions)).Resolve([enemy.Id], random);
    }

    private static ItemDefinition Item(string id) => new()
    {
        Id = id,
        DisplayNameKey = $"{id}.name",
        DescriptionKey = $"{id}.description",
    };

    private static EnemyDefinition Enemy(string id, string? lootTableId) => new()
    {
        Id = id,
        DisplayNameKey = $"{id}.name",
        LootTableId = lootTableId,
    };

    private static LootTableDefinition Table(
        string id,
        params LootEntryDefinition[] entries) => new()
        {
            Id = id,
            Entries = entries.ToList(),
        };

    private static LootEntryDefinition Entry(
        string itemId,
        decimal chance,
        int minimum,
        int maximum) => new()
        {
            ItemId = itemId,
            Chance = chance,
            MinQuantity = minimum,
            MaxQuantity = maximum,
        };

    private sealed class ScriptedRandomSource : IRandomSource
    {
        private readonly Queue<int> _values;

        public ScriptedRandomSource(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public List<RandomCall> Calls { get; } = [];

        public int Next(int minInclusive, int maxExclusive)
        {
            Calls.Add(new RandomCall(minInclusive, maxExclusive));
            if (_values.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Unexpected random request for {minInclusive}..{maxExclusive - 1}.");
            }

            int value = _values.Dequeue();
            if (value < minInclusive || value >= maxExclusive)
            {
                throw new InvalidOperationException(
                    $"Scripted value {value} is outside {minInclusive}..{maxExclusive - 1}.");
            }

            return value;
        }
    }

    private sealed record RandomCall(int MinInclusive, int MaxExclusive);

    private sealed class TestCatalog : IContentCatalog
    {
        private readonly Dictionary<Type, Dictionary<string, ContentDefinition>> _definitions;

        public TestCatalog(params ContentDefinition[] definitions)
        {
            _definitions = definitions
                .GroupBy(definition => definition.GetType())
                .ToDictionary(
                    group => group.Key,
                    group => group.ToDictionary(
                        definition => definition.Id,
                        definition => definition,
                        StringComparer.Ordinal));
            Count = definitions.Length;
        }

        public int Count { get; }

        public IReadOnlyCollection<TDefinition> GetAll<TDefinition>()
            where TDefinition : ContentDefinition
        {
            if (!_definitions.TryGetValue(typeof(TDefinition), out var definitions))
            {
                return Array.Empty<TDefinition>();
            }

            return definitions.Values
                .Cast<TDefinition>()
                .OrderBy(definition => definition.Id, StringComparer.Ordinal)
                .ToArray();
        }

        public TDefinition GetRequired<TDefinition>(string id)
            where TDefinition : ContentDefinition
        {
            if (TryGet<TDefinition>(id, out TDefinition? definition))
            {
                return definition;
            }

            throw new KeyNotFoundException(
                $"Content definition '{id}' was not found as {typeof(TDefinition).Name}.");
        }

        public bool TryGet<TDefinition>(
            string id,
            [NotNullWhen(true)] out TDefinition? definition)
            where TDefinition : ContentDefinition
        {
            if (_definitions.TryGetValue(typeof(TDefinition), out var definitions)
                && definitions.TryGetValue(id, out ContentDefinition? untyped))
            {
                definition = (TDefinition)untyped;
                return true;
            }

            definition = null;
            return false;
        }
    }
}
