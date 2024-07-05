using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

/// <summary>
/// Single node in a bounding volume hierarchy 
/// </summary>
public class Bvh
{
    private readonly List<IBoundaryElement> _elements;
    
    public Bvh(IEnumerable<IBoundaryElement> elements) : this(elements, true) { }
    
    private Bvh(IEnumerable<IBoundaryElement> elements, bool horizontal)
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
    
    
    /// <summary>
    /// Find all intersections between items in this subtree and the specified entity, using the BVH structure to
    /// accelerate the search.
    ///
    /// The results will be an array of `Position` objects, which will reference the `IContourElement` object that
    /// was found *in the BVH* to intersect with the given entity. 
    /// </summary>
    /// <param name="entity">An entity capable of testing for intersections against both the Aabb2 bounds and
    /// any `IContourElement` object.</param>
    /// <returns>An array with all valid intersections.</returns>
    public Position[] Intersections(IBvhIntersect entity)
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

    /// <summary>
    /// Find all intersections between items in this subtree and the other subtree, using the BVH structure to
    /// accelerate the search.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public IntersectionPair[] Intersections(Bvh other)
    {
        var results = new List<IntersectionPair>();
        
        if (!other.Bounds.Intersects(Bounds)) return [];
        
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
            foreach (var e0 in _elements)
            {
                foreach (var e1 in other._elements)
                {
                    var intersections = e0 switch
                    {
                        Arc arc => e1.IntersectionsWithCircle(arc.Circle),
                        Segment seg => e1.IntersectionsWithLine(seg),
                        _ => throw new ArgumentOutOfRangeException(nameof(e0))
                    };
                    results.AddRange(e0.MatchIntersections(intersections));
                }
            }
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