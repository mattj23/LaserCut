using System.Diagnostics.Contracts;
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
    /// decreasing area.  The loop will be modified in place.
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

    /// <summary>
    /// This method will create an offsetted version of the loop, and then fix any self-intersections which occur from
    /// the operation. Because it's possible for particular types of offsets to result in multiple resulting loops
    /// (for instance an inward-facing dog-bone shape) this method will return a list of PointLoops.
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    public List<PointLoop> OffsetAndFixed(double distance)
    {
        var results = new List<PointLoop>();
        
        var working = Copy();
        working.Offset(distance);
        var workingLoops = new List<PointLoop> { working };

        while (workingLoops.Any())
        {
            // Pop the first loop off the list
            var loop = workingLoops[0];
            workingLoops.RemoveAt(0);
            
            // If there are self-intersections, split the loop at the first intersection and add the resulting loops
            // to the working list, otherwise the loop is good to go.
            var intersections = loop.SelfIntersections();
            if (!intersections.Any())
            {
                results.Add(loop);
            }
            else
            {
                var i = intersections[0];
                var (loop0, loop1) = loop.Split(i.Item1, i.Item2, i.Item3);
                workingLoops.Add(loop0);
                workingLoops.Add(loop1);
            }
        }

        if (Area > 0)
        {
            return results.Where(x => x.Area > 0).ToList();
        }
        else
        {
            return results.Where(x => x.Area < 0).ToList();
        }
    }
    
    [Pure]
    public PointLoop Offsetted(double distance)
    {
        var loop = Copy();
        loop.Offset(distance);
        return loop;
    }
    
    /// <summary>
    /// Creates a filled area offset of the loop which can be drawn as a filled polygon.
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    [Pure]
    public PointLoop FilledOffset(double distance)
    {
        var working = Copy();
        working.Reverse();
        
        var offset = Offsetted(distance);
        
        var cursor = working.GetCursor();
        cursor.InsertAbs(working.Head);

        foreach (var p in offset.IterItems(offset.TailId))
        {
            cursor.InsertAbs(p.Item);
        }
        
        cursor.InsertAbs(offset.Tail);

        return working;
    }
    
    public override PointLoop Copy()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        foreach (var value in IterItems())
        {
            cursor.InsertAfter(value.Item);
        }

        return loop;
    }

    public List<(int, int, Point2D)> SelfIntersections()
    {
        var results = new Dictionary<(int, int), Point2D>();
        // var results = new List<(int, int, Point2D)>();
        foreach (var seg0 in Segments)
        {
            var index = seg0.Index;
            var nextIndex = Nodes[index].NextId;
            var prevIndex = Nodes[index].PreviousId;

            var items = Bvh.Intersections(seg0)
                .Where(s => s.Segment.Index != index && s.Segment.Index != nextIndex && s.Segment.Index != prevIndex)
                .ToArray();
            foreach (var item in items)
            {
                var key = (Math.Min(index, item.Segment.Index), Math.Max(index, item.Segment.Index));
                if (!results.ContainsKey(key))
                {
                    results[key] = item.Segment.PointAt(item.T);
                }
            }
        }

        return results.Select(r => (r.Key.Item1, r.Key.Item2, r.Value)).ToList();
    }

    public (PointLoop, PointLoop) Split(int i0, int i1, Point2D p)
    {
        var loop0 = new PointLoop();
        var c0 = loop0.GetCursor();
        var loop1 = new PointLoop();
        var c1 = loop1.GetCursor();
        
        bool inLoop0 = true;
        var push = new Action<Point2D> (p =>
        {
            if (inLoop0)
                c0.InsertAfter(p);
            else
                c1.InsertAfter(p);
        });

        var read = GetCursor(HeadId);
        push(read.Current);
        if (read.CurrentId == i0 || read.CurrentId == i1)
        {
            push(p);
            inLoop0 = false;
            push(p);
        }
        read.MoveForward();

        bool done = false;
        while (read.CurrentId != HeadId)
        {
            push(read.Current);
            read.MoveForward();
            
            if (!done && (read.CurrentId == i0 || read.CurrentId == i1))
            {
                push(read.Current);
                inLoop0 = true;
                read.MoveForward();
                done = true;
            }
        }
        
        return (loop0, loop1);
    }

    public List<SegPairIntersection> Intersections(PointLoop other)
    {
        return Bvh.Intersections(other.Bvh);
    }
    
    public List<SegIntersection> Intersections(IBvhIntersect other)
    {
        return Bvh.Intersections(other);
    }

    public LoopRelation RelationTo(PointLoop other)
    {
        if (Intersections(other).Any()) return LoopRelation.Intersecting;
        
        // Is the other loop inside this loop?
        var ray = new Ray2(Head, Vector2D.XAxis);
        var intersections = other.Intersections(ray);
        if (intersections.Count % 2 == 1) return LoopRelation.Inside;

        return LoopRelation.Outside;
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