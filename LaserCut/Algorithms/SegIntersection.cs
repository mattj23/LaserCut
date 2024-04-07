using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public readonly record struct SegIntersection(Segment Segment, double T);

public readonly record struct SegPairIntersection(Segment Segment0, double T0, Segment Segment1, double T1)
{
    public SegPairIntersection Flipped => new(Segment1, T1, Segment0, T0);
}

public static class SegIntersectionExtensions
{
    public static SegPairIntersection[] Flipped(this IEnumerable<SegPairIntersection> items)
    {
        return items.Select(i => i.Flipped).ToArray();
    }
}