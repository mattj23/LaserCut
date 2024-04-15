using LaserCut.Avalonia.ViewModels;
using LaserCut.Geometry;

namespace LaserCut.Avalonia;

public record LoopLink(PointLoop Loop, IDrawViewModel ViewModel);