using Godot;
using RpgGame.Core.Content;
using RpgGame.Core.Content.Definitions;
using RpgGame.Core.State;

namespace RpgGame.Exploration;

/// <summary>
/// Coordinates keyboard intent, the one room's collision, persistent state, and dialogue UI.
/// </summary>
/// <remarks>
/// Godot concerns stop here: keys become logical tile requests, the room accepts or rejects
/// them, and only accepted coordinates/facing are submitted to <see cref="IGameSession"/>.
/// Reconstructing this scene therefore needs no hidden Node state—only content and GameState.
/// </remarks>
public partial class ExplorationSceneController : Node2D
{
    public const string SupportedMapId = "map.prologue.test-room";

    private TestRoomView _room = null!;
    private PlayerMarkerView _player = null!;
    private TestGuideNpc _guide = null!;
    private DialoguePanel _dialogue = null!;
    private Label _developmentStatus = null!;
    private IContentCatalog? _content;
    private IGameSession? _session;
    private IExplorationDevelopmentCommands? _developmentCommands;
    private bool _developmentCommandInProgress;

    /// <summary>Requests reconstruction by the composition root without adding navigation.</summary>
    public event EventHandler? ReloadRequested;

    public override void _Ready()
    {
        _room = GetNode<TestRoomView>("Room");
        _player = GetNode<PlayerMarkerView>("Player");
        _guide = GetNode<TestGuideNpc>("Guide");
        _dialogue = GetNode<DialoguePanel>("Interface/Dialogue");
        _developmentStatus = GetNode<Label>("Interface/DevelopmentStatus");
        SetProcessUnhandledInput(false);
    }

    /// <summary>
    /// Explicitly injects application-lifetime services after the PackedScene is instantiated.
    /// The scene never searches the global tree for GameRoot or an autoload.
    /// </summary>
    public void Initialize(
        IContentCatalog content,
        IGameSession session,
        IExplorationDevelopmentCommands developmentCommands)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(developmentCommands);

        if (_session is not null)
        {
            throw new InvalidOperationException("The exploration scene is already initialized.");
        }

