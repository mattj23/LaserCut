using Avalonia.Media;
using LaserCut.Geometry.Primitives;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

/// <summary>
/// A base class for view models that represent a drawable object
/// </summary>
public abstract class DrawViewModelBase : ReactiveObject, IDrawViewModel
{
    protected double ZoomValue = 1;
    protected Aabb2 BoundsValue;

    private IBrush? _fill;
    private IBrush? _stroke;
    private double _strokeThickness;

    protected DrawViewModelBase()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }

    public IBrush? Stroke
    {
        get => _stroke;
        set => this.RaiseAndSetIfChanged(ref _stroke, value);
    }

    public IBrush? Fill
    {
        get => _fill;
        set => this.RaiseAndSetIfChanged(ref _fill, value);
    }

    public double DisplayThickness => StrokeThickness / ZoomValue;

    public void UpdateZoom(double zoom)
    {
        ZoomValue = zoom;
        this.RaisePropertyChanged(nameof(DisplayThickness));
    }

    public Aabb2 Bounds
    {
        get => BoundsValue;
        set => this.RaiseAndSetIfChanged(ref BoundsValue, value);
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

}