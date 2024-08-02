using Avalonia;
namespace LaserCut.Avalonia;

public static class Extensions
{
    public static Matrix ToAvalonia(this MathNet.Numerics.LinearAlgebra.Double.Matrix matrix)
    {
        var m = matrix.Transpose();
        return new Matrix(
            m[0, 0], m[0, 1], m[0, 2],
            m[1, 0], m[1, 1], m[1, 2],
            m[2, 0], m[2, 1], m[2, 2]
        );
    }
    
}