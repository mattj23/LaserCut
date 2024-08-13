using MathNet.Spatial.Euclidean;

namespace LaserCut.Box;

public abstract class BoxFace
{
    // Theoretical vertex sources of the face
    private readonly BoxParams _boxParams;

    protected BoxFace(FaceIndices indices, Point3D[] envelope, Point3D[] common, int priority, BoxParams boxParams)
    {
        Common = new FaceVertices(common, indices);
        Envelope = new FaceVertices(envelope, indices);
        Indices = indices;
        Priority = priority;
        _boxParams = boxParams;

        BottomEdge = new BoxEdge(this, Indices.A, Indices.B, envelope, common, _boxParams);
        RightEdge = new BoxEdge(this, Indices.B, Indices.C, envelope, common, _boxParams);
        TopEdge = new BoxEdge(this, Indices.C, Indices.D, envelope, common, _boxParams);
        LeftEdge = new BoxEdge(this, Indices.D, Indices.A, envelope, common, _boxParams);

        BottomEdge.SetAdjacent(LeftEdge, RightEdge);
        RightEdge.SetAdjacent(BottomEdge, TopEdge);
        TopEdge.SetAdjacent(RightEdge, LeftEdge);
        LeftEdge.SetAdjacent(TopEdge, BottomEdge);

        AllEdges = [BottomEdge, RightEdge, TopEdge, LeftEdge];

        var csVx = (Envelope.B - Envelope.A).Normalize();
        var csVy = (Envelope.C - Envelope.B).Normalize();
        var csVz = csVx.CrossProduct(csVy);

        var toOrigin = csVz.DotProduct(Envelope.A.ToVector3D()) * csVz;
        Cs = new CoordinateSystem(toOrigin.ToPoint3D(), csVx, csVy, csVz);
        CsInv = Cs.Invert();
    }

    public int Priority { get; }

    public FaceIndices Indices { get; }

    public BoxEdge BottomEdge { get; }
    public BoxEdge TopEdge { get; }
    public BoxEdge LeftEdge { get; }
    public BoxEdge RightEdge { get; }

    public FaceVertices Common { get; }

    public FaceVertices Envelope { get; }

    public BoxEdge[] AllEdges { get; }

    /// <summary>
    /// Get a coordinate system which will transform a 3D point on the face to the corresponding x, y, z coordinate in
    /// the world coordinate system.
    /// </summary>
    public CoordinateSystem Cs { get; }

    public CoordinateSystem CsInv { get; }

    public Point2D WorldToFace(Point3D world)
    {
        var local = CsInv.Transform(world);
        return new Point2D(local.X, local.Y);
    }

    public Point3D FaceToWorld(Point2D face)
    {
        var local = new Point3D(face.X, face.Y, 0);
        return Cs.Transform(local);
    }
}

public class BoxLeftFace : BoxFace
{
    public BoxLeftFace(Point3D[] envelope, Point3D[] common, BoxParams boxParams)
        : base(FaceIndices.Left, envelope, common, 2, boxParams)
    {
    }
}

public class BoxRightFace : BoxFace
{
    public BoxRightFace(Point3D[] envelope, Point3D[] common, BoxParams boxParams)
        : base(FaceIndices.Right, envelope, common, 2, boxParams)
    {
    }
}

public class BoxFrontFace : BoxFace
{
    public BoxFrontFace(Point3D[] envelope, Point3D[] common, BoxParams boxParams)
        : base(FaceIndices.Front, envelope, common, 1, boxParams)
    {
    }
}

public class BoxBackFace : BoxFace
{
    public BoxBackFace(Point3D[] envelope, Point3D[] common, BoxParams boxParams)
        : base(FaceIndices.Back, envelope, common, 1, boxParams)
    {
    }
}

public class BoxTopFace : BoxFace
{
    public BoxTopFace(Point3D[] envelope, Point3D[] common, BoxParams boxParams)
        : base(FaceIndices.Top, envelope, common, 0, boxParams)
    {
    }
}

public class BoxBottomFace : BoxFace
{
    public BoxBottomFace(Point3D[] envelope, Point3D[] common, BoxParams boxParams)
        : base(FaceIndices.Bottom, envelope, common, 0, boxParams)
    {
    }
}
