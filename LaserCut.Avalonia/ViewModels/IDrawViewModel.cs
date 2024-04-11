using Avalonia.Media;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Avalonia.ViewModels;

/// <summary>
/// A view model for geometry displayed in a drawn viewport
/// </summary>
public interface IDrawViewModel
{
    IBrush? Stroke { get; set; }
    IBrush? Fill { get; set; }
    double StrokeThickness { get; set; }
    
    double DisplayThickness { get; }
    
    void UpdateZoom(double zoom);
    
    Aabb2 Bounds { get; }
    
}