using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry.Primitives;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public abstract record BoundaryPoint(Point2D Point)
{
    public abstract BoundaryPoint Copy();
    
    public abstract string Serialize();
}

public record BoundaryLine(Point2D Point) : BoundaryPoint(Point)
{
    public override BoundaryPoint Copy() => new BoundaryLine(Point);
    
    public override string Serialize() => $"L[{Point.X:F6},{Point.Y:F6}]";
}

public record BoundaryArc(Point2D Point, Point2D Center, bool Clockwise) : BoundaryPoint(Point)
{
    public override BoundaryPoint Copy() => new BoundaryArc(Point, Center, Clockwise);
    
    public override string Serialize() => $"A[{Point.X:F6},{Point.Y:F6},{Center.X:F6},{Center.Y:F6},{Clockwise}]";
}

/// <summary>
/// A `Contour` is a loop of entities which define a closed shape in space, where each entity is either a straight
/// line segment or a circular (non-elliptical) arc.  Each entity defines the starting point in 2D space, and the
/// information that describes how the shape's boundary is defined between its starting point and the starting point
/// of the next entity.
///
/// The base entity is the `ContourPoint`, from which the `ContourLine` and `ContourArc` are derived.  For line
/// segments, the `ContourLine` is used and only the starting point is defined.  It is assumed that the boundary will
/// be a straight line segment which ends at the next entity's starting point.  For arcs, the `ContourArc` is used and
/// requires (in addition to the starting point) the center of the arc and the direction of the arc (clockwise or
/// counter-clockwise).
///
/// During construction, the `Contour` will consist only of `ContourPoint` entities.  The `ContourCursor` is used to
/// insert, remove, and replace entities in the contour.
///
/// When a geometric operation is performed with or on the contour, such as calculating area or checking for
/// intersections, the `Contour` will build an internal list of `IContourElement` entities.  These entities are
/// constructed from each `ContourPoint` and it's successor in the loop.  If the `Contour` is modified after the
/// geometric entities are built, the internal values will be reset and the entities will be rebuilt the next time they
/// are used.
///
/// During the building of geometric elements, if the starting point of the next entity is not the same distance from
/// the arc center as the arc's starting point, an error will be thrown.  If an arc's starting point is the same as its
/// ending point, the arc will be considered a full circle.
/// 
/// </summary>
public class BoundaryLoop : Loop<BoundaryPoint>, IHasBounds
{
    private Bvh? _bvh = null;
    private List<IBoundaryElement>? _elements = null;
    private double _area = double.NaN;
    
    /// <summary>
    /// Create a new contour with a unique identifier.
    /// </summary>
    /// <param name="id"></param>
    public BoundaryLoop(Guid id) { Id = id; }
    
    /// <summary>
    /// Create a new contour. A unique identifier will be generated automatically.
    /// </summary>
    public BoundaryLoop() : this(Guid.NewGuid()) { }

    /// <summary>
    /// Create a new contour with a unique identifier and a list of initial entities.
    /// </summary>
    /// <param name="id">The uuid to assign to this contour</param>
    /// <param name="entities">A list of initial entities which will be added to the contour at creation</param>
    public BoundaryLoop(Guid id, IEnumerable<BoundaryPoint> entities)
    {
        Id = id;
        var cursor = GetCursor();
        foreach (var entity in entities)
        {
            cursor.InsertAfter(entity);
        }
    }
    
    /// <summary>
    /// Create a new contour with a list of initial entities.  A unique identifier will be generated automatically.
    /// </summary>
    /// <param name="entities">A list of initial entities which will be added to the contour at creation</param>
    public BoundaryLoop(IEnumerable<BoundaryPoint> entities) : this(Guid.NewGuid(), entities) { }
        
    // ==============================================================================================================
    // Properties
    // ==============================================================================================================
    
    /// <summary>
    /// Gets a unique identifier for the contour.  This was either generated automatically during construction or
    /// provided by the client code when the contour was created.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the list of geometric elements that define the contour.  This will trigger the construction of the
    /// geometric elements. If the contour is in an invalid state, an exception will be thrown.
    /// </summary>
    public IReadOnlyList<IBoundaryElement> Elements => _elements ??= BuildElements();
    
    /// <summary>
    /// Gets the bounding volume hierarchy for the contour for accelerated geometric operations.  This will trigger
    /// the construction of the geometric elements.  If the contour is in an invalid state, an exception will be thrown.
    /// </summary>
    public Bvh Bvh => _bvh ??= new Bvh(Elements);
    
    /// <summary>
    /// Gets the bounding box of the contour.  This will trigger the construction of the geometric elements.  If the
    /// contour is in an invalid state, an exception will be thrown.
    /// </summary>
    public Aabb2 Bounds => Bvh.Bounds;
    
