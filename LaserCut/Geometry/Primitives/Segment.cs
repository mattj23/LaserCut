using LaserCut.Algorithms;
using LaserCut.Mesh;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry.Primitives;

public class Segment : Line2, IBoundaryElement
{
    public Segment(double x0, double y0, double x1, double y1, int index) 
        : this(new Point2D(x0, y0), new Point2D(x1, y1), index) { }
    
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
    
    public int Index { get; private set; }

    public void SetIndex(int index)
    {
        Index = index;
    }
    
    public double CrossProductWedge => Start.X * End.Y - End.X * Start.Y;

    public bool RoughIntersects(Aabb2 box)
    {
        return Bounds.Intersects(box);
    }

    public Position[] Intersections(IBoundaryElement element)
    {
        var results = new List<Position>();
        foreach (var position in element.IntersectionsWithLine(this))
        {
            var t = ProjectionParam(position.Surface.Point);
            if (t >= -GeometryConstants.DistEquals && t <= Length + GeometryConstants.DistEquals)
            {
                results.Add(position);
            }
        }

        return results.ToArray();
    }

    public override string ToString()
    {
        return $"[Segment {Index}]";
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
    public Position[] IntersectionsWithLine(Line2 line)
    {
        var (t0, t1) = IntersectionParams(line);
        
        if (double.IsNaN(t0) || double.IsNaN(t1) || t0 < -GeometryConstants.DistEquals ||
            t0 > Length + GeometryConstants.DistEquals) return [];

        return [new Position(t0, this)];
    }

    /// <summary>
    /// Compute the intersections between the segment and the given circle.  
    /// </summary>
    /// <param name="circle"></param>
    /// <returns></returns>
    public Position[] IntersectionsWithCircle(Circle2 circle)
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
    
    
    public IntersectionPair[] MatchIntersections(IEnumerable<Position> positions)
    {
        var results = new List<IntersectionPair>();
        foreach (var other in positions)
        {
            var t = ProjectionParam(other.Surface.Point);
            if (t >= -GeometryConstants.DistEquals && t <= Length + GeometryConstants.DistEquals)
            {
                results.Add(new IntersectionPair(new Position(t, this), other));
            }
        }

        return results.ToArray();
    }

    public IBoundaryElement? SplitAfter(double length)
    {
        if (length >= Length - GeometryConstants.DistEquals)
        {
            return null;
        }

        return new Segment(PointAt(length), End, -1);
    }

    public IBoundaryElement? SplitBefore(double length)
    {
        if (length <= GeometryConstants.DistEquals)
        {
            return null;
        }
        
        return new Segment(Start, PointAt(length), -1);
    }

    public IBoundaryElement OffsetBy(double distance)
    {
        var offset = Normal * distance;
        return new Segment(Start + offset, End + offset, -1);
    }

    public IBoundaryElement Reversed()
    {
        return new Segment(End, Start, -1);
    }

    public (double, Position, Position) Closest(IBoundaryElement other)
    {
        return other switch
        {
            Arc arc => Distances.Closest(this, arc),
            Segment seg => Distances.Closest(this, seg),
            _ => throw new ArgumentException(
                $"No means of computing closest distance between Segment and {other.GetType()}")
        };
    }

    public IBoundaryElement Transformed(Matrix transform)
    {
        return new Segment(Start.Transformed(transform), End.Transformed(transform), -1);
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