using Avalonia.Media;
using LaserCut.Geometry;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels.Etch;

public abstract class EtchEntityViewModelBase : ReactiveObject, IDrawViewModel
{
    private bool _isVisible;
    private bool _isSuppressed;
    private double _strokeThickness;
    private IBrush? _stroke;
    private IBrush? _fill;
    protected Xyr ParentXyr;
    
    protected EtchEntityViewModelBase(Guid id)
    {
        Id = id;
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public virtual double DisplayThickness => StrokeThickness;

    public virtual bool IsSuppressed
    {
        get => _isSuppressed;
        set => this.RaiseAndSetIfChanged(ref _isSuppressed, value);
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

    public virtual double StrokeThickness
    {
        get => _strokeThickness;
        set => this.RaiseAndSetIfChanged(ref _strokeThickness, value);
    }
    
    public void UpdateParentXyr(Xyr xyr)
    {
        ParentXyr = xyr;
        OnParentXyrChanged();
    }
    
    public abstract void OnParentXyrChanged();
    
    public abstract void UpdateZoom(double zoom);
}