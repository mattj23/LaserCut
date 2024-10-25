﻿using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using DynamicData;
using LaserCut.Geometry.Primitives;
using MathNet.Spatial.Euclidean;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class DrawableEntities : ReactiveObject
{
    private double _lastZoom = 1.0;
    private readonly ObservableCollection<IDrawViewModel> _geometries = new();
    private readonly Dictionary<Guid, RegisteredDrawable> _drawables = new();
    private readonly Subject<(MouseViewportEventArgs, IInteractiveDrawable)> _pointerPressed = new();

    private Guid _activeDrawable = Guid.Empty;
    private Point2D? _lastDragReference;
    private bool _dragStarted;

    public ReadOnlyObservableCollection<IDrawViewModel> Geometries => new(_geometries);

    public IObservable<(MouseViewportEventArgs, IInteractiveDrawable)> PointerPressed => _pointerPressed;

    public double DragMinimumThreshold { get; set; } = 3.0;

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
        foreach (var ge in drawable.Geometries)
        {
            ge.UpdateZoom(_lastZoom);
        }
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

    /// <summary>
    /// Unregisters all drawables and clears all visible geometry
    /// </summary>
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
            if (item.Drawable.UseBounds)
                bounds = bounds.Union(item.Drawable.Bounds);
        }

        return bounds;
    }

    public void UpdateZoom(double zoom)
    {
        _lastZoom = zoom;
        foreach (var geometry in Geometries)
        {
            geometry.UpdateZoom(_lastZoom);
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
        else if (_lastDragReference is { } lastDrag)
        {
            if (!_dragStarted && lastDrag.DistanceTo(e.Point) > DragMinimumThreshold)
            {
                _dragStarted = true;
            }

            if (_dragStarted && GetInteractive(_activeDrawable) is { } interactive)
            {
                _lastDragReference += interactive.DragTransform(e.Point - lastDrag);
            }
        }
    }

    public void OnPointerPressed(MouseViewportEventArgs e)
    {
        // Are we clicking on any draggable objects
        if (GetInteractive(_activeDrawable) is { } interactive)
        {
            if (e.LeftButton && interactive.IsDraggable)
            {
                // Initiate dragging mechanics
                _dragStarted = false;
                _lastDragReference = e.Point;
            }
            else
            {
                interactive.MouseClick(e);
            }

            _pointerPressed.OnNext((e, interactive));
        }

    }

    public void OnPointerReleased(MouseViewportEventArgs e)
    {
        _lastDragReference = null;
    }

    public void OnPointerExited()
    {
        if (GetInteractive(_activeDrawable) is { } interactive)
        {
            interactive.MouseExit();
        }
        _activeDrawable = Guid.Empty;
        _lastDragReference = null;
    }

    private Guid InteractiveUnderPoint(Point2D p)
    {
        // We want to find the interactive object with the smallest hit area that contains the point.  This will
        // prevent larger objects from taking precedence over smaller ones.
        var smallest = double.MaxValue;
        var smallestId = Guid.Empty;
        foreach (var i in IterateInteractive())
        {
            if (i.HitArea < smallest && i.Contains(p))
            {
                smallest = i.HitArea;
                smallestId = i.Id;
            }
        }

        return smallestId;
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
        geometry.UpdateZoom(_lastZoom);
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