    /// <summary>
    /// Gets the area of the contour found by the shoelace formula.  This will trigger the construction of the
    /// geometric elements.  If the contour is in an invalid state, an exception will be thrown.
    /// </summary>
    public double Area => _area = double.IsNaN(_area) ? CalculateArea() : _area;
    
    /// <summary>
    /// Gets a flag indicating whether the area of the contour is positive.  A positive area implies that the contour
    /// defines a region where a shape exists, while a negative area implies that it defines an area where a shape
    /// does not exist (such as a hole).  This will trigger the construction of geometric elements, so if the contour is
    /// in an invalid state, an exception will be thrown.
    /// </summary>
    public bool IsPositive => Area > 0;
    
    /// <summary>
    /// Gets whether the boundary represents a null set.  This is true if the contour has no elements, or if it has
    /// a single element which is a line segment.  This does not trigger the construction of geometric elements, and
    /// will only work if RemoveThinSections has been called.
    /// </summary>
    public bool IsNullSet => Count == 0 || Count == 1 && Nodes.First().Value.Item is BoundaryLine;

    // ==============================================================================================================
    // Geometric operations
    // ==============================================================================================================
    /// <summary>
    /// Transform the contour in place by the given transformation matrix.
    /// </summary>
    /// <param name="t">A matrix defining the transform to apply</param>
    public void Transform(Matrix t)
    {
        foreach (var node in Nodes)
        {
            node.Value.Item = node.Value.Item switch
            {
                BoundaryArc arc => new BoundaryArc(arc.Point.Transformed(t), arc.Center.Transformed(t), arc.Clockwise),
                BoundaryLine line => new BoundaryLine(line.Point.Transformed(t)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        ResetCachedValues();
    }
    
    /// <summary>
    /// Translate the contour in place by the given vector.
    /// </summary>
    /// <param name="v"></param>
    public void Translate(Vector2D v)
    {
        Transform(Isometry2.Translate(v));
    }

    /// <summary>
    /// Translate the contour in place by the given x and y values.
    /// </summary>
    /// <param name="tx"></param>
    /// <param name="ty"></param>
    public void Translate(double tx, double ty)
    {
        Transform(Isometry2.Translate(tx, ty));
    }

    /// <summary>
    /// Mirror the contour in place across the given line.  
    /// </summary>
    /// <param name="cl">The center-line to mirror the contour across</param>
    public void Mirror(Line2 cl)
    {
        foreach (var node in Nodes)
        {
            node.Value.Item = node.Value.Item switch
            {
                BoundaryArc arc => new BoundaryArc(cl.Mirror(arc.Point), cl.Mirror(arc.Center), !arc.Clockwise),
                BoundaryLine line => new BoundaryLine(cl.Mirror(line.Point)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        ResetCachedValues();
    }

    public void MirrorX(double x0 = 0)
    {
        var line = new Line2(new Point2D(x0, 0), Vector2D.YAxis);
        Mirror(line);
    }
    
    public void MirrorY(double y0 = 0)
    {
        var line = new Line2(new Point2D(0, y0), Vector2D.XAxis);
        Mirror(line);
    }

    /// <summary>
    /// Reverse the direction of the contour in place
    /// </summary>
    public new void Reverse()
    {
        var elements = new Dictionary<int, BoundaryPoint>();
        
        // We will iterate through each entity in the contour in forward order and create its replacement.  Because the
        // entity is actually representing the definition of the border between itself and the next entity, each entity
        // type will remain the same, but the starting points will be changed. On arcs, the center will remain the same
        // but the direction will be reversed (clockwise vs counterclockwise).
        foreach (var (a, b) in IterEdges())
        {
            elements[a.Id] = a.Item switch
            {
                BoundaryLine _ => new BoundaryLine(b.Item.Point),
                BoundaryArc arc => new BoundaryArc(b.Item.Point, arc.Center, !arc.Clockwise),
                _ => throw new NotImplementedException()
            };
        }

        foreach (var node in Nodes)
        {
            node.Value.Item = elements[node.Key];
            node.Value.SwapNextAndPrevious();
        }
        
        ResetCachedValues();
    }

    /// <summary>
    /// Generate a new contour which is the same as this contour but with the direction of entities reversed. The new
    /// contour will have a new unique identifier and the entity integer IDs will be different from the original. While
    /// the location of the boundary will be the same, the "direction" of the boundary will invert.  Contours which
    /// represent a positive area will become negative, and vice versa.
    /// </summary>
    /// <returns>A new `Contour` entity with a new id.</returns>
    [Pure]
    public BoundaryLoop Reversed()
    {
        var working = Copy();
        working.Reverse();
        return working;
    }

    /// <summary>
    /// Generate a new contour in which the boundaries are offset in their positive direction by a specified distance.
    /// For instance, if the contour has positive area, a positive distance will increase its area and a negative
    /// distance will decrease it. If the contour has a negative area, the opposite will be true. (In both cases this
    /// assumes that the offset is small enough that the contour does not invert.)
    ///
    /// Offsetting a contour may result in a new contour with self intersection. If this is the case, using the
    /// `NonSelfIntersectingLoops` method will return a set of non-self-intersecting loops which can be used to
    /// determine which part(s) of the offset contour is the desired intention of the operation.
    /// </summary>
    /// <param name="distance">The distance to offset the surface by</param>
    /// <returns>A new contour, which may have self-intersections</returns>
    public BoundaryLoop Offset(double distance)
    {
        /* Offsetting a contour is more complicated than offsetting a polyline, because the arc and segment boundaries
         * behave different. Instead of moving the vertices directly, we will have to perform the geometric offset
         * operation on each segment and arc and then determine the location of the new vertices between neighboring
         * elements.
         */

        // First, create the new elements by offsetting each element in the contour
        var newElements = Elements.ToDictionary(e => e.Index, e => e.OffsetBy(distance));
        
        // Now we will need to find the new start point for each element.  We will look at each element and its 
        // previous neighbor to determine what its new start point is.  We will take advantage of the fact that 
        // arc element endpoints offset correctly, so we only need to compute the new start point for segments 
        // preceded by another segment.
        var newContour = new BoundaryLoop();
        var write = newContour.GetCursor();

        foreach (var item in IterItems())
        {
            var element = newElements[item.Id];
            var previous = newElements[Nodes[item.Id].PreviousId];

            if (element is Arc arc)
            {
                write.InsertFromElement(arc);
            }
            else if (previous is Arc prevArc)
            {
                // This is a segment preceded by an arc, so we can use the arc's endpoint as the start point
                write.SegAbs(prevArc.End.X, prevArc.End.Y);
            }
            else if (previous is Segment prevSeg && element is Segment seg)
            {
                // This is a segment preceded by a segment, so we will need to calculate the new start point. If the
                // two segments are collinear, the midpoint between the previous end and the new start will be the new
                // starting point. If they are not collinear, they will have a single intersection point which must be 
                // the new start point.
                if (prevSeg.IsCollinear(seg))
                {
                    write.SegAbs((prevSeg.End.X + seg.Start.X) / 2, (prevSeg.End.Y + seg.Start.Y) / 2);
                }
                else
                {
                    var (_, t1) = prevSeg.IntersectionParams(seg);
                    var p = seg.PointAt(t1);
                    write.SegAbs(p.X, p.Y);
                }
            }
            else
            {
                // This case should not occur, so throw an error
                throw new UnreachableException();
            }
        }

        return newContour;
    }
    
    /// <summary>
    /// Return a new contour which is the result of offsetting the contour by a specified distance and then repairing
    /// any self intersections.  This method will split the contour into non-self-intersecting loops and return the
    /// one which has the closest area to the original contour.  This will function correctly for both positive and
    /// negative area contours when small offsets are used and the contour does not split into multiple loops.
    /// </summary>
    /// <param name="distance">The distance to offset</param>
    /// <returns></returns>
    public BoundaryLoop OffsetAndRepaired(double distance)
    {
        var offset = Offset(distance);
        var loops = offset.NonSelfIntersectingLoops();
        return loops.OrderBy(l => Math.Abs(l.Area - Area)).First();
    }

    /// <summary>
    /// This method recursively removes (in place) any adjacent zero-length elements.  This is useful for cleaning up
    /// a boundary loop before elements are generated, as any zero-length elements at that time will cause an error.
    /// </summary>
    public void RemoveZeroLengthElements()
    {
        var cursor = GetCursor();
        var lastChecked = -1;
        
        while (Count > 1 && lastChecked != cursor.CurrentId)
        {
            // TODO: Think through...do we need a special method for arcs?
            if (cursor.Current.Point.DistanceTo(cursor.PeekNext().Point) < GeometryConstants.DistEquals)
            {
                cursor.Remove();
            }
            else
            {
                if (lastChecked == -1) lastChecked = cursor.CurrentId;
                cursor.MoveForward();
            }
        }
    }

    /// <summary>
    /// This method removes (in place) any infinitesimally thin portions of the boundary formed by the boundary
    /// doubling back on itself.
    /// </summary>
    public void RemoveThinSections()
    {
        // To find portions of the boundary that double back on themselves, we can examine every boundary point in the
        // loop and its previous and next neighbor.  If the element from p -> c is the same type of element as c -> n
        // (and if they have the same center if they're both arcs), and the direction at start of c -> n is the exact
        // opposite of the direction at the end of p -> c, then we have an infinitely thin portion of the boundary 
        // where c is the turning point.
        //
        // To remove it, we will remove the element c, and move the cursor back to p.  We then repeat this process
        // until we have made it all the way around the loop.
        
        var cursor = GetCursor();
        var visited = new HashSet<int>();

        while (Count > 1 && !visited.Contains(cursor.CurrentId))
        {
            var curr = cursor.Current;
            var prev = cursor.PeekPrevious();
            var next = cursor.PeekNext();
            
            // Remove any zero-length elements between the current and previous elements
            if (curr.Point.DistanceTo(prev.Point) < GeometryConstants.DistEquals)
            {
                visited.Remove(cursor.CurrentId);
                cursor.MoveBackward();
                visited.Remove(cursor.CurrentId);
                cursor.Remove();
                continue;
            }
            
            // Remove any zero-length elements between the current and next elements
            if (curr.Point.DistanceTo(next.Point) < GeometryConstants.DistEquals)
            {
                visited.Remove(cursor.CurrentId);
                cursor.Remove();
                continue;
            }

            if (curr is BoundaryLine cl && prev is BoundaryLine pl)
            {
                var pd = (cl.Point - pl.Point).Normalize();
                var cd = (next.Point - cl.Point).Normalize();
                if (pd.DotProduct(cd) < -1.0 + GeometryConstants.DistEquals)
                {
                    // Remove the element from the completed list (will no-op if it hasn't been added) just in case
                    // we're moving backwards, since this may have been a node we already checked that was fine before
                    // we started deleting its followers.
                    visited.Remove(cursor.CurrentId);
                    cursor.Remove();
                    continue;
                }
            }
            else if (curr is BoundaryArc ca && prev is BoundaryArc pa)
            {
                if (pa.Center.DistanceTo(ca.Center) < GeometryConstants.DistEquals && pa.Clockwise != ca.Clockwise)
                {
                    visited.Remove(cursor.CurrentId);
                    cursor.Remove();
                    continue;
                }
            }
            
            visited.Add(cursor.CurrentId);
            cursor.MoveForward();
        }
    }

    /// <summary>
    /// This method removes (in place) any adjacent redundant elements, such as collinear segments or arcs with the
    /// same center.
    /// </summary>
    public void RemoveAdjacentRedundancies()
    {
        var visited = new HashSet<int>();
        var cursor = GetCursor();

        while (Count > 1 && !visited.Contains(cursor.CurrentId))
        {
            if (cursor.Current is BoundaryLine seg && cursor.PeekPrevious() is BoundaryLine prevSeg)
            {
                var p0 = prevSeg.Point;
                var p1 = seg.Point;
                var p2 = cursor.PeekNext().Point;
                var s = new Segment(p0, p2, -1);
                var error = Math.Abs(s.SignedDistanceTo(p1));
                if (error < GeometryConstants.DistEquals)
                {
                    cursor.Remove();
                    continue;
                }
            }
            else if (cursor.Current is BoundaryArc arc && cursor.PeekPrevious() is BoundaryArc prvArc)
            {
                // Look for arcs with the same center and direction
                if (arc.Center.DistanceTo(prvArc.Center) < GeometryConstants.DistEquals &&
                    arc.Clockwise == prvArc.Clockwise && 
                    cursor.CurrentId != cursor.PreviousId)
                {
                    cursor.Remove();
                    continue;
                }
            }
            
            visited.Add(cursor.CurrentId);
            cursor.MoveForward();
        }
    }
    
    
    // ==============================================================================================================
    // Intersections and distance testing
    // ==============================================================================================================

    /// <summary>
    /// Determines whether the contour encloses the specified point inside its boundary, ignoring the "direction" of
    /// the boundary. This works by casting a ray from the point to the right and counting the number of times it
    /// enters and exits the boundary. Points on the boundary are considered enclosed.
    ///
    /// To determine if a point is on the side of the boundary considered "inside" the contour, compare this result
    /// against the `IsPositive` property.  If the point is inside the contour, the result will be true if the area is
    /// positive, and false if the area is negative.
    /// </summary>
    /// <remarks>
    /// This will trigger the construction of the geometric elements.  If the contour is in an invalid state, an
    /// exception will be thrown.
    /// </remarks>
    /// <param name="p">The specific point to test.</param>
    /// <returns></returns>
    public bool Encloses(Point2D p)
    {
        var ray = new Ray2(p, Vector2D.XAxis);
        var positions = Bvh.Intersections(ray);
        return EnclosesPoint.Check(ray, positions);
    }
    
    /// <summary>
    /// Determines whether the boundary loop *includes* the specified point.  This is similar to `Encloses`, but
    /// accounts for the direction of the loop.  A point is included if it is enclosed and the loop is positive, or if
    /// it is not enclosed and the loop is negative.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool Includes(Point2D p)
    {
        return Encloses(p) && IsPositive || !Encloses(p) && !IsPositive;
    }

    /// <summary>
    /// Calculates all intersections between this contour and another contour, returning the results as an array of
    /// `IntersectionPairs`.  The `First` element of each pair will be the element from *this* contour, while the
    /// `Second` will be from the *other* contour. 
    /// </summary>
    /// <param name="other">The contour to test for intersections with</param>
    /// <returns>An array of intersection pairs where the first element is from *this* contour.</returns>
    public IntersectionPair[] IntersectionPairs(BoundaryLoop other)
    {
        return Bvh.Intersections(other.Bvh);
    }

    /// <summary>
    /// Determines the loop intersection/spatial relationship between this loop and another. The relationship
    /// will be either that the loops are disjoint, one is enclosed by the other, one encloses the other, or they
    /// intersect in some way.
    ///
    /// The resulting enum value can be interpreted as a verb describing the relation of *this loop* to the
    /// *other loop*. For example, if the result is `EnclosedBy`, interpret it as "this loop is enclosed by the
    /// other loop".  If the result is `Encloses`, interpret it as "this loop encloses the other loop".
    ///
    /// Enclosure is defined as the relationship where the path of one loop never exits the space enclosed by the other
    /// loop.  Two identical loops will be mutually enclosing, although this method will only return one or
    /// the other.
    ///
    /// Note that enclosure does not imply anything about whether the loops exist on the positive or negative side
    /// of the boundary.  To determine this, you must interpret the `IsPositive` property of each contour against the
    /// specific relationship, or use the `ShapeRelationTo` method.
    /// </summary>
    /// <param name="other">The other contour to test the relationship to</param>
    /// <returns>The relation of *this contour* to the *other contour* and a list of any intersections.</returns>
    public (BoundaryRelation, IntersectionPair[]) LoopRelationTo(BoundaryLoop other)
    {
        // Intersections that are on portions of the paths that are identical to each other are not intersections that
        // we need to be concerned about.  Because the only place this can happen(?) is at a vertex, we can look for
        // intersections that are at a point that is a vertex on both boundaries.  Then, we look and see if the paths
        // on both sides of the vertex 
        var filtered = new List<IntersectionPair>();
        foreach (var pair in IntersectionPairs(other))
        {
            if (pair.First.IsAtVertex && pair.Second.IsAtVertex)
            {
                var (f0, f1) = ElementsAtVertex(pair.Point);
                var (s0, s1) = other.ElementsAtVertex(pair.Point);
                
                // Now, if one of two configurations is true, we can ignore this intersection:
                // * f0 == s0 and f1 == s1 => The elements are the same type with the same geometry start/end
                // * f0 == s1 and f1 == s0 => The elements are the same type with the reverse geometry start/end
                // We already know the vertices match
                var matchFwd = f0.Matches(s0) && f1.Matches(s1);
                var matchRev = f0.Matches(s1.Reversed()) && f1.Matches(s0.Reversed());
                if (!matchFwd && !matchRev)
                {
                    filtered.Add(pair);
                }
            }
            else
            {
                filtered.Add(pair);
            }
        }

        var intersections = filtered.ToArray();
        if (intersections.Length != 0)
        {
            // The presence of intersections alone isn't enough to determine that the boundaries are not enclosing one
            // another.  Instead, we need to look to see if there are any places where one of the boundaries actually
            // leaves the space enclosed by the other.
            //
            // This can be difficult in the case of boundaries with overlapping regions.  However, if one enters the
            // other but the reverse is not true, 
            // 
            
            // We can still have enclosure with intersections if one contour never exits the other. To check for this
            // we will need to consider the intersections themselves.
            var firstExits = intersections.Any(i => i.FirstExitsSecond);
            var firstEnters = intersections.Any(i => i.FirstEntersSecond);
            
            var reversed = intersections.Select(i => i.Swapped()).ToArray();
            var secondExits = reversed.Any(i => i.FirstExitsSecond);
            var secondEnters = reversed.Any(i => i.FirstEntersSecond);
            
            if ((other.IsPositive && !firstExits) || (!other.IsPositive && !firstEnters)) 
                return (BoundaryRelation.EnclosedBy, intersections);
            
            if ((IsPositive && !secondExits) || (!IsPositive && !secondEnters)) 
                return (BoundaryRelation.Encloses, intersections);


            return (BoundaryRelation.Intersects, intersections);
        }

        // Is the other loop enclosing this loop?
        if (other.Encloses(Head.Point)) return (BoundaryRelation.EnclosedBy, []);
        
        // Is this loop enclosing the other loop?
        if (Encloses(other.Head.Point)) return (BoundaryRelation.Encloses, []);

        return (BoundaryRelation.DisjointTo, []);
    }

    /// <summary>
    /// Determine the relation between the two simple shapes defined by this loop and another loop.  This will return
    /// a verb describing the relationship between the two shapes, and a list of any intersections between the two
    /// boundaries.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public (ShapeRelation, IntersectionPair[]) ShapeRelationTo(BoundaryLoop other)
    {
        var (relation, intersections) = LoopRelationTo(other);
        bool hasIntersections = intersections.Length > 0;
        
        var label = relation switch
        {
            // If the two loops are disjoint, the result will be according to the following table:
            //  * this (positive) other (positive) => DisjointTo
            //  * this (positive) other (negative) => IsSubsetOf
            //  * this (negative) other (positive) => IsSupersetOf
            //  * this (negative) other (negative) => Intersects
            BoundaryRelation.DisjointTo => (IsPositive, other.IsPositive) switch
            {
                (true, true) => ShapeRelation.DisjointTo,
                (true, false) => ShapeRelation.IsSubsetOf,
                (false, true) => ShapeRelation.IsSupersetOf,
                (false, false) => ShapeRelation.Intersects,
            },
            
            // If this loop encloses the other loop, the result will be according to the following table:
            //  * this (positive) other (positive) => IsSupersetOf
            //  * this (positive) other (negative) => Intersects
            //  * this (negative) other (positive) => DisjointTo (but may have intersections?)
            //  * this (negative) other (negative) => IsSubsetOf
            BoundaryRelation.Encloses => (IsPositive, other.IsPositive) switch
            {
                (true, true) => ShapeRelation.IsSupersetOf,
                (true, false) => ShapeRelation.Intersects,
                (false, true) => hasIntersections ? ShapeRelation.Intersects : ShapeRelation.DisjointTo,
                (false, false) => ShapeRelation.IsSubsetOf,
            },
            
            // If the other loop encloses this loop, the result will be according to the following table:
            //  * this (positive) other (positive) => IsSubsetOf 
            //  * this (positive) other (negative) => DisjointTo (but may have intersections?)
            //  * this (negative) other (positive) => Intersects
            //  * this (negative) other (negative) => IsSupersetOf
            BoundaryRelation.EnclosedBy => (IsPositive, other.IsPositive) switch
            {
                (true, true) => ShapeRelation.IsSubsetOf,
                (true, false) => hasIntersections ? ShapeRelation.Intersects : ShapeRelation.DisjointTo,
                (false, true) => ShapeRelation.Intersects,
                (false, false) => ShapeRelation.IsSupersetOf,
            },
            
            BoundaryRelation.Intersects => ShapeRelation.Intersects,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return (label, intersections);
    }

    /// <summary>
    /// Calculate any self-intersections within the contour.  This will return an array of `IntersectionPair` objects,
    /// but be aware that the `First` and `Second` elements of each pair will be from the same contour. The results of
    /// this method cannot be interchanged with the results of `IntersectionPairs` with another contour.
    /// </summary>
    /// <returns>An array of self-intersection pairs, if there are any</returns>
    public IntersectionPair[] SelfIntersections()
    {
        var results = new List<IntersectionPair>();
        foreach (var e in Elements)
        {
            var index = e.Index;
            var nextIndex = Nodes[index].NextId;
            var prevIndex = Nodes[index].PreviousId;
            
            // We find all `Position` objects which represent intersections of element `e` against the other elements
            // in the contour.  Keep in mind that the `Position` results will reference the other element, so to
            // create the pairs we need to match the positions against `e`.
            
            // By definition, an element can't intersect itself.  However, an element will intersect its neigbors at 
            // its endpoints, so we need to exclude intersections where endpoints are involved.
            var intersections = new List<Position>();
            foreach (var p in Bvh.Intersections(e))
            {
                if (p.Element.Index == index) continue;
                
                if (p.Element.Index == nextIndex && p.L < GeometryConstants.DistEquals) continue;
                
                if (p.Element.Index == prevIndex && p.L > p.Element.Length - GeometryConstants.DistEquals) continue;
                
                intersections.Add(p);
            }
            
            results.AddRange(e.MatchIntersections(intersections));
        }
        
        // Finally we will have to individualize the results, since theoretically each intersection should have 
        // appeared twice in our list.
        // TODO: Optimize this, for now it's brute force
        var uniqueResults = new List<IntersectionPair>();
        foreach (var pair in results)
        {
            if (!uniqueResults.Any(p => p.IsEquivalentTo(pair))) uniqueResults.Add(pair);
        }
        
        return uniqueResults.ToArray();
    }

    /// <summary>
    /// Splits a contour at a self-intersection. The intersection pair must be a self-intersection of the contour, and
    /// its elements must be elements in this contour, or undefined behavior may occur.
    /// </summary>
    /// <param name="split"></param>
    /// <returns></returns>
    [Pure]
    public (BoundaryLoop, BoundaryLoop) SplitAtSelfIntersection(IntersectionPair split)
    {
        /* Splitting at self-intersection
         * 
         * At any self intersection, we will have two elements which intersect. We will trace out the two loops formed
         * by the split by tracing our way along the contour from the intersection point for both the first and
         * second element.  Each contour ends when it reaches the other element's equivalent intersection point.
         */
        
        // We'll start by the `First` element
        var c0 = ExtractLoopFromTo(split.First.Element, split.First.L, split.Second.Element, split.Second.L);
        var c1 = ExtractLoopFromTo(split.Second.Element, split.Second.L, split.First.Element, split.First.L);
        
        return (c0, c1);
    }

    /// <summary>
    /// Decompose this contour into a set of non-self-intersecting loops.  This will return an array of `Contour`
    /// objects, each of which will be a non-self-intersecting loop.  If the contour is already non-self-intersecting,
    /// it will return an array with a single element, which is the original contour.  If the contour *is* self
    /// intersecting, the result will be a set of completely new contours, each with a new identifier.
    /// </summary>
    /// <returns></returns>
    [Pure]
    public BoundaryLoop[] NonSelfIntersectingLoops()
    {
        var final = new List<BoundaryLoop>();
        var working = new List<BoundaryLoop> {this};
        
        while (working.Count > 0)
        {
            var current = working[0];
            working.RemoveAt(0);
            
            var intersections = current.SelfIntersections();
            if (intersections.Length == 0)
            {
                final.Add(current);
            }
            else
            {
                var (c0, c1) = current.SplitAtSelfIntersection(intersections.First());
                working.Add(c0);
                working.Add(c1);
            }
        }
        
        return final.ToArray();
    }

    /// <summary>
    /// Traces out a new contour starting at `l0` on `e0` and ending at `l1` on `e1`.  The new contour will have a new
    /// identifier and the entity integer IDs will be different from the original.
    /// </summary>
    /// <param name="e0">The element to begin the new contour at</param>
    /// <param name="l0">The distance at which to begin on the first element</param>
    /// <param name="e1">The element to end the new contour on</param>
    /// <param name="l1">THe distance at which to end on the end element</param>
    /// <returns>A new contour</returns>
    [Pure]
    private BoundaryLoop ExtractLoopFromTo(IBoundaryElement e0, double l0, IBoundaryElement e1, double l1)
    {
        var contour = new BoundaryLoop();
        var write = contour.GetCursor();
        var read = GetCursor(e0.Index);
        
        // Add the first element's portion after the intersection
        write.InsertFromElement(e0.SplitAfter(l0));
        read.MoveForward();
        
        // Now we will iterate forward until the read cursor reaches the second element's index
        while (read.CurrentId != e1.Index)
        {
            write.InsertAfter(read.Current);
            read.MoveForward();
        }
        
        // Now we add the second element's portion before the intersection
        write.InsertFromElement(e1.SplitBefore(l1));

        return contour;
    }
    
    // ==============================================================================================================
    // Management methods
    // ==============================================================================================================

    public override BoundaryLoop Copy()
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        foreach (var item in IterItems())
        {
            cursor.InsertAfter(item.Item.Copy());
        }

        return contour;
    }

    public override IBoundaryLoopCursor GetCursor(int? id = null)
    {
        return new BoundaryLoopCursor(this, id ?? GetTailId());
    }

    public override void OnItemChanged(BoundaryPoint item)
    {
        ResetCachedValues();
        base.OnItemChanged(item);
    }

    private void ResetCachedValues()
    {
        _area = double.NaN;
        _bvh = null;
        _elements = null;
    }

    /// <summary>
    /// Calculate the area of the contour using the shoelace formula.
    /// </summary>
    /// <returns></returns>
    private double CalculateArea()
    {
        double area = 0.0;
        foreach (var e in Elements)
        {
            area += e.CrossProductWedge;
        }
        
        return area / 2.0;
    }

    /// <summary>
    /// Build the internal geometric entities from the contour points.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private List<IBoundaryElement> BuildElements()
    {
        var elements = new List<IBoundaryElement>();
        foreach (var (a, b) in IterEdges())
        {
            IBoundaryElement e = a.Item switch
            {
                BoundaryLine line => new Segment(line.Point, b.Item.Point, a.Id),
                BoundaryArc arc => Arc.FromEnds(arc.Point, b.Item.Point, arc.Center, arc.Clockwise, a.Id),
                _ => throw new NotImplementedException()
            };
            elements.Add(e);
        }

        return elements;
    }

    public string Serialize()
    {
        return string.Join(";", IterItems().Select(i => i.Item.Serialize()));
    }

    protected (IBoundaryElement, IBoundaryElement) ElementsAtVertex(Point2D vertex)
    {
        // TODO: this can me made way more efficient
        foreach (var e in Elements)
        {
            if (e.End.DistanceTo(vertex) < GeometryConstants.DistEquals)
            {
                var next = Elements.First(x => x.Index == NextId(e.Index));
                return (e, next);
            }
        }
        
        throw new ArgumentException("Vertex not found in contour");
    }
    
    
    // ==============================================================================================================
    // Contour Cursor
    // ==============================================================================================================
    
    private class BoundaryLoopCursor : LoopCursor, IBoundaryLoopCursor
    {
        public BoundaryLoopCursor(Loop<BoundaryPoint> loop, int nodeId) : base(loop, nodeId) { }

        protected override void OnItemAdded(BoundaryPoint item, int id)
        {
            base.OnItemAdded(item, id);
        }

        public int SegAbs(double x, double y)
        {
            return InsertAfter(new BoundaryLine(new Point2D(x, y)));
        }
        
        public int ArcAbs(double x, double y, double cx, double cy, bool cw)
        {
            return InsertAfter(new BoundaryArc(new Point2D(x, y), new Point2D(cx, cy), cw));
        }
        
        public int SegRel(double x, double y)
        {
            var previous = Loop.Count == 0 ? new Point2D(0, 0) : Current.Point;
            var pv = previous + new Vector2D(x, y);
            return SegAbs(pv.X, pv.Y);
        }
        
        public int ArcRel(double x, double y, double cx, double cy, bool cw)
        {
            var previous = Loop.Count == 0 ? new Point2D(0, 0) : Current.Point;
            var pv = previous + new Vector2D(x, y);
            var cv = previous + new Vector2D(cx, cy);
            return ArcAbs(pv.X, pv.Y, cv.X, cv.Y, cw);
        }

        public int? InsertFromElement(IBoundaryElement? element)
        {
            if (element == null) return null;
            return element switch
            {
                Segment s => SegAbs(s.Start.X, s.Start.Y),
                Arc a => ArcAbs(a.Start.X, a.Start.Y, a.Center.X, a.Center.Y, !a.IsCcW),
                _ => throw new NotImplementedException()
            };
        }
    }
    
    // ==============================================================================================================
    // Static convenience builder methods
    // ==============================================================================================================

    /// <summary>
    /// Create a new contour consisting of a single arc which is a full circle, starting and ending at the same point.
    /// Specify the radius of the circle and its center in absolute coordinates.  A clockwise arc implies a negative
    /// area, while a counterclockwise arc implies a positive area.
    /// </summary>
    /// <param name="cx">The x center of the circle in absolute coordinates</param>
    /// <param name="cy">The y center of the circle in absolute coordinates</param>
    /// <param name="radius">The radius of the circle to create</param>
    /// <param name="cw">True for a clockwise (negative) circle, false for a counter-clockwise (positive) circle</param>
    /// <returns>A newly created `Contour` object</returns>
    public static BoundaryLoop Circle(double cx, double cy, double radius, bool cw = false)
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        cursor.ArcAbs(cx + radius, cy, cx, cy, cw);
        return contour;
    }
    
    /// <summary>
    /// Create a new rectangular contour with the lower-left corner at (x, y) and the width and height specified.
    /// </summary>
    /// <param name="x">The x position of the lower-left corner in absolute coordinates</param>
    /// <param name="y">The y position of the lower-left corner in absolute coordinates</param>
    /// <param name="w">The width of the rectangle</param>
    /// <param name="h">The height of the rectangle</param>
    /// <returns>A newly created `Contour` object</returns>
    public static BoundaryLoop Rectangle(double x, double y, double w, double h)
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        cursor.SegAbs(x, y);
        cursor.SegRel(w, 0);
        cursor.SegRel(0, h);
        cursor.SegRel(-w, 0);
        return contour;
    }
    
    /// <summary>
    /// Create a new rectangular contour with the center at (cx, cy) and the width and height specified.
    /// </summary>
    /// <param name="cx">The x position of the rectangle center in absolute coordinates</param>
    /// <param name="cy">The y position of the rectangle center in absolute coordinates</param>
    /// <param name="w">The width of the rectangle</param>
    /// <param name="h">The height of the rectangle</param>
    /// <returns>A newly created `Contour` object</returns>
    public static BoundaryLoop CenteredRectangle(double cx, double cy, double w, double h)
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        cursor.SegAbs(cx - w / 2, cy - h / 2);
        cursor.SegRel(w, 0);
        cursor.SegRel(0, h);
        cursor.SegRel(-w, 0);
        return contour;
    }

    public static BoundaryLoop Polygon(IEnumerable<Point2D> points)
    {
        var contour = new BoundaryLoop();
        var cursor = contour.GetCursor();
        foreach (var p in points)
        {
            if (contour.Count == 0 || cursor.Current.Point.DistanceTo(p) > GeometryConstants.DistEquals)
                cursor.SegAbs(p.X, p.Y);
        }
        
        // Check if the last point is the same as the first point, and if so, remove it
        if (contour.Head.Point.DistanceTo(contour.Tail.Point) < GeometryConstants.DistEquals)
        {
            cursor.Remove();
        }
        
        return contour;
    }

}