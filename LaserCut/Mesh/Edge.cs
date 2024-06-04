namespace LaserCut.Mesh;

public readonly record struct Edge(uint A, uint B)
{
    // public uint A;
    // public uint B;
    //
    // public Edge(uint a, uint b)
    // {
    //     A = a;
    //     B = b;
    // }
    
    /// <summary>
    /// A key that identifies this edge based on the indices of its vertices but not the order. An edge from A to B
    /// will have the same Key value as an edge from B to A.
    /// </summary>
    public ulong Key => (ulong)Math.Min(A, B) << 32 | Math.Max(A, B);
    
    public bool IsEmpty => A == 0 && B == 0;
    
    public static Edge Empty => new(0, 0);
}