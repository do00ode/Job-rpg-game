using Godot;
using RpgGame.Adapters.Content;
using RpgGame.Core.Content;
using RpgGame.Core.Content.Loading;
using RpgGame.Core.Mods;
using RpgGame.Core.Persistence;
using RpgGame.Core.State;
using RpgGame.Exploration;

namespace RpgGame.Bootstrap;

/// <summary>
/// Godot composition root for scene-facing adapters and narrowly scoped application services.
/// </summary>
/// <remarks>
/// Godot creates this Node from <c>game/scenes/bootstrap/GameRoot.tscn</c>, which is the
/// project's configured main scene. It constructs the validated content catalog, campaign
/// session, and save coordinator. Future scene controllers should receive only the narrow
/// service interfaces they actually use.
///
/// Gameplay behavior does not belong in this node. Keeping the root intentionally boring
/// prevents it from becoming an unrestricted global GameManager over time.
///
/// Godot requires scripts deriving from GodotObject to be <c>partial</c>; its source
/// generators supply the engine-facing portion of the class during compilation.
/// </remarks>
public partial class GameRoot : Node, IExplorationDevelopmentCommands
{
    private const string GameVersion = "0.2.1-milestone2.1";
    private const string TestRoomScenePath = "res://game/scenes/exploration/TestRoom.tscn";
    private const string DevelopmentQuickSlotId = "slot_1";

    private ExplorationSceneController? _explorationScene;

    /// <summary>Validated immutable content available after <see cref="_Ready"/>.</summary>
    public IContentCatalog Content { get; private set; } = null!;

    /// <summary>Scene-independent owner of the currently active campaign.</summary>
    public IGameSession Session { get; private set; } = null!;

    /// <summary>Save/load application service using Godot's writable user directory.</summary>
    public SaveCoordinator Saves { get; private set; } = null!;

    /// <summary>
    /// Validated loose-folder data mods active for this process, in dependency order. The
    /// collection is diagnostic/application metadata; scenes should still request definitions
    /// from <see cref="Content"/> instead of reaching into mod folders.
    /// </summary>
    public IReadOnlyList<ModReference> EnabledMods { get; private set; } = [];

    /// <inheritdoc />
    public string QuickSlotId => DevelopmentQuickSlotId;

    /// <summary>
    /// Godot calls this after the node enters the scene tree. Startup is deliberately
    /// synchronous while the fixture pack is small, making failures deterministic and visible.
    /// </summary>
    public override void _Ready()
    {
        try
        {
            InitializeApplicationServices();
            RebuildExplorationScene();
            GD.Print(
                $"Milestone 2 ready: loaded {Content.Count} definitions "
                + $"with {EnabledMods.Count} data mod(s); "
                + $"new game {Session.Current.SaveId} starts at {Session.Current.Location.MapId}.");
        }
        catch (Exception exception)
        {
            GD.PushError($"Application startup failed:{System.Environment.NewLine}{exception}");
            GetTree().Quit(1);
        }
    }

    /// <summary>
    /// Replaces the active campaign with a newly constructed default game.
    /// </summary>
    /// <remarks>
    /// A future title-screen controller can call this method after the player confirms
    /// "New Game." Keeping creation here for now demonstrates the complete use case without
    /// introducing a title screen or global service locator during this milestone.
    /// </remarks>
    public void StartNewGame(string? startingClassId = null)
    {
        EnsureInitialized(Content, nameof(Content));
        EnsureInitialized(Session, nameof(Session));

        // Until the title screen supplies a player choice, select the first stable ID from
        // the resolved pool. This is deterministic and remains valid when a mod removes the
        // vanilla Vanguard choice. It is a bootstrap fallback, not actor configuration.
        string selectedClassId = startingClassId
            ?? StartingClassPool.Resolve(Content).FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Content does not provide any selectable starting classes.");

        GameState initialState = new NewGameFactory(Content).Create(
            DefaultGameSetup.CreateRequest(selectedClassId));
        Session.ReplaceState(initialState);
    }

    /// <summary>
    /// Saves the authoritative campaign snapshot to a logical slot such as
    /// <c>slot_1</c>. The slot is validated before it becomes a filename.
    /// </summary>
    public Task SaveCurrentGameAsync(
        string slotId,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized(Session, nameof(Session));
        EnsureInitialized(Saves, nameof(Saves));
        return Saves.SaveAsync(slotId, Session.Current, cancellationToken);
    }

