using System.Text.Json.Nodes;

namespace RpgGame.Core.Persistence;

/// <summary>Migrates the retired Vanguard class ID to the canonical Knight class ID.</summary>
public sealed class VanguardClassIdMigration : ISaveMigration
{
    public const string LegacyClassId = "class.martial.vanguard";
    public const string KnightClassId = "class.martial.knight";

    public int FromVersion => 1;

    public int ToVersion => 2;

    public JsonObject Migrate(JsonObject source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source["state"] is JsonObject state
            && state["actorProgress"] is JsonObject actorProgress)
        {
            foreach (JsonObject progress in actorProgress
                         .Select(pair => pair.Value)
                         .OfType<JsonObject>())
            {
                if (string.Equals(
                        progress["classId"]?.GetValue<string>(),
                        LegacyClassId,
                        StringComparison.Ordinal))
                {
                    progress["classId"] = KnightClassId;
                }
            }
        }

        source["saveFormatVersion"] = ToVersion;
        return source;
    }
}
