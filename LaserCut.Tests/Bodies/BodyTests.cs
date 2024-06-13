using LaserCut.Geometry;

namespace LaserCut.Tests.Bodies;

public class BodyTests
{
    [Fact]
    public void CorrectArea()
    {
        var body = new Body(Contour.Rectangle(0, 0, 3, 3), [Contour.Rectangle(1, 1, 1, 1).Reversed()]);
        Assert.Equal(8, body.Area, 10);
    }
    
}