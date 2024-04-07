using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

/// <summary>
/// A 3D ray is a line that extends infinitely in one direction from a start point, but has no end point.  Thus it
/// exists for all positive values of t.
/// </summary>
public class Ray3D : Line2
{
    public Ray3D(Point2D start, Vector2D direction) : base(start, direction)
    {
    }

    public bool Intersects(Aabb2 box)
    {
        var (a, b) = SlabAabbBase(box);
        a = Math.Max(0.0, a);
        return b >= a;
    }
    
    public bool Intersects(Segment segment)
    {
        var t = IntersectionParams(segment);
        return t.X >= 0 && t.Y >= 0 && t.Y <= segment.Length;
    }
}