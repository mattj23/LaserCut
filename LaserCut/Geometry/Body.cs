using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;
using MathNet.Numerics.LinearAlgebra.Double;

namespace LaserCut.Geometry;

/// <summary>
/// Represents a single geometric body, consisting of one single outer boundary and zero or more inner boundaries.
/// </summary>
public class Body : IHasBounds
{
    
    public Body(Guid id, PointLoop outer, List<PointLoop> inners)
    {
        Outer = outer;
        Inners = inners;
        Id = id;
    }
    
    public Body(Guid id) : this(id, new PointLoop(), new List<PointLoop>()) { }
    
    public Body(PointLoop outer, List<PointLoop> inners) : this(Guid.NewGuid(), outer, inners) { }
    
    public Body() : this(new PointLoop(), new List<PointLoop>()) { }
    
    public Guid Id { get; } 
    
    public PointLoop Outer { get; private set; }
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

    /// <summary>
    /// Rotate the body around a specified point
    /// </summary>
    /// <param name="degrees"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RotateAround(double degrees, double x, double y)
    {
        // First shift to the origin, then rotate, then shift back
        var t = Isometry2.Translate(x, y) * Isometry2.Rotate(degrees) * Isometry2.Translate(-x, -y);
        Transform((Matrix)t);
    }
    
    public void Translate(double x, double y)
    {
        Transform(Isometry2.Translate(x, y));
    }

    /// <summary>
    /// Rotate the body around its center
    /// </summary>
    /// <param name="degrees"></param>
    public void Rotate(double degrees)
    {
        RotateAround(degrees, Bounds.Center.X, Bounds.Center.Y);
    }

    public void FlipX()
    {
        MirrorX(Bounds.Center.X);
    }
    
    public void FlipY()
    {
        MirrorY(Bounds.Center.Y);
    }

    public Body Copy()
    {
        return new Body(Outer.Copy(), Inners.Select(i => i.Copy()).ToList());
    }
    
    public Body Copy(Guid id)
    {
        return new Body(id, Outer.Copy(), Inners.Select(i => i.Copy()).ToList());
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

    /// <summary>
    /// This performs a *simple* boolean merge operation with a shape defined by a tool loop. If the tool has positive
    /// area, the result will be a union operation. If the tool has negative area, the result will be a cut operation.
    /// This is a simplified version of the more general boolean shape merge, and requires the following conditions for
    /// the result to be correct: (1) the tool must either intersect with tbe body's outer loop or be contained within
    /// it, and (2) if the tool is a negative area it must not split the body into multiple bodies. If either of these
    /// conditions cannot be guarenteed, use the more general boolean merge operation.
    /// </summary>
    /// 
    /// <remarks>
    /// Internally, first the tool is intersected with the outer loop of the body. If the two do not intersect at all,
    /// no change is made to the outer body.
    ///
    /// If the tool is a positive area, the tool is merged individually with each inner loop. If the tool contains the
    /// inner loop, the inner loop is removed from the body. Otherwise, the inner loop is replaced with the results of
    /// the merge.
    ///
    /// If the tool is a negative area, and it did intersect with the outer loop of the body, each of the inner loops is
    /// merged with the body's outer loop. If the tool did not intersect with the outer loop, then each 
    /// 
    /// </remarks>
    /// 
    /// <param name="tool"></param>
    public void SimpleBooleanMerge(PointLoop tool)
    {
        Outer = ShapeOperation.SimpleMergeLoops(Outer, tool);

    }
}