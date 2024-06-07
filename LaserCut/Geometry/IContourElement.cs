using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public interface IContourElement
{
    int Index { get; }
    
    Point2D Start { get; }
    Point2D End { get; }
    
    Aabb2 Bounds { get; }
    
    double Length { get; }
    
    SurfacePoint AtLength(double length);

    Position Closest(Point2D point);
    
    /// <summary>
    /// Gets the cross product of the vectors between the origin and the end points of this element, the way that is
    /// performed when finding the area associated with a segment of a polygon
    /// </summary>
    double CrossProductWedge { get; }
    
    /// <summary>
    /// Update the index of this element.
    /// </summary>
    /// <param name="index">The new value of the index</param>
    void SetIndex(int index);
    
    /// <summary>
    /// Compute the intersection positions between this element and a line. The positions returned will reference *this*
    /// entity as the `Element` property.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    Position[] IntersectionsWithLine(Line2 line);

    /// <summary>
    /// Compute the intersection positions between this element and a circle. The positions returned will reference
    /// *this* entity as the `Element` property.
    /// </summary>
    /// <param name="circle"></param>
    /// <returns></returns>
    Position[] IntersectionsWithCircle(Circle2 circle);
    
    /// <summary>
    /// Given a collection of positions on another element, return an array of valid ElementIntersections where the
    /// positions exist on this element.  The position on this element will be the `First` property of the intersection,
    /// and the position which was matched will be the `Second` property.
    /// </summary>
    /// <param name="positions">An iterable of `Position` items representing candidate locations on another
    /// element.</param>
    /// <returns></returns>
    IntersectionPair[] MatchIntersections(IEnumerable<Position> positions);

    /// <summary>
    /// Determines if this element is followed by the given element, such that the end of this element is within the
    /// given tolerance of the start of the other element.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="tol">The max allowable between the end of this element and the start of the next. If no value
    /// is specified, the `GeometryConstants.DistEquals` is used.</param>
    /// <returns></returns>
    bool FollowedBy(IContourElement other, double? tol = null)
    {
        return End.DistanceTo(other.Start) <= (tol ?? GeometryConstants.DistEquals);
    }
    
    /// <summary>
    /// Determines if this element is preceded by the given element, such that the start of this element is within the
    /// given tolerance of the end of the other element.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="tol">The max allowable between the start of this element and the end of the next. If no value
    /// is specified, the `GeometryConstants.DistEquals` is used.</param>
    /// <returns></returns>
    bool PrecededBy(IContourElement other, double? tol = null)
    {
        return Start.DistanceTo(other.End) <= (tol ?? GeometryConstants.DistEquals);
    }
}