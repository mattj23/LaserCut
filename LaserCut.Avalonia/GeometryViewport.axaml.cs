using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using LaserCut.Avalonia.ViewModels;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Avalonia;

public partial class GeometryViewport : UserControl
{
    public static readonly StyledProperty<DrawableEntities> EntitiesProperty =
        AvaloniaProperty.Register<GeometryViewport, DrawableEntities>(nameof(Entities), new DrawableEntities());

    public static readonly StyledProperty<double> ZoomToFitScaleProperty =
        AvaloniaProperty.Register<GeometryViewport, double>(nameof(ZoomToFitScale), 1.5);
    
    public GeometryViewport()
    {
        InitializeComponent();
    }
    
    public DrawableEntities Entities
    {
        get => GetValue(EntitiesProperty);
        set => SetValue(EntitiesProperty, value);
    }

    public double ZoomToFitScale
    {
        get => GetValue(ZoomToFitScaleProperty);
        set => SetValue(ZoomToFitScaleProperty, value);
    }


    private void ViewPort_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        Entities.UpdateZoom(e.ZoomX);
    }

    public async Task ZoomToFit()
    {
        var bounds = Entities.GetBounds();
        if (bounds.IsEmpty)
            return;

        var workingX = bounds.Width * ZoomToFitScale + 1.0;
        var workingY = bounds.Height * ZoomToFitScale + 1.0;
        var zx = ViewPort.Bounds.Width / workingX;
        var zy = ViewPort.Bounds.Height / workingY;
        var zoom = Math.Min(zx, zy);

        // Figure out the offset to center the box in the viewport
        var offsetX = bounds.Center.X - ViewPort.Bounds.Width / (2 * zoom);
        var offsetY = bounds.Center.Y - ViewPort.Bounds.Height / (2 * zoom);

        ViewPort.Zoom(zoom, offsetX, offsetY);
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        Entities.UpdateZoom(zoom);
    }

    private void ViewCanvas_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
    }

    private void ViewCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var p = e.GetCurrentPoint(ViewPort);
        Entities.OnPointerMoved(BuildEvent(p, e.KeyModifiers));
    }

    private void ViewCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var p = e.GetCurrentPoint(ViewPort);
        Entities.OnPointerMoved(BuildEvent(p, e.KeyModifiers));
    }

    private void ViewCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        
        var p = e.GetCurrentPoint(ViewPort);
        Entities.OnPointerMoved(BuildEvent(p, e.KeyModifiers));
    }

    private void ViewCanvas_OnPointerExited(object? sender, PointerEventArgs e)
    {
        Entities.OnPointerExited();
    }

    private MouseViewportEventArgs BuildEvent(PointerPoint p, KeyModifiers m)
    {
        var x = (p.Position.X - ViewPort.OffsetX) / ViewPort.ZoomX;
        var y = (p.Position.Y - ViewPort.OffsetY) / ViewPort.ZoomY;

        return new MouseViewportEventArgs(new Point2D(x, y), m, p.Properties.IsLeftButtonPressed,
            p.Properties.IsRightButtonPressed, p.Properties.IsMiddleButtonPressed);
    }
}