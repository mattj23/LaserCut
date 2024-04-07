using System.Text;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public class BvhNode
{
    private readonly List<Segment> _segments;
    
    public BvhNode(IEnumerable<Segment> segments) : this(segments, true) { }

    private BvhNode(IEnumerable<Segment> segments, bool horizontal)
    {
        _segments = segments.ToList();
        
        Bounds = Aabb2.Empty;
        foreach (var segment in _segments)
        {
            Bounds = Bounds.Union(segment.Bounds);
        }
        Split(horizontal);
        
        // Check that the leaf nodes are not empty
        if (IsLeaf && _segments.Count == 0)
        {
            throw new ArgumentException("Leaf node cannot be empty");
        }
        
        // Check that the non-leaf nodes are empty
        if (!IsLeaf && _segments.Count > 0)
        {
            throw new ArgumentException("Non-leaf node cannot have segments");
        }
        
        // Check that non-leaf nodes have both children
        if (!IsLeaf && (Left is null || Right is null))
        {
            throw new ArgumentException("Non-leaf node must have both children");
        }
    }

    public override string ToString()
    {
        if (IsLeaf)
        {
            return $"[Leaf Node {string.Join(", ", _segments.Select(s => s.Index.ToString()))}]";
        }

        return "[Node]";
    }

    public BvhNode? Left { get; set; }

    public BvhNode? Right { get; set; }
    
    public bool IsLeaf => Left == null && Right == null;
    
    public Aabb2 Bounds { get; private set; }
    
    public List<T> Collect<T>(Func<BvhNode, List<T>> func)
    {
        var thisList = func(this);
        if (Left is not null)
        {
            thisList.AddRange(Left.Collect(func));
        }
        
        if (Right is not null)
        {
            thisList.AddRange(Right.Collect(func));
        }
        
        return thisList;
    }

    public List<SegPairIntersection> Intersections(BvhNode other)
    {
        var results = new List<SegPairIntersection>();
        
        if (!other.Bounds.Intersects(Bounds))
        {
            return results;
        }
        
        // Four possible cases:
        // 1. Both nodes are leaves
        // 2. This node is a leaf
        // 3. Other node is a leaf
        // 4. Both nodes are not leaves
        
        // Only leaf nodes can have intersections, so if both nodes are leaves, check all pairwise combinations, but
        // if only one of the nodes is a leaf we continue to recurse down on the non-leaf side.  If both nodes are
        // non-leaf, we recurse down both sides.

        if (IsLeaf && other.IsLeaf)
        {
            foreach (var seg0 in _segments)
                foreach (var seg1 in other._segments)
                    if (seg0.IntersectsAsPair(seg1) is { } intersection)
                        results.Add(intersection);
        }
        else if (IsLeaf)
        {
            results.AddRange(Intersections(other.Left!));
            results.AddRange(Intersections(other.Right!));
        }
        else if (other.IsLeaf)
        {
            results.AddRange(Left!.Intersections(other));
            results.AddRange(Right!.Intersections(other));
        }
        else
        {
            results.AddRange(Left!.Intersections(other.Left!));
            results.AddRange(Left.Intersections(other.Right!));
            results.AddRange(Right!.Intersections(other.Left!));
            results.AddRange(Right.Intersections(other.Right!));
        }
        
        return results;
    }
    
    public List<SegIntersection> Intersections(IBvhIntersect entity)
    {
        var results = new List<SegIntersection>();
        
        if (!entity.RoughIntersects(Bounds))
        {
            return results;
        }

        foreach (var seg in _segments)
        {
            if (entity.Intersects(seg) is { } intersection)
            {
                results.Add(intersection);
            }
        }
        
        if (Left is not null)
        {
            results.AddRange(Left.Intersections(entity));
        }
        
        if (Right is not null)
        {
            results.AddRange(Right.Intersections(entity));
        }
        
        return results;
    }
    
    private void Split(bool horizontal)
    {
        if (_segments.Count <= 3)
        {
            return;
        }
        
        // Order the segments either by x or y, depending on the horizontal flag
        _segments.Sort((a, b) => horizontal ? a.Start.X.CompareTo(b.Start.X) : a.Start.Y.CompareTo(b.Start.Y));
        
        // Split the segments into two groups
        var mid = _segments.Count / 2;
        Left = new BvhNode(_segments.Take(mid), !horizontal);
        Right = new BvhNode(_segments.Skip(mid), !horizontal);
        _segments.Clear();
    }
    
}