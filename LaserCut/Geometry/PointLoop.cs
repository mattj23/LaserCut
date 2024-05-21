using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry.Primitives;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public class PointLoop : Loop<Point2D>
{
    private List<Segment>? _segments;
    private BvhNode? _bvh;
    private double _area = double.NaN;

    public PointLoop(Guid id)
    {
        Id = id;
    }

    public PointLoop() : this(Guid.NewGuid())
    {
    }

    public PointLoop(IEnumerable<Point2D> points, Guid id) : base(points)
    {
        Id = id;
    }

    public PointLoop(IEnumerable<Point2D> points) : this(points, Guid.NewGuid())
    {
    }

    public Guid Id { get; }


    public IReadOnlyList<Segment> Segments => _segments ??= BuildSegments();

    public BvhNode Bvh => _bvh ??= new BvhNode(Segments);

    public Aabb2 Bounds => Bvh.Bounds;

    public double Area => _area = double.IsNaN(_area) ? CalculateArea() : _area;
    
    public AreaPolarity Polarity => Area > 0 ? AreaPolarity.Positive : AreaPolarity.Negative;

    public override IPointLoopCursor GetCursor(int? id = null)
    {
        return new PointLoopCursor(this, id ?? GetTailId());
    }

    public override void OnItemChanged(Point2D item)
    {
        ResetCachedValues();
        base.OnItemChanged(item);
    }

    public override string ToString()
    {
        var text = new StringBuilder();
        text.Append("[PointLoop ");
        foreach (var node in Nodes) text.Append(node.Value.Item).Append(" ");
        text.Append("]");
        return text.ToString();
    }

    // ==============================================================================================================
    // Bulk transformations
    // ==============================================================================================================
    
    public void Translate(double x, double y)
    {
        Transform(Isometry2.Translate(x, y));
    }

    public void Transform(Matrix t)
    {
        foreach (var node in Nodes) node.Value.Item = node.Value.Item.Transformed(t);
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

    [Pure]
    public PointLoop Reversed()
    {
        var loop = Copy();
        loop.Reverse();
        return loop;
    }

    // ==============================================================================================================
    // Simplifications and Error Correction
    // ==============================================================================================================

    /// <summary>
    ///     Remove adjacent duplicate points from the loop.  This will make a single pass through the loop, removing any
    ///     vertices whose distance to its previous neighbor is less than the specified distance tolerance.
    /// </summary>
    /// <param name="tol">
    ///     The distance tolerance below which vertices are considered duplicates of each other. If no
    ///     tolerance is specified, the global constant distance equals is used.
    /// </param>
    public void RemoveAdjacentDuplicates(double? tol = null)
    {
        var t = tol ?? GeometryConstants.DistEquals;
        var visited = new HashSet<int>();
        var cursor = GetCursor();
        while (!visited.Contains(cursor.CurrentId))
            if (cursor.Current.DistanceTo(cursor.PeekNext()) < t)
            {
                cursor.Remove();
            }
            else
            {
                visited.Add(cursor.CurrentId);
                cursor.MoveForward();
            }
    }

    /// <summary>
    ///     Remove vertices who are collinear with their neighbors within a specified tolerance.  This will make a single
    ///     pass through the loop, examining each vertex and its neighbors to determine if they are collinear.  If the
    ///     distance from the vertex to the point projected onto the line between its predecessor and successor is less than
    ///     the specified distance tolerance, the vertex will be removed.
    /// </summary>
    /// <param name="tol"></param>
    public void RemoveAdjacentCollinear(double? tol = null)
    {
        var t = tol ?? GeometryConstants.DistEquals;
        var visited = new HashSet<int>();
        var cursor = GetCursor();
        while (!visited.Contains(cursor.CurrentId))
        {
            var n = cursor.PeekNext();
            var p = cursor.PeekPrevious();
            var d = n - p;
            if (d.Length > 0 && new Line2(p, d).DistanceTo(cursor.Current) < t)
            {
                cursor.Remove();
            }
            else
            {
                visited.Add(cursor.CurrentId);
                cursor.MoveForward();
            }
        }
    }
    
    // ==============================================================================================================
    // Shape operations
    // ==============================================================================================================
    
    public (PointLoop, PointLoop) Split(int i0, int i1, Point2D p)
    {
        var i0n = Nodes[i0].NextId;
        var i1n = Nodes[i1].NextId;

        var loop0 = new PointLoop(SliceItems(i0n, i1n));
        var loop1 = new PointLoop(SliceItems(i1n, i0n));
        loop0.GetCursor().InsertAbs(p);
        loop1.GetCursor().InsertAbs(p);

        return (loop0, loop1);
    }

    /// <summary>
    /// This performs a very simplistic merge of two loops. This loop is considered to be the primary item, and the
    /// other loop will modify it.  If the other loop doesn't intersect with this loop at all, the result will be a
    /// perfect copy of this loop.  If the other loop does intersect with this loop, but the intersection results in
    /// more than one loop, there is no guarantee of which loop will be returned.
    ///
    /// This is not a boolean operation except under specific conditions: the two loops *do* intersect, and the result
    /// is a single loop.
    ///
    /// To implement boolean operations, use the functions in the ShapeOperation class.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    [Pure]
    public PointLoop MergedWith(PointLoop other)
    {
        return ShapeOperation.SimpleMergeLoops(this, other);
    }

    /// <summary>
    ///     Offset the loop by a distance in the direction of the edge normals.  A positive distance will offset the loop
    ///     in the direction of increasing area, while a negative distance will offset the loop in the direction of
    ///     decreasing area.  The loop will be modified in place.
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

        foreach (var (id, point) in updates) Nodes[id].Item = point;

        ResetCachedValues();
    }
    

    /// <summary>
    ///     This method will create an offsetted version of the loop, and then fix any self-intersections which occur from
    ///     the operation. Because it's possible for particular types of offsets to result in multiple resulting loops
    ///     (for instance an inward-facing dog-bone shape) this method will return a list of PointLoops.
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
            loop.RemoveAdjacentDuplicates();

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
                if (loop0.Count == 0 || loop1.Count == 0) Console.WriteLine("Strange behavior");
                loop0.RemoveAdjacentDuplicates();
                loop1.RemoveAdjacentDuplicates();
                if (loop0.Count > 2) workingLoops.Add(loop0);
                if (loop1.Count > 2) workingLoops.Add(loop1);
            }
        }

        if (Area > 0)
            return results.Where(x => x.Area > 0).ToList();
        return results.Where(x => x.Area < 0).ToList();
    }

    [Pure]
    public PointLoop Offsetted(double distance)
    {
        var loop = Copy();
        loop.Offset(distance);
        return loop;
    }

    /// <summary>
    ///     Creates a filled area offset of the loop which can be drawn as a filled polygon.
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

        foreach (var p in offset.IterItems(offset.TailId)) cursor.InsertAbs(p.Item);

        cursor.InsertAbs(offset.Tail);

        return working;
    }

    public override PointLoop Copy()
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        foreach (var value in IterItems()) cursor.InsertAfter(value.Item);

        return loop;
    }

    // ==============================================================================================================
    // Intersections, measurements, and relations
    // ==============================================================================================================
    
    public (int, int) ClosestVertices(PointLoop other)
    {
        var best = (-1, -1, double.MaxValue);
        foreach (var a in IterItems())
        foreach (var b in other.IterItems())
        {
            var d = a.Item.DistanceTo(b.Item);
            if (d < best.Item3) best = (a.Id, b.Id, d);
        }

        return (best.Item1, best.Item2);
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
                if (!results.ContainsKey(key)) results[key] = item.Segment.PointAt(item.T);
            }
        }

        return results.Select(r => (r.Key.Item1, r.Key.Item2, r.Value)).ToList();
    }

    public List<SegPairIntersection> Intersections(PointLoop other)
    {
        return Bvh.Intersections(other.Bvh);
    }

    public List<SegIntersection> Intersections(IBvhIntersect other)
    {
        // Remove non-unique intersections
        var unique = new List<SegIntersection>();
        foreach (var i in Bvh.Intersections(other))
        {
            var found = false;
            foreach (var u in unique)
            {
                if (u.Point.DistanceTo(i.Point) < GeometryConstants.DistEquals)
                {
                    found = true;
                    break;
                }
            }

            if (!found) unique.Add(i);
        }

        return unique;
    }
    
    public bool ContainsPoint(Point2D p)
    {
        var r0 = new Ray2(p, new Vector2D(1, 1));
        var r1 = r0.Reversed();
        var i0 = Intersections(r0);
        var i1 = Intersections(r1);
        
        // If either direction returns 0, the point is for sure outside of the loop
        if (i0.Count == 0 || i1.Count == 0) return false;
        
        // At this point, the only thing that could screw up the count is if the test ray is collinear with an edge
        // How should we deal with that?
        var check0 = i0.Count % 2 == 1;
        var check1 = i1.Count % 2 == 1;

        if (check0 != check1)
        {
            throw new ArgumentException($"Inconsistent results on a ContainsPoint call");
        }

        return check0;
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
    
    // ==============================================================================================================
    // Internal state management
    // ==============================================================================================================

    private List<Segment> BuildSegments()
    {
        var segments = new List<Segment>();
        foreach (var (a, b) in IterEdges()) segments.Add(new Segment(a.Item, b.Item, a.Id));

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
        foreach (var seg in Segments)
        {
            area += seg.Start.X * seg.End.Y;
            area -= seg.End.X * seg.Start.Y;
        }

        return area / 2;
    }
    
    // ==============================================================================================================
    // Construction helpers
    // ==============================================================================================================

    /// <summary>
    /// Creates a positive rectangle with the specified height and width.  The rectangle will be centered at the origin. 
    /// </summary>
    /// <param name="height">The height (y) of the rectangle</param>
    /// <param name="width">The width (x) of the rectangle</param>
    /// <returns>A positive (counter-clockwise) loop with four corners and four edges</returns>
    public static PointLoop Rectangle(double height, double width)
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        cursor.InsertAbs(-width / 2, -height / 2);
        cursor.InsertRelX(width);
        cursor.InsertRelY(height);
        cursor.InsertRelX(-width);
        return loop;
    }
    
    public static PointLoop RoundedRectangle(double height, double width, double radius, int segments = 9)
    {
        var x = width / 2;
        var y = height / 2;
        var xc = x - radius;
        var yc = y - radius;

        var loop = new PointLoop();
        var cursor = loop.GetCursor();

        cursor.InsertRadius(new(-x, -yc), new(-xc, -y), new(-xc, -yc), segments);
        cursor.InsertRadius(new(xc, -y), new(x, -yc), new(xc, -yc), segments);
        cursor.InsertRadius(new(x, yc), new(xc, y), new(xc, yc), segments);
        cursor.InsertRadius(new(-xc, y), new(-x, yc), new(-xc, yc), segments);
        
        return loop;
    }
    
    /// <summary>
    /// Creates a positive circle with the specified radius.  The circle will be centered at the origin.  The circle
    /// will be split into the specified number of segments.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="segments"></param>
    /// <returns></returns>
    public static PointLoop Circle(double radius, int segments = 36)
    {
        var loop = new PointLoop();
        var cursor = loop.GetCursor();
        for (var i = 0; i < segments; i++)
        {
            var angle = i * 2 * Math.PI / segments;
            cursor.InsertAbs(radius * Math.Cos(angle), radius * Math.Sin(angle));
        }

        return loop;
    }

    // ==============================================================================================================
    // Internal classes
    // ==============================================================================================================

    /// <summary>
    ///     A cursor for a PointLoop.  This adds some convenience features.
    /// </summary>
    private class PointLoopCursor : LoopCursor, IPointLoopCursor
    {
        public PointLoopCursor(PointLoop loop, int id) : base(loop, id)
        {
        }

        public int InsertAbs(Point2D p)
        {
            return InsertAfter(p);
        }

        public int InsertAbs(double x, double y)
        {
            return InsertAbs(new Point2D(x, y));
        }

        public int InsertRel(Vector2D v)
        {
            var previous = Loop.Count == 0 ? new Point2D(0, 0) : Current;
            return InsertAbs(previous + v);
        }

        public int InsertRel(double x, double y)
        {
            return InsertRel(new Vector2D(x, y));
        }

        public int InsertRelX(double x)
        {
            return InsertRel(x, 0);
        }

        public int InsertRelY(double y)
        {
            return InsertRel(0, y);
        }

        public int InsertRadius(Point2D start, Point2D end, Point2D center, int segments)
        {
            var v0 = start - center;
            var v1 = end - center;
            if (Math.Abs(v0.Length - v1.Length) > GeometryConstants.DistEquals)
                throw new ArgumentException("Start and end points must be equidistant from the center");

            var angle = v0.SignedAngleTo(v1);
            var step = angle / segments;
            
            InsertAbs(start);
            for (var i = 1; i < segments + 1; i++)
            {
                var v = v0.Rotate(i * step);
                InsertAbs(center + v);
            }
            
            return CurrentId;
        }
    }

    public enum AreaPolarity
    {
        Positive,
        Negative
    }
}