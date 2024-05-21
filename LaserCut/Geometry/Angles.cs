namespace LaserCut.Geometry;

public static class Angles
{
    /// <summary>
    /// Calculate the angle from theta0 to theta1 in the clockwise direction. This is the clockwise rotation which
    /// would rotate theta0 to theta1.  The resulting value will always be positive.
    /// </summary>
    /// <param name="theta0"></param>
    /// <param name="theta1"></param>
    /// <returns></returns>
    public static double BetweenCw(double theta0, double theta1)
    {
        // Rotations in the clockwise direction are negative by convention, so we are looking for the negative
        // angle which will rotate theta0 to theta1.  This means that theta1 must be less than theta0.
        var t0 = AsSignedAbs(theta0);
        var t1 = AsSignedAbs(theta1);
        if (t1 > t0) t1 -= 2 * Math.PI;

        return t0 - t1;
    }
    
    /// <summary>
    /// Calculate the angle from theta0 to theta1 in the counterclockwise direction. This is the counterclockwise
    /// rotation which would rotate theta0 to theta1.  The resulting value will always be positive.
    /// </summary>
    /// <param name="theta0"></param>
    /// <param name="theta1"></param>
    /// <returns></returns>
    public static double BetweenCcw(double theta0, double theta1)
    {
        // Rotations in the counterclockwise direction are positive by convention, so we are looking for the positive
        // angle which will rotate theta0 to theta1.  This means that theta1 must be greater than theta0.
        var t0 = AsSignedAbs(theta0);
        var t1 = AsSignedAbs(theta1);
        if (t1 < t0) t1 += 2 * Math.PI;
        
        return t1 - t0;
    }
    
    /// <summary>
    /// Normalize the angle to be between -pi and pi.
    /// </summary>
    /// <param name="theta"></param>
    /// <returns></returns>
    public static double AsSignedAbs(double theta)
    {
        // Normalize the angle to be between -pi and pi
        
        while (theta > Math.PI)
        {
            theta -= 2 * Math.PI;
        }
        
        while (theta <= -Math.PI)
        {
            theta += 2 * Math.PI;
        }
        
        return theta;
    }

    /// <summary>
    /// Normalize the angle to be between 0 and 2pi.
    /// </summary>
    /// <param name="theta"></param>
    /// <returns></returns>
    public static double AsPositiveAbs(double theta)
    {
        // Normalize the angle to be between 0 and 2pi
        while (theta < 0)
        {
            theta += 2 * Math.PI;
        }
        
        while (theta >= 2 * Math.PI)
        {
            theta -= 2 * Math.PI;
        }
        
        return theta;
    }
    
}