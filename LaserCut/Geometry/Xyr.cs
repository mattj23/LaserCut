using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Geometry;

/// <summary>
/// A struct describing the three components of a 2D rigid body transformation, X and Y translation and R rotation. The
/// rotation value is in radians.
/// </summary>
/// <param name="X">Translation in X</param>
/// <param name="Y">Translation in Y</param>
/// <param name="R">Rotation, in radians, in the counter-clockwise direction</param>
public readonly record struct Xyr(double X, double Y, double R)
{
    public Matrix AsMatrix() 
    {
        var cos = Math.Cos(R);
        var sin = Math.Sin(R);
        return DenseMatrix.OfArray(new[,]
        {
            {cos, -sin, X},
            {sin, cos, Y},
            {0, 0, 1}
        });
    }
    
    public static Xyr FromMatrix(Matrix m)
    {
        var x = m[0, 2];
        var y = m[1, 2];
        var r = Math.Atan2(m[1, 0], m[0, 0]);
        return new Xyr(x, y, r);
    }

    public Point2D Point => new Point2D(X, Y);
}
