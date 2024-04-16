using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using LaserCut.Geometry.Primitives;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class LineViewModel : DrawViewModelBase
{
    private Point _start;
    private Point _end;
    private AvaloniaList<double>? _dashArray;
    
    public LineViewModel() : base() {}
    
    public Point Start
    {
        get => _start;
        set => this.RaiseAndSetIfChanged(ref _start, value);
    }
    
    public Point End
    {
        get => _end;
        set => this.RaiseAndSetIfChanged(ref _end, value);
    }
    
    public AvaloniaList<double>? DashArray
    {
        get => _dashArray;
        set => this.RaiseAndSetIfChanged(ref _dashArray, value);
    }

}