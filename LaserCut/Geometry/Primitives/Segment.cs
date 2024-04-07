using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

public class Segment : Line2, IBvhIntersect
{
    public Segment(Point2D start, Point2D end, int index) : base(start, (end - start).Normalize())
    {
        if (start.DistanceTo(end) < 1e-6)
        {
            throw new ArgumentException("Segment must have non-zero length");
        }
        End = end;

        Bounds = GetAabb();
        Index = index;
    }

    public Point2D End { get; }
    
    public double Length => Start.DistanceTo(End);
    
    public Point2D Midpoint => PointAt(Length * 0.5);

    public Aabb2 Bounds { get; }
    
    public int Index { get; }
    
    public bool RoughIntersects(Aabb2 box)
    {
        return Bounds.Intersects(box);
    }
    
    public SegIntersection? Intersects(Segment segment)
    {
        var (t0, t1) = IntersectionParams(segment);

        return t0 >= 0 && t1 >= 0 && t0 <= Length && t1 <= segment.Length ? new SegIntersection(segment, t1) : null;
    }
    
    public Point2D? Intersect(Segment other)
    {
        var (t0, t1) = IntersectionParams(other);
        if (double.IsNaN(t0) || double.IsNaN(t1))
        {
            return null;
        }
        
        if (t0 < 0 || t0 > Length || t1 < 0 || t1 > other.Length)
        {
            return null;
        }
        
        return PointAt(t0);
    }

    public override string ToString()
    {
        return $"[Segment {Start.X:0.000}, {Start.Y:0.000} -> {End.X:0.000}, {End.Y:0.000} | {Index}]";
    }

    private Aabb2 GetAabb()
    {
        // We slightly pad the bounding box to account for the issues with the fast slab intersection test when a 
        // test line is collinear with certain edges of the box.
        var xMin = Math.Min(Start.X, End.X);
        var xMax = Math.Max(Start.X, End.X);
        var yMin = Math.Min(Start.Y, End.Y);
        var yMax = Math.Max(Start.Y, End.Y);
        return new Aabb2(xMin - GeometryConstants.DistEquals, yMin - GeometryConstants.DistEquals, 
            xMax + GeometryConstants.DistEquals, yMax + GeometryConstants.DistEquals);
    }
}