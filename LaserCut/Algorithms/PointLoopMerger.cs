using LaserCut.Geometry;

namespace LaserCut.Algorithms;

public static class PointLoopMerge
{
    public static PointLoop Merged(PointLoop loop0, PointLoop loop1)
    {
        // This will return a list of intersection pairs between the two loops. For each intersection, segment 0 will
        // refer to the segment in loop0, and segment 1 will refer to the segment in loop1
        var intersections = loop0.Intersections(loop1);

        // If there are no intersections, we can just return loop0 as is
        if (intersections.Count == 0) return loop0.Copy();
        
        // First, we want to find an intersection pair where segment 0's direction has a positive dot product with 
        // segment 1's normal. This is an intersection where this loop is emerging from inside the other loop.
        var start = intersections.FirstOrDefault(i => i.Seg0ExitsSeg1);
        
        if (start.Segment0 == null)
            throw new InvalidOperationException("No starting intersection found");
        
        // Now we can remove the start from the list of intersections
        intersections.Remove(start);
        
        // We will create a new point loop and begin from the start intersection
        var working = new PointLoop();
        var workingCursor = working.GetCursor();
        workingCursor.InsertAbs(start.Point);

        // Now we will work our way through the loop.
        var lastT = start.T0;
        var readCursor = loop0.GetCursor(start.Segment0.Index);
        bool isLoop0 = true;

        while (intersections.Count != 0)
        {
            // Are there any intersections between where we are and the next segment?
            if (PopNext(intersections, lastT, isLoop0, readCursor.CurrentId) is { } next)
            {
                // If so, we will insert the intersection point into the working loop
                workingCursor.InsertAbs(next.Point);
                
                // Now we will switch to the other loop
                isLoop0 = !isLoop0;
                lastT = isLoop0 ? next.T0 : next.T1;
                readCursor = isLoop0 ? loop0.GetCursor(next.Segment0.Index) : loop1.GetCursor(next.Segment1.Index);
            }
            // Otherwise, we will advance to the start of the next segment and insert it into the loop
            else
            {
                readCursor.MoveForward();
                workingCursor.InsertAbs(readCursor.Current);
                lastT = 0;
            }
        }

        if (isLoop0) throw new InvalidOperationException("We somehow ended up on loop 0 at the end");
        
        // Finally, we will insert the remaining segments from loop1, until we would hit the index of seg 1 on start
        while (readCursor.CurrentId != start.Segment1.Index)
        {
            readCursor.MoveForward();
            workingCursor.InsertAbs(readCursor.Current);
        }

        working.RemoveAdjacentDuplicates();

        return working;
    }

    private static SegPairIntersection? PopNext(List<SegPairIntersection> intersections, double lastT, bool isLoop0, int currentId)
    {
        var more = isLoop0
            ? intersections.Where(i => i.Segment0.Index == currentId && i.T0 > lastT).ToList()
            : intersections.Where(i => i.Segment1.Index == currentId && i.T0 > lastT).ToList();
        
        if (more.Count == 0) return null;

        var next = isLoop0
            ? more.MinBy(i => i.T0)
            : more.MinBy(i => i.T1);

        intersections.Remove(next);
        return next;
    }
    
    
    
}