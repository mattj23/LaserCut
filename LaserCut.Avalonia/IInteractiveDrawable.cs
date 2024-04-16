using LaserCut.Geometry;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia;

public interface IInteractiveDrawable : IDrawable
{
    bool Contains(Point2D point);
    
    void MouseEnter();
    void MouseExit();

    bool IsDraggable { get; }
    
    /// <summary>
    /// The drag transform is called when the mouse is moved while dragging. The client should return the actual delta
    /// which was applied to the object, which the parent will use to update the last drag reference. This allows for
    /// things like snapping to a grid or other objects without the mouse interaction feeling glitchy.
    /// </summary>
    /// <param name="delta">The change in position that the view measured from the user's dragging gesture</param>
    /// <returns>The actual change in position which the object applied to itself</returns>
    Vector2D DragTransform(Vector2D delta);
    
    void MouseClick(MouseViewportEventArgs e);
    
}