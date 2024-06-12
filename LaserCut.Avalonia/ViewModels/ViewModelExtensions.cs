using Avalonia;
using Avalonia.Media;
using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia.ViewModels;

public static class ViewModelExtensions
{
    
    public static Point ToAvalonia(this Point2D point) => new Point(point.X, point.Y);
    
    public static Point ToAvalonia(this Point3D point) => new Point(point.X, point.Y);
    
    
    public static PolygonViewModel ToPolygonViewModel(this PointLoop loop, IBrush? fill = null, IBrush? stroke = null,
        double strokeThickness = 1)
    {
        return new PolygonViewModel
        {
            Points = loop.ToItemArray().Select(x => x.ToAvalonia()).ToList(), 
            Fill = fill,
            Stroke = stroke,
            StrokeThickness = strokeThickness,
        };
    }
    
    public static List<Point> ToAvaloniaPoints(this PointLoop loop)
    {
        return loop.ToItemArray().Select(x => x.ToAvalonia()).ToList();
    }
    
    public static ContourViewModel ToViewModel(this Contour contour, IBrush? fill = null, IBrush? stroke = null,
        double strokeThickness = 1)
    {
        var figure = new PathFigure()
        {
            IsClosed = true,
            StartPoint = contour.Head.Point.ToAvalonia(),
            Segments = new PathSegments()
        };

        foreach (var e in contour.Elements)
        {
            switch (e)
            {
                case Segment segment:
                    figure.Segments.Add(new LineSegment() { Point = segment.End.ToAvalonia() });
                    break;
                case Arc arc:
                    figure.Segments.Add(new ArcSegment
                    {
                        Point = arc.End.ToAvalonia(),
                        RotationAngle = 0,
                        IsLargeArc = Math.Abs(arc.Theta) > Math.PI,
                        Size = new Size(arc.Radius, arc.Radius),
                    });
                    break;
            }
        }
        
        var geometry = new PathGeometry() { Figures = new PathFigures() { figure } };
        
        return new ContourViewModel
        {
            Geometry = geometry,
            Fill = fill,
            Stroke = stroke,
            StrokeThickness = strokeThickness,
        };
    }
    
}