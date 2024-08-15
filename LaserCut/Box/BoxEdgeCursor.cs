using LaserCut.Geometry;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Box;

public class BoxEdgeCursor
{
    private readonly BoxFace _face;

    public BoxEdgeCursor(Point2D start, Point2D end, BoxFace face)
    {
        Start = start;
        End = end;
        _face = face;
        Length = (end - start).Length;

        Cs = Isometry2.FromBasisY(start, end - start);
        CsInv = (Matrix)Cs.Inverse();
    }

    /// <summary>
    /// Gets the start point of the edge in the face coordinate system.
    /// </summary>
    public Point2D Start { get; }

    /// <summary>
    /// Gets the end point of the edge in the face coordinate system.
    /// </summary>
    public Point2D End { get; }
    public double Length { get; }

    /// <summary>
    /// Gets a matrix which will transform a point on the edge (x, y) to the corresponding point in the face.
    /// </summary>
    public Matrix Cs { get; }

    /// <summary>
    /// Gets a matrix which will transform a point on the face to the corresponding point on the edge.
    /// </summary>
    public Matrix CsInv { get; }

    /// <summary>
    /// Gets the start point of the edge in the world coordinate system.
    /// </summary>
    public Point3D StartWorld => _face.FaceToWorld(Start);

    /// <summary>
    /// Gets the end point of the edge in the world coordinate system.
    /// </summary>
    public Point3D EndWorld => _face.FaceToWorld(End);

    /// <summary>
    /// Transforms a point on the edge to the corresponding point on the face.
    /// </summary>
    /// <param name="edgePoint"></param>
    /// <returns></returns>
    public Point2D EdgeToFace(Point2D edgePoint)
    {
        return edgePoint.Transformed(Cs);
    }

    /// <summary>
    /// Transforms a point on the face to the corresponding point on the edge.
    /// </summary>
    /// <param name="facePoint"></param>
    /// <returns></returns>
    public Point2D FaceToEdge(Point2D facePoint)
    {
        return facePoint.Transformed(CsInv);
    }

    /// <summary>
    /// Transforms a point in the world coordinate system to the corresponding point on the edge, dropping any
    /// mismatched z values.
    /// </summary>
    /// <param name="worldPoint"></param>
    /// <returns></returns>
    public Point2D WorldToEdge(Point3D worldPoint)
    {
        return FaceToEdge(_face.WorldToFace(worldPoint));
    }

    /// <summary>
    /// Transforms a point on the edge to the corresponding point in the world coordinate system.
    /// </summary>
    /// <param name="edgePoint"></param>
    /// <returns></returns>
    public Point3D EdgeToWorld(Point2D edgePoint)
    {
        return _face.FaceToWorld(EdgeToFace(edgePoint));
    }

    public void Operate(BoundaryLoop tool)
    {
        var copy = tool.Copy();
        copy.Transform(Cs);
        _face.Operate(copy);
    }

}
