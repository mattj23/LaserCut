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

    public double MinimumValue
    {
        get => GetValue(MinimumValueProperty);
        set => SetValue(MinimumValueProperty, value);
    }

    public LengthEditView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        _unitChangedSubscription?.Dispose();

        if (DataContext is LengthEditViewModel vm)
        {
            var converter = new LengthTextConverter(vm);
            ValueControl.TextConverter = converter;
            ValueControl.Increment = (decimal)vm.GetIncrement();

            _unitChangedSubscription = vm.WhenAnyValue(x => x.Units.Unit)
                .Subscribe(_ => OnUnitsChanged());

        }

        base.OnDataContextChanged(e);
    }

    private void OnUnitsChanged()
    {
        ValueControl.Increment = (decimal)((LengthEditViewModel)DataContext!).GetIncrement();
    }
}
