using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia;

public interface IInteractiveDrawable : IDrawable
{
    bool Contains(Point2D point);
    
    void MouseEnter();
    void MouseExit();

    bool IsDraggable { get; }
    
    void DragTransform(Vector2D delta);
    
}