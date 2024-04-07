using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public record struct SegIntersection(Segment Segment, double T);