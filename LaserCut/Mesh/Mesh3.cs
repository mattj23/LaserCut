using System.Diagnostics;
using System.Diagnostics.Contracts;
using LaserCut.Algorithms;
using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Mesh;

public class Mesh3
{
    private readonly List<Point3D> _vertices;
    private readonly List<Face> _faces;
    private readonly List<Vector3D> _normals;
    private EdgeFaceMap? _edgeFaceMap;
    
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
    
    // ==============================================================================================================
    // Public Properties
    // ==============================================================================================================
    
    /// <summary>
    /// Gets a read-only list of the vertices in the mesh.  The index of each vertex in the list is the index referred
    /// to in the faces of the mesh.
    /// </summary>
    public IReadOnlyList<Point3D> Vertices => _vertices;
    
    /// <summary>
    /// Gets a read-only list of the faces in the mesh.  Faces contain three indices that refer to the vertices in the
    /// Vertices list.  Faces are typically referred to by their index in this list.
    /// </summary>
    public IReadOnlyList<Face> Faces => _faces;
    
    /// <summary>
    /// Gets a read-only list of the normals of the faces in the mesh.  The element at index `i` in this list
    /// corresponds with the face at index `i` in the Faces list.
    /// </summary>
    public IReadOnlyList<Vector3D> Normals => _normals;
    
    /// <summary>
    /// Gets a mapping of edges to the faces that contain them.  This is a cached structure that is built on demand
    /// and cleared whenever the structure of the mesh changes.  The EdgeFaceMap is useful for determining adjacency
    /// and other operations that require edge information.
    /// </summary>
    public EdgeFaceMap EdgeMap => _edgeFaceMap ??= EdgeFaceMap.FromFaces(_faces);
    
    // ==============================================================================================================
    // Utility Methods
    // ==============================================================================================================
    
    public Mesh3 Clone()
    {
        return new Mesh3([.._vertices], [.._faces], [.._normals]);
    }

    public override string ToString()
    {
        return $"[Mesh {_vertices.Count} points, {_faces.Count} faces]";
    }

