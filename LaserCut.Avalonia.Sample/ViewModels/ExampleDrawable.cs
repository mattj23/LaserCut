﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Media;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Avalonia.Sample.ViewModels;

public class ExampleDrawable : IDrawable
{
    private readonly List<IDrawViewModel> _geometries = new();
    private readonly Subject<IDrawViewModel> _added = new();
    private readonly Subject<IDrawViewModel> _removed = new();

    public ExampleDrawable()
    {
        Id = Guid.NewGuid();

        var circle = BoundaryLoop.Circle(200, 200, 150);
        // var loop = new Contour();
        // var cursor = loop.GetCursor();
        // cursor.ArcAbs(100, 300, 200, 300, false);
        // cursor.ArcAbs(300, 300, 200, 300, false);

        Add(circle.ToViewModel(null, Brushes.Black, 2));
    }

    public Guid Id { get; }

    public IReadOnlyList<IDrawViewModel> Geometries => _geometries;

    public Aabb2 Bounds { get; private set; }

    public IObservable<IDrawViewModel> Added => _added.AsObservable();

    public IObservable<IDrawViewModel> Removed => _removed.AsObservable();
    public bool UseBounds => true;

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
