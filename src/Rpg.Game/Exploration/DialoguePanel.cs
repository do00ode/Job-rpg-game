using Godot;
using RpgGame.Core.Content.Definitions;
using RpgGame.Localization;

namespace RpgGame.Exploration;

/// <summary>Minimal linear dialogue presenter for the single Milestone 2 exchange.</summary>
public partial class DialoguePanel : PanelContainer
{
    private Label _speakerLabel = null!;
    private Label _lineLabel = null!;
    private DialogueDefinition? _dialogue;
    private LocalizedTextCatalog? _text;
    private int _lineIndex;

    /// <summary>Whether exploration input should currently be paused for dialogue.</summary>
    public bool IsOpen => Visible;

    public override void _Ready()
    {
        _speakerLabel = GetNode<Label>("Margin/VBox/Speaker");
        _lineLabel = GetNode<Label>("Margin/VBox/Line");
    }

    /// <summary>Begins at the first validated line of the selected content record.</summary>
    public void ShowDialogue(DialogueDefinition dialogue, LocalizedTextCatalog text)
    {
        ArgumentNullException.ThrowIfNull(dialogue);
        ArgumentNullException.ThrowIfNull(text);
        _dialogue = dialogue;
        _text = text;
        _lineIndex = 0;
        Visible = true;
        RefreshText();
    }

    /// <summary>Advances one line, closing after the final line.</summary>
    public void Advance()
    {
        if (_dialogue is null)
        {
            Close();
            return;
        }

        _lineIndex++;
        if (_lineIndex >= _dialogue.LineTextKeys.Count)
        {
            Close();
            return;
        }

        RefreshText();
    }

    /// <summary>Closes presentation only; persistent interaction state remains in GameState.</summary>
    public void Close()
    {
        Visible = false;
        _dialogue = null;
        _text = null;
        _lineIndex = 0;
    }

    private void RefreshText()
    {
        if (_dialogue is null)
        {
            return;
        }

        LocalizedTextCatalog text = _text
            ?? throw new InvalidOperationException("DialoguePanel has no localization catalog.");
        _speakerLabel.Text = text.Resolve(_dialogue.SpeakerNameKey);
        _lineLabel.Text = text.Resolve(_dialogue.LineTextKeys[_lineIndex]);
    }
}
