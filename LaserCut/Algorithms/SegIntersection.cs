using System.Diagnostics;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

public readonly record struct SegIntersection(Segment Segment, double T);

public readonly struct SegPairIntersection
{
    public SegPairIntersection(Segment segment0, double t0, Segment segment1, double t1)
    {
        Segment0 = segment0;
        T0 = t0;
        Segment1 = segment1;
        T1 = t1;
    }
    
    public Segment Segment0 { get; }
    public Segment Segment1 { get; }
    public double T0 { get; }
    public double T1 { get; }
    public SegPairIntersection Flipped => new(Segment1, T1, Segment0, T0);
    
    public bool Seg0ExitsSeg1 => Segment0.Direction.DotProduct(Segment1.Normal) > 0;
    
    public bool Seg1ExitsSeg0 => Segment1.Direction.DotProduct(Segment0.Normal) > 0;

    public Point2D Point => Segment0.PointAt(T0);

    public override string ToString()
    {
        return $"[Seg Pair S0={Segment0.Index}@{T0:F3} S1={Segment1.Index}@{T1:F3}]";
    }
}

// public readonly record struct SegPairIntersection(Segment Segment0, double T0, Segment Segment1, double T1)
// {
//     public SegPairIntersection Flipped => new(Segment1, T1, Segment0, T0);
//     
//     public bool Seg0ExitsSeg1 => Segment0.Direction.DotProduct(Segment0.Normal) > 0;
//     
//     public Point2D Point => Segment0.PointAt(T0);
// }

public static class SegIntersectionExtensions
{
    public static SegPairIntersection[] Flipped(this IEnumerable<SegPairIntersection> items)
    {
        return items.Select(i => i.Flipped).ToArray();
    }
}