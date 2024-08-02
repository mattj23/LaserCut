using LaserCut.Data;
using LaserCut.Geometry;

namespace LaserCut.Avalonia.Models;

public record ImportedGeometry(string FilePath, Body[] Silhouettes, FlatPatches[] Flat);