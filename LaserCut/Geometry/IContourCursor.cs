using LaserCut.Algorithms.Loop;

namespace LaserCut.Geometry;

/// <summary>
/// This is a cursor that can be used to build and/or modify a contour.  It is a loop cursor that has a current
/// position in the loop of `ContourPoint` entities.  New entities can be inserted at the current position, the cursor
/// can be advanced or moved backwards, and entities can be removed.  The cursor can also be used to peek at neighboring
/// entities without moving.
/// </summary>
public interface IContourCursor : ILoopCursor<ContourPoint>
{
    /// <summary>
    /// Insert a segment entity that starts at the absolute position (x, y) into the contour.  It will be added *after*
    /// the current position of the cursor, and the cursor will advance so that the just-added segment is now the
    /// current cursor position.
    /// </summary>
    /// <param name="x">The x position in absolute coordinates</param>
    /// <param name="y">The y position in absolute coordinates</param>
    /// <returns>The integer ID of the entity created by this operation</returns>
    int SegAbs(double x, double y);

    /// <summary>
    /// Insert an arc entity that starts at the absolute position (x, y) into the contour.  The center coordinates are
    /// also specified in absolute coordinates.  The arc will be added *after* the current position of the cursor, and
    /// the cursor will advance so that the just-added arc is now the current cursor position.
    /// </summary>
    /// <param name="x">The x position of the arc start in absolute coordinates</param>
    /// <param name="y">The y position of the arc start in absolute coordinates</param>
    /// <param name="cx">The x position of the arc center in absolute coordinates</param>
    /// <param name="cy">The y position of the arc center in absolute coordinates</param>
    /// <param name="cw">If true, the arc will be clockwise, otherwise it will be counterclockwise. A counterclockwise
    /// arc implies the positive surface direction faces away from the center, a clockwise arc implies the positive
    /// surface direction faces towards the center.</param>
    /// <returns>The integer ID of the entity created by this operation</returns>
    int ArcAbs(double x, double y, double cx, double cy, bool cw);

    /// <summary>
    /// Insert a segment entity that starts at a position relative to the entity at the current cursor position.  The
    /// new entity will be added *after* the current position of the cursor, and the cursor will advance so that the
    /// just-added segment is now the current cursor position.
    ///
    /// If the contour is currently empty, the new segment will be added at the absolute position (x, y).
    /// </summary>
    /// <param name="x">The x position relative to the current cursor entity's start point</param>
    /// <param name="y">The y position relative to the current cursor entity's start point</param>
    /// <returns>The integer ID of the entity created by this operation</returns>
    public int SegRel(double x, double y);

    /// <summary>
    /// Insert an arc entity that starts at a position relative to the entity at the current cursor position. The center
    /// coordinates are also specified in the same relative coordinates. The arc will be added *after* the current
    /// position of the cursor, and the cursor will advance so that the just-added arc is now the current cursor
    /// position.
    ///
    /// If the contour is currently empty, the new arc will be added at the absolute position (x, y) and the center
    /// will also be in absolute coordinates. 
    /// </summary>
    /// <param name="x">The x position of the arc start relative to the current cursor entity's start position</param>
    /// <param name="y">The y position of the arc start relative to the current cursor entity's start position</param>
    /// <param name="cx">The x position of the arc center relative to the current cursor entity's start position</param>
    /// <param name="cy">The y position of the arc center relative to the current cursor entity's start position</param>
    /// <param name="cw">If true, the arc will be clockwise, otherwise it will be counterclockwise. A counterclockwise
    /// arc implies the positive surface direction faces away from the center, a clockwise arc implies the positive
    /// surface direction faces towards the center.</param>
    /// <returns>The integer ID of the entity created by this operation</returns>
    public int ArcRel(double x, double y, double cx, double cy, bool cw);

}