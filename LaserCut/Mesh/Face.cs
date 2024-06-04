namespace LaserCut.Mesh;

public struct Face
{
    public uint A;
    public uint B;
    public uint C;
    
    public Face(uint a, uint b, uint c)
    {
        A = a;
        B = b;
        C = c;
    }
    
    public Edge EdgeA => new Edge(A, B);
    public Edge EdgeB => new Edge(B, C);
    public Edge EdgeC => new Edge(C, A);
    
    public bool HasRepeatedVertex => A == B || B == C || C == A;
}