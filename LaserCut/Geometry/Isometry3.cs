using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace LaserCut.Geometry;

public static class Isometry3
{
    public static CoordinateSystem Default =>
        new(Point3D.Origin, UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);

    public static CoordinateSystem ZMinusToZ => CoordinateSystem.Roll(Angle.FromDegrees(180));

    public static CoordinateSystem YMinusToZ => CoordinateSystem.Roll(Angle.FromDegrees(90));

    public static CoordinateSystem YPlusToZ => CoordinateSystem.Roll(Angle.FromDegrees(-90));

    public static CoordinateSystem XMinusToZ => CoordinateSystem.Pitch(Angle.FromDegrees(90));

    public static CoordinateSystem XPlusToZ => CoordinateSystem.Pitch(Angle.FromDegrees(-90));

    /// <summary>
    ///     Generate a new coordinate system at the origin by specifying only the x and y unit vectors.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static CoordinateSystem ByXAndY(UnitVector3D x, UnitVector3D y)
    {
        return new CoordinateSystem(Point3D.Origin, x, y, x.CrossProduct(y));
    }

    public static CoordinateSystem Rx(double degrees)
    {
        return CoordinateSystem.Roll(Angle.FromDegrees(degrees));
    }

    public static CoordinateSystem Ry(double degrees)
    {
        return CoordinateSystem.Pitch(Angle.FromDegrees(degrees));
    }

    public static CoordinateSystem Rz(double degrees)
    {
        return CoordinateSystem.Yaw(Angle.FromDegrees(degrees));
    }

    public static CoordinateSystem Tx(double distance)
    {
        return CoordinateSystem.Translation(new Vector3D(distance, 0, 0));
    }

    public static CoordinateSystem Ty(double distance)
    {
        return CoordinateSystem.Translation(new Vector3D(0, distance, 0));
    }

    public static CoordinateSystem Tz(double distance)
    {
        return CoordinateSystem.Translation(new Vector3D(0, 0, distance));
    }

    public static CoordinateSystem Rx(this CoordinateSystem cs, double degrees)
    {
        return cs.TransformBy(Rx(degrees));
    }

    public static CoordinateSystem Ry(this CoordinateSystem cs, double degrees)
    {
        return cs.TransformBy(Ry(degrees));
    }

    public static CoordinateSystem Rz(this CoordinateSystem cs, double degrees)
    {
        return cs.TransformBy(Rz(degrees));
    }

    public static CoordinateSystem Tx(this CoordinateSystem cs, double distance)
    {
        return cs.TransformBy(Tx(distance));
    }

    public static CoordinateSystem Ty(this CoordinateSystem cs, double distance)
    {
        return cs.TransformBy(Ty(distance));
    }

    public static CoordinateSystem Tz(this CoordinateSystem cs, double distance)
    {
        return cs.TransformBy(Tz(distance));
    }
}