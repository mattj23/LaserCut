using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Box;

public abstract class BoxFace
{
    // Theoretical vertex sources of the face
    private readonly BoxParams _boxParams;
    private Body? _body;
    private readonly Vector3D _vz;

    protected BoxFace(FaceIndices indices, Point3D[] envelope, Point3D[] common, int priority, BoxParams boxParams)
    {
        Common = new FaceVertices(common, indices);
        Envelope = new FaceVertices(envelope, indices);
        Indices = indices;
        Priority = priority;
        _boxParams = boxParams;

        Bottom = new BoxEdge(this, Indices.A, Indices.B, envelope, common, _boxParams);
        Right = new BoxEdge(this, Indices.B, Indices.C, envelope, common, _boxParams);
        Top = new BoxEdge(this, Indices.C, Indices.D, envelope, common, _boxParams);
        Left = new BoxEdge(this, Indices.D, Indices.A, envelope, common, _boxParams);

        Bottom.SetAdjacent(Left, Right);
        Right.SetAdjacent(Bottom, Top);
        Top.SetAdjacent(Right, Left);
        Left.SetAdjacent(Top, Bottom);

        AllEdges = [Bottom, Right, Top, Left];

        var csVx = (Envelope.B - Envelope.A).Normalize();
        var csVy = (Envelope.C - Envelope.B).Normalize();
        var csVz = csVx.CrossProduct(csVy);

        var toOrigin = csVz.DotProduct(Common.A.ToVector3D()) * csVz;
        Cs = new CoordinateSystem(toOrigin.ToPoint3D(), csVx, csVy, csVz);
        CsInv = Cs.Invert();
    }

    public int Priority { get; }

    public FaceIndices Indices { get; }

    public BoxEdge Bottom { get; }
    public BoxEdge Top { get; }
    public BoxEdge Left { get; }
    public BoxEdge Right { get; }

    /// <summary>
    /// Gets the theoretical 3D world positions of the face vertices which are based on the common edges, accounting
    /// for face insets but not for material thicknesses.
    /// </summary>
    public FaceVertices Common { get; }

    /// <summary>
    /// Gets the theoretical 3D world positions of the face vertices which are based on the overall outer dimensions
    /// of the box.  This does not account for any features added to the face, but simply represents the outer envelope
    /// of the theoretical bare box.
    /// </summary>
    public FaceVertices Envelope { get; }

    /// <summary>
    /// Gets all the edges of the face in an array.
    /// </summary>
    public BoxEdge[] AllEdges { get; }

    /// <summary>
    /// Gets the body which represents the face.  After object construction and initialization, this will be a bare
    /// rectangle with no features added, but material thicknesses accounted for.
    /// </summary>
    public Body Body => _body ??= CreateBody();

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

    public void Initialize()
    {
        // All edges must be initialized before this runs
        _body = CreateBody();
    }

    private Body CreateBody()
    {
        var loop = new BoundaryLoop();
        var cursor = loop.GetCursor();

        cursor.SegAbs(Bottom.EnvelopeCursor.Start);
        cursor.SegAbs(Right.EnvelopeCursor.Start);
        cursor.SegAbs(Top.EnvelopeCursor.Start);
        cursor.SegAbs(Left.EnvelopeCursor.Start);

        return new Body(loop);
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
