using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

public class Segment : Line2D
{
    public Segment(Point2D start, Point2D end, int index) : base(start, (end - start).Normalize())
    {
        if (start.DistanceTo(end) < 1e-6)
        {
            throw new ArgumentException("Segment must have non-zero length");
        }
        End = end;
        
        Bounds = Aabb2.FromPoints(new[] {Start, End});
        Index = index;
    }

    public Point2D End { get; }
    
    public double Length => Start.DistanceTo(End);
    
    public Point2D Midpoint => PointAt(Length * 0.5);

    public Aabb2 Bounds { get; }
    
    public int Index { get; }
    
    public bool Parallel(Segment other)
    {
        return Math.Abs(Direction.CrossProduct(other.Direction)) < 1e-6;
    }

    public Point2D? Intersect(Segment other)
    {
        var t = IntersectionParams(other);
        if (double.IsNaN(t.X) || double.IsNaN(t.Y))
        {
            return null;
        }
        
        if (t.X < 0 || t.X > Length || t.Y < 0 || t.Y > other.Length)
        {
            return null;
        }
        
        return PointAt(t.X);
    }
}