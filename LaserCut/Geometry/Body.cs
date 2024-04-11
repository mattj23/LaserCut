using LaserCut.Geometry.Primitives;
using MathNet.Numerics.LinearAlgebra.Double;

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
    
    public double Area => Outer.Area - Inners.Sum(i => i.Area);
    
    public void Transform(Matrix t)
    {
        Outer.Transform(t);
        foreach (var inner in Inners)
        {
            inner.Transform(t);
        }
    }
    
    public void MirrorX(double x = 0)
    {
        Outer.MirrorX(x);
        foreach (var inner in Inners)
        {
            inner.MirrorX(x);
        }
    }
    
    public void MirrorY(double y = 0)
    {
        Outer.MirrorY(y);
        foreach (var inner in Inners)
        {
            inner.MirrorY(y);
        }
    }

    public void FlipX()
    {
        MirrorX(Bounds.Center.X);
    }
    
    public void FlipY()
    {
        MirrorY(Bounds.Center.Y);
    }

    public PointLoop ToSingleLoop()
    {
        var loop = Outer.Copy();

        foreach (var inner in Inners)
        {
            // Find the closest point on the inner loop to the outer loop
            var (c0, c1) = loop.ClosestVertices(inner);
            var cursor = loop.GetCursor(c0);
            var outerInsertion = cursor.Current;
            var innerInsertion = inner.GetCursor(c1).Current;
            foreach (var item in inner.IterItems(c1))
            {
                cursor.InsertAbs(item.Item);
            }

            cursor.InsertAbs(innerInsertion);
            cursor.InsertAbs(outerInsertion);
        }
        
        return loop;
    }
}