    /// <summary>
    /// Loads a slot into the persistent session. Returns false when the slot has never been
    /// saved; malformed or incompatible saves intentionally throw a visible error.
    /// </summary>
    public async Task<bool> LoadGameAsync(
        string slotId,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized(Session, nameof(Session));
        EnsureInitialized(Saves, nameof(Saves));

        GameState? loadedState = await Saves.LoadAsync(slotId, cancellationToken);
        if (loadedState is null)
        {
            return false;
        }

        Session.ReplaceState(loadedState);
        return true;
    }

    /// <inheritdoc />
    public Task SaveQuickSlotAsync(CancellationToken cancellationToken = default) =>
        SaveCurrentGameAsync(DevelopmentQuickSlotId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> LoadQuickSlotAsync(CancellationToken cancellationToken = default) =>
        LoadGameAsync(DevelopmentQuickSlotId, cancellationToken);

    private void InitializeApplicationServices()
    {
        // Mods live in user:// so players can install data without modifying the exported
        // game's read-only res:// pack. GlobalizePath converts that virtual Godot location
        // into the operating-system path consumed by the plain .NET discovery service.
        string modsDirectory = ProjectSettings.GlobalizePath("user://mods");
        ModDiscoveryResult modResult = new DirectoryModDiscovery().Discover(modsDirectory);
        if (!modResult.IsSuccess)
        {
            string details = string.Join(
                System.Environment.NewLine,
                modResult.Problems.Select(problem => problem.ToString()));
            throw new InvalidDataException(
                $"Data-mod validation failed with {modResult.Problems.Count} problem(s):"
                + System.Environment.NewLine
                + details);
        }

        // Base content is always first. Mod discovery has already produced a deterministic
        // topological order in which every dependency precedes the mod that needs it.
        var sources = new List<IContentSource>
        {
            new GodotContentSource(ContentSourceIds.Base, "res://game/content"),
        };
        sources.AddRange(modResult.Mods.Select(mod =>
            new DirectoryContentSource(
                mod.Manifest.Id,
                mod.ContentDirectory,
                mod.Manifest.Dependencies)));

        var loader = new JsonContentLoader();
        ContentLoadResult contentResult = loader.Load(sources);

        if (!contentResult.IsSuccess)
        {
            string details = string.Join(
                System.Environment.NewLine,
                contentResult.Problems.Select(problem => problem.ToString()));
            throw new InvalidDataException(
                $"Content validation failed with {contentResult.Problems.Count} problem(s):"
                + System.Environment.NewLine
                + details);
        }

        Content = contentResult.Catalog!;
        EnabledMods = modResult.Mods.Select(mod => mod.ToReference()).ToArray();

        Session = new GameSession();

        string saveDirectory = ProjectSettings.GlobalizePath("user://saves");
        var serializer = new SaveJsonSerializer();
        var store = new JsonFileSaveStore(saveDirectory, serializer);
        Saves = new SaveCoordinator(store, GameVersion, EnabledMods);

        // Creating the initial session last guarantees every dependency used by the public
        // new-game/save/load methods is ready before StateChanged can notify listeners.
        StartNewGame();
    }

    /// <summary>
    /// Reconstructs the only current map from GameState. This is deliberately not a general
    /// scene navigator; that abstraction waits until a second real map needs transitions.
    /// </summary>
    private void RebuildExplorationScene(string? developmentStatus = null)
    {
        if (_explorationScene is not null)
        {
            _explorationScene.ReloadRequested -= OnExplorationReloadRequested;
            RemoveChild(_explorationScene);
            _explorationScene.QueueFree();
        }

        PackedScene packedScene = ResourceLoader.Load<PackedScene>(TestRoomScenePath)
            ?? throw new InvalidOperationException(
                $"Could not load exploration scene '{TestRoomScenePath}'.");
        var scene = packedScene.Instantiate<ExplorationSceneController>();
        AddChild(scene);
        scene.Initialize(Content, Session, this);
        scene.ReloadRequested += OnExplorationReloadRequested;
        if (developmentStatus is not null)
        {
            scene.ShowDevelopmentStatus(developmentStatus);
        }

        _explorationScene = scene;
    }

    private void OnExplorationReloadRequested(object? sender, EventArgs eventArgs) =>
        RebuildExplorationScene("Room reconstructed from in-memory GameState.");

    private static void EnsureInitialized(object? service, string serviceName)
    {
        if (service is null)
        {
            throw new InvalidOperationException(
                $"{serviceName} is unavailable until GameRoot._Ready has initialized services.");
        }
    }
}
