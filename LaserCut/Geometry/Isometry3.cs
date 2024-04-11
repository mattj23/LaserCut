using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace LaserCut.Geometry;

public static class Isometry3
{
    public static CoordinateSystem Default =>
        new CoordinateSystem(Point3D.Origin, UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
    
    public static CoordinateSystem ZMinusToZ => CoordinateSystem.Roll(Angle.FromDegrees(180));
    
    public static CoordinateSystem YMinusToZ => CoordinateSystem.Roll(Angle.FromDegrees(90));
    
    public static CoordinateSystem YPlusToZ => CoordinateSystem.Roll(Angle.FromDegrees(-90));
    
    public static CoordinateSystem XMinusToZ => CoordinateSystem.Pitch(Angle.FromDegrees(90));
    
    public static CoordinateSystem XPlusToZ => CoordinateSystem.Pitch(Angle.FromDegrees(-90));

}