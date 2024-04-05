using MathNet.Spatial.Euclidean;

namespace LaserCut.Mesh;

public class Mesh3D
{
    private readonly List<Point3D> _vertices;
    private readonly List<Face> _faces;
    private readonly List<Vector3D> _normals;
    
    public Mesh3D(List<Point3D> vertices, List<Face> faces, List<Vector3D> normals)
    {
        _vertices = vertices;
        _faces = faces;
        _normals = normals;
    }
    
    public Mesh3D()
    {
        _normals = new List<Vector3D>();
        _vertices = new List<Point3D>();
        _faces = new List<Face>();
    }
    
    public IReadOnlyList<Point3D> Vertices => _vertices;
    
    public IReadOnlyList<Face> Faces => _faces;
    
    public IReadOnlyList<Vector3D> Normals => _normals;

    public static Mesh3D ReadStl(string path)
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

        return new Mesh3D(vertices, faces, normals);
    }
}