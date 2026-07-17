using RpgGame.Core.Presentation;
using Xunit;

namespace RpgGame.Core.Tests.Presentation;

public sealed class PixelPerfectGeometryTests
{
    [Fact]
    public void ViewportUsesIntegerScaleAndPillarboxesWidescreen()
    {
        ViewportLayout layout = PixelPerfectGeometry.CalculateViewportLayout(1280, 720);

        Assert.Equal(3, layout.IntegerScale);
        Assert.Equal(new PixelSize(960, 720), layout.OutputSize);
        Assert.Equal(new PixelPoint(160, 0), layout.Offset);
    }

    [Fact]
    public void WindowCoordinatesAccountForPillarboxOffset()
    {
        ViewportLayout layout = PixelPerfectGeometry.CalculateViewportLayout(1280, 720);

        Assert.Equal(new PixelPoint(0, 0), PixelPerfectGeometry.WindowToNative(new(160, 0), layout));
        Assert.Equal(new PixelPoint(319, 239), PixelPerfectGeometry.WindowToNative(new(1119, 719), layout));
    }

    [Fact]
    public void WorldCoordinatesUseSixteenPixelTilesIncludingNegativeValues()
    {
        Assert.Equal(new PixelPoint(2, 3), PixelPerfectGeometry.WorldToTile(new(47, 63)));
        Assert.Equal(new PixelPoint(-1, -1), PixelPerfectGeometry.WorldToTile(new(-1, -1)));
    }

    [Fact]
    public void PixelSnappingRoundsToWholeNativePixels()
    {
        Assert.Equal(new PixelPoint(3, -2), PixelPerfectGeometry.SnapToPixel(2.6, -1.6));
    }

    [Fact]
    public void CameraLimitsAreMapPixelBounds()
    {
        Assert.Equal(new CameraLimits(0, 0, 640, 480),
            PixelPerfectGeometry.CalculateCameraLimits(40, 30));
    }

    [Fact]
    public void CameraFollowsTargetAndClampsAtMapEdges()
    {
        Assert.Equal(new PixelPoint(160, 120),
            PixelPerfectGeometry.CalculateCameraCenter(640, 480, new(160, 120)));
        Assert.Equal(new PixelPoint(160, 120),
            PixelPerfectGeometry.CalculateCameraCenter(640, 480, new(0, 0)));
        Assert.Equal(new PixelPoint(480, 360),
            PixelPerfectGeometry.CalculateCameraCenter(640, 480, new(640, 480)));
    }

    [Fact]
    public void SmallerMapsStayCenteredAndDoNotFollowTarget()
    {
        Assert.Equal(new PixelPoint(96, 72),
            PixelPerfectGeometry.CalculateCameraCenter(192, 144, new(24, 40)));
    }

    [Fact]
    public void CharacterFramesShareBottomCenterPivotWithoutStretching()
    {
        CharacterFramePlacement horizontal = PixelPerfectGeometry.PlaceCharacterFrame(13, 16);
        CharacterFramePlacement vertical = PixelPerfectGeometry.PlaceCharacterFrame(16, 16);

        Assert.Equal(new PixelPoint(1, 0), horizontal.Offset);
        Assert.Equal(new PixelPoint(0, 0), vertical.Offset);
        Assert.Equal(new PixelPoint(8, 16), horizontal.Pivot);
        Assert.Equal(horizontal.Pivot, vertical.Pivot);
    }

    [Fact]
    public void InvalidViewportAndMapDimensionsFailClearly()
    {
        Assert.Throws<ArgumentException>(() => PixelPerfectGeometry.CalculateViewportLayout(319, 240));
        Assert.Throws<ArgumentOutOfRangeException>(() => PixelPerfectGeometry.CalculateCameraLimits(0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => PixelPerfectGeometry.PlaceCharacterFrame(17, 16));
    }
}
