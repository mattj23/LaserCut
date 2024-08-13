using MathNet.Spatial.Euclidean;

namespace LaserCut.Box;

public static class BoxGen
{
    public static Point3D[] Vertices(double front, double back, double left, double right, double top, double bottom)
    {
        if (front < back) throw new ArgumentException("Front must be greater than back");
        if (left > right) throw new ArgumentException("Left must be less than than right");
        if (top < bottom) throw new ArgumentException("Top must be greater than bottom");

        return
        [
            new Point3D(front, left, bottom),
            new Point3D(front, right, bottom),
            new Point3D(front, right, top),
            new Point3D(front, left, top),
            new Point3D(back, left, bottom),
            new Point3D(back, right, bottom),
            new Point3D(back, right, top),
            new Point3D(back, left, top)
        ];
    }

    public static Point3D[] CommonVertices(this BoxParams boxParams)
    {

        var front = boxParams.Length / 2;
        var back = -front;
        var left = -boxParams.Width / 2;
        var right = -left;
        var top = boxParams.Height / 2;
        var bottom = -top;

        return Vertices(front, back, left, right, top, bottom + boxParams.BaseInset);
    }

    public static Point3D[] EnvelopeVertices(this BoxParams boxParams)
    {
        var front = boxParams.Length / 2;
        var back = -front;
        var left = -boxParams.Width / 2;
        var right = -left;
        var top = boxParams.Height / 2;
        var bottom = -top;

        return Vertices(front, back, left, right, top, bottom);
    }

}
