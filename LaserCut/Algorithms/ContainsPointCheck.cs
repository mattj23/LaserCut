using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

/// <summary>
/// This class packages up the logic for checking if a point is contained within a geometric entity based on
/// the intersections of a ray with the entity's boundaries. The number of unique entrances and exits of the entity
/// are counted, and if they are not equal, the point is considered to be contained within the entity.
///
/// There are challenges related to the potential for the ray to intersect the entity at a vertex, which would produce
/// multiple intersections some of which are unique and some of which are not. This is handled by the logic here
/// in this class.
/// </summary>
public static class ContainsPoint
{
    public static bool Check(Ray2 ray, Position[] positions)
    {
        // Ultimately, we need to count the number of times the ray crosses the entity's boundaries based off of the
        // total number of intersections. If the number of crossings is odd, the point is contained within the entity.
        // However, a crossing at a vertex is not a unique crossing, so we need to determine if each position is an
        // entrance or an exit, then strip non-unique entrances and exits from their respective lists.
        
        var entrances = new List<Position>();
        var exits = new List<Position>();

        foreach (var position in positions)
        {
            // Note that a position can be neither an entrance nor an exit if the ray direction and the surface normal
            // are very close to perpendicular. 
            var dot = ray.Direction.DotProduct(position.Surface.Normal);

            // A position is an exit if the ray direction and the surface normal are in the same direction 
            if (dot > GeometryConstants.DistEquals && !exits.Any(p =>
                    p.Surface.Point.DistanceTo(position.Surface.Point) <= GeometryConstants.DistEquals))
                exits.Add(position);
            
            // A position is an entrance if the ray direction and the surface normal are in opposite directions
            if (dot < -GeometryConstants.DistEquals && !entrances.Any(p =>
                    p.Surface.Point.DistanceTo(position.Surface.Point) <= GeometryConstants.DistEquals))
                entrances.Add(position);
        }
        
        // If the number of unique entrances and exits is not equal, the point is contained within the entity.
        return entrances.Count != exits.Count;
    }
    

}