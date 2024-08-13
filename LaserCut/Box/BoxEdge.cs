using LaserCut.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Box;

public class BoxEdge
{
    private readonly Point3D[] _envelope;
    private readonly Point3D[] _common;
    private readonly BoxParams _boxParams;
    private BoxEdgeCursor? _commonCursor;
    private BoxEdgeCursor? _sharedCursor;

    public BoxEdge(BoxFace face, int startIndex, int endIndex, Point3D[] envelope, Point3D[] common, BoxParams boxParams)
    {
        Face = face;
        StartIndex = startIndex;
        EndIndex = endIndex;
        _envelope = envelope;
        _common = common;
        _boxParams = boxParams;
        Key = IntKeys.MakeOrdered(StartIndex, EndIndex);
        KeyIsFirst = StartIndex < EndIndex;
    }

    public int StartIndex { get; }

    public int EndIndex { get; }

    public BoxFace Face { get; }

    /// <summary>
    /// Gets a common ulong key for the edge, based on the indices of the start and end vertices.  The key is the same
    /// for edges that share the same vertices, regardless of the order of the vertices.
    /// </summary>
    public ulong Key { get; }

    /// <summary>
    /// Get a flag indicating whether the start index is less than the end index.  This is useful for identifying which
    /// of the two edges that share the same indices is the "first" edge.  For instance, for two edges which connect
    /// vertices 0 and 1, the "first" edge is 0 -> 1 and the "second" edge is 1 -> 0.
    /// </summary>
    public bool KeyIsFirst { get; }

    /// <summary>
    /// Get the edge on the same face which follows this edge. This is the edge that starts at the same vertex that
    /// this edge ends at.
    /// </summary>
    public BoxEdge Next { get; private set; }

    /// <summary>
    /// Get the edge on the same face which precedes this edge. This is the edge that ends at the same vertex that this
    /// edge starts at.
    /// </summary>
    public BoxEdge Previous { get; private set; }

    /// <summary>
    /// Get the edge on the adjacent face which shares the same vertices as this edge.  This is the edge that starts at
    /// the same vertex that this edge ends at and vice versa.
    /// </summary>
    public BoxEdge Neighbor { get; private set; }

    public BoxEdgeCursor EnvelopeCursor { get; private set; }

    public BoxEdgeCursor SharedCursor => _sharedCursor ??= MakeSharedCursor();

    public bool HasPriority => Face.Priority > Neighbor.Face.Priority;

    /// <summary>
    /// Set the first and second edges that follow this edge on the same face.  This must be performed before
    /// Initialize is called.
    /// </summary>
    public void SetAdjacent(BoxEdge previous, BoxEdge next)
    {
        Next = next;
        Previous = previous;
    }

    /// <summary>
    /// Set the edge on the adjacent face which shares the same vertices as this edge.  This must be performed before
    /// Initialize is called.
    /// </summary>
    /// <param name="neighbor"></param>
    public void SetNeighbor(BoxEdge neighbor)
    {
        Neighbor = neighbor;
    }

    /// <summary>
    /// Generate the geometry constructions for this edge.  This must be called before any of the edges are used.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Initialize()
    {
        if (Next == null || Previous == null || Neighbor == null)
        {
            throw new InvalidOperationException("Adjacent edges must be set before initializing.");
        }

        // Set the envelope edge cursor
        EnvelopeCursor = ActualCursor(_envelope);
        _commonCursor = ActualCursor(_common);

    }

    /// <summary>
    /// Given a y position on the shared cursor of this edge's neighbor, get the corresponding y position on this edge's
    /// shared cursor.  This will only function correctly if the edges have been initialized.
    /// </summary>
    /// <param name="neighborY"></param>
    /// <returns></returns>
    public double SharedY(double neighborY)
    {
        var n = Neighbor.SharedCursor.EdgeToWorld(new Point2D(0, neighborY));
        var l = SharedCursor.WorldToEdge(n);

        return l.Y;
    }

    private BoxEdgeCursor ActualCursor(Point3D[] boxVertices)
    {
        var theory = new BoxEdgeCursor(Face.WorldToFace(boxVertices[StartIndex]),
            Face.WorldToFace(boxVertices[EndIndex]), Face);

        var sx = HasPriority ? 0 : -_boxParams.Thickness;
        var sy0 = Previous.HasPriority ? 0 : _boxParams.Thickness;
        var sy1 = Next.HasPriority ? 0 : -_boxParams.Thickness;
        var p0 = new Point2D(sx, sy0);
        var p1 = new Point2D(sx, theory.Length + sy1);

        var start = theory.EdgeToFace(p0);
        var end = theory.EdgeToFace(p1);

        return new BoxEdgeCursor(start, end, Face);
    }

    private BoxEdgeCursor MakeSharedCursor()
    {
        if (Neighbor?._commonCursor == null || _commonCursor == null)
        {
            throw new InvalidOperationException("Edges must be initialized before shared cursor can be created.");
        }

        var nStart = _commonCursor.WorldToEdge(Neighbor._commonCursor.EndWorld);
        var nEnd = _commonCursor.WorldToEdge(Neighbor._commonCursor.StartWorld);

        var startY = Math.Max(0, nStart.Y);
        var endY = Math.Min(_commonCursor.Length, nEnd.Y);

        var startFace = _commonCursor.EdgeToFace(new Point2D(0, startY));
        var endFace = _commonCursor.EdgeToFace(new Point2D(0, endY));
        return new BoxEdgeCursor(startFace, endFace, Face);
    }

}
