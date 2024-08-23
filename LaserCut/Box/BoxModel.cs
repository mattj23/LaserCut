using LaserCut.Geometry;

namespace LaserCut.Box;

public class BoxModel
{
    private readonly BoxParams _boxParams;

    private BoxModel(BoxParams boxParams, bool hasLid, BoxFrontFace front, BoxBackFace back, BoxLeftFace left, BoxRightFace right, BoxTopFace top, BoxBottomFace bottom)
    {
        _boxParams = boxParams;
        HasLid = hasLid;
        Front = front;
        Back = back;
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;

        AllFaces = [front, back, left, right, top, bottom];
    }

    public double Length => _boxParams.Length;
    public double Width => _boxParams.Width;
    public double Height => _boxParams.Height;
    public double Thickness => _boxParams.Thickness;

    public bool HasLid { get; }

    public BoxFrontFace Front { get; }
    public BoxBackFace Back { get; }
    public BoxLeftFace Left { get; }
    public BoxRightFace Right { get; }
    public BoxTopFace Top { get; }
    public BoxBottomFace Bottom { get; }

    public BoxFace[] AllFaces { get; }

    public Dictionary<Guid, Body> ExtraBodies { get; } = new();

    public static BoxModel Create(BoxParams boxParams, bool hasLid)
    {
        var envelope = boxParams.EnvelopeVertices();
        var common = boxParams.CommonVertices();

        // Create the six faces of the box
        var front = new BoxFrontFace(envelope, common, boxParams);
        var back = new BoxBackFace(envelope, common, boxParams);
        var left = new BoxLeftFace(envelope, common, boxParams);
        var right = new BoxRightFace(envelope, common, boxParams);
        var top = new BoxTopFace(envelope, common, boxParams);
        var bottom = new BoxBottomFace(envelope, common, boxParams);

        var allFaces = new BoxFace[] {front, back, left, right, top, bottom};

        // Set the neighbours of each face using a common registration.  First we will register them to a registration
        // object, then we will go through the registration object and set the neighbours of each face.
        var edgeRegistry = new BoxEdgeRegistry();
        var allEdges = allFaces.SelectMany(x => x.AllEdges).ToArray();

        // Register the edge
        foreach (var edge in allEdges) edgeRegistry.Register(edge);

        // Set the neighbours
        foreach (var edge in allEdges) edge.SetNeighbor(edgeRegistry.FindNeighbor(edge));

        // Initialize the edge
        foreach (var edge in allEdges) edge.Initialize();

        // Initialize the faces
        foreach (var face in allFaces) face.Initialize();

        return new BoxModel(boxParams, hasLid, front, back, left, right, top, bottom);
    }
}
