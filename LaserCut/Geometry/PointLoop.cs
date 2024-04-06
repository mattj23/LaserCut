using LaserCut.Algorithms;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public class PointLoop : Loop<Point2D>
{
    private List<Segment>? _segments;
    private BvhNode? _bvh;
    private double _area = double.NaN;
    private Aabb2 _bounds = Aabb2.Empty;
    
    public PointLoop() { }
    
    public PointLoop(IEnumerable<Point2D> points) : base(points) { }
    
    public override void OnItemChanged(Point2D item)
    {
        _segments = null;
        _bvh = null;
        _area = double.NaN;
        _bounds = Aabb2.Empty;
        base.OnItemChanged(item);
    }
    
    private List<Segment> BuildSegments()
    {
        var segments = new List<Segment>();
        foreach (var edge in GetEdges())
        {
            
        }
    }
    
}