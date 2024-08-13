namespace LaserCut.Box;

/// <summary>
/// Contains the indices of the vertices that make up a face.
/// </summary>
public readonly record struct FaceIndices(int A, int B, int C, int D)
{
    public static FaceIndices Front => new(0, 1, 2, 3);
    public static FaceIndices Right => new(1, 5, 6, 2);
    public static FaceIndices Back => new(5, 4, 7, 6);
    public static FaceIndices Left => new(4, 0, 3, 7);
    public static FaceIndices Top => new(6, 7, 3, 2);
    public static FaceIndices Bottom => new(4, 5, 1, 0);
}
