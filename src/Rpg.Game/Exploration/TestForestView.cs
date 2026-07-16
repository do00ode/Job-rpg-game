using Godot;
using RpgGame.Core.Content.Definitions;
using RpgGame.Core.Maps;

namespace RpgGame.Exploration;

/// <summary>Small visually distinct second map used to prove map transitions.</summary>
public partial class TestForestView : Node2D, IExplorationMapView
{
	public const string MapId = "map.test-forest";
	string IExplorationMapView.MapId => MapId;
	public const string EncounterId = "encounter.test-forest.slime-01";
	public Vector2I GuideTile => new(-1, -1);
	private static readonly Vector2 DrawingOrigin = new(96, 136);
	private MapQueryService _map = null!;
	private bool _encounterCleared;

	public void Initialize(MapQueryService map) => _map = map ?? throw new ArgumentNullException(nameof(map));

	public Vector2 TileToWorld(Vector2I tile) => DrawingOrigin + new Vector2(
		(tile.X + 0.5f) * 48, (tile.Y + 0.5f) * 48);

	public bool IsWalkable(Vector2I tile) => _map.IsPassable(tile.X, tile.Y);

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
		for (int y = 0; y < _map.Height; y++)
		for (int x = 0; x < _map.Width; x++)
		{
			var tile = new Vector2I(x, y);
			var rect = new Rect2(DrawingOrigin + new Vector2(x * tileSize, y * tileSize),
				new Vector2(tileSize, tileSize));
			DrawRect(rect, _map.GetSymbol(x, y) == '#' ? wall : ((x + y) % 2 == 0 ? floor : alternate));
			DrawRect(rect, grid, false, 1.0f);
		}
		if (!_encounterCleared)
		{
			MapEncounterMarkerDefinition marker = _map.EncounterMarkers.Single(
				candidate => candidate.EncounterId == EncounterId);
			Vector2 center = TileToWorld(new Vector2I(marker.X, marker.Y));
			Vector2[] diamond = [center + Vector2.Up * 15, center + Vector2.Right * 15,
				center + Vector2.Down * 15, center + Vector2.Left * 15];
			DrawColoredPolygon(diamond, new Color(0.95f, 0.72f, 0.18f));
			DrawPolyline([diamond[0], diamond[1], diamond[2], diamond[3], diamond[0]], Colors.White, 2);
		}

		foreach (MapTransitionDefinition transition in _map.TransitionMarkers)
		{
			Vector2 center = TileToWorld(new Vector2I(
				transition.SourceCell.X,
				transition.SourceCell.Y));
			DrawCircle(center, 10.0f, new Color(0.30f, 0.85f, 0.95f));
			DrawCircle(center, 10.0f, Colors.White, false, 2.0f);
		}
	}
}
