using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

public class Segment : Line2
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