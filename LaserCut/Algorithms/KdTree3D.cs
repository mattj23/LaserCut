using MathNet.Spatial.Euclidean;
using Supercluster.KDTree;

namespace LaserCut.Algorithms;

public class KdTree3D
{
    private KDTree<double, uint> _tree;
    
    public KdTree3D(IEnumerable<Point3D> points)
    {
        var data = points.Select(p => new double[] {p.X, p.Y, p.Z}).ToArray();
        var indices = Enumerable.Range(0, data.Length).Select(i => (uint)i).ToArray();
        _tree = new KDTree<double, uint>(3, data, indices, (a, b) =>
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += (a[i] - b[i]) * (a[i] - b[i]);
            }

            return sum;
        });
    }
    
    public uint[] WithinDistance(Point3D point, double distance)
    {
        var result = _tree.RadialSearch(new double[] {point.X, point.Y, point.Z}, distance);
        return result.Select(t => t.Item2).ToArray();
    }

}