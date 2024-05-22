using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

/// <summary>
/// Single node in a bounding volume hierarchy 
/// </summary>
public class Bvh
{
    private readonly List<IContourElement> _elements;
    
    public Bvh(IEnumerable<IContourElement> elements) : this(elements, true) { }
    
    private Bvh(IEnumerable<IContourElement> elements, bool horizontal)
    {
        _elements = elements.ToList();
        
        Bounds = Aabb2.Empty;
        foreach (var element in _elements)
        {
            Bounds = Bounds.Union(element.Bounds);
        }
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
    
    public Bvh? Left { get; private set; }
    
    public Bvh? Right { get; private set; }
    
    public bool IsLeaf => Left == null && Right == null;
    
    
    public Position[] Intersections(IBvhTest entity)
    {
        
        if (!entity.RoughIntersects(Bounds))
        {
            return [];
        }

        var results = new List<Position>();
        foreach (var element in _elements)
        {
            results.AddRange(entity.Intersections(element));
        }
        
        if (Left is not null)
        {
            results.AddRange(Left.Intersections(entity));
        }
        
        if (Right is not null)
        {
            results.AddRange(Right.Intersections(entity));
        }
        
        return results.ToArray();
    }
    

    private void Split(bool horizontal)
    {
        if (_elements.Count <= 3)
        {
            return;
        }
        
        // Order the segments either by x or y, depending on the horizontal flag
        _elements.Sort((a, b) => horizontal ? a.Start.X.CompareTo(b.Start.X) : a.Start.Y.CompareTo(b.Start.Y));
        
        // Split the segments into two groups
        var mid = _elements.Count / 2;
        Left = new Bvh(_elements.Take(mid), !horizontal);
        Right = new Bvh(_elements.Skip(mid), !horizontal);
        _elements.Clear();
    }
}