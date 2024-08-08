using System.Collections.ObjectModel;
using LaserCut.Algorithms;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry.Primitives;

namespace LaserCut.Avalonia;

/// <summary>
/// An entity which exposes a set of geometries to be drawn
/// </summary>
public interface IDrawable : IHasBounds
{
    Guid Id { get; }
    
    IReadOnlyList<IDrawViewModel> Geometries { get; }
    
    Aabb2 Bounds { get; }
    
    IObservable<IDrawViewModel> Added { get; }
    IObservable<IDrawViewModel> Removed { get; }
    
    /// <summary>
    /// Gets a value indicating whether the bounds of the drawable should be used for zoom to fit calculations
    /// </summary>
    bool UseBounds { get; }
    
}