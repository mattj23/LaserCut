using System.Diagnostics;
using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;
using MathNet.Numerics.LinearAlgebra.Double;

namespace LaserCut.Geometry;

/// <summary>
/// Represents a single geometric body, consisting of one single outer boundary and zero or more inner boundaries.
/// </summary>
public class Body : IHasBounds
{
    
    public Body(Guid id, BoundaryLoop outer, List<BoundaryLoop> inners)
    {
        Outer = outer;
        Inners = inners;
        Id = id;
    }
    
    public Body(Guid id) : this(id, new BoundaryLoop(), new List<BoundaryLoop>()) { }

    public Body(BoundaryLoop outer) : this(Guid.NewGuid(), outer, new List<BoundaryLoop>()) {}
    
    public Body(BoundaryLoop outer, List<BoundaryLoop> inners) : this(Guid.NewGuid(), outer, inners) { }
    
    public Body() : this(new BoundaryLoop(), new List<BoundaryLoop>()) { }
    
    public Guid Id { get; } 
    
    public BoundaryLoop Outer { get; private set; }
    public List<BoundaryLoop> Inners { get; }
    
    public Aabb2 Bounds => Outer.Bounds;
    
    public double Area => Outer.Area + Inners.Sum(i => i.Area);
    
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
    
    public Body MirroredY()
    {
        var temp = Copy();
        temp.MirrorY();
        
        return new Body(temp.Outer.Reversed(), temp.Inners.Select(x => x.Reversed()).ToList());
    }

    public Body Copy()
    {
        return new Body(Outer.Copy(), Inners.Select(i => i.Copy()).ToList());
    }
    
    public Body Copy(Guid id)
    {
        return new Body(id, Outer.Copy(), Inners.Select(i => i.Copy()).ToList());
    }

    public override string ToString()
    {
        return $"[Body: Area={Area:F2}, {Inners.Count} Holes, {Bounds.Height:F2}H x {Bounds.Width:F2}W]";
    }

    // public Body OffsetAndFixed(double offset)
    // {
    //     var outer = Outer.OffsetAndRepaired(offset);
    //     if (outer.Count != 1) throw new InvalidOperationException("Expected a single loop");
    //
    //     var working = new Body(outer[0]);
    //     foreach (var inner in Inners)
    //     {
    //         var loops = inner.OffsetAndFixed(offset);
    //         foreach (var loop in loops)
    //         {
    //             working.Operate(loop);
    //         }
    //     }
    //     
    //     return working;
    // }
    //
    // public PointLoop ToSingleLoop()
    // {
    //     var loop = Outer.Copy();
    //
    //     foreach (var inner in Inners)
    //     {
    //         // Find the closest point on the inner loop to the outer loop
    //         var (c0, c1) = loop.ClosestVertices(inner);
    //         var cursor = loop.GetCursor(c0);
    //         var outerInsertion = cursor.Current;
    //         var innerInsertion = inner.GetCursor(c1).Current;
    //         foreach (var item in inner.IterItems(c1))
    //         {
    //             cursor.InsertAbs(item.Item);
    //         }
    //
    //         cursor.InsertAbs(innerInsertion);
    //         cursor.InsertAbs(outerInsertion);
    //     }
    //     
    //     return loop;
    // }

    /// <summary>
    /// Perform a shape operation with a tool contour. The tool contour can be either positive or negative, and the
    /// result will be a new set of bodies which are the result of the operation.
    /// </summary>
    /// <param name="tool"></param>
    // public Body[] Operate(BoundaryLoop tool)
    // {
    //     if (tool.IsPositive)
    //     {
    //         return this.OperatePositive(tool);
    //     }
    //     else
    //     {
    //         return this.OperateNegative(tool);
    //     }
    // }
}