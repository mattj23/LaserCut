using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

public class Segment
{
    private readonly LineSegment2D _seg;

    public Segment(Point2D start, Point2D end, int index)
    {
        _seg = new LineSegment2D(start, end);
        Bounds = Aabb2.FromPoints(new[] {start, end});
        Index = index;
    }
    
    public Point2D Start => _seg.StartPoint;
    
    public Point2D End => _seg.EndPoint;
        
    public Aabb2 Bounds { get; }
    
    public int Index { get; }
}