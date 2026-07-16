using Godot;
using RpgGame.Input;

namespace RpgGame.Display;

/// <summary>Disposable full-screen display preset menu owned by exploration presentation.</summary>
public partial class DisplaySettingsPanel : PanelContainer
{
    private Label _currentResolution = null!;
    private VBoxContainer _presets = null!;
    private Button _closeButton = null!;
    private readonly List<Button> _buttons = [];
    private readonly Dictionary<Button, Action> _actions = [];
    private Button? _focusedButton;
    private DisplaySettingsService? _settings;

    public bool IsOpen => Visible;

    public override void _Ready()
    {
        _currentResolution = GetNode<Label>("Margin/VBox/CurrentResolution");
        _presets = GetNode<VBoxContainer>("Margin/VBox/Presets");
        _closeButton = GetNode<Button>("Margin/VBox/Close");
        Visible = false;
    }

    public void Initialize(DisplaySettingsService settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _settings.ResolutionChanged += OnResolutionChanged;
        foreach (Vector2I resolution in _settings.Resolutions)
        {
            Vector2I captured = resolution;
            AddPresetButton($"{captured.X} x {captured.Y}", () => RequireSettings().SetResolution(captured));
        }

        AddButton(_closeButton, Close);
        Refresh();
    }

    public override void _ExitTree()
    {
        if (_settings is not null) _settings.ResolutionChanged -= OnResolutionChanged;
    }

    public void Open()
    {
        Refresh();
        Visible = true;
        _buttons.FirstOrDefault()?.GrabFocus();
    }

    public void Close() => Visible = false;

    public override void _Input(InputEvent @event)
    {
        if (!Visible || @event is not InputEventKey { Pressed: true, Echo: false } keyEvent) return;
        if (keyEvent.IsActionPressed(GameInputActions.Menu)) Close();
        else if (keyEvent.IsActionPressed(GameInputActions.MoveUp)) CycleFocus(-1);
        else if (keyEvent.IsActionPressed(GameInputActions.MoveDown)) CycleFocus(1);
        else if (keyEvent.IsActionPressed(GameInputActions.Interact)
            && _focusedButton is not null
            && _actions.TryGetValue(_focusedButton, out Action? action)) action();
        else return;
        GetViewport().SetInputAsHandled();
    }

    private void Refresh()
    {
        Vector2I current = RequireSettings().CurrentResolution;
        _currentResolution.Text = $"Current: {current.X} x {current.Y}";
    }

    private void AddPresetButton(string text, Action action)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0.0f, 30.0f) };
        _presets.AddChild(button);
        AddButton(button, action);
    }

    private void AddButton(Button button, Action action)
    {
        button.Pressed += action;
        button.FocusEntered += () => _focusedButton = button;
        _buttons.Add(button);
        _actions.Add(button, action);
    }

    private void CycleFocus(int direction)
    {
        int index = _focusedButton is null ? 0 : _buttons.IndexOf(_focusedButton);
        _buttons[(index + direction + _buttons.Count) % _buttons.Count].GrabFocus();
    }

    private void OnResolutionChanged(object? sender, EventArgs eventArgs) => Refresh();
    private DisplaySettingsService RequireSettings() => _settings ?? throw new InvalidOperationException("Display settings panel is not initialized.");
}
