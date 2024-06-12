using Avalonia;
using Avalonia.Media;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class ContourViewModel : DrawViewModelBase
{
    private PathGeometry _geometry = new();
    
    public ContourViewModel() : base()
    {

    }
    
    public PathGeometry Geometry
    {
        get => _geometry;
        set => this.RaiseAndSetIfChanged(ref _geometry, value);
    }
}