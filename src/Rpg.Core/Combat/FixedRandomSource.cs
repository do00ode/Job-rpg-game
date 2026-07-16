namespace RpgGame.Core.Combat;

internal sealed class FixedRandomSource(int value) : IRandomSource
{
    public int Next(int minInclusive, int maxExclusive) => Math.Clamp(value, minInclusive, maxExclusive - 1);
}
