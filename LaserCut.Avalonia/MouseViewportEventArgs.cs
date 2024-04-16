using Avalonia.Input;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia;

public record MouseViewportEventArgs(
    Point2D Point,
    KeyModifiers Modifiers,
    bool LeftButton,
    bool RightButton,
    bool MiddleButton);
