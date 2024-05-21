using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

public interface IContourElement
{
    Point2D Start { get; }
    Point2D End { get; }
    
    Aabb2 Bounds { get; }
    
    double Length { get; }
    
    SurfacePoint AtLength(double length);

    Position Closest(Point2D point);
    
    Position[] Intersections(Line2 line);

    Position[] Intersections(Circle2 circle);
    
    /// <summary>
    /// Given a collection of positions on another element, return an array of valid ElementIntersections where the
    /// positions exist on this element.  The position on this element will be the `First` property of the intersection,
    /// and the position which was matched will be the `Second` property.
    /// </summary>
    /// <param name="positions">An iterable of `Position` items representing candidate locations on another
    /// element.</param>
    /// <returns></returns>
    ElementIntersection[] MatchIntersections(IEnumerable<Position> positions);
}