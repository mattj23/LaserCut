using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry.Primitives;
using MathNet.Numerics.LinearAlgebra.Double;
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
        ResetCachedValues();
        base.OnItemChanged(item);
    }

    public void Transform(Matrix t)
    {
        foreach (var node in Nodes)
        {
            node.Value.Item = node.Value.Item.Transformed(t);
        }
        ResetCachedValues();
    }

    public void MirrorX(double x0 = 0)
    {
        foreach (var node in Nodes)
        {
            var dx = node.Value.Item.X - x0;
            node.Value.Item = new Point2D(x0 - dx, node.Value.Item.Y);
        }
        
        Reverse();
        ResetCachedValues();
    }
    
    public void MirrorY(double y0 = 0)
    {
        foreach (var node in Nodes)
        {
            var dy = node.Value.Item.Y - y0;
            node.Value.Item = new Point2D(node.Value.Item.X, y0 - dy);
        }
        
        Reverse();
        ResetCachedValues();
    }

    /// <summary>
    /// Offset the loop by a distance in the direction of the edge normals.  A positive distance will offset the loop
    /// in the direction of increasing area, while a negative distance will offset the loop in the direction of
    /// decreasing area.  
    /// </summary>
    /// <param name="distance">A distance value to offset the loop by</param>
    public void Offset(double distance)
    {
        var updates = new Dictionary<int, Point2D>();
        foreach (var node in Nodes)
        {
            var b = node.Value;
            var a = Nodes[b.PreviousId];
            var c = Nodes[b.NextId];

            var line0 = new Line2(a.Item, b.Item - a.Item).Offset(distance);
            var line1 = new Line2(b.Item, c.Item - b.Item).Offset(distance);
            if (!line0.IsCollinear(line1))
            {
                var (t0, _) = line0.IntersectionParams(line1);
                updates[node.Key] = line0.PointAt(t0);
            }
            else
            {
                updates[node.Key] = line1.Start;
            }
        }
        
        foreach (var (id, point) in updates)
        {
            Nodes[id].Item = point;
        }
        
        ResetCachedValues();
    }

    public List<(SegIntersection, SegIntersection)> Intersections(PointLoop other)
    {
        throw new NotImplementedException();

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
    
    private void ResetCachedValues()
    {
        _segments = null;
        _bvh = null;
        _area = double.NaN;
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