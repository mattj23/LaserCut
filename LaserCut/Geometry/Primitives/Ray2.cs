using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

/// <summary>
/// A 3D ray is a line that extends infinitely in one direction from a start point, but has no end point.  Thus it
/// exists for all positive values of t.
/// </summary>
public class Ray2 : Line2, IBvhIntersect, IBvhTest
{
    public Ray2(Point2D start, Vector2D direction) : base(start, direction)
    {
    }
    
    public bool RoughIntersects(Aabb2 box)
    {
        return Intersects(box);
    }

    public Position[] Intersections(IContourElement element)
    {
        var results = new List<Position>();
        foreach (var position in element.Intersections(this))
        {
            var t = ProjectionParam(position.Surface.Point);
            if (t >= 0)
            {
                results.Add(position);
            }
        }

        return results.ToArray();
    }

    public bool Intersects(Aabb2 box)
    {
        var (a, b) = SlabAabbBase(box);
        a = Math.Max(0.0, a);
        return b >= a;
    }
    
    public SegIntersection? Intersects(Segment segment)
    {
        var (t0, t1) = IntersectionParams(segment);

        return t0 >= 0 && t1 >= 0 && t1 <= segment.Length ? new SegIntersection(segment, t1, IsParallel(segment)) : null;
    }
    
    public Ray2 Reversed()
    {
        return new Ray2(Start, Direction.Negate());
    }
}