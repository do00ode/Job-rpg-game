using RpgGame.Core.State;

namespace RpgGame.Encounters;

/// <summary>
/// Owns the stable encounter-to-clearance mapping for the fixed test-room encounter.
/// </summary>
/// <remarks>
/// This is intentionally not a generalized encounter-progress framework. The game has one
/// encounter trigger, one clearance fact, and one concrete victory rule. Keeping that mapping in
/// one game-specific application helper prevents the exploration scene and composition root from
/// duplicating persistent IDs while leaving reusable combat rules free of campaign knowledge.
/// </remarks>
public static class TestRoomEncounterProgress
{
    public const string EncounterId = "encounter.forest.slimes-01";

    public const string ClearedFlagId = "flag.encounter.forest.slimes-01.cleared";
    public const string TestForestEncounterId = "encounter.test-forest.slime-01";
    public const string TestForestClearedFlagId = "flag.encounter.test-forest.slime-01.cleared";

    /// <summary>Maps the one supported encounter ID to its persistent clearance flag.</summary>
    public static string GetClearanceFlagId(string encounterId) => encounterId switch
    {
        EncounterId => ClearedFlagId,
        TestForestEncounterId => TestForestClearedFlagId,
        _ => throw new ArgumentException($"Unsupported encounter '{encounterId}'.", nameof(encounterId)),
    };

    /// <summary>Reads the fixed persistent clearance fact; an absent flag means not cleared.</summary>
    public static bool IsCleared(IGameSession session, string encounterId)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.GetEventFlag(GetClearanceFlagId(encounterId));
    }

}
