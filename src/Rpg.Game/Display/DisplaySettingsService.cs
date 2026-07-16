using Godot;

namespace RpgGame.Display;

/// <summary>Application-lifetime owner of the small supported window-resolution preset list.</summary>
public sealed class DisplaySettingsService
{
    private static readonly Vector2I[] SupportedResolutions =
    [
        new(640, 480),
        new(800, 600),
        new(1024, 768),
        new(1280, 720),
        new(1366, 768),
        new(1920, 1080),
    ];

    public event EventHandler? ResolutionChanged;
    public IReadOnlyList<Vector2I> Resolutions => SupportedResolutions;
    public Vector2I CurrentResolution => DisplayServer.WindowGetSize();

    public void SetResolution(Vector2I resolution)
    {
        if (!SupportedResolutions.Contains(resolution))
        {
            throw new ArgumentException($"Unsupported display resolution '{resolution}'.", nameof(resolution));
        }

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(resolution);
        ResolutionChanged?.Invoke(this, EventArgs.Empty);
    }
}
