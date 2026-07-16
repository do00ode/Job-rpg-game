using Godot;

namespace RpgGame.Exploration;

/// <summary>Presentation-only contract shared by the small authored exploration maps.</summary>
public interface IExplorationMapView
{
    string MapId { get; }
    Vector2I GuideTile { get; }
    Vector2 TileToWorld(Vector2I tile);
    bool IsWalkable(Vector2I tile);
    bool TryGetEncounterAt(Vector2I tile, out string encounterId);
    void SetEncounterCleared(bool cleared);
}
