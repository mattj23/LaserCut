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

    public Body(PointLoop outer) : this(Guid.NewGuid(), outer, new List<PointLoop>()) {}
    
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

    public Body OffsetAndFixed(double offset)
    {
        var outer = Outer.OffsetAndFixed(offset);
        if (outer.Count != 1) throw new InvalidOperationException("Expected a single loop");

        var working = new Body(outer[0]);
        foreach (var inner in Inners)
        {
            var loops = inner.OffsetAndFixed(offset);
            foreach (var loop in loops)
            {
                working.Operate(loop);
            }
        }
        
        return working;
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
    /// Perform a shape operation with a tool loop. The tool loop can be either positive or negative, and the body
    /// will be modified accordingly.
    /// </summary>
    /// <param name="tool"></param>
    public void Operate(PointLoop tool)
    {
        if (tool.Polarity is AreaPolarity.Negative)
        {
            OperateNegative(tool);
        }
        else
        {
            OperatePositive(tool);
        }
    }

    private void OperatePositive(PointLoop tool)
    {
        if (tool.Polarity is AreaPolarity.Negative)
            throw new InvalidOperationException("Cannot perform a positive operation with a negative tool");

        /*
         * In the case of a positive tool (union operation):
         * 1. Consider the operation with the outer loop:
         *  - If the two are completely disjoint, we should probably throw an error
         *  - If the outer loop is subsumed, the resulting body is the tool with no internal boundaries
         *  - The outer loop cannot be destroyed because the outer loop is positive and so is the tool
         *  - If the outer loop is merged with the tool, we replace the outer loop with the result
         *
         * 2. At this point we know that the tool protrudes into the body to some extent, and the outer loop may
         * have been modified but in doing so it will have grown and so cannot intersect with any of the existing
         * inner loops. The only thing that can happen at this point is that inner boundaries can be shrunk or
         * destroyed by the tool.  None of them will merge with each other as the result of a union, though they
         * can be split into multiple loops.
         * 3. We iterate through all the inner loops and merge them with the tool. They can be one of the following
         * results:
         *  - Disjoint: we leave them alone
         *  - Destroyed: we remove them from the inner loop
         *  - Merged: we replace them with the result of the merge
         *  - Shape encloses tool: this is the odd case, as it would theoretically result in the creation of a new
         *    body which does not intersect with this one.  In this case we will ignore it, considering it to be
         *    something which performs no modification to the body (technically at this point we should be able to
         *    simply return the original body unmodified)
         */
        
        var (outerResult, outerLoops) = ShapeOperation.Operate(Outer, tool);
        
        switch (outerResult)
        {
            case ShapeOperation.ResultType.Disjoint:
                throw new InvalidOperationException("The outer loop and the tool are completely disjoint");
            case ShapeOperation.ResultType.Destroyed:
                throw new InvalidOperationException("This shouldn't happen, as the body and tool should be positive");
            case ShapeOperation.ResultType.Merged:
                Outer = outerLoops[0];
                break;
            case ShapeOperation.ResultType.ShapeEnclosesTool:
                break;
            case ShapeOperation.ResultType.Subsumed:
                Outer = tool;
                Inners.Clear();
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        var working = new Queue<PointLoop>(Inners);
        Inners.Clear();
        
        while (working.TryDequeue(out var loop))
        {
            var (result, loops) = ShapeOperation.Operate(loop, tool);
            switch (result)
            {
                case ShapeOperation.ResultType.Disjoint or ShapeOperation.ResultType.ShapeEnclosesTool:
                    Inners.Add(loop);
                    break;
                case ShapeOperation.ResultType.Merged:
                    Inners.AddRange(loops);
                    break;
            }
        }
    }
    
    private void OperateNegative(PointLoop tool)
    {
        if (tool.Polarity is AreaPolarity.Positive)
            throw new InvalidOperationException("Cannot perform a negative operation with a positive tool");

        /*
         * In the case of a negative tool (cut operation):
         * 1. Consider the operation with the outer loop
         *  - If the two are completely disjoint, the entire body is unmodified
         *  - If the outer loop is completely destroyed, the body is empty, and we should throw an error
         *  - If there is a merge result with the outer loop, we replace the outer loop with the result
         *  - If the shape encloses the tool, the tool will become a new inner boundary
         *
         * 2. Now we consider a new body with no inner boundaries (except in the case of the enclosed one), but
         * we put all the old inner boundaries in a working list
         * 3. If the outer boundary changed, we iterate through all the old inner boundaries, and if any of them
         * intersect with the outer boundary we merge them with the outer boundary and remove them from the working
         * list.  We do this until there are none which merge with the outer boundary.
         * 4. At this point we know that no remaining inner boundaries will intersect with the outer boundary, and
         * there are either 0 or 1 inner boundaries already in the body.
         * 5. Now we pop a loop from the working list and check it against every inner boundary already in the
         * body.  If it merges or subsumes any of them we remove the inner boundary from the body and add the result
         * to the working list.  If it is enclosed by any of them, we discard it. Because it's a cut it will never
         * be destroyed by any of them.  If it makes it through the list then it is disjoint from all of them and
         * so it is added to the body.
         * 6. We repeat step 5 until the working list is empty.
         */
        var (outerResult, outerLoops) = ShapeOperation.Operate(Outer, tool);

        var working = new Queue<PointLoop>(Inners);
        switch (outerResult)
        {
            case ShapeOperation.ResultType.Disjoint:
                return;
            case ShapeOperation.ResultType.Destroyed:
                throw new InvalidOperationException("The body was completely destroyed by the tool");
            case ShapeOperation.ResultType.Merged:
                Outer = outerLoops[0];
                break;
            case ShapeOperation.ResultType.ShapeEnclosesTool:
                working.Enqueue(tool);
                break;
            case ShapeOperation.ResultType.Subsumed:
                throw new InvalidOperationException("This shouldn't happen, as the body should be positive and the tool negative");
            default:
                throw new ArgumentOutOfRangeException();
        }

        Inners.Clear();

        // This next section will merge the inner boundaries with the updated outer boundary, and remove any from
        // the working queue which end up being merged. This will continue until no more merges are possible.
        if (outerResult is ShapeOperation.ResultType.Merged)
        {
            // Transfer the whole working queue to a temporary queue
            var temp = new Queue<PointLoop>();
            working.TransferTo(temp);

            // Repeat until the temporary queue is empty
            while (temp.TryDequeue(out var testLoop))
            {
                // Try a merge operation on the outer loop. If it succeeds, replace the outer loop with the result
                // and transfer everything from working back into temp, otherwise (if it fails) add it back to the
                // working queue
                var (result, loops) = ShapeOperation.Operate(Outer, testLoop);
                if (result is ShapeOperation.ResultType.Merged)
                {
                    Outer = loops[0];
                    working.TransferTo(temp);
                }
                else
                {
                    working.Enqueue(testLoop);
                }
            }
        }

        // Now we know that none of the remaining inner boundaries will intersect with the outer boundary, so we
        // only need to merge them together
        while (working.TryDequeue(out var loop))
        {
            // From here, we will treat each inner loop from the working queue as a tool of its own and merge it 
            // into the existing inner boundaries.  We will compare the working loop with each inner loop.
            // The possible outcomes will be (1) disjoint, (2) subsumed, (3) merged, or (4) shape encloses tool. If
            // the existing inner boundary is subsumed or merged with the working inner boundary, we remove it
            // from the list and add the results to the working queue. If we find any existing inner boundary
            // encloses the working boundary, we can discard the working boundary. If we make it to the end without
            // any merges, we add the working boundary to the list.
            var discard = false;
            for (var i = 0; i < Inners.Count; i++)
            {
                var (result, loops) = ShapeOperation.Operate(Inners[i], loop);
                
                // If the existing inner boundary is subsumed by the working boundary we can remove it completely
                if (result is ShapeOperation.ResultType.Subsumed) Inners.RemoveAt(i);

                // If a merge occurs between the working loop and the existing loop, we place the merge result into
                // the working queue and discard both the working loop and the existing loop
                if (result is ShapeOperation.ResultType.Merged)
                {
                    if (loops.Length != 1) throw new InvalidOperationException($"Expected a single loop, got {loops.Length}");
                    
                    Inners.RemoveAt(i);
                    working.Enqueue(loops[0]);
                    discard = true;
                    break;
                }

                if (result is ShapeOperation.ResultType.ShapeEnclosesTool)
                {
                    // The existing inner boundary encloses the working boundary, so we discard the working boundary
                    discard = true;
                    break;
                }
            }

            if (!discard) Inners.Add(loop);
        }       
    }

}