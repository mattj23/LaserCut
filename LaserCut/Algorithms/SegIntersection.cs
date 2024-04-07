using LaserCut.Geometry.Primitives;

namespace LaserCut.Algorithms;

public record struct SegIntersection(Segment Segment, double T);

public record struct SegPairIntersection(Segment Segment0, double T0, Segment Segment1, double T1);