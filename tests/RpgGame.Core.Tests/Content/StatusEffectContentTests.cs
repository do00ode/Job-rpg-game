using RpgGame.Core.Content;
using RpgGame.Core.Content.Definitions;
using RpgGame.Core.Content.Loading;
using Xunit;

namespace RpgGame.Core.Tests.Content;

public sealed class StatusEffectContentTests
{
    [Fact]
    public void ValidStatusDefinition_LoadsThroughProductionContentPipeline()
    {
        ContentLoadResult result = Load(StatusDocument());

        Assert.True(result.IsSuccess);
        Assert.Single(result.Catalog!.GetAll<StatusEffectDefinition>());
    }

    [Theory]
    [InlineData("unknown-stacking-rule", "status.unknown-stacking-rule")]
    [InlineData("bad-duration", "status.invalid-duration")]
    [InlineData("unknown-effect-kind", "status.unknown-effect-kind")]
    public void InvalidStatusDefinition_IsRejected(string caseId, string expectedCode)
    {
        string json = caseId switch
        {
            "unknown-stacking-rule" => StatusJson("unknown-rule", 20, "[]"),
            "bad-duration" => StatusJson("refresh-duration", 0, "[]"),
            _ => StatusJson(
                "refresh-duration",
                20,
                "[\"status-effect.test.unknown\"]"),
        };

        ContentLoadResult result = Load(new ContentDocument(
            $"status-effects/{caseId}.json",
            json));

        Assert.Contains(result.Problems, problem => problem.Code == expectedCode);
        Assert.Null(result.Catalog);
    }

    [Fact]
    public void DuplicateStatusIds_AreRejected()
    {
        ContentLoadResult result = Load(
            StatusDocument("status.test.duplicate", "one"),
            StatusDocument("status.test.duplicate", "two"));

        Assert.Contains(result.Problems, problem => problem.Code == "id.duplicate");
        Assert.Null(result.Catalog);
    }

    private static ContentLoadResult Load(params ContentDocument[] statusDocuments) =>
        new JsonContentLoader().Load(new MemoryContentSource(
            ContentSourceIds.Base,
            [
                new ContentDocument(
                    "classes/test.json",
                    """
                    {
                      "schemaVersion": 1,
                      "id": "class.test.starting",
                      "displayNameKey": "class.test.starting.name",
                      "baseStatisticBonuses": {},
                      "abilityUnlocks": [],
                      "magicDisciplineUnlocks": []
                    }
                    """),
                new ContentDocument(
                    "starting-class-rules/default.json",
                    """
                    {
                      "schemaVersion": 1,
                      "id": "newgame.class-rule.base.test",
                      "includeClassIds": ["class.test.starting"],
                      "excludeClassIds": []
                    }
                    """),
                .. statusDocuments,
            ]));

    private static ContentDocument StatusDocument(
        string id = "status.test.focus",
        string name = "focus") => new(
        $"status-effects/{name}.json",
        StatusJson("refresh-duration", 20, "[]", id));

    private static string StatusJson(
        string stackingRule,
        long duration,
        string effectKindIdsJson,
        string id = "status.test.focus") => $$"""
        {
          "schemaVersion": 1,
          "id": "{{id}}",
          "displayNameKey": "{{id}}.name",
          "stackingRuleId": "{{stackingRule}}",
          "defaultDuration": {{duration}},
          "durationUnitId": "timeline-time",
          "effectKindIds": {{effectKindIdsJson}}
        }
        """;

    private sealed class MemoryContentSource(
        string sourceId,
        IReadOnlyList<ContentDocument> documents)
        : IContentSource
    {
        public string SourceId => sourceId;

        public IReadOnlyList<ContentDocument> ReadAll() => documents;
    }
}
