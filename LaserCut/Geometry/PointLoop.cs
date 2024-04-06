using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public class PointLoop : Loop<Point2D>
{
    private List<Segment>? _segments;
    private BvhNode? _bvh;
    private double _area = double.NaN;
    
    public PointLoop() { }
    
    public PointLoop(IEnumerable<Point2D> points) : base(points) { }
    
    public IReadOnlyList<Segment> Segments => _segments ??= BuildSegments();
    
    public BvhNode Bvh => _bvh ??= new BvhNode(Segments);

    public Aabb2 Bounds => Bvh.Bounds;
    
    public double Area => _area = double.IsNaN(_area) ? CalculateArea() : _area;
    
    
    public override void OnItemChanged(Point2D item)
    {
        _segments = null;
        _bvh = null;
        _area = double.NaN;
        base.OnItemChanged(item);
    }
    
    private List<Segment> BuildSegments()
    {
        var segments = new List<Segment>();
        foreach (var (a, b) in IterEdges())
        {
            segments.Add(new Segment(a.Item, b.Item, a.Id));
        }
        
        return segments;
    }
    
    private double CalculateArea()
    {
        var area = 0.0;
        // foreach (var (a, b) in IterEdges())
        // {
        //     area += a.Item.X * b.Item.Y;
        //     area -= b.Item.X * a.Item.Y;
        // }
        foreach (var seg in Segments)
        {
            area += seg.Start.X * seg.End.Y;
            area -= seg.End.X * seg.Start.Y;
        }
        
        return area / 2;
    }
    
}