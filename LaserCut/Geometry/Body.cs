using System.Diagnostics;
using LaserCut.Algorithms;
using LaserCut.Geometry.Primitives;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

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

    public IEnumerable<BoundaryLoop> AllLoops => new[] {Outer}.Concat(Inners);

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

    public bool Encloses(Point2D point)
    {
        return Outer.Encloses(point) && Inners.All(i => !i.Encloses(point));
    }

    /// <summary>
    /// Produces an equivalent, single-loop representation of the body by joining any holes to the outer loop at the
    /// closest point. This creates a loop that has self-intersecting edges, where one edge goes from the outer loop
    /// to the hole, and then another edge goes from the hole back to the outer loop.  This is useful for filled
    /// polygons.
    /// </summary>
    /// <returns></returns>
    public BoundaryLoop ToSingleLoop()
    {
        var working = Outer.Copy();
        var remainingHoles = Inners.ToList();
        while (remainingHoles.Count > 0)
        {
            var closest = remainingHoles.MinBy(loop => loop.ClosestNode(working).Item1);
            remainingHoles.Remove(closest!);

            var (_, nodeId, workingPosition) = closest!.ClosestNode(working);
            var insertPoint = workingPosition.Surface.Point;

            // We'll create the write cursor and insert the element which will finish the splice, then we'll back
            // up so that we're back at correct insertion point.  We'll do this now because we need to know the
            // element type under the cursor.
            var write = working.GetCursor(workingPosition.Element.Index);
            switch (write.Current)
            {
                case BoundaryLine line:
                    write.SegAbs(insertPoint);
                    break;
                case BoundaryArc arc:
                    write.ArcAbs(insertPoint, arc.Center, arc.Clockwise);
                    break;
                default:
                    throw new ArgumentException("Unexpected boundary element type");
            }

            write.MoveBackward();

            // First we insert a segment from the outer loop to the hole boundary
            write.SegAbs(insertPoint);

            // Now we will create the read cursor on the hole boundary.  We will read and write until we get back to
            // the first node
            var read = closest.GetCursor(nodeId);
            write.InsertAfter(read.Current);
            read.MoveForward();

            while (read.CurrentId != nodeId)
            {
                write.InsertAfter(read.Current);
                read.MoveForward();
            }

            // Now we insert a segment back to the outer loop
            write.SegAbs(read.Current.Point);
        }

        return working;
    }
}
