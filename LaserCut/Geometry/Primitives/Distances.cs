using LaserCut.Algorithms;

namespace LaserCut.Geometry.Primitives;

/// <summary>
/// This class contains the measurement methods for distances between elements
/// </summary>
public static class Distances
{

    private static (double, Position, Position) MinFrom(IEnumerable<(Position, Position)> pairs)
    {
        var items = pairs.Select(x => (x.Item1.DistanceTo(x.Item2), x.Item1, x.Item2));
        return items.MinBy(x => x.Item1);
    }

    public static (double, Position, Position) Closest(Segment a, Segment b)
    {
        return MinFrom([
            (new Position(0, a), b.Closest(a.Start)),
            (new Position(1, a), b.Closest(a.End)),
            (a.Closest(b.Start), new Position(0, b)),
            (a.Closest(b.End), new Position(1, b))
        ]);
    }
    
    public static (double, Position, Position) Closest(Arc a, Segment b)
    {
        // Point of the 
        return MinFrom([
            (new Position(0, a), b.Closest(a.Start)),
            (new Position(1, a), b.Closest(a.End)),
            (a.Closest(b.Start), new Position(0, b)),
            (a.Closest(b.End), new Position(1, b))
        ]);
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