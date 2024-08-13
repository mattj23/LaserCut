using LaserCut.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Box;

public class BoxEdge
{
    private readonly Point3D[] _envelope;
    private readonly Point3D[] _common;
    private readonly BoxParams _boxParams;

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


    }

}
