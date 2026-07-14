using Godot;

namespace RpgGame.Exploration;

/// <summary>
/// Draws and answers collision queries for the single Milestone 2 room.
/// </summary>
/// <remarks>
/// This fixed grid is intentionally game-specific. A reusable map loader or navigation
/// framework would be premature with only one room. The scene owns pixels and blocked tiles;
/// save data stores only logical coordinates and never depends on this drawing code.
/// </remarks>
public partial class TestRoomView : Node2D
{
    public const int TileSize = 48;
    public const int WidthInTiles = 12;
    public const int HeightInTiles = 9;

    private static readonly Vector2 DrawingOrigin = new(96, 80);

    private static readonly HashSet<Vector2I> InteriorWalls =
    [
        new Vector2I(5, 2),
        new Vector2I(5, 3),
        new Vector2I(5, 5),
        new Vector2I(5, 6),
        new Vector2I(9, 5),
        new Vector2I(9, 6),
    ];

    /// <summary>Converts persistent grid coordinates to this scene's pixel center.</summary>
    public Vector2 TileToWorld(Vector2I tile) => DrawingOrigin + new Vector2(
        (tile.X + 0.5f) * TileSize,
        (tile.Y + 0.5f) * TileSize);

    /// <summary>True when a logical tile is inside the room and is not a wall.</summary>
    public bool IsWalkable(Vector2I tile) => IsInside(tile) && !IsWall(tile);

    public override void _Draw()
    {
        var floorColor = new Color(0.16f, 0.19f, 0.25f);
        var alternateFloorColor = new Color(0.18f, 0.21f, 0.28f);
        var wallColor = new Color(0.34f, 0.38f, 0.48f);
        var gridColor = new Color(0.08f, 0.10f, 0.14f);

        for (int y = 0; y < HeightInTiles; y++)
        {
            for (int x = 0; x < WidthInTiles; x++)
            {
                var tile = new Vector2I(x, y);
                var topLeft = DrawingOrigin + new Vector2(x * TileSize, y * TileSize);
                var rectangle = new Rect2(topLeft, new Vector2(TileSize, TileSize));
                Color fill = IsWall(tile)
                    ? wallColor
                    : ((x + y) % 2 == 0 ? floorColor : alternateFloorColor);

                DrawRect(rectangle, fill);
                DrawRect(rectangle, gridColor, false, 1.0f);
            }
        }
    }

    private static bool IsInside(Vector2I tile) =>
        tile.X >= 0
        && tile.Y >= 0
        && tile.X < WidthInTiles
        && tile.Y < HeightInTiles;

    private static bool IsWall(Vector2I tile) =>
        tile.X == 0
        || tile.Y == 0
        || tile.X == WidthInTiles - 1
        || tile.Y == HeightInTiles - 1
        || InteriorWalls.Contains(tile);
}
