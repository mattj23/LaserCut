using LaserCut.Algorithms;

namespace LaserCut.Geometry.Primitives;

/// <summary>
/// This class contains the measurement methods for distances between elements
/// </summary>
public static class Distances
{

    public static (double, Position, Position) Closest(Segment a, Segment b)
    {
        throw new NotImplementedException();
    }
    
    public static (double, Position, Position) Closest(Arc a, Segment b)
    {
        throw new NotImplementedException();
    }
    
    public static (double, Position, Position) Closest(Segment a, Arc b)
    {
        var (d, p0, p1) = Closest(b, a);
        return (d, p1, p0);
    }
    
    public static (double, Position, Position) Closest(Arc a, Arc b)
    {
        throw new NotImplementedException();
    }
}