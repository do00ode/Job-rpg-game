namespace RpgGame.Core.Presentation;

/// <summary>Pure geometry contracts for the game's fixed low-resolution presentation.</summary>
public static class PixelPerfectGeometry
{
    public const int NativeViewportWidth = 320;
    public const int NativeViewportHeight = 240;
    public const int NativeTileSize = 16;
    public const int NativeTilesWide = NativeViewportWidth / NativeTileSize;
    public const int NativeTilesHigh = NativeViewportHeight / NativeTileSize;

    public static ViewportLayout CalculateViewportLayout(int windowWidth, int windowHeight)
    {
        if (windowWidth <= 0) throw new ArgumentOutOfRangeException(nameof(windowWidth));
        if (windowHeight <= 0) throw new ArgumentOutOfRangeException(nameof(windowHeight));

        int scale = Math.Min(windowWidth / NativeViewportWidth, windowHeight / NativeViewportHeight);
        if (scale < 1)
        {
            throw new ArgumentException(
                $"Window {windowWidth}x{windowHeight} is smaller than the native viewport.",
                nameof(windowWidth));
        }

        int outputWidth = NativeViewportWidth * scale;
        int outputHeight = NativeViewportHeight * scale;
        return new ViewportLayout(
            new PixelSize(windowWidth, windowHeight),
            scale,
            new PixelSize(outputWidth, outputHeight),
            new PixelPoint((windowWidth - outputWidth) / 2, (windowHeight - outputHeight) / 2));
    }

    public static PixelPoint WindowToNative(PixelPoint windowPoint, ViewportLayout layout)
    {
        int x = Math.Clamp(
            (windowPoint.X - layout.Offset.X) / layout.IntegerScale,
            0,
            NativeViewportWidth - 1);
        int y = Math.Clamp(
            (windowPoint.Y - layout.Offset.Y) / layout.IntegerScale,
            0,
            NativeViewportHeight - 1);
        return new PixelPoint(x, y);
    }

    public static PixelPoint WorldToTile(PixelPoint worldPoint) => new(
        FloorDivide(worldPoint.X, NativeTileSize),
        FloorDivide(worldPoint.Y, NativeTileSize));

    public static PixelPoint SnapToPixel(double x, double y) => new(
        checked((int)Math.Round(x, MidpointRounding.AwayFromZero)),
        checked((int)Math.Round(y, MidpointRounding.AwayFromZero)));

    public static CameraLimits CalculateCameraLimits(int mapWidthTiles, int mapHeightTiles)
    {
        if (mapWidthTiles <= 0) throw new ArgumentOutOfRangeException(nameof(mapWidthTiles));
        if (mapHeightTiles <= 0) throw new ArgumentOutOfRangeException(nameof(mapHeightTiles));

        return new CameraLimits(
            0,
            0,
            checked(mapWidthTiles * NativeTileSize),
            checked(mapHeightTiles * NativeTileSize));
    }

    /// <summary>
    /// Returns the native camera center. Axes smaller than the viewport stay centered on the map;
    /// larger axes follow the target while remaining inside the map limits.
    /// </summary>
    public static PixelPoint CalculateCameraCenter(
        int mapWidthPixels,
        int mapHeightPixels,
        PixelPoint target)
    {
        if (mapWidthPixels <= 0) throw new ArgumentOutOfRangeException(nameof(mapWidthPixels));
        if (mapHeightPixels <= 0) throw new ArgumentOutOfRangeException(nameof(mapHeightPixels));

        return new PixelPoint(
            ClampCameraAxis(mapWidthPixels, target.X, NativeViewportWidth),
            ClampCameraAxis(mapHeightPixels, target.Y, NativeViewportHeight));
    }

    public static CharacterFramePlacement PlaceCharacterFrame(int frameWidth, int frameHeight)
    {
        if (frameWidth <= 0 || frameWidth > NativeTileSize)
            throw new ArgumentOutOfRangeException(nameof(frameWidth));
        if (frameHeight <= 0 || frameHeight > NativeTileSize)
            throw new ArgumentOutOfRangeException(nameof(frameHeight));

        return new CharacterFramePlacement(
            new PixelPoint((NativeTileSize - frameWidth) / 2, NativeTileSize - frameHeight),
            new PixelPoint(NativeTileSize / 2, NativeTileSize));
    }

    private static int ClampCameraAxis(int mapPixels, int target, int viewportPixels)
    {
        if (mapPixels <= viewportPixels)
        {
            return mapPixels / 2;
        }

        int halfViewport = viewportPixels / 2;
        return Math.Clamp(target, halfViewport, mapPixels - halfViewport);
    }

    private static int FloorDivide(int value, int divisor)
    {
        int quotient = Math.DivRem(value, divisor, out int remainder);
        return remainder < 0 ? quotient - 1 : quotient;
    }
}

public readonly record struct PixelSize(int Width, int Height);

public readonly record struct PixelPoint(int X, int Y);

public readonly record struct ViewportLayout(
    PixelSize WindowSize,
    int IntegerScale,
    PixelSize OutputSize,
    PixelPoint Offset);

public readonly record struct CameraLimits(int Left, int Top, int Right, int Bottom);

public readonly record struct CharacterFramePlacement(PixelPoint Offset, PixelPoint Pivot);
