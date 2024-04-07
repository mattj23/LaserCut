using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public interface IPointLoopCursor : ILoopCursor<Point2D>
{
    int InsertAbs(Point2D p);
    
    int InsertAbs(double x, double y);
    
    int InsertRel(Vector2D v);
    
    int InsertRel(double x, double y);
    
    int InsertRelX(double x);
    
    int InsertRelY(double y);
    
}

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

    public override IPointLoopCursor GetCursor(int? id = null)
    {
        return new PointLoopCursor(this, id ?? GetTailId());
    }

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
    
    /// <summary>
    /// A cursor for a PointLoop.  This adds some convenience features.
    /// </summary>
    private class PointLoopCursor : LoopCursor, IPointLoopCursor
    {
        public PointLoopCursor(PointLoop loop, int id) : base(loop, id) { }

        public int InsertAbs(Point2D p) => InsertAfter(p);
        
        public int InsertAbs(double x, double y) => InsertAbs(new Point2D(x, y));

        public int InsertRel(Vector2D v)
        {
            var previous = Loop.Count == 0 ? new Point2D(0, 0) : Current;
            return InsertAbs(previous + v);
        }
        
        public int InsertRel(double x, double y) => InsertRel(new Vector2D(x, y));

        public int InsertRelX(double x) => InsertRel(x, 0);
        
        public int InsertRelY(double y) => InsertRel(0, y);


    }
    
}