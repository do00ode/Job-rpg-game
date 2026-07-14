using RpgGame.Core.Content.Definitions;
using RpgGame.Core.Content.Loading;
using Xunit;

namespace RpgGame.Core.Tests.Content;

/// <summary>End-to-end and failure-reporting tests for the authored JSON pipeline.</summary>
public sealed class ContentLoadingTests
{
    /// <summary>
    /// Loads every checked-in JSON record and proves all ten content categories made it
    /// through parsing, identity checks, semantic checks, and cross-reference validation.
    /// </summary>
    [Fact]
    public void FixturePack_LoadsAsOneValidatedCatalog()
    {
        var catalog = TestContent.LoadCatalog();

        Assert.Equal(18, catalog.Count);
        Assert.Single(catalog.GetAll<ActorDefinition>());
        Assert.Equal(3, catalog.GetAll<ClassDefinition>().Count);
        Assert.Equal(5, catalog.GetAll<StatisticDefinition>().Count);
        Assert.Equal(2, catalog.GetAll<ItemDefinition>().Count);
        Assert.Single(catalog.GetAll<EquipmentDefinition>());
        Assert.Equal(2, catalog.GetAll<AbilityDefinition>().Count);
        Assert.Single(catalog.GetAll<EnemyDefinition>());
        Assert.Single(catalog.GetAll<EncounterDefinition>());
        Assert.Single(catalog.GetAll<QuestDefinition>());
        Assert.Single(catalog.GetAll<StartingClassRuleDefinition>());

        ActorDefinition actor = catalog.GetRequired<ActorDefinition>("actor.hero.james");
        Assert.Equal("actor.james.name", actor.DisplayNameKey);
    }

