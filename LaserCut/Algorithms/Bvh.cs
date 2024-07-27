using System.Diagnostics;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

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
    /// Find the closest distance and corresponding position between the test point and any element in this subtree.
    /// </summary>
    /// <param name="point">The point to compare the distance to</param>
    /// <returns></returns>
    public (double, Position) Closest(Point2D point)
    {
        // The potential closest distance is pruned by using the observation that (1) the elements are entirely 
        // enclosed in their bounding boxes, (2) the distance between the test point and the elements in any bounding
        // box will always be less than or equal to the distance between the test point and the farthest corner of the
        // bounding box, and (3) any bounding box with the shortest distance to the bounding box larger than the
        // smallest farthest distance can be ignored.
        
        // We will want to visit the nodes in breadth first order, so we will use a queue to store the nodes to visit
        var queue = new Queue<Bvh>();
        queue.Enqueue(this);

        var leaves = new Queue<Bvh>();
        
        double minFarthest = double.MaxValue;
        while (queue.TryDequeue(out var node))
        {
            // If the closest distance to the bounding box is greater than the smallest farthest distance, we can skip
            // this node entirely, as none of its children will be closer.
            var closest = node.Bounds.ClosestDistance(point);
            if (closest > minFarthest) continue;
            
            // Now let's check if this node has a farthest distance that is less than the current minimum we've found.
            // If it does, we can update the minimum.
            var farthest = node.Bounds.FarthestDistance(point);
            if (farthest < minFarthest)
            {
                minFarthest = farthest;
            }
            
            // If this node is a leaf and we've made it this far, we can add it to the list of leaves to check.
            // Otherwise, we will add the children to the end of the queue.
            if (node.IsLeaf)
            {
                leaves.Enqueue(node);
            }
            else
            {
                queue.Enqueue(node.Left!);
                queue.Enqueue(node.Right!);
            }
        }
        
        // Now that we have the list of leaves, we can check the distance to each element in the leaves.
        var minDistance = double.MaxValue;
        var position = default(Position);
        foreach (var leaf in leaves)
        {
            var closest = leaf.Bounds.ClosestDistance(point);
            if (closest > minFarthest || closest > minDistance) continue;
            
            foreach (var e in leaf._elements)
            {
                var p = e.Closest(point);
                var d = point.DistanceTo(p.Surface.Point);
                if (d < minDistance)
                {
                    minDistance = d;
                    position = p;
                }
            }
        }
        
        return (minDistance, position);
    }
    
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

    public (double, Position, Position) Closest(Bvh other)
    {
        // In this case, the closest distance is pruned by computing both the closest distance and 
        // the farthest distance between the bounds of each pair of nodes.  For any pair of nodes, if the
        // closest distance between nodes is larger than the minimum farthest distance we've come across
        // so far, we can ignore it.
        //
        var minFarthest = double.MaxValue;

        var queue = new Queue<(Bvh, Bvh)>();
        var leafPairs = new Queue<(Bvh, Bvh)>();
        queue.Enqueue((this, other));

        while (queue.TryDequeue(out var pair))
        {
            var (a, b) = pair;
            var closest = a.Bounds.ClosestDistance(b.Bounds);
            var farthest = a.Bounds.FarthestDistance(b.Bounds);
            
            if (closest > minFarthest) continue;
            if (farthest < minFarthest) minFarthest = farthest;

            switch (a.IsLeaf)
            {
                case true when b.IsLeaf:
                    leafPairs.Enqueue((a, b));
                    break;
                case true when !b.IsLeaf:
                    queue.Enqueue((a, b.Left!));
                    queue.Enqueue((a, b.Right!));
                    break;
                case false when b.IsLeaf:
                    queue.Enqueue((a.Right!, b));
                    queue.Enqueue((a.Left!, b));
                    break;
                default:
                    queue.Enqueue((a.Right!, b.Right!));
                    queue.Enqueue((a.Left!, b.Right!));
                    queue.Enqueue((a.Right!, b.Left!));
                    queue.Enqueue((a.Left!, b.Left!));
                    break;
            }
        }
        
        // Now validate the leaf pairs
        var best = double.MaxValue;
        var p0 = default(Position);
        var p1 = default(Position);

        while (leafPairs.TryDequeue(out var pair))
        {
            var (a, b) = pair;
            if (a.Bounds.ClosestDistance(b.Bounds) > minFarthest) continue;

            foreach (var e0 in a._elements)
            {
                foreach (var e1 in b._elements)
                {
                    var (d, pa, pb) = e0.Closest(e1);
                    if (!(d < best)) continue;
                    best = d;
                    p0 = pa;
                    p1 = pb;
                }
            }
        }

        return (best, p0, p1);
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