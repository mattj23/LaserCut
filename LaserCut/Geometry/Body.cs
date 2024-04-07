using LaserCut.Geometry.Primitives;

namespace LaserCut.Geometry;

/// <summary>
/// Represents a single geometric body, consisting of one single outer boundary and zero or more inner boundaries.
/// </summary>
public class Body
{
    public Body(PointLoop outer, List<PointLoop> inners)
    {
        Outer = outer;
        Inners = inners;
    }
    
    public Body() : this(new PointLoop(), new List<PointLoop>()) { }
    
    public PointLoop Outer { get; }
    public List<PointLoop> Inners { get; }
    
    public Aabb2 Bounds => Outer.Bounds;
}