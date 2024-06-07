using System.Diagnostics.Contracts;
using LaserCut.Algorithms;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public abstract record ContourPoint(Point2D Point);

public record ContourLine(Point2D Point) : ContourPoint(Point);

public record ContourArc(Point2D Point, Point2D Center, bool Clockwise) : ContourPoint(Point);

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
public class Contour : Loop<ContourPoint>
{
    private Bvh? _bvh = null;
    private List<IContourElement>? _elements = null;
    private double _area = double.NaN;
    
    /// <summary>
    /// Create a new contour with a unique identifier.
    /// </summary>
    /// <param name="id"></param>
    public Contour(Guid id) { Id = id; }
    
    /// <summary>
    /// Create a new contour. A unique identifier will be generated automatically.
    /// </summary>
    public Contour() : this(Guid.NewGuid()) { }

    /// <summary>
    /// Create a new contour with a unique identifier and a list of initial entities.
    /// </summary>
    /// <param name="id">The uuid to assign to this contour</param>
    /// <param name="entities">A list of initial entities which will be added to the contour at creation</param>
    public Contour(Guid id, IEnumerable<ContourPoint> entities)
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
    public Contour(IEnumerable<ContourPoint> entities) : this(Guid.NewGuid(), entities) { }
        
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
    public IReadOnlyList<IContourElement> Elements => _elements ??= BuildElements();
    
    /// <summary>
    /// Gets the bounding volume hierarchy for the contour for accelerated geometric operations.  This will trigger
    /// the construction of the geometric elements.  If the contour is in an invalid state, an exception will be thrown.
    /// </summary>
    public Bvh Bvh => _bvh ??= new Bvh(Elements);
    
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

    // ==============================================================================================================
    // Geometric operations
    // ==============================================================================================================

    /// <summary>
    /// Generate a new contour which is the same as this contour but with the direction of entities reversed. The new
    /// contour will have a new unique identifier and the entity integer IDs will be different from the original. While
    /// the location of the boundary will be the same, the "direction" of the boundary will invert.  Contours which
    /// represent a positive area will become negative, and vice versa.
    /// </summary>
    /// <returns>A new `Contour` entity with a new id.</returns>
    [Pure]
    public Contour Reversed()
    {
        var elements = new List<ContourPoint>();
        
        // We will iterate through each entity in the contour in forward order and create its replacement.  Because the
        // entity is actually representing the definition of the border between itself and the next entity, each entity
        // type will remain the same, but the starting points will be changed. On arcs, the center will remain the same
        // but the direction will be reversed (clockwise vs counterclockwise).
        foreach (var (a, b) in IterEdges())
        {
            ContourPoint e = a.Item switch
            {
                ContourLine _ => new ContourLine(b.Item.Point),
                ContourArc arc => new ContourArc(b.Item.Point, arc.Center, !arc.Clockwise),
                _ => throw new NotImplementedException()
            };
            elements.Add(e);
        }
        
        // At this point, we only need to reverse the list of elements.
        elements.Reverse();
        return new Contour(elements);
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
    /// Calculates all intersections between this contour and another contour, returning the results as an array of
    /// `IntersectionPairs`.  The `First` element of each pair will be the element from *this* contour, while the
    /// `Second` will be from the *other* contour. 
    /// </summary>
    /// <param name="other">The contour to test for intersections with</param>
    /// <returns>An array of intersection pairs where the first element is from *this* contour.</returns>
    public IntersectionPair[] IntersectionPairs(Contour other)
    {
        return Bvh.Intersections(other.Bvh);
    }
    
    /// <summary>
    /// Determines the intersection/spatial relationship between this contour and another contour. The relationship
    /// will be either that the contours are disjoint, one is enclosed by the other, one encloses the other, or they
    /// intersect in some way.
    ///
    /// The resulting enum value can be interpreted as a verb describing the relation of *this contour* to the
    /// *other contour*. For example, if the result is `EnclosedBy`, interpret it as "this contour is enclosed by the
    /// other contour".  If the result is `Encloses`, interpret it as "this contour encloses the other contour".
    ///
    /// Note that enclosure does not imply anything about whether the contours exist on the positive or negative side
    /// of the boundary.  To determine this, you must interpret the `IsPositive` property of each contour against the
    /// specific relationship.
    /// </summary>
    /// <param name="other">The other contour to test the relationship to</param>
    /// <returns>The relation of *this contour* to the *other contour*.</returns>
    public ContourRelation RelationTo(Contour other)
    {
        if (IntersectionPairs(other).Any()) return ContourRelation.Intersects;

        // Is the other loop enclosing this loop?
        if (other.Encloses(Head.Point)) return ContourRelation.EnclosedBy;
        
        // Is this loop enclosing the other loop?
        if (Encloses(other.Head.Point)) return ContourRelation.Encloses;

        return ContourRelation.DisjointTo;
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
    
    // ==============================================================================================================
    // Management methods
    // ==============================================================================================================
    
    public override IContourCursor GetCursor(int? id = null)
    {
        return new ContourCursor(this, id ?? GetTailId());
    }

    public override void OnItemChanged(ContourPoint item)
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
    private List<IContourElement> BuildElements()
    {
        var elements = new List<IContourElement>();
        foreach (var (a, b) in IterEdges())
        {
            IContourElement e = a.Item switch
            {
                ContourLine line => new Segment(line.Point, b.Item.Point, a.Id),
                ContourArc arc => Arc.FromEnds(arc.Point, b.Item.Point, arc.Center, arc.Clockwise, a.Id),
                _ => throw new NotImplementedException()
            };
            elements.Add(e);
        }

        return elements;
    }

    // ==============================================================================================================
    // Contour Cursor
    // ==============================================================================================================
    
    private class ContourCursor : LoopCursor, IContourCursor
    {
        public ContourCursor(Loop<ContourPoint> loop, int nodeId) : base(loop, nodeId) { }

        protected override void OnItemAdded(ContourPoint item, int id)
        {
            base.OnItemAdded(item, id);
        }

        public int SegAbs(double x, double y)
        {
            return InsertAfter(new ContourLine(new Point2D(x, y)));
        }
        
        public int ArcAbs(double x, double y, double cx, double cy, bool cw)
        {
            return InsertAfter(new ContourArc(new Point2D(x, y), new Point2D(cx, cy), cw));
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
    public static Contour Circle(double cx, double cy, double radius, bool cw = false)
    {
        var contour = new Contour();
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
    public static Contour Rectangle(double x, double y, double w, double h)
    {
        var contour = new Contour();
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
    /// <param name="x">The x position of the rectangle center in absolute coordinates</param>
    /// <param name="y">The y position of the rectangle center in absolute coordinates</param>
    /// <param name="w">The width of the rectangle</param>
    /// <param name="h">The height of the rectangle</param>
    /// <returns>A newly created `Contour` object</returns>
    public static Contour CenteredRectangle(double cx, double cy, double w, double h)
    {
        var contour = new Contour();
        var cursor = contour.GetCursor();
        cursor.SegAbs(cx - w / 2, cy - h / 2);
        cursor.SegRel(w, 0);
        cursor.SegRel(0, h);
        cursor.SegRel(-w, 0);
        return contour;
    }
    
}