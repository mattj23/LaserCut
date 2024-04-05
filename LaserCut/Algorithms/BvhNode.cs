using LaserCut.Geometry;

namespace LaserCut.Algorithms;

public class BvhNode
{
    private readonly List<Segment> _segments;
    
    public BvhNode(IEnumerable<Segment> segments)
    {
        _segments = segments.ToList();
        
        Bounds = Aabb2.Empty;
        foreach (var segment in _segments)
        {
            Bounds = Bounds.Union(segment.Bounds);
        }
    }

    public void Split(bool horizontal = true)
    {
        if (_segments.Count <= 3)
        {
            return;
        }
        
        // Order the segments either by x or y, depending on the horizontal flag
        _segments.Sort((a, b) => horizontal ? a.Start.X.CompareTo(b.Start.X) : a.Start.Y.CompareTo(b.Start.Y));
        
        // Split the segments into two groups
        var mid = _segments.Count / 2;
        Left = new BvhNode(_segments.Take(mid));
        Right = new BvhNode(_segments.Skip(mid));
        _segments.Clear();
        Left.Split(!horizontal);
        Right.Split(!horizontal);
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
    
}