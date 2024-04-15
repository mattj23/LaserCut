using Avalonia;
using Avalonia.Media;
using LaserCut.Geometry.Primitives;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class PolygonViewModel : ReactiveObject, IDrawViewModel
{
    private double _zoom = 1;
    private IBrush? _fill;
    private IBrush? _stroke;
    private IList<Point> _points;
    private double _strokeThickness;
    private Aabb2 _bounds;

    public PolygonViewModel()
    {
        _points = new List<Point>();
        Id = Guid.NewGuid();
    }
    
    public Guid Id { get; }
    
    public double DisplayThickness => StrokeThickness / _zoom;

    public void UpdateZoom(double zoom)
    {
        _zoom = zoom;
        this.RaisePropertyChanged(nameof(DisplayThickness));
    }

    public Aabb2 Bounds
    {
        get => _bounds;
        set => this.RaiseAndSetIfChanged(ref _bounds, value);
    }

    public double StrokeThickness
    {
        get => _strokeThickness;
        set
        {
            this.RaiseAndSetIfChanged(ref _strokeThickness, value);
            this.RaisePropertyChanged(nameof(DisplayThickness));
        }
    }

    public IList<Point> Points
    {
        get => _points;
        set => this.RaiseAndSetIfChanged(ref _points, value);
    }

    public IBrush? Fill
    {
        get => _fill;
        set => this.RaiseAndSetIfChanged(ref _fill, value);
    }
    
    public IBrush? Stroke
    {
        get => _stroke;
        set => this.RaiseAndSetIfChanged(ref _stroke, value);
    }
}