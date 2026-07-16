using Godot;

namespace RpgGame.Exploration;

/// <summary>Small visually distinct second map used to prove map transitions.</summary>
public partial class TestForestView : Node2D, IExplorationMapView
{
    public const string MapId = "map.test-forest";
    string IExplorationMapView.MapId => MapId;
    public const string EncounterId = "encounter.test-forest.slime-01";
    public Vector2I GuideTile => new(-1, -1);
    private static readonly Vector2 DrawingOrigin = new(96, 136);
    private static readonly Vector2I EncounterTile = new(6, 4);
    private bool _encounterCleared;

    public Vector2 TileToWorld(Vector2I tile) => DrawingOrigin + new Vector2(
        (tile.X + 0.5f) * 48, (tile.Y + 0.5f) * 48);

    public bool IsWalkable(Vector2I tile) => tile.X > 0 && tile.Y > 0
        && tile.X < 11 && tile.Y < 8;

    public bool TryGetEncounterAt(Vector2I tile, out string encounterId)
    {
        if (!_encounterCleared && tile == EncounterTile)
        {
            encounterId = EncounterId;
            return true;
        }
        encounterId = string.Empty;
        return false;
    }

    public void SetEncounterCleared(bool cleared)
    {
        if (_encounterCleared == cleared) return;
        _encounterCleared = cleared;
        QueueRedraw();
    }

    public override void _Draw()
    {
        const int tileSize = 48;
        var floor = new Color(0.12f, 0.27f, 0.20f);
        var alternate = new Color(0.15f, 0.33f, 0.24f);
        var wall = new Color(0.24f, 0.42f, 0.29f);
        var grid = new Color(0.05f, 0.12f, 0.08f);
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 12; x++)
        {
            var tile = new Vector2I(x, y);
            var rect = new Rect2(DrawingOrigin + new Vector2(x * tileSize, y * tileSize),
                new Vector2(tileSize, tileSize));
            DrawRect(rect, IsWalkable(tile) ? ((x + y) % 2 == 0 ? floor : alternate) : wall);
            DrawRect(rect, grid, false, 1.0f);
        }
        if (!_encounterCleared)
        {
            Vector2 center = TileToWorld(EncounterTile);
            Vector2[] diamond = [center + Vector2.Up * 15, center + Vector2.Right * 15,
                center + Vector2.Down * 15, center + Vector2.Left * 15];
            DrawColoredPolygon(diamond, new Color(0.95f, 0.72f, 0.18f));
            DrawPolyline([diamond[0], diamond[1], diamond[2], diamond[3], diamond[0]], Colors.White, 2);
        }
    }
}
