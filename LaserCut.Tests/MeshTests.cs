using LaserCut.Mesh;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests;

public class MeshTests
{
    // [Fact]
    // public void SplitPatches()
    // {
    //     var vertices = new List<Point3D>
    //     {
    //         new Point3D(0, 0, 0),
    //         new Point3D(1, 0, 0),
    //         new Point3D(1, 1, 0),
    //         new Point3D(0, 1, 0),
    //         new Point3D(-1, 0, 0),
    //         
    //         new Point3D(0, 0, 1),
    //         new Point3D(1, 0, 1),
    //         new Point3D(1, 1, 1),
    //         new Point3D(0, 1, 1),
    //         new Point3D(-1, 0, 1),
    //     };
    //
    //     var faces = new List<Face>
    //     {
    //         new Face(0, 1, 2),
    //         new Face(0, 2, 3),
    //         new Face(0, 3, 4),
    //
    //         new Face(5, 6, 7),
    //         new Face(5, 7, 8),
    //         new Face(5, 8, 9),
    //     };
    //     
    //     var mesh = new Mesh3(vertices, faces, new List<Vector3D>());
    //     var patches = mesh.GetPatchIndices();
    //     var tested = patches
    //         .Select(x => x.Order().ToList())
    //         .OrderBy(x => x.First())
    //         .ToList();
    //     
    //     var expected = new List<int>[]
    //     {
    //         new List<int> {0, 1, 2},
    //         new List<int> {3, 4, 5},
    //     };
    //     
    //     Assert.Equal(expected, tested);
    // }

}