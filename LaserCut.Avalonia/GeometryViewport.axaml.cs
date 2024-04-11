using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using LaserCut.Avalonia.ViewModels;

namespace LaserCut.Avalonia;

public partial class GeometryViewport : UserControl
{
    public static readonly StyledProperty<DrawableEntities> EntitiesProperty =
        AvaloniaProperty.Register<GeometryViewport, DrawableEntities>(nameof(Entities), new DrawableEntities());

    public static readonly StyledProperty<double> ZoomToFitScaleProperty =
        AvaloniaProperty.Register<GeometryViewport, double>(nameof(ZoomToFitScale), 1.5);

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

    public GeometryViewport()
    {
        InitializeComponent();
    }

    private void ViewPort_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        Entities.UpdateZoom(e.ZoomX);
    }

    public void ZoomToFit()
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
    }
}