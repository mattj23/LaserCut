using LaserCut.Algorithms;

namespace LaserCut.Tests.Helpers;

public static class IntersectionPairHelpers
{
    public static bool HasIndices(this IntersectionPair pair, int i0, int i1)
    {
        return (pair.First.Element.Index == i0 && pair.Second.Element.Index == i1) || 
               (pair.First.Element.Index == i1 && pair.Second.Element.Index == i0);
    }
    
}