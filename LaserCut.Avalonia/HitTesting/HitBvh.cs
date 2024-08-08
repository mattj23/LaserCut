using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia.HitTesting;

public class HitBvh
{
    private readonly List<IHitTestable> _elements;
    
    public HitBvh(IEnumerable<IHitTestable> elements) :this(elements, true) {}

    public HitBvh(IEnumerable<IHitTestable> elements, bool horizontal)
    {
        _elements = elements.ToList();

        Bounds = _elements.CombinedBounds();
        Split(horizontal);

        // Check that the leaf nodes are not empty
        if (IsLeaf && _elements.Count == 0)
        {
            throw new ArgumentException("Leaf node cannot be empty");
        }
        
        // Check that the non-leaf nodes are empty
        if (!IsLeaf && _elements.Count > 0)
        {
            throw new ArgumentException("Non-leaf node cannot have elements");
        }
        
        // Check that non-leaf nodes have both children
        if (!IsLeaf && (Left is null || Right is null))
        {
            throw new ArgumentException("Non-leaf node must have both children");
        }
    }
    
    public Aabb2 Bounds { get; }
    
    public HitBvh? Left { get; private set; }
    
    public HitBvh? Right { get; private set; }
    
    public bool IsLeaf => Left == null && Right == null;

    public bool Hit(Point2D point)
    {
        if (!Bounds.Contains(point)) return false;
        
        if (IsLeaf)
        {
            return _elements.Any(e => e.Hit(point));
        }
        
        return Left!.Hit(point) || Right!.Hit(point);
    }

    private void Split(bool horizontal)
    {
        if (_elements.Count <= 3) return;
        
        _elements.Sort((a, b) => horizontal ? a.Bounds.Center.X.CompareTo(b.Bounds.Center.X) 
            : a.Bounds.Center.Y.CompareTo(b.Bounds.Center.Y));
        
        var mid = _elements.Count / 2;
        Left = new HitBvh(_elements.Take(mid), !horizontal);
        Right = new HitBvh(_elements.Skip(mid), !horizontal);
        _elements.Clear();
    }

}