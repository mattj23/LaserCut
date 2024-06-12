using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Media;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Avalonia.Sample.ViewModels;

public class Drawable : IDrawable
{
    private readonly List<IDrawViewModel> _geometries = new();
    private readonly Subject<IDrawViewModel> _added = new();
    private readonly Subject<IDrawViewModel> _removed = new();
    
    public Drawable()
    {
        Id = Guid.NewGuid();
        
        var box = PointLoop.Rectangle(100, 200);
        box.Translate(125, 75);
        
        Add(box.ToPolygonViewModel(null, new SolidColorBrush(Colors.Black)));
    }
    
    public Guid Id { get; }
    
    public IReadOnlyList<IDrawViewModel> Geometries => _geometries;
    
    public Aabb2 Bounds { get; private set; }
    
    public IObservable<IDrawViewModel> Added => _added.AsObservable();
    
    public IObservable<IDrawViewModel> Removed => _removed.AsObservable();
    
    private void Add(IDrawViewModel geometry)
    {
        _geometries.Add(geometry);
        _added.OnNext(geometry);
    }
    
    private void Remove(IDrawViewModel geometry)
    {
        _geometries.Remove(geometry);
        _removed.OnNext(geometry);
    }
}