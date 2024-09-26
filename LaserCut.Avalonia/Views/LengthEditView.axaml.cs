using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LaserCut.Avalonia.ViewModels;
using ReactiveUI;

namespace LaserCut.Avalonia.Views;

public partial class LengthEditView : UserControl
{
    private IDisposable? _unitChangedSubscription;

    public static readonly StyledProperty<double> MinimumValueProperty =
        AvaloniaProperty.Register<LengthEditView, double>("MinimumValue", double.MinValue);

    public static readonly StyledProperty<double> IncrementValueProperty = AvaloniaProperty.Register<LengthEditView, double>(
        "IncrementValue", 0.1);

    public double IncrementValue
    {
        get => GetValue(IncrementValueProperty);
        set => SetValue(IncrementValueProperty, value);
    }

    public double MinimumValue
    {
        get => GetValue(MinimumValueProperty);
        set => SetValue(MinimumValueProperty, value);
    }

    public LengthEditView()
    {
        InitializeComponent();
        // ValueControl.TextConverter
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        _unitChangedSubscription?.Dispose();

        if (DataContext is LengthEditViewModel vm)
        {
            // vm.Units.WhenAnyValue(x => x.Unit)
            //     .Subscribe()

            var converter = new LengthTextConverter(vm.Units);
            ValueControl.TextConverter = converter;


        }
        base.OnDataContextChanged(e);
    }
}
