using MathNet.Spatial.Euclidean;

namespace LaserCut.Text;

public class EtchText
{
    
    public string Text { get; set; }
    public string FontFamily { get; set; }
    public double EmSize { get; set; }
    public Point2D Position { get; set; }
}