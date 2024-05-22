using LaserCut.Algorithms;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Algorithms;

public class ContainsPointTests
{
    [Fact]
    public void SimpleContainsPoint()
    {
        var seg = new Segment(new Point2D(0, 0), new Point2D(0, 1), 0);
        var ray = new Ray2(new Point2D(-1, 0.5), Vector2D.XAxis);

        var positions = ray.Intersections(seg);
        Assert.Single(positions);
        
        Assert.True(ContainsPoint.Check(ray, positions));
    }

    [Fact]
    public void SimpleDoesNotContainPoint()
    {
        
    }

    private class SimpleGeometry
    {
        private readonly List<IContourElement> _elements = new();
        
        public SimpleGeometry()
        {
            _elements.Add(new Segment(new Point2D(0, 0), new Point2D(0, 1), 0));
            _elements.Add(new Segment(new Point2D(0, 1), new Point2D(1, 1), 0));
            _elements.Add(new Segment(new Point2D(1, 1), new Point2D(1, 0), 0));
            _elements.Add(new Arc(0, 0.5, 1, Math.PI / 2, Math.PI));
        }
        
        public Position[] Intersections(Ray2 ray)
        {
            var results = new List<Position>();
            foreach (var element in _elements)
            {
                results.AddRange(element.Intersections(ray));
            }

            return results.ToArray();
        }
    }
    
}