    /// <summary>
    /// A loader pass must report independent problems together so a solo content author
    /// does not have to rerun the game after fixing each individual file.
    /// </summary>
    [Fact]
    public void InvalidPack_AggregatesProblemsAndPublishesNoCatalog()
    {
        ContentDocument[] documents =
        [
            new("actors/broken-hero.json", """
                {
                  "schemaVersion": 1,
                  "id": "actor.hero.broken",
                  "displayNameKey": "actor.broken.name",
                  "baseStatistics": {},
                  "startingAbilityIds": []
                }
                """),
            new("actors/wrong-category.json", """
                {
                  "schemaVersion": 1,
                  "id": "actor.hero.wrong-category",
                  "displayNameKey": "actor.wrong-category.name",
                  "baseStatistics": {},
                  "startingAbilityIds": []
                }
                """),
            new("starting-class-rules/broken.json", """
                {
                  "schemaVersion": 1,
                  "id": "newgame.class-rule.base.broken",
                  "includeClassIds": [
                    "class.missing.class",
                    "item.test.duplicate"
                  ],
                  "excludeClassIds": []
                }
                """),
            new("items/first.json", """
                {
                  "schemaVersion": 1,
                  "id": "item.test.duplicate",
                  "displayNameKey": "item.test.name",
                  "descriptionKey": "item.test.description",
                  "buyPrice": 0,
                  "sellPrice": 0,
                  "maxStack": 1
                }
                """),
            new("items/second.json", """
                {
                  "schemaVersion": 1,
                  "id": "item.test.duplicate",
                  "displayNameKey": "item.test-again.name",
                  "descriptionKey": "item.test-again.description",
                  "buyPrice": 0,
                  "sellPrice": 0,
                  "maxStack": 1
                }
                """),
            new("mystery/unknown.json", "{}"),
            new("items/null-id.json", """
                {
                  "schemaVersion": 1,
                  "id": null,
                  "displayNameKey": "item.null.name",
                  "descriptionKey": "item.null.description",
                  "buyPrice": 0,
                  "sellPrice": 0,
                  "maxStack": 1
                }
                """),
        ];

        ContentLoadResult result = new JsonContentLoader().Load(
            new MemoryContentSource(ContentSourceIds.Base, documents));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Catalog);
        Assert.Contains(result.Problems, problem => problem.Code == "reference.missing");
        Assert.Contains(result.Problems, problem => problem.Code == "reference.wrong-category");
        Assert.Contains(result.Problems, problem => problem.Code == "id.duplicate");
        Assert.Contains(result.Problems, problem => problem.Code == "category.unknown");
        Assert.Contains(result.Problems, problem => problem.Code == "id.invalid");
    }

    /// <summary>
    /// Community records may reference base content, but they may not impersonate built-in
    /// IDs or another author's namespace. This also keeps duplicate behavior unambiguous.
    /// </summary>
    [Fact]
    public void ModRecord_OutsideManifestNamespace_IsRejectedWithSourceInDiagnostic()
    {
        ContentDocument[] documents =
        [
            new("items/potion.json", """
                {
                  "schemaVersion": 1,
                  "id": "item.consumable.potion",
                  "displayNameKey": "item.potion.name",
                  "descriptionKey": "item.potion.description",
                  "buyPrice": 10,
                  "sellPrice": 5,
                  "maxStack": 99
                }
                """),
        ];

        ContentLoadResult result = new JsonContentLoader().Load(
            new MemoryContentSource("mod.example.test", documents));

        ContentProblem problem = Assert.Single(
            result.Problems,
            problem => problem.Code == "id.wrong-namespace");
        Assert.Equal("mod.example.test/items/potion.json", problem.FilePath);
        Assert.Contains("item.example.test.", problem.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CrossModReference_RequiresDirectManifestDependency()
    {
        ContentDocument item = new("items/sword.json", """
            {
              "schemaVersion": 1,
              "id": "item.example.library.sword",
              "displayNameKey": "item.example.library.sword.name",
              "descriptionKey": "item.example.library.sword.description",
              "buyPrice": 10,
              "sellPrice": 5,
              "maxStack": 1
            }
            """);
        ContentDocument equipment = new("equipment/sword.json", """
            {
              "schemaVersion": 1,
              "id": "equipment.example.addon.sword",
              "itemId": "item.example.library.sword",
              "slotId": "slot.weapon.main-hand",
              "statisticModifiers": {},
              "grantedAbilityIds": []
            }
            """);
        ContentDocument baseClass = new("classes/test.json", """
            {
              "schemaVersion": 1,
              "id": "class.test.starting",
              "displayNameKey": "class.test.starting.name",
              "baseStatisticBonuses": {},
              "abilityUnlocks": []
            }
            """);
        ContentDocument startingClassRule = new("starting-class-rules/default.json", """
            {
              "schemaVersion": 1,
              "id": "newgame.class-rule.base.test",
              "includeClassIds": ["class.test.starting"],
              "excludeClassIds": []
            }
            """);

        var loader = new JsonContentLoader();
        ContentLoadResult undeclared = loader.Load(
        [
            new MemoryContentSource(ContentSourceIds.Base, [baseClass, startingClassRule]),
            new MemoryContentSource("mod.example.library", [item]),
            new MemoryContentSource("mod.example.addon", [equipment]),
        ]);
        ContentLoadResult declared = loader.Load(
        [
            new MemoryContentSource(ContentSourceIds.Base, [baseClass, startingClassRule]),
            new MemoryContentSource("mod.example.library", [item]),
            new MemoryContentSource(
                "mod.example.addon",
                [equipment],
                ["mod.example.library"]),
        ]);

        Assert.Contains(
            undeclared.Problems,
            problem => problem.Code == "reference.undeclared-mod-dependency");
        Assert.True(declared.IsSuccess, string.Join(Environment.NewLine, declared.Problems));
    }

    [Fact]
    public void StartingClassRule_ContradictionAndEmptyPool_AreRejected()
    {
        ContentDocument classDefinition = new("classes/test.json", """
            {
              "schemaVersion": 1,
              "id": "class.test.starting",
              "displayNameKey": "class.test.starting.name",
              "baseStatisticBonuses": {},
              "abilityUnlocks": []
            }
            """);
        ContentDocument contradictoryRule = new("starting-class-rules/default.json", """
            {
              "schemaVersion": 1,
              "id": "newgame.class-rule.base.test",
              "includeClassIds": ["class.test.starting"],
              "excludeClassIds": ["class.test.starting"]
            }
            """);

        ContentLoadResult result = new JsonContentLoader().Load(
            new MemoryContentSource(
                ContentSourceIds.Base,
                [classDefinition, contradictoryRule]));

        Assert.Contains(
            result.Problems,
            problem => problem.Code == "starting-class-rule.contradictory");
        Assert.Contains(
            result.Problems,
            problem => problem.Code == "starting-class-pool.empty");
    }

    /// <summary>Minimal source double keeps failure scenarios free from temporary-file IO.</summary>
    private sealed class MemoryContentSource(
        string sourceId,
        IReadOnlyList<ContentDocument> documents,
        IReadOnlyCollection<string>? declaredDependencyIds = null)
        : IContentSource
    {
        public string SourceId => sourceId;

        public IReadOnlyCollection<string> DeclaredDependencyIds { get; } =
            declaredDependencyIds ?? [];

        public IReadOnlyList<ContentDocument> ReadAll() => documents;
    }
}