        _content = content;
        _session = session;
        _developmentCommands = developmentCommands;
        _session.StateChanged += OnSessionStateChanged;
        ApplyAuthoritativeState();
        SetProcessUnhandledInput(true);
    }

    /// <summary>
    /// Displays feedback for temporary room reconstruction and quick-slot commands.
    /// </summary>
    public void ShowDevelopmentStatus(string message, bool isError = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _developmentStatus.Text = $"Status: {message}";
        _developmentStatus.Modulate = isError
            ? new Color(1.0f, 0.45f, 0.45f)
            : new Color(0.55f, 1.0f, 0.65f);
    }

    public override void _ExitTree()
    {
        if (_session is not null)
        {
            _session.StateChanged -= OnSessionStateChanged;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_session is null
            || @event is not InputEventKey { Pressed: true, Echo: false } keyEvent)
        {
            return;
        }

        // R is intentionally used instead of F6: Godot commonly reserves F6 in the
        // editor for "Run Current Scene", which can prevent the running game from
        // receiving this development-only reconstruction command.
        if (MatchesKey(keyEvent, Key.R))
        {
            GetViewport().SetInputAsHandled();
            if (_developmentCommandInProgress)
            {
                ShowDevelopmentStatus("Wait for the current save/load operation to finish.");
                return;
            }

            ReloadRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (MatchesKey(keyEvent, Key.K))
        {
            GetViewport().SetInputAsHandled();
            _ = SaveQuickSlotAsync();
            return;
        }

        if (MatchesKey(keyEvent, Key.L))
        {
            GetViewport().SetInputAsHandled();
            _ = LoadQuickSlotAsync();
            return;
        }

        if (_dialogue.IsOpen)
        {
            if (IsInteractKey(keyEvent.Keycode))
            {
                _dialogue.Advance();
                GetViewport().SetInputAsHandled();
            }
            else if (keyEvent.Keycode == Key.Escape)
            {
                _dialogue.Close();
                GetViewport().SetInputAsHandled();
            }

            return;
        }

        if (TryGetMovement(keyEvent.Keycode, out Vector2I delta, out string facing))
        {
            TryMove(delta, facing);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (IsInteractKey(keyEvent.Keycode))
        {
            TryInteract();
            GetViewport().SetInputAsHandled();
        }
    }

    private void TryMove(Vector2I delta, string facing)
    {
        IGameSession session = RequireSession();
        MapLocationState location = session.Current.Location;
        var currentTile = new Vector2I(location.X, location.Y);
        Vector2I requestedTile = currentTile + delta;
        bool canEnter = _room.IsWalkable(requestedTile)
            && requestedTile != _guide.TilePosition;
        Vector2I acceptedTile = canEnter ? requestedTile : currentTile;

        // Facing changes even when a wall blocks movement, matching classic JRPG controls
        // and allowing the player to turn toward an adjacent NPC before interacting.
        // `with` preserves unknown future location fields held by JsonExtensionData. Creating
        // a brand-new DTO here would make ordinary movement erase data written by a newer build.
        session.UpdateLocation(location with
        {
            X = acceptedTile.X,
            Y = acceptedTile.Y,
            Facing = facing,
        });
    }

    private void TryInteract()
    {
        IGameSession session = RequireSession();
        MapLocationState location = session.Current.Location;
        var playerTile = new Vector2I(location.X, location.Y);
        Vector2I targetTile = playerTile + FacingToOffset(location.Facing);

        if (targetTile != _guide.TilePosition)
        {
            return;
        }

        ExplorationInteractionResult result = _guide.Interact(session);
        DialogueDefinition dialogue = RequireContent()
            .GetRequired<DialogueDefinition>(result.DialogueId);
        _dialogue.ShowDialogue(dialogue);
    }

    private void OnSessionStateChanged(object? sender, EventArgs eventArgs) =>
        ApplyAuthoritativeState();

    private void ApplyAuthoritativeState()
    {
        IGameSession session = RequireSession();
        MapLocationState location = session.Current.Location;
        if (!string.Equals(location.MapId, SupportedMapId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"TestRoom cannot present map '{location.MapId}'.");
        }

        var tile = new Vector2I(location.X, location.Y);
        if (!_room.IsWalkable(tile) || tile == _guide.TilePosition)
        {
            throw new InvalidOperationException(
                $"Saved tile ({tile.X}, {tile.Y}) is not walkable in TestRoom.");
        }

        _player.Position = _room.TileToWorld(tile);
        _player.SetFacing(location.Facing);
        _guide.Position = _room.TileToWorld(_guide.TilePosition);
        _guide.RefreshFromState(session);
    }

    private static bool TryGetMovement(
        Key key,
        out Vector2I delta,
        out string facing)
    {
        switch (key)
        {
            case Key.Up:
            case Key.W:
                delta = Vector2I.Up;
                facing = "north";
                return true;
            case Key.Right:
            case Key.D:
                delta = Vector2I.Right;
                facing = "east";
                return true;
            case Key.Down:
            case Key.S:
                delta = Vector2I.Down;
                facing = "south";
                return true;
            case Key.Left:
            case Key.A:
                delta = Vector2I.Left;
                facing = "west";
                return true;
            default:
                delta = Vector2I.Zero;
                facing = string.Empty;
                return false;
        }
    }

    private static bool IsInteractKey(Key key) =>
        key is Key.Space or Key.Enter or Key.KpEnter or Key.E;

    /// <summary>
    /// Accepts both the localized keycode and physical keyboard position. This keeps the
    /// development shortcuts reliable across keyboard layouts and with either letter case.
    /// </summary>
    private static bool MatchesKey(InputEventKey keyEvent, Key expected) =>
        keyEvent.Keycode == expected || keyEvent.PhysicalKeycode == expected;

    private static Vector2I FacingToOffset(string facing) => facing switch
    {
        "north" => Vector2I.Up,
        "east" => Vector2I.Right,
        "west" => Vector2I.Left,
        _ => Vector2I.Down,
    };

    private IGameSession RequireSession() => _session
        ?? throw new InvalidOperationException("ExplorationSceneController is not initialized.");

    private IContentCatalog RequireContent() => _content
        ?? throw new InvalidOperationException("ExplorationSceneController is not initialized.");

    private IExplorationDevelopmentCommands RequireDevelopmentCommands() =>
        _developmentCommands
        ?? throw new InvalidOperationException("ExplorationSceneController is not initialized.");

    private async Task SaveQuickSlotAsync()
    {
        if (_developmentCommandInProgress)
        {
            ShowDevelopmentStatus("A save/load operation is already running.");
            return;
        }

        _developmentCommandInProgress = true;
        IExplorationDevelopmentCommands commands = RequireDevelopmentCommands();
        ShowDevelopmentStatus($"Saving {commands.QuickSlotId}...");

        try
        {
            await commands.SaveQuickSlotAsync();
            ShowDevelopmentStatus($"Saved {commands.QuickSlotId}.");
        }
        catch (Exception exception)
        {
            // This is a development surface, so the concrete exception message is useful.
            // Production save UI will translate failures into player-facing categories.
            ShowDevelopmentStatus($"Save failed: {exception.Message}", isError: true);
            GD.PushError($"Quick save failed:{System.Environment.NewLine}{exception}");
        }
        finally
        {
            _developmentCommandInProgress = false;
        }
    }

    private async Task LoadQuickSlotAsync()
    {
        if (_developmentCommandInProgress)
        {
            ShowDevelopmentStatus("A save/load operation is already running.");
            return;
        }

        _developmentCommandInProgress = true;
        IExplorationDevelopmentCommands commands = RequireDevelopmentCommands();
        ShowDevelopmentStatus($"Loading {commands.QuickSlotId}...");

        try
        {
            bool loaded = await commands.LoadQuickSlotAsync();
            if (loaded)
            {
                // Dialogue progress belongs to the disposable UI, not GameState. Keeping an
                // old panel open after restoring a save would display stale presentation.
                _dialogue.Close();
            }

            ShowDevelopmentStatus(loaded
                ? $"Loaded {commands.QuickSlotId}."
                : $"No save exists in {commands.QuickSlotId}.",
                isError: !loaded);
        }
        catch (Exception exception)
        {
            ShowDevelopmentStatus($"Load failed: {exception.Message}", isError: true);
            GD.PushError($"Quick load failed:{System.Environment.NewLine}{exception}");
        }
        finally
        {
            _developmentCommandInProgress = false;
        }
    }
}
