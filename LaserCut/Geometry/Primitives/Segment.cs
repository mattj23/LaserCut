using LaserCut.Algorithms;
using LaserCut.Mesh;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

public class Segment : Line2, IBvhIntersect, IContourElement
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
            return null;
        }

        var (t0, t1) = IntersectionParams(segment);
        return t0 >= 0 && t1 >= 0 && t0 <= Length && t1 <= segment.Length ? new SegIntersection(segment, t1, false) : null;
    }

    public SegPairIntersection? IntersectsAsPair(Segment segment)
    {
        if (IsCollinear(segment))
        {
            return null;
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

    public SurfacePoint AtLength(double length)
    {
        return new SurfacePoint(PointAt(length), Direction);
    }

    /// <summary>
    /// Compute the position on the segment that is closest to the given point.  This may be the start or end point if
    /// the projection of the point onto the line is outside the segment.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Position Closest(Point2D point)
    {
        var t = ProjectionParam(point);
        if (t >= 0 && t <= Length)
        {
            return new Position(t, this);
        }
        
        return point.DistanceTo(Start) < point.DistanceTo(End) ? new Position(0, this) : new Position(Length, this);
    }

    /// <summary>
    /// Compute the intersection point between the segment and the given line.  If the line is parallel to the segment,
    /// no intersection will be found.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public Position[] Intersections(Line2 line)
    {
        var (t0, t1) = IntersectionParams(line);
        if (double.IsNaN(t0) || double.IsNaN(t1) || t0 < 0 || t0 > Length)
        {
            return [];
        }

        return [new Position(t0, this)];
    }

    /// <summary>
    /// Compute the intersections between the segment and the given circle.  
    /// </summary>
    /// <param name="circle"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Position[] Intersections(Circle2 circle)
    {
        var results = new List<Position>();
        foreach (var p in circle.Intersections(this))
        {
            var t = ProjectionParam(p);
            if (t >= 0 && t <= Length)
            {
                results.Add(new Position(t, this));
            }
        }
        
        return results.ToArray();
    }
    
    
    public ElementIntersection[] MatchIntersections(IEnumerable<Position> positions)
    {
        var results = new List<ElementIntersection>();
        foreach (var other in positions)
        {
            var t = ProjectionParam(other.Surface.Point);
            if (t >= 0 && t <= Length)
            {
                results.Add(new ElementIntersection(new Position(t, this), other));
            }
        }

        return results.ToArray();
    }
    
    private Aabb2 GetAabb()
    {
        var xMin = Math.Min(Start.X, End.X);
        var xMax = Math.Max(Start.X, End.X);
        var yMin = Math.Min(Start.Y, End.Y);
        var yMax = Math.Max(Start.Y, End.Y);
        var bounds = new Aabb2(xMin, yMin, xMax, yMax);
        
        // We slightly pad the bounding box to account for the issues with the fast slab intersection test when a 
        // test line is collinear with certain edges of the box.
        bounds.Expand(GeometryConstants.DistEquals);
        return bounds;
    }
}