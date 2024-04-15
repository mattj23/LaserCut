using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using LaserCut.Geometry.Primitives;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class DrawableEntities : ReactiveObject
{
    private readonly ObservableCollection<IDrawViewModel> _geometries = new();
    private readonly Dictionary<Guid, RegisteredDrawable> _drawables = new();

    public ReadOnlyObservableCollection<IDrawViewModel> Geometries => new(_geometries);
    
    public void Register(IDrawable drawable)
    {
        if (_drawables.ContainsKey(drawable.Id))
        {
            return;
        }

        var addSub = drawable.Added
            .Subscribe(GeometryAdded);
        var removeSub = drawable.Removed
            .Subscribe(GeometryRemoved);
        _geometries.AddRange(drawable.Geometries);
        _drawables.Add(drawable.Id, new RegisteredDrawable(drawable, addSub, removeSub));
    }

    public void Clear()
    {
        foreach (var pair in _drawables)
        {
            pair.Value.AddSub.Dispose();
            pair.Value.RemoveSub.Dispose();
        }
        _drawables.Clear();
        _geometries.Clear();
    }
    
    
    public Aabb2 GetBounds()
    {
        var bounds = Aabb2.Empty;
        foreach (var (_, item) in _drawables)
        {
            bounds = bounds.Union(item.Drawable.Bounds);
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
    
    private void GeometryAdded(IDrawViewModel geometry)
    {
        _geometries.Add(geometry);
    }
    
    private void GeometryRemoved(IDrawViewModel geometry)
    {
        _geometries.Remove(geometry);
    }
    
    private record RegisteredDrawable(IDrawable Drawable, IDisposable AddSub, IDisposable RemoveSub);
}