using System.Text.Json;
using RpgGame.Core.Persistence;
using Xunit;

namespace RpgGame.Core.Tests.Persistence;

/// <summary>
/// Executable compatibility guarantees for serialized save documents.
/// </summary>
public sealed class SaveCompatibilityTests
{
    // JsonSerializerDefaults.Web selects camelCase JSON, matching CONTENT_SCHEMA.md.
    // Keeping one options instance ensures the read and subsequent write use the same rules.
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>
    /// Proves that an older game version can load and re-save a newer document without
    /// silently deleting a state field it does not understand.
    /// </summary>
    [Fact]
    public void UnknownStateFields_SurviveLoadAndResave()
    {
        // This hand-written fixture deliberately contains "futureField", which has no
        // matching C# property on GameState. JsonExtensionData should capture it.
        const string json = """
            {
              "saveFormatVersion": 1,
              "gameVersion": "0.0.1",
              "savedAtUtc": "2026-07-13T12:00:00Z",
              "state": {
                "schemaVersion": 1,
                "saveId": "test-save",
                "location": {
                  "mapId": "map.test-room",
                  "x": 3,
                  "y": 5,
                  "facing": "south"
                },
                "activePartyActorIds": [],
                "actorProgress": {},
                "eventFlags": {},
                "futureField": { "value": 42 }
              }
            }
            """;

        // Failing immediately here makes a serializer configuration regression clearer than
        // a later null-reference failure in the assertions.
        SaveEnvelope save = JsonSerializer.Deserialize<SaveEnvelope>(json, JsonOptions)
            ?? throw new InvalidOperationException("The fixture did not deserialize.");

        // Verify the unknown field was captured during deserialization.
        Dictionary<string, JsonElement> extensionData = save.State.ExtensionData
            ?? throw new InvalidOperationException("Unknown state fields were not retained.");

        Assert.Contains("futureField", extensionData);

        // Serialize and parse again to verify the field is written back at its original JSON
        // level, not merely retained in memory or nested under an "extensionData" property.
        string rewritten = JsonSerializer.Serialize(save, JsonOptions);
        using JsonDocument document = JsonDocument.Parse(rewritten);

        int futureValue = document.RootElement
            .GetProperty("state")
            .GetProperty("futureField")
            .GetProperty("value")
            .GetInt32();

        Assert.Equal(42, futureValue);
    }
}
