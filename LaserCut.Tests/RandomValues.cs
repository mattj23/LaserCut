using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class RandomValues
{
    private readonly Random _random = new();
    
    public double Double(double min, double max)
    {
        return min + _random.NextDouble() * (max - min);
    }
    
    public bool Bool()
    {
        return _random.NextDouble() < 0.5;
    }
    
    public double Double(double max)
    {
        return Double(-max, max);
    }
    
    public double Degree()
    {
        return Double(180);
    }
    
    public double Flip()
    {
        return _random.NextDouble() < 0.5 ? -1 : 1;
    }
    
    public Point2D Point(double max)
    {
        return new Point2D(Double(max), Double(max));
    }
    
    public Point2D Point(Aabb2 bounds)
    {
        return new Point2D(Double(bounds.MinX, bounds.MaxX), Double(bounds.MinY, bounds.MaxY));
    }
    
    public Vector2D Vector(double max)
    {
        return new Vector2D(Double(max), Double(max));
    }

    public Vector2D UnitVector()
    {
        var angle = Degree();
        return Vector2D.XAxis.Transformed(Isometry.Rotate(angle));
    }
    
    public Segment Segment(Aabb2 bounds)
    {
        var start = Point(bounds);
        var end = Point(bounds);
        return new Segment(start, end, 0);
    }
    
    public Segment[] Segments(Aabb2 bounds, int count)
    {
        var segments = new Segment[count];
        for (var i = 0; i < count; i++)
        {
            var start = Point(bounds);
            var end = Point(bounds);
            segments[i] = new Segment(start, end, i);
        }
        return segments;
    }

    public (Segment, double, double) IntersectingSegment(Segment seg, int index = 0)
    {
        var t0 = Double(0.0, seg.Length);

        var angle = Double(0.1, 179.9) * Flip();
        var d2 = seg.Direction.Transformed(Isometry.Rotate(angle));
        var l1 = Double(0.1, 5.0);
        var t1 = Double(0, l1);
        var pi = seg.PointAt(t0);
        var seg1 = new Segment(pi - d2 * t1, pi + d2 * l1, index);
        
        return (seg1, t0, t1);
    }

    public Segment NonIntersectingSegment(Segment seg, int index = 0)
    {
        var x0 = Double(-1, 2);
        var x1 = Double(-1, 2);
        while (x0 is >= 0 and <= 1 || x1 is >= 0 and <= 1)
        {
            x0 = Double(-1, 2);
            x1 = Double(-1, 2);
        }

        var t0 = x0 * seg.Length;
        var l1 = Double(0.1, 5.0);
        var t1 = x1 * l1;

        var pi = seg.PointAt(t0);
        var angle = Double(0.1, 179.9) * Flip();
        var d2 = seg.Direction.Transformed(Isometry.Rotate(angle));
        var seg1 = new Segment(pi - d2 * t1, pi + d2 * l1, index);

        return seg1;
    }

}