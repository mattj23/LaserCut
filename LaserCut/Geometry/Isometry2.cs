using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;


namespace LaserCut.Geometry;
public static class Isometry2
{
    public static Matrix Translate(Vector2D vector)
    {
        return Translate(vector.X, vector.Y);
    }
    
    public static Matrix Translate(double x, double y)
    {
        return DenseMatrix.OfArray(new double[,]
        {
            {1, 0, x},
            {0, 1, y},
            {0, 0, 1}
        });
    }
    
    public static Matrix Rotate(Vector2D from, Vector2D to)
    {
        var angle = from.SignedAngleTo(to);
        return Rotate(angle.Degrees);
    }
    
    public static Matrix Rotate(double degrees)
    {
        var radians = degrees * Math.PI / 180;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return DenseMatrix.OfArray(new double[,]
        {
            {cos, -sin, 0},
            {sin, cos, 0},
            {0, 0, 1}
        });
    }

    public static Point2D Transformed(this Point2D point, Matrix m)
    {
        var p = Vector<double>.Build.DenseOfArray([point.X, point.Y, 1]);
        var result = m * p;
        return new Point2D(result[0], result[1]);
    }
    
    public static Vector2D Transformed(this Vector2D vector, Matrix m)
    {
        var v = Vector<double>.Build.DenseOfArray([vector.X, vector.Y, 0]);
        var result = m * v;
        return new Vector2D(result[0], result[1]);
    }
    
}
