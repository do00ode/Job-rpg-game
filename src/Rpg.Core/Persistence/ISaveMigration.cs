using System.Text.Json.Nodes;

namespace RpgGame.Core.Persistence;

/// <summary>
/// One explicit, ordered JSON transformation between adjacent save-format versions.
/// </summary>
/// <remarks>
/// Migrations run on raw JSON before deserialization because an old shape may no longer
/// fit current C# types. Keeping each step small (1→2, 2→3) makes every released format
/// reproducible and independently testable.
/// </remarks>
public interface ISaveMigration
{
    /// <summary>Save format version accepted by this migration.</summary>
    int FromVersion { get; }

    /// <summary>Version produced; normally exactly <see cref="FromVersion"/> plus one.</summary>
    int ToVersion { get; }

    /// <summary>
    /// Returns migrated JSON. Implementations should not perform file IO or mutate unrelated
    /// fields, and released migrations must remain stable.
    /// </summary>
    JsonObject Migrate(JsonObject source);
}
