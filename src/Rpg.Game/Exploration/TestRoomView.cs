using Godot;
using RpgGame.Core.Content.Definitions;
using RpgGame.Core.Maps;
using RpgGame.Encounters;

namespace RpgGame.Exploration;

/// <summary>
/// Draws and answers collision queries for the single Milestone 2 room.
/// </summary>
/// <remarks>
/// This fixed grid is intentionally game-specific. A reusable map loader or navigation
/// framework would be premature with only one room. The scene owns pixels and blocked tiles;
/// save data stores only logical coordinates and never depends on this drawing code.
/// </remarks>
public partial class TestRoomView : Node2D, IExplorationMapView
{
	public const string MapId = "map.prologue.test-room";
	public const int TileSize = 48;
	public const string EncounterId = TestRoomEncounterProgress.EncounterId;

	/// <summary>
	/// Game-specific location of the one fixed encounter marker. It is intentionally not
	/// stored in GameState: the room owns what exists at a tile, while a save owns where
	/// James currently stands.
	/// </summary>
	public Vector2I GuideTile => new(7, 4);
	string IExplorationMapView.MapId => MapId;

	// The room begins below the two-line, remappable-controls HUD. Logical tile coordinates
	// remain unchanged; this is presentation-only spacing owned by the exploration scene.
	private static readonly Vector2 DrawingOrigin = new(96, 136);

	private MapQueryService _map = null!;
	private bool _encounterCleared;

	public void Initialize(MapQueryService map) => _map = map ?? throw new ArgumentNullException(nameof(map));

	/// <summary>Converts persistent grid coordinates to this scene's pixel center.</summary>
	public Vector2 TileToWorld(Vector2I tile) => DrawingOrigin + new Vector2(
		(tile.X + 0.5f) * TileSize,
		(tile.Y + 0.5f) * TileSize);

	/// <summary>True when a logical tile is inside the room and is not a wall.</summary>
	public bool IsWalkable(Vector2I tile) => _map.IsPassable(tile.X, tile.Y);

	/// <summary>
	/// Applies the campaign-owned clearance fact to this disposable room presentation.
	/// </summary>
	public void SetEncounterCleared(bool cleared)
	{
		if (_encounterCleared == cleared)
		{
			return;
		}

		_encounterCleared = cleared;
		QueueRedraw();
	}

	/// <summary>
	/// Answers the one map-specific encounter lookup needed by Milestone 2.5.
	/// </summary>
	/// <remarks>
	/// This remains a narrow room query rather than an encounter table or generalized map
	/// format. The returned value is a stable content ID, never a scene or file path.
	/// </remarks>
	public bool TryGetEncounterAt(Vector2I tile, out string encounterId)
	{
		if (!_encounterCleared && _map.TryGetEncounterAt(tile.X, tile.Y, out MapEncounterMarkerDefinition? marker))
		{
			encounterId = marker!.EncounterId;
			return true;
		}

		encounterId = string.Empty;
		return false;
	}

	public bool TryGetTransitionAt(Vector2I tile, out MapTransitionDefinition? transition) =>
		_map.TryGetTransitionAt(tile.X, tile.Y, out transition);

	public override void _Draw()
	{
		var floorColor = new Color(0.16f, 0.19f, 0.25f);
		var alternateFloorColor = new Color(0.18f, 0.21f, 0.28f);
		var wallColor = new Color(0.34f, 0.38f, 0.48f);
		var gridColor = new Color(0.08f, 0.10f, 0.14f);

		for (int y = 0; y < _map.Height; y++)
		{
			for (int x = 0; x < _map.Width; x++)
			{
				var tile = new Vector2I(x, y);
				var topLeft = DrawingOrigin + new Vector2(x * TileSize, y * TileSize);
				var rectangle = new Rect2(topLeft, new Vector2(TileSize, TileSize));
				Color fill = _map.GetSymbol(x, y) == '#'
					? wallColor
					: ((x + y) % 2 == 0 ? floorColor : alternateFloorColor);

				DrawRect(rectangle, fill);
				DrawRect(rectangle, gridColor, false, 1.0f);
			}
		}

		if (!_encounterCleared)
		{
			// A bright inset diamond makes the uncleared trigger obvious in the gray-box room.
			MapEncounterMarkerDefinition marker = _map.EncounterMarkers.Single(
				candidate => candidate.EncounterId == EncounterId);
			Vector2 center = TileToWorld(new Vector2I(marker.X, marker.Y));
			Vector2[] diamond =
			[
				center + new Vector2(0.0f, -15.0f),
				center + new Vector2(15.0f, 0.0f),
				center + new Vector2(0.0f, 15.0f),
				center + new Vector2(-15.0f, 0.0f),
			];
			DrawColoredPolygon(diamond, new Color(0.82f, 0.23f, 0.36f));
			Vector2[] outline = [diamond[0], diamond[1], diamond[2], diamond[3], diamond[0]];
			DrawPolyline(outline, Colors.White, 2.0f);
		}

		foreach (MapTransitionDefinition transition in _map.TransitionMarkers)
		{
			Vector2 transitionCenter = TileToWorld(new Vector2I(
				transition.SourceCell.X,
				transition.SourceCell.Y));
			DrawCircle(transitionCenter, 10.0f, new Color(0.30f, 0.85f, 0.95f));
			DrawCircle(transitionCenter, 10.0f, Colors.White, false, 2.0f);
		}
	}

}
