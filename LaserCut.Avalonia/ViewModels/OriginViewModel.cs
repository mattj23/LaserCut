using Avalonia;
using Avalonia.Media;
using LaserCut.Geometry;
using ReactiveUI;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;

namespace LaserCut.Avalonia.ViewModels;

public class OriginViewModel : ReactiveObject, IDrawViewModel
{
    protected double ZoomValue = 1;
    private bool _isVisible;
    private double _strokeThickness = 3.0;
    private Xyr _xyr;
    private ITransform _transform;

    public OriginViewModel(Guid id, Matrix transform)
    {
        Id = id;
        _xyr = Xyr.FromMatrix(transform);
        _transform = MakeTransform(_xyr, ZoomValue);
        IsVisible = true;
    }

    public Guid Id { get; }
    public IBrush? Stroke { get; set; }
    
    public IBrush? Fill => null;

    public double StrokeThickness
    {
        get => _strokeThickness;
        set => this.RaiseAndSetIfChanged(ref _strokeThickness, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public ITransform Transform
    {
        get => _transform;
        set => this.RaiseAndSetIfChanged(ref _transform, value);
    }

    public double DisplayThickness => StrokeThickness;

    public void Update(Matrix transform)
    {
        _xyr = Xyr.FromMatrix(transform);
        Transform = MakeTransform(_xyr, ZoomValue);
    }
    
    public void UpdateZoom(double zoom)
    {
        ZoomValue = zoom;
        this.RaisePropertyChanged(nameof(DisplayThickness));
        Transform = MakeTransform(_xyr, ZoomValue);
    }

    private static Transform MakeTransform(Xyr xyr, double zoom)
    {
        return new TransformGroup()
        {
            Children =
            [
                new ScaleTransform(1 / zoom, 1 / zoom),
                new RotateTransform(double.RadiansToDegrees(xyr.R), 0, 0),
                new TranslateTransform(xyr.X, xyr.Y),
            ]
        };
    }
}