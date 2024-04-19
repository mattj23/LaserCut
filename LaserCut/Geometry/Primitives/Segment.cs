using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

public class Segment : Line2, IBvhIntersect
{
    public Segment(Point2D start, Point2D end, int index) : base(start, (end - start).Normalize())
    {
        if (start.DistanceTo(end) < GeometryConstants.DistEquals)
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
        if (IsCollinear(segment))
        {
            // Any point that is on both segments is a valid intersection point
            var t1Start = segment.ProjectionParam(Start);
            var t1End = segment.ProjectionParam(End);
            var t1A = Math.Min(t1Start, t1End);
            var t1B = Math.Max(t1Start, t1End);
            
            if (t1B < 0 || t1A > segment.Length)
            {
                return null;
            }

            var valid0 = Math.Max(t1A, 0);
            var valid1 = Math.Min(t1B, segment.Length);
            return new SegIntersection(segment, (valid0 + valid1) * 0.5);
        }

        var (t0, t1) = IntersectionParams(segment);
        return t0 >= 0 && t1 >= 0 && t0 <= Length && t1 <= segment.Length ? new SegIntersection(segment, t1) : null;
    }

    public SegPairIntersection? IntersectsAsPair(Segment segment)
    {
        if (IsCollinear(segment))
        {
            var t1Start = segment.ProjectionParam(Start);
            var t1End = segment.ProjectionParam(End);
            var t1A = Math.Min(t1Start, t1End);
            var t1B = Math.Max(t1Start, t1End);
            
            if (t1B < 0 || t1A > segment.Length)
            {
                return null;
            }

            var valid0 = Math.Max(t1A, 0);
            var valid1 = Math.Min(t1B, segment.Length);
            var pt1 = (valid0 + valid1) * 0.5;
            var pt0 = ProjectionParam(segment.PointAt(pt1));
            return new SegPairIntersection(this, pt0, segment, pt1);
        }
        
        var (t0, t1) = IntersectionParams(segment);
        bool valid = t0 >= 0 && t1 >= 0 && t0 <= Length && t1 <= segment.Length;
        return valid ? new SegPairIntersection(this, t0, segment, t1) : null;
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