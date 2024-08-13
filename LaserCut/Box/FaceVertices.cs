using MathNet.Spatial.Euclidean;

namespace LaserCut.Box;

public class FaceVertices
{
    private readonly Point3D[] _vertices;
    private readonly FaceIndices _indices;

    public FaceVertices(Point3D[] vertices, FaceIndices indices)
    {
        _vertices = vertices;
        _indices = indices;
    }

    public Point3D A => _vertices[_indices.A];
    public Point3D B => _vertices[_indices.B];
    public Point3D C => _vertices[_indices.C];
    public Point3D D => _vertices[_indices.D];
}
