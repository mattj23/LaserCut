using System.Diagnostics;
using LaserCut.Algorithms;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Mesh;

public class Mesh3
{
    private readonly List<Point3D> _vertices;
    private readonly List<Face> _faces;
    private readonly List<Vector3D> _normals;
    
    public Mesh3(List<Point3D> vertices, List<Face> faces, List<Vector3D> normals)
    {
        _vertices = vertices;
        _faces = faces;
        _normals = normals;
    }
    
    public Mesh3()
    {
        _normals = new List<Vector3D>();
        _vertices = new List<Point3D>();
        _faces = new List<Face>();
    }
    
    public IReadOnlyList<Point3D> Vertices => _vertices;
    
    public IReadOnlyList<Face> Faces => _faces;
    
    public IReadOnlyList<Vector3D> Normals => _normals;
    
    public Mesh3 Clone()
    {
        return new Mesh3([.._vertices], [.._faces], [.._normals]);
    }
    
    /// <summary>
    /// Compute the area of a face in the mesh  The face does not need to be one of the actual faces in the mesh, but
    /// the vertices used to calculate the area will be the indices in the face applied to the vertex list of this
    /// mesh.  This allows testing of hypothetical faces, in addition to actual faces.
    /// </summary>
    /// <param name="face"></param>
    /// <returns></returns>
    public double FaceArea(Face face)
    {
        var a = _vertices[(int)face.A];
        var b = _vertices[(int)face.B];
        var c = _vertices[(int)face.C];
        var v0 = b - a;
        var v1 = c - a;
        return 0.5 * v0.CrossProduct(v1).Length;
    }
    
    /// <summary>
    /// Returns the vertices of a face in the mesh.  The face does not need to be one of the actual faces in the mesh,
    /// but the vertices referenced by the face will come from the vertex list of this mesh.  This allows testing of
    /// hypothetical faces, in addition to actual faces.
    /// </summary>
    /// <param name="face"></param>
    /// <returns></returns>
    public Point3D[] FaceVertices(Face face)
    {
        return new[] { _vertices[(int)face.A], _vertices[(int)face.B], _vertices[(int)face.C] };
    }

    /// <summary>
    /// Remove degenerate faces from the mesh in place.  A face is degenerate if its vertices are co-linear.
    /// </summary>
    public void RemoveDegenerate()
    {
        var toRemove = new List<int>();
        for (var i = 0; i < _faces.Count; i++)
        {
            if (FaceArea(_faces[i]) < 1e-6)
            {
                toRemove.Add(i);
            }
        }
        
        if (toRemove.Any())
        {
            toRemove.Sort();
            toRemove.Reverse();
            foreach (var i in toRemove)
            {
                _faces.RemoveAt(i);
                _normals.RemoveAt(i);
            }
        }
    }

    public uint[] FaceIndices(Func<Face, Vector3D, bool> predicate)
    {
        var indices = new List<uint>();
        for (var i = 0; i < _faces.Count; i++)
        {
            if (predicate(_faces[i], _normals[i]))
            {
                indices.Add((uint)i);
            }
        }
        
        return indices.ToArray();
    }

    public Mesh3 FromFacesWhere(Func<Face, Vector3D, bool> predicate)
    {
        var indices = FaceIndices(predicate);
        return FromFaceIndices(indices);
    }
    
    public Mesh3 FromFaceIndices(IEnumerable<uint> indices)
    {
        var vertexMap = new Dictionary<uint, uint>();
        var faces = new List<Face>();
        var normals = new List<Vector3D>();

        foreach (var i in indices)
        {
            if (!vertexMap.ContainsKey(_faces[(int)i].A))
            {
                vertexMap[_faces[(int)i].A] = (uint)vertexMap.Count;
            }
            
            if (!vertexMap.ContainsKey(_faces[(int)i].B))
            {
                vertexMap[_faces[(int)i].B] = (uint)vertexMap.Count;
            }
            
            if (!vertexMap.ContainsKey(_faces[(int)i].C))
            {
                vertexMap[_faces[(int)i].C] = (uint)vertexMap.Count;
            }
            
            faces.Add(new Face
            {
                A = vertexMap[_faces[(int)i].A],
                B = vertexMap[_faces[(int)i].B],
                C = vertexMap[_faces[(int)i].C]
            });
            
            normals.Add(_normals[(int)i]);
        }

        var vertices = new Point3D[vertexMap.Count];
        foreach (var pair in vertexMap)
        {
            vertices[pair.Value] = _vertices[(int)pair.Key];
        }
        
        return new Mesh3(vertices.ToList(), faces, normals);
    }
    
    /// <summary>
    /// Mutate the vertices of the mesh in-place using a mutation function that takes a Point3D and returns a new,
    /// modified Point3D. The new value will be stored in the same index as the original value.
    /// </summary>
    /// <param name="mutation"></param>
    public void MutateVertices(Func<Point3D, Point3D> mutation)
    {
        for (var i = 0; i < _vertices.Count; i++)
        {
            _vertices[i] = mutation(_vertices[i]);
        }
    }

