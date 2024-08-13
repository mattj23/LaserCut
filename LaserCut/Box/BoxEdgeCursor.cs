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

    public Point2D Start { get; }
    public Point2D End { get; }
    public double Length { get; }

    public Matrix Cs { get; }
    public Matrix CsInv { get; }

    public Point3D StartWorld => EdgeToWorld(Start);
    public Point3D EndWorld => EdgeToWorld(End);

    public Point2D StartFace => EdgeToFace(Start);
    public Point2D EndFace => EdgeToFace(End);

    public Point2D EdgeToFace(Point2D edgePoint)
    {
        return edgePoint.TransformBy(Cs);
    }

    public Point2D FaceToEdge(Point2D facePoint)
    {
        return facePoint.TransformBy(CsInv);
    }

    public Point2D WorldToEdge(Point3D worldPoint)
    {
        return FaceToEdge(_face.WorldToFace(worldPoint));
    }

    public Point3D EdgeToWorld(Point2D edgePoint)
    {
        return _face.FaceToWorld(EdgeToFace(edgePoint));
    }


}
