namespace LaserCut.Box;

public class BoxEdgeRegistry
{
    private readonly Dictionary<ulong, BoxEdge> _firstEdges = new();
    private readonly Dictionary<ulong, BoxEdge> _secondEdges = new();

    public void Register(BoxEdge edge)
    {
        if (edge.KeyIsFirst)
        {
            _firstEdges[edge.Key] = edge;
        }
        else
        {
            _secondEdges[edge.Key] = edge;
        }
    }

    public BoxEdge FindNeighbor(BoxEdge edge)
    {
        return edge.KeyIsFirst ? _secondEdges[edge.Key] : _firstEdges[edge.Key];
    }
}
