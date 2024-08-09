using System.ComponentModel;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia.Models;

public enum AnchorLocation
{
    [Description("Center")]
    Center = 0,
    
    [Description("Top Left")]
    TopLeft = 1,
    
    [Description("Top Center")]
    TopCenter = 2,
    
    [Description("Top Right")]
    TopRight = 3,
    
    [Description("Bottom Left")]
    BottomLeft = 4,
    
    [Description("Bottom Center")]
    BottomCenter = 5,
    
    [Description("Bottom Right")]
    BottomRight = 6,
    
    [Description("Left Center")]
    LeftCenter = 7,
    
    [Description("Right Center")]
    RightCenter = 8,
}

public static class AnchorLocationExtensions
{
    public static Point2D ToXcYc(this AnchorLocation a, double width, double height)
    {
        return a switch
        {
            AnchorLocation.Center => new Point2D(0, 0),
            AnchorLocation.TopLeft => new Point2D(width / 2, height / 2),
            AnchorLocation.TopCenter => new Point2D(0, height / 2),
            AnchorLocation.TopRight => new Point2D(-width / 2, height / 2),
            AnchorLocation.BottomLeft => new Point2D(width / 2, -height / 2),
            AnchorLocation.BottomCenter => new Point2D(0, -height / 2),
            AnchorLocation.BottomRight => new Point2D(-width / 2, -height / 2),
            AnchorLocation.LeftCenter => new Point2D(width / 2, 0),
            AnchorLocation.RightCenter => new Point2D(-width / 2, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(a), a, null)
        };
    }
    
    /// <summary>
    /// Gets the X0, Y0 point for the specified anchor location in relation to a rectangle with the specified width and
    /// height, where X0, Y0 is the top left corner of the rectangle and Y increases downward.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Point2D ToX0Y0(this AnchorLocation a, double width, double height)
    {
        return a switch
        {
            AnchorLocation.Center => new Point2D(-width / 2, -height / 2),
            AnchorLocation.TopLeft => new Point2D(0, 0),
            AnchorLocation.TopCenter => new Point2D(-width / 2, 0),
            AnchorLocation.TopRight => new Point2D(-width, 0),
            AnchorLocation.BottomLeft => new Point2D(0, -height),
            AnchorLocation.BottomCenter => new Point2D(-width / 2, -height),
            AnchorLocation.BottomRight => new Point2D(-width, -height),
            AnchorLocation.LeftCenter => new Point2D(0, -height / 2),
            AnchorLocation.RightCenter => new Point2D(-width, -height / 2),
            _ => throw new ArgumentOutOfRangeException(nameof(a), a, null)
        };
    }
}