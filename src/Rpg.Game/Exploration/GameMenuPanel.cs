using Godot;
using RpgGame.Input;

namespace RpgGame.Exploration;

/// <summary>Small exploration-local menu that routes to existing panels and save slots.</summary>
public partial class GameMenuPanel : PanelContainer
{
    private Button _equipmentButton = null!;
    private Button _saveButton = null!;
    private Button _loadButton = null!;
    private Button _controlsButton = null!;
    private Button _displayButton = null!;
    private Button _soundButton = null!;
    private Button _closeButton = null!;
    private Label _slotTitle = null!;
    private Button[] _slotButtons = [];
    private Button _slotBackButton = null!;
    private readonly List<Button> _focusButtons = [];
    private readonly Dictionary<Button, Action> _actionByButton = [];
    private Button? _focusedButton;
    private bool _isLoadMode;

    public event EventHandler? EquipmentRequested;

    public event EventHandler<SaveSlotRequestedEventArgs>? SaveSlotRequested;

    public event EventHandler? ControlsRequested;

    public event EventHandler? DisplayRequested;
    public event EventHandler? SoundRequested;

    public override void _Ready()
    {
        _equipmentButton = GetNode<Button>("Margin/VBox/Equipment");
        _saveButton = GetNode<Button>("Margin/VBox/Save");
        _loadButton = GetNode<Button>("Margin/VBox/Load");
        _controlsButton = GetNode<Button>("Margin/VBox/Controls");
        _displayButton = GetNode<Button>("Margin/VBox/Display");
        _soundButton = GetNode<Button>("Margin/VBox/Sound");
        _closeButton = GetNode<Button>("Margin/VBox/Close");
        _slotTitle = GetNode<Label>("Margin/VBox/SlotPanel/Title");
        _slotButtons =
        [
            GetNode<Button>("Margin/VBox/SlotPanel/Slot1"),
            GetNode<Button>("Margin/VBox/SlotPanel/Slot2"),
            GetNode<Button>("Margin/VBox/SlotPanel/Slot3"),
            GetNode<Button>("Margin/VBox/SlotPanel/Slot4"),
        ];
        _slotBackButton = GetNode<Button>("Margin/VBox/SlotPanel/Back");
        AddButton(_equipmentButton, () => EquipmentRequested?.Invoke(this, EventArgs.Empty));
        AddButton(_saveButton, () => OpenSlotMenu(isLoad: false));
        AddButton(_loadButton, () => OpenSlotMenu(isLoad: true));
        AddButton(_controlsButton, () => ControlsRequested?.Invoke(this, EventArgs.Empty));
        AddButton(_displayButton, () => DisplayRequested?.Invoke(this, EventArgs.Empty));
        AddButton(_soundButton, () => SoundRequested?.Invoke(this, EventArgs.Empty));
        AddButton(_closeButton, Close);
        for (int index = 0; index < _slotButtons.Length; index++)
        {
            string slotId = $"slot_{index + 1}";
            AddButton(_slotButtons[index], () => RequestSlot(slotId));
        }
        AddButton(_slotBackButton, ShowMainMenu);
        ShowMainMenu();
        Visible = false;
    }

    public void Open()
    {
        Visible = true;
        ShowMainMenu();
    }

    public void Close() => Visible = false;

    public override void _Input(InputEvent @event)
    {
        if (!Visible || @event is not InputEventKey { Pressed: true, Echo: false } keyEvent)
        {
            return;
        }

        if (keyEvent.IsActionPressed(GameInputActions.Menu))
        {
            Close();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (keyEvent.IsActionPressed(GameInputActions.MoveUp))
        {
            CycleFocus(-1);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (keyEvent.IsActionPressed(GameInputActions.MoveDown))
        {
            CycleFocus(1);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (keyEvent.IsActionPressed(GameInputActions.Interact)
            && _focusedButton is not null
            && _actionByButton.TryGetValue(_focusedButton, out Action? action))
        {
            action();
            GetViewport().SetInputAsHandled();
        }
    }

    private void AddButton(Button button, Action action)
    {
        button.Pressed += action;
        button.FocusEntered += () => _focusedButton = button;
        _actionByButton.Add(button, action);
    }

    private void OpenSlotMenu(bool isLoad)
    {
        _isLoadMode = isLoad;
        _slotTitle.Text = isLoad ? "Load Game" : "Save Game";
        _equipmentButton.Visible = false;
        _saveButton.Visible = false;
        _loadButton.Visible = false;
        _controlsButton.Visible = false;
        _displayButton.Visible = false;
        _soundButton.Visible = false;
        _closeButton.Visible = false;
        GetNode<Control>("Margin/VBox/SlotPanel").Visible = true;
        _focusButtons.Clear();
        _focusButtons.AddRange(_slotButtons);
        _focusButtons.Add(_slotBackButton);
        _focusedButton = null;
        _slotButtons[0].GrabFocus();
    }

    private void ShowMainMenu()
    {
        _equipmentButton.Visible = true;
        _saveButton.Visible = true;
        _loadButton.Visible = true;
        _controlsButton.Visible = true;
        _displayButton.Visible = true;
        _soundButton.Visible = true;
        _closeButton.Visible = true;
        GetNode<Control>("Margin/VBox/SlotPanel").Visible = false;
        _focusButtons.Clear();
        _focusButtons.AddRange(
            [_equipmentButton, _saveButton, _loadButton, _controlsButton, _displayButton,
                _soundButton, _closeButton]);
        _focusedButton = null;
        if (Visible)
        {
            _equipmentButton.GrabFocus();
        }
    }

    private void RequestSlot(string slotId)
    {
        Close();
        SaveSlotRequested?.Invoke(this, new SaveSlotRequestedEventArgs(slotId, _isLoadMode));
    }

    private void CycleFocus(int direction)
    {
        int index = _focusedButton is null ? 0 : _focusButtons.IndexOf(_focusedButton);
        int next = (index + direction + _focusButtons.Count) % _focusButtons.Count;
        _focusButtons[next].GrabFocus();
    }
}

public sealed class SaveSlotRequestedEventArgs : EventArgs
{
    public SaveSlotRequestedEventArgs(string slotId, bool isLoad)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotId);
        SlotId = slotId;
        IsLoad = isLoad;
    }

    public string SlotId { get; }

    public bool IsLoad { get; }
}
