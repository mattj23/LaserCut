using MathNet.Spatial.Euclidean;

namespace LaserCut.Algorithms;

public static class PointOps
{
    /// <summary>
    /// Calculate the two points that are farthest apart from each other in a collection of points.
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static (Point2D, Point2D) FarthestPoints(IEnumerable<Point2D> points)
    {
        // TODO: replace this with a KD tree and the rotating calipers algorithm.
        var array = points.ToArray();
        var maxDist = 0.0;
        var bestPair = (array[0], array[1]);

        for (int i = 0; i < array.Length; i++)
        {
            for (int j = i + 1; j < array.Length; j++)
            {
                var dist = array[i].DistanceTo(array[j]);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    bestPair = (array[i], array[j]);
                }
            }
        }

        return bestPair;
    }

    /// <summary>
    /// Calculate the distance from a point to a line defined by two points.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="testPoint"></param>
    /// <returns></returns>
    public static double DistanceOutOfLine(Point2D p0, Point2D p1, Point2D testPoint)
    {
        var line = new Line2D(p0, p1);
        var cp = line.ClosestPointTo(testPoint, false);
        var d = cp.DistanceTo(testPoint);
        // Console.WriteLine($"Distance {d}");

        return d;
    }

}
