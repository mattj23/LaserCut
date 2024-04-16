using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class DrawableEntities : ReactiveObject
{
    private readonly ObservableCollection<IDrawViewModel> _geometries = new();
    private readonly Dictionary<Guid, RegisteredDrawable> _drawables = new();
    
    private Guid _activeDrawable = Guid.Empty;

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
    
    public void UnRegister(IDrawable drawable)
    {
        if (!_drawables.TryGetValue(drawable.Id, out var registered))
        {
            return;
        }

        registered.AddSub.Dispose();
        registered.RemoveSub.Dispose();
        foreach (var geometry in drawable.Geometries)
        {
            _geometries.Remove(geometry);
        }
        _drawables.Remove(drawable.Id);
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

    public void OnPointerMoved(MouseViewportEventArgs e)
    {
        // If no mouse buttons are pressed, we're just hovering
        if (!e.LeftButton)
        {
            var nextActive = InteractiveUnderPoint(e.Point);
            
            // The active drawable is changing, so let's update the items
            if (nextActive != _activeDrawable)
            {
                if (GetInteractive(_activeDrawable) is { } interactive)
                {
                    interactive.MouseExit();
                }
                if (GetInteractive(nextActive) is { } nextInteractive)
                {
                    nextInteractive.MouseEnter();
                }
                _activeDrawable = nextActive;
            }
        }
    }

    public void OnPointerPressed(MouseViewportEventArgs e)
    {
        // Are we clicking on any draggable objects
        
    }
    
    public void OnPointerReleased(MouseViewportEventArgs e)
    {
        
    }
    
    public void OnPointerExited()
    {
        if (GetInteractive(_activeDrawable) is { } interactive)
        {
            interactive.MouseExit();
        }
        _activeDrawable = Guid.Empty;
    }

    private Guid InteractiveUnderPoint(Point2D p)
    {
        if (GetInteractive(_activeDrawable) is { } interactive && interactive.Contains(p))
        {
            return _activeDrawable;
        }
        
        foreach (var i in IterateInteractive())
        {
            if (i.Contains(p))
            {
                return i.Id;
            }
        }
        
        return Guid.Empty;
    }

    private IEnumerable<IInteractiveDrawable> IterateInteractive()
    {
        foreach (var (_, item) in _drawables)
        {
            if (item.Drawable is IInteractiveDrawable interactive)
            {
                yield return interactive;
            }
        }
    }

    private IInteractiveDrawable? GetInteractive(Guid id)
    {
        if (_drawables.TryGetValue(id, out var drawable))
        {
            return drawable.Drawable as IInteractiveDrawable;
        }
        
        return null;
    }
    
    
    private void GeometryAdded(IDrawViewModel geometry)
    {
        _geometries.Add(geometry);
    }
    
    private void GeometryRemoved(IDrawViewModel geometry)
    {
        var i = _geometries.IndexOf(geometry);
        if (i >= 0)
        {
            _geometries.RemoveAt(i);
        }
    }
    
    private record RegisteredDrawable(IDrawable Drawable, IDisposable AddSub, IDisposable RemoveSub);
}