using Godot;

namespace RpgGame.Exploration;

/// <summary>Simple code-drawn James placeholder with a visible facing marker.</summary>
public partial class PlayerMarkerView : Node2D
{
    private string _facing = "south";

    /// <summary>Updates the logical direction rendered on the next frame.</summary>
    public void SetFacing(string facing)
    {
        _facing = facing;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var body = new Rect2(new Vector2(-17, -17), new Vector2(34, 34));
        DrawRect(body, new Color(0.17f, 0.55f, 0.95f));
        DrawRect(body, Colors.White, false, 2.0f);

        Vector2 direction = _facing switch
        {
            "north" => Vector2.Up,
            "east" => Vector2.Right,
            "west" => Vector2.Left,
            _ => Vector2.Down,
        };
        DrawCircle(direction * 11.0f, 4.0f, Colors.White);
    }
}
