using LaserCut.Algorithms;

namespace LaserCut.Mesh;

public class EdgeFaceMap
{
    private readonly Dictionary<ulong, List<int>> _edgeToFaceMap = new();
    private readonly HashSet<int> _faceIndices = new();

    public void AddFace(Face face, int faceIndex)
    {
        AddEdge(face.EdgeA, faceIndex);
        AddEdge(face.EdgeB, faceIndex);
        AddEdge(face.EdgeC, faceIndex);
    }

    public void AddEdge(Edge edge, int faceIndex)
    {
        if (!_edgeToFaceMap.ContainsKey(edge.Key))
        {
            _edgeToFaceMap[edge.Key] = [faceIndex];
        }
        else
        {
            _edgeToFaceMap[edge.Key].Add(faceIndex);
        }
        _faceIndices.Add(faceIndex);
    }

    public static EdgeFaceMap FromFaces(IReadOnlyList<Face> faces)
    {
        var map = new EdgeFaceMap();
        for (var i = 0; i < faces.Count; i++)
        {
            map.AddFace(faces[i], i);
        }

        return map;
    }
    
    public HashSet<ulong> EdgeKeysWithSingleFace()
    {
        var singleFaceEdges = new HashSet<ulong>();
        foreach (var (key, faceIndices) in _edgeToFaceMap)
        {
            if (faceIndices.Count == 1)
            {
                singleFaceEdges.Add(key);
            }
        }

        return singleFaceEdges;
    }

    /// <summary>
    /// Construct a mapping of face index to a hashset of face indices that are adjacent to the face.  This is built
    /// from the edge to face map, and is useful for any operations that require face adjacency information.
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, HashSet<int>> BuildFaceAdjacencyMap()
    {
        var adjacencyPairs = new HashSet<(int, int)>();
        foreach (var items in _edgeToFaceMap.Values)
        {
            if (items.Count == 2)
            {
                adjacencyPairs.Add((items[0], items[1]));
                adjacencyPairs.Add((items[1], items[0]));
            }
            else if (items.Count > 2)
            {
                throw new NotImplementedException("Non-manifold mesh detected");
            }
        }
        
        var faceAdjacencyMap = new Dictionary<int, HashSet<int>>();
        foreach (var (a, b) in adjacencyPairs)
        {
            if (!faceAdjacencyMap.ContainsKey(a))
            {
                faceAdjacencyMap[a] = new HashSet<int> {b};
            }
            else
            {
                faceAdjacencyMap[a].Add(b);
            }
        }

        return faceAdjacencyMap;
    }

    public List<int[]> GetPatchIndices()
    {
        var adjacency = BuildFaceAdjacencyMap();
        var patches = new List<int[]>();
        var remaining = new HashSet<int>(_faceIndices);
        
        while (remaining.Count > 0)
        {
            var patch = new HashSet<int>();
            var stack = new HashSet<int> { remaining.Pop() };

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                patch.Add(current);
                if (!adjacency.ContainsKey(current)) continue;
                
                foreach (var fi in adjacency[current])
                {
                    if (remaining.Contains(fi))
                    {
                        remaining.Remove(fi);
                        stack.Add(fi);
                    }
                }
            }
            
            patches.Add(patch.ToArray());
        }
        
        return patches;
    }
    
}