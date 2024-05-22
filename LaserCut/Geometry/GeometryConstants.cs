namespace LaserCut.Geometry;

public static class GeometryConstants
{
    /// <summary>
    /// A small value used to determine if two distances are equal.
    /// </summary>
    public static double DistEquals = 1e-8;
    
    /// <summary>
    /// A numeric zero value used to determine if a number is effectively zero, used for comparisons such as with
    /// determinants and parallelism.
    /// </summary>
    public static double NumericZero = 1e-6;
    
    /// <summary>
    /// Gets the angle tolerance for determining if two angles are equal based on the `DistEquals` value, calculated
    /// by determining the angle value that, at the given radius, results in a distance equal to `DistEquals`.
    /// </summary>
    /// <param name="radius">The radius to evaluate the tolerance at</param>
    /// <returns>An angle in radians</returns>
    public static double AngleFromDist(double radius) => DistEquals / radius;
    
}