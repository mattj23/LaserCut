using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Media;
using LaserCut.Geometry.Primitives;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class BedLimitsViewModel : ReactiveObject, IDrawable
{
    private readonly List<IDrawViewModel> _geometries = new();
    private readonly Subject<IDrawViewModel> _added = new();
    private readonly Subject<IDrawViewModel> _removed = new();
    
    public BedLimitsViewModel(double height, double width)
    {
        Id = Guid.NewGuid();
        Bounds = Aabb2.Empty;

        var zero = 0.001;
        
        AddLine(zero, zero, zero, height);
        AddLine(zero, height, width, height);
        AddLine(width, height, width, zero);
        AddLine(width, zero, zero, zero);
    }
    
    public Guid Id { get; }
    public IReadOnlyList<IDrawViewModel> Geometries => _geometries;
    public Aabb2 Bounds { get; }
    public IObservable<IDrawViewModel> Added => _added.AsObservable();
    public IObservable<IDrawViewModel> Removed => _removed.AsObservable();
    public bool UseBounds => false;

    private void AddLine(double x0, double y0, double x1, double y1)
    {
        var line = new LineViewModel
        {
            Start = new Point(x0, y0),
            IsVisible = true,
            End = new Point(x1, y1),
            Stroke = Brushes.Gray,
            StrokeThickness = 1.5,
            DashArray = [4.0, 4.0]
        };
        
        _geometries.Add(line);
        _added.OnNext(line);
    }
}