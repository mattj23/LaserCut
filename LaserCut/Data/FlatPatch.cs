using LaserCut.Geometry;

namespace LaserCut.Data;

public record FlatPatch(double Height, Body Body)
{
    public FlatPatch MirrorY()
    {
        return this with { Body = Body.MirroredY() };
    }
}
