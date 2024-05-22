using System.Reflection.Metadata;
using LaserCut.Algorithms;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Algorithms;

/// <summary>
/// These tests exercise some of the logic in the ContainsPoint class, including some of the stranger edge cases which
/// involve the test ray passing through vertices or corners of the geometry.
/// </summary>
public class EnclosesPointTests
{
    [Fact]
    public void SimpleContainsPoint()
    {
        var geom = new SimpleGeometry();
        var ray = new Ray2(new Point2D(0, 0.5), Vector2D.XAxis);

        var positions = geom.Intersections(ray);
        Assert.Single(positions);
        
        Assert.True(EnclosesPoint.Check(ray, positions));
    }

    [Fact]
    public void SimpleDoesNotContainPointMisses()
    {
        var geom = new SimpleGeometry();
        var ray = new Ray2(new Point2D(-1, 3), Vector2D.XAxis);

        var positions = geom.Intersections(ray);
        Assert.False(EnclosesPoint.Check(ray, positions));
    }
    
    [Fact]
    public void SimpleDoesNotContainPointEntersAndExits()
    {
        var geom = new SimpleGeometry();
        var ray = new Ray2(new Point2D(-2, 0.5), Vector2D.XAxis);

        var positions = geom.Intersections(ray);
        Assert.False(EnclosesPoint.Check(ray, positions));
    }

    [Fact]
    public void OutsideButPassesThroughCorner()
    {
        var geom = new SimpleGeometry();
        var ray = new Ray2(new Point2D(0, 2), new Vector2D(1, -1));

        var positions = geom.Intersections(ray);
        Assert.Equal(2, positions.Length);
        Assert.False(EnclosesPoint.Check(ray, positions));
    }
    
    [Fact]
    public void InsideButPassesThroughCorner()
    {
        var geom = new SimpleGeometry();
        var ray = new Ray2(new Point2D(0.5, 0.5), new Vector2D(1, -1));

        var positions = geom.Intersections(ray);
        Assert.Equal(2, positions.Length);
        Assert.True(EnclosesPoint.Check(ray, positions));
    }

    [Fact]
    public void InsideButPassesThroughFlatVertex()
    {
        var geom = new SimpleGeometry();
        var ray = new Ray2(new Point2D(0.0, 0.5), new Vector2D(0, 1));

        var positions = geom.Intersections(ray);
        Assert.Equal(2, positions.Length);
        Assert.True(EnclosesPoint.Check(ray, positions));
    }
    
    [Fact]
    public void OutsideButPassesThroughFlatVertex()
    {
        var geom = new SimpleGeometry();
        var ray = new Ray2(new Point2D(0.0, -4), new Vector2D(0, 1));

        var positions = geom.Intersections(ray);
        Assert.Equal(4, positions.Length);
        Assert.False(EnclosesPoint.Check(ray, positions));
    }

    private class SimpleGeometry
    {
        private readonly List<IContourElement> _elements = new();
        
        public SimpleGeometry()
        {
            _elements.Add(new Segment(new Point2D(0, 0), new Point2D(1, 0), 0));
            _elements.Add(new Segment(new Point2D(1, 0), new Point2D(1, 1), 0));
            _elements.Add(new Segment(new Point2D(1, 1), new Point2D(0, 1), 0));
            _elements.Add(new Arc(0, 0.5, 0.5, Math.PI / 2.0, Math.PI));
        }
        
        public Position[] Intersections(Ray2 ray)
        {
            var results = new List<Position>();
            foreach (var element in _elements)
            {
                results.AddRange(ray.Intersections(element));
            }

            return results.ToArray();
        }
    }
    
}