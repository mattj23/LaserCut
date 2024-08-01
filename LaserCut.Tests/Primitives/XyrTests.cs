using LaserCut.Geometry;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Primitives;

public class XyrTests
{
    [Fact]
    public void DefaultIsEmpty()
    {
        var xyr = default(Xyr);
        Assert.Equal(0, xyr.X, 1e-12);
        Assert.Equal(0, xyr.Y, 1e-12);
        Assert.Equal(0, xyr.R, 1e-12);
    }

    [Fact]
    public void SimpleMove()
    {
        var xyr = new Xyr(1, 2, Math.PI / 2);
        var point = new Point2D(0, 1);
        var result = point.Transformed(xyr.AsMatrix());

        var expected = new Point2D(0, 2);
        
        Assert.Equal(expected, result, PointCheck.Default);
    }
    
    [Fact]
    public void RoundTrip()
    {
        var xyr = new Xyr(1, 2, Math.PI / 2);
        var matrix = xyr.AsMatrix();
        var result = Xyr.FromMatrix(matrix);
        
        Assert.Equal(xyr, result, XyrCheck.Default);
    }

    [Fact]
    public void StressTestRoundTrip()
    {
        var r = new RandomValues();
        for (var i = 0; i < 10000; i++)
        {
            var xyr = new Xyr(r.Double(5), r.Double(5), r.Double(-Math.PI, Math.PI));
            var matrix = xyr.AsMatrix();
            var result = Xyr.FromMatrix(matrix);
            
            Assert.Equal(xyr, result, XyrCheck.Default);
        }
    }
    
}

public class XyrCheck : IEqualityComparer<Xyr>
{
    private readonly double _tolerance;

    public XyrCheck(double tolerance)
    {
        _tolerance = tolerance;
    }

    public bool Equals(Xyr x, Xyr y)
    {
        return Math.Abs(x.X - y.X) < _tolerance &&
               Math.Abs(x.Y - y.Y) < _tolerance &&
               Math.Abs(x.R - y.R) < _tolerance;
    }

    public int GetHashCode(Xyr obj)
    {
        return HashCode.Combine(obj.X, obj.Y, obj.R);
    }
    
    public static XyrCheck Tol(double tolerance) => new(tolerance);

    public static XyrCheck Default => Tol(GeometryConstants.DistEquals);
}