    /// <summary>
    /// Compute the area of a face in the mesh  The face does not need to be one of the actual faces in the mesh, but
    /// the vertices used to calculate the area will be the indices in the face applied to the vertex list of this
    /// mesh.  This allows testing of hypothetical faces, in addition to actual faces.
    /// </summary>
    /// <param name="face"></param>
    /// <returns></returns>
    [Pure]
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
    [Pure]
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
            ClearCachedStructureData();
        }
    }

    /// <summary>
    /// Return the indices of the faces in the mesh that satisfy a predicate.  The predicate is a function that takes
    /// the face and its normal and returns a boolean.  The indices of the faces that return true will be returned in
    /// the result.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    [Pure]
    public int[] FaceIndices(Func<Face, Vector3D, bool> predicate)
    {
        var indices = new List<int>();
        for (var i = 0; i < _faces.Count; i++)
        {
            if (predicate(_faces[i], _normals[i]))
            {
                indices.Add(i);
            }
        }
        
        return indices.ToArray();
    }

    public Mesh3 FromFacesWhere(Func<Face, Vector3D, bool> predicate)
    {
        var indices = FaceIndices(predicate);
        return FromFaceIndices(indices);
    }
    
    /// <summary>
    /// Create a new mesh from a subset of the faces in this mesh, identified by their indices.  The current mesh
    /// will not be modified.
    /// </summary>
    /// <param name="indices"></param>
    /// <returns></returns>
    [Pure]
    public Mesh3 FromFaceIndices(IEnumerable<int> indices)
    {
        var vertexMap = new Dictionary<uint, uint>();
        var faces = new List<Face>();
        var normals = new List<Vector3D>();

        foreach (var i in indices)
        {
            if (!vertexMap.ContainsKey(_faces[i].A))
            {
                vertexMap[_faces[i].A] = (uint)vertexMap.Count;
            }
            
            if (!vertexMap.ContainsKey(_faces[i].B))
            {
                vertexMap[_faces[i].B] = (uint)vertexMap.Count;
            }
            
            if (!vertexMap.ContainsKey(_faces[i].C))
            {
                vertexMap[_faces[i].C] = (uint)vertexMap.Count;
            }
            
            faces.Add(new Face
            {
                A = vertexMap[_faces[i].A],
                B = vertexMap[_faces[i].B],
                C = vertexMap[_faces[i].C]
            });
            
            normals.Add(_normals[i]);
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

    /// <summary>
    /// Goes through the mesh's faces and clusters them into patches, returning a list of lists of indices.  Each list
    /// of indices corresponds to the triangles in a single patch.
    ///
    /// Patches are determined by adjacency.  Triangles which share a common edge are considered adjacent.
    /// </summary>
    /// <returns></returns>
    public List<int[]> GetPatchIndicesDep()
    {
        
        var edgeMap = new Dictionary<ulong, List<int>>();
        var remaining = new HashSet<int>();
        
        // Construct a table of edges and the faces that contain them
        for (int i = 0; i < _faces.Count; i++)
        {
            foreach (var edge in _faces[i].Edges())
            {
                if (!edgeMap.ContainsKey(edge.Key))
                {
                    edgeMap[edge.Key] = new List<int>();
                }
                
                edgeMap[edge.Key].Add(i);
            }
            
            remaining.Add(i);
        }
        
        // Empty all faces from a working list
        var patches = new List<int[]>();
        while (remaining.Count > 0)
        {
            var patch = new HashSet<int>();
            var working = new HashSet<int>{remaining.Pop()};
            
            while (working.Count != 0)
            {
                var current = working.Pop();
                patch.Add(current);
                
                foreach (var edge in _faces[current].Edges())
                {
                    if (!edgeMap.Remove(edge.Key, out var indices)) continue;
                    
                    foreach (var j in indices)
                    {
                        remaining.Remove(j);
                        if (!patch.Contains(j))
                        {
                            working.Add(j);
                        }
                    }
                }

            }
            
            patches.Add(patch.ToArray());
        }

        return patches;
    }
    
    public BoundaryLoop[] ExtractSilhouetteContours(CoordinateSystem view)
    {
        var results = new List<BoundaryLoop>();
        
        var workingMesh = FromFacesWhere((_, n) => n.DotProduct(view.ZAxis) > 1e-6);
        workingMesh.Transform(view);
        workingMesh.MergeVertices();
        
        // First we need to separate the mesh into patches of connected triangles. This requires us to compute the
        // adjacency of the faces.  Face adjacency is determined by whether two faces share an edge, so by computing a
        // mapping of edges to faces, we can construct an adjacency map of faces.
        var edgeMap = EdgeFaceMap.FromFaces(workingMesh._faces);
        var patches = edgeMap.GetPatchIndices();
        var singleFaceEdgeKeys = edgeMap.EdgeKeysWithSingleFace();
        
        // Now we'll go through each patch and find the edges that are on the boundary of the patch.  These are the 
        // edges that are only shared by a single face.
        foreach (var patch in patches)
        {
            // For each face index in the patch we check all three edges.  If the edge map reports that the edge only is
            // attached to a single face, then we add it to the boundary edges.
            var boundaryVertices = new Dictionary<int, int>();
            foreach (var i in patch)
            {
                foreach (var edge in workingMesh._faces[i].Edges())
                {
                    if (singleFaceEdgeKeys.Contains(edge.Key))
                    {
                        boundaryVertices[(int)edge.A] = (int)edge.B;
                    }
                }
            }
            
            // Now we can extract the patch chains
            var patchChains = new List<BoundaryLoop>();
            while (boundaryVertices.Count > 0)
            {
                var start = boundaryVertices.First().Key;
                var chain = new List<Point2D> { workingMesh._vertices[start].ToPoint2D() };
                var current = start;
                while (boundaryVertices.ContainsKey(current))
                {
                    var next = boundaryVertices[current];
                    chain.Add(workingMesh._vertices[next].ToPoint2D());
                    boundaryVertices.Remove(current);
                    current = next;
                }
                
                patchChains.Add(BoundaryLoop.Polygon(chain));
            }

            results.AddRange(patchChains);
        }
        
        return results.ToArray();
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
                .Where(inside => inside.RelationTo(outside) == ContourRelation.EnclosedBy)
                .ToList();

            throw new NotImplementedException();
            // results.Add(new Body(outside, insides));
        }

        return results.ToArray();
    }

    /// <summary>
    /// Transform the mesh in place using a coordinate system.  This will transform all the vertices and normals in the
    /// mesh but leave the structure of the mesh (faces, edges) unchanged.
    /// </summary>
    /// <param name="cs">The coordinate system/transform to apply to the mesh</param>
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
    /// Merge vertices that are within a certain tolerance distance of each other.  The tolerance is the maximum
    /// distance at which two vertices will be considered the same.  The default tolerance (or any non-finite double
    /// value) will result in `GeometryConstants.DistEquals` being used.
    /// </summary>
    /// <param name="tolerance">The maximum distance at which two verticies will be considered the same.</param>
    public void MergeVertices(double tolerance = double.NaN)
    {
        if (!double.IsFinite(tolerance))
        {
            tolerance = GeometryConstants.DistEquals;
        }
        
        var tree = new KdTree3D(_vertices);
        
        // First we're going to go through all vertices and find all the neighbors that are within the tolerance
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
        ClearCachedStructureData();
    }

    /// <summary>
    /// Read an STL file from disk and return a mesh.  The mergeVertices parameter will merge vertices that are within
    /// the `GeometryConstants.DistEquals` tolerance distance of each other. If you want to use a different tolerance,
    /// call `MergeVertices` on the mesh after reading it.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mergeVertices"></param>
    /// <returns></returns>
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
            mesh.MergeVertices(GeometryConstants.DistEquals);
        }

        return mesh;
    }
    
    /// <summary>
    /// Clear any cached data structures that are specific to the mesh structure.  This method should be called whenever
    /// anything related to the faces or edges of the mesh is modified, but NOT if the locations of the verticies
    /// change without changing the connectivity of the mesh.
    /// </summary>
    private void ClearCachedStructureData()
    {
        _edgeFaceMap = null;
    }
    
}