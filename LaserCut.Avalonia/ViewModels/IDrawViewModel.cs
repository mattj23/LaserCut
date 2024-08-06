using Avalonia.Media;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Avalonia.ViewModels;

/// <summary>
/// A view model for geometry displayed in a drawn viewport
/// </summary>
public interface IDrawViewModel: IHasGuid
{
    Guid Id { get; }
    
    IBrush? Stroke { get; }
    IBrush? Fill { get; }
    
    double StrokeThickness { get; set; }
    
    bool IsVisible { get; }
    
    double DisplayThickness { get; }
    
    void UpdateZoom(double zoom);
    
}