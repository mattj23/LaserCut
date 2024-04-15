using System.Collections.ObjectModel;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Avalonia;

/// <summary>
/// An entity which exposes a set of geometries to be drawn
/// </summary>
public interface IDrawable
{
    Guid Id { get; }
    
    IReadOnlyList<IDrawViewModel> Geometries { get; }
    
    Aabb2 Bounds { get; }
    
    IObservable<IDrawViewModel> Added { get; }
    IObservable<IDrawViewModel> Removed { get; }
}