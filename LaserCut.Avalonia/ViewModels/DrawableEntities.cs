using System.Collections.ObjectModel;
using LaserCut.Geometry.Primitives;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class DrawableEntities : ReactiveObject
{
    public ObservableCollection<IDrawViewModel> Geometries { get; } = new();
    
    
    public Aabb2 GetBounds()
    {
        var bounds = Aabb2.Empty;
        foreach (var geometry in Geometries)
        {
            bounds = bounds.Union(geometry.Bounds);
        }

        return bounds;
    }
    
    public void UpdateZoom(double zoom)
    {
        foreach (var geometry in Geometries)
        {
            geometry.UpdateZoom(zoom);
        }
    }
    
}