    /// <summary>
    /// Extract chains of unique edges from the mesh.  A unique edge is one that only belongs to a single face when
    /// ignoring the direction of the edge.  These edges will be present at every boundary of a connected patch of
    /// triangles. This method will return an array of Point3D arrays that represent the boundary chains.
    /// </summary>
    /// <returns></returns>
    public Point3D[][] ExtractEdgeChains()
    {
        var edges = new List<Edge>();
        foreach (var face in _faces)
        {
            edges.Add(face.EdgeA);
            edges.Add(face.EdgeB);
            edges.Add(face.EdgeC);
        }
        
        // Find all edges whose directionless key is unique
        var edgeMap = new Dictionary<ulong, int>();
        foreach (var edge in edges)
        {
            edgeMap.TryAdd(edge.Key, 0);
            edgeMap[edge.Key]++;
        }

        var boundaryEdges = edges.Where(edge => edgeMap[edge.Key] == 1).ToList();
        Console.WriteLine($"Original edges: {edges.Count}, Boundary edges: {boundaryEdges.Count}");
        
        // Now we're going to represent the boundary edges as a map of vertex index to the next vertex index
        var map = boundaryEdges.ToDictionary(e => e.A, e => e.B);
        var chains = new List<Point3D[]>();
        while (map.Count > 0)
        {
            var start = map.First().Key;
            var chain = new List<Point3D> { _vertices[(int)start] };
            var current = start;
            while (map.ContainsKey(current))
            {
                var next = map[current];
                chain.Add(_vertices[(int)next]);
                map.Remove(current);
                current = next;
            }
            
            chains.Add(chain.ToArray());
        }

        return chains.ToArray();
    }

    public Body[] ExtractSilhouetteBodies(CoordinateSystem view)
    {
        var workingMesh = FromFacesWhere((_, n) => n.DotProduct(view.ZAxis) > 0);
        workingMesh.Transform(view);
        
        // TODO: Separate into patch bodies first
        
        // Flatten everything to the XY plane
        workingMesh.MutateVertices(p => new Point3D(p.X, p.Y, 0));
        workingMesh.MergeVertices();
        
        var chains = workingMesh
            .ExtractEdgeChains()
            .Select(c => new PointLoop(c.ToPoint2Ds(true).SkipLast(1)))
            .ToArray();
        
        // Separate inside and outside chains
        var insideChains = chains.Where(c => c.Area < 0).ToList();
        var outsideChains = chains.Where(c => c.Area > 0).ToList();

        // if (outsideChains.Count > 1)
        // {
        //     throw new NotImplementedException("Doesn't yet handle multiple outside chains");
        // }
        
        var results = new List<Body>();
        foreach (var outside in outsideChains)
        {
            // Find all inside chains which are contained within the outside chain
            var insides = insideChains
                .Where(inside => inside.RelationTo(outside) == LoopRelation.Inside)
                .ToList();
            
            results.Add(new Body(outside, insides));
        }

        return results.ToArray();
    }

    public void Transform(CoordinateSystem cs)
    {
        for (var i = 0; i < _vertices.Count; i++)
        {
            _vertices[i] = cs.Transform(_vertices[i]);
        }
        
        for (var i = 0; i < _normals.Count; i++)
        {
            _normals[i] = cs.Transform(_normals[i]);
        }
    }

    /// <summary>
    /// Merge vertices that are within a certain tolerance distance of each other.
    /// </summary>
    /// <param name="tolerance"></param>
    public void MergeVertices(double tolerance = 1e-6)
    {
        var tree = new KdTree3D(_vertices);
        
        // First we're going to go through all vertices and find all of the neighbors that are within the tolerance
        // for each vertex.  Of these, we'll keep the one with the lowest index.  The first vertex map is an array
        // the same length as the number of vertices that maps each vertex to the vertex with the lowest index that is
        // at the same position.
        var vertexMap = _vertices.Select(v =>
        {
            var neighbors = tree.WithinDistance(v, tolerance);
            return neighbors.Min();
        }).ToArray();
        
        // Now we'll find the original indices of the unique vertices and map them to a position in the new vertices
        var uniqueMap = new Dictionary<uint, uint>();
        var uniqueVertices = vertexMap.Distinct().ToArray();
        foreach (var i in uniqueVertices)
        {
            uniqueMap[i] = (uint)uniqueMap.Count;
        }
        
        // We generate the new vertices and faces
        var newVertices = uniqueVertices.Select(i => _vertices[(int)i]).ToArray();
        var newFaces = _faces.Select(f => new Face
        {
            A = uniqueMap[vertexMap[(int)f.A]],
            B = uniqueMap[vertexMap[(int)f.B]],
            C = uniqueMap[vertexMap[(int)f.C]]
        }).ToArray();
        
        // Update the vertices and faces
        _vertices.Clear();
        _vertices.AddRange(newVertices);
        _faces.Clear();
        _faces.AddRange(newFaces);
    }

    public static Mesh3 ReadStl(string path, bool mergeVertices = false)
    {
        // Open file with binary stream reader
        using var binary = new BinaryReader(File.OpenRead(path));
        var reader = new BinaryStlReader(binary);
        
        // Read header
        reader.ReadBytes(80);
        
        var count = reader.ReadUInt32();
        
        var vertices = new List<Point3D>();
        var faces = new List<Face>();
        var normals = new List<Vector3D>();
        
        for (var i = 0; i < count; i++)
        {
            // Read normal
            var n = new Vector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            normals.Add(n);
            
            // Read vertices
            var a = new Point3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var b = new Point3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var c = new Point3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            
            // Read attribute
            reader.ReadUInt16();
            
            // Add vertices
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            
            // Add face
            var last = vertices.Count - 1;
            faces.Add(new Face
            {
                A = (uint)last - 2,
                B = (uint)last - 1,
                C = (uint)last
            });
        }
        
        var mesh = new Mesh3(vertices, faces, normals);
        if (mergeVertices)
        {
            mesh.MergeVertices();
        }

        return mesh;
    }
}