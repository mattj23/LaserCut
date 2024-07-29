using LaserCut.Data;
using LaserCut.Geometry;

namespace LaserCut.Avalonia.Models;

public record ImportedGeometry(Body[] Silhouettes, FlatPatches[] Flat);