

using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data;
using Avalonia.Data.Converters;
using LaserCut;
using LaserCut.Avalonia.ViewModels;

public class LengthTextConverter : IValueConverter
{
    private static readonly Regex Pattern = new Regex(@"^\s*(\d*\.?\d*)\s*([A-Za-z]*)");

    private readonly LengthEditViewModel _vm;

    public LengthTextConverter(LengthEditViewModel vm)
    {
        _vm = vm;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // The string comes in here, target type out is a decimal
        if (value is not string str)
        {
            return new BindingNotification(new ArgumentException("Invalid length string"), BindingErrorType.DataValidationError);
        }

        // First, we figure out the suffix.  If it's "inch", "in", or "mm", we will use that.  If there is no suffix,
        // we will use the default suffix for the unit.  If there's text which is not a number, we throw an error.
        var match = Pattern.Match(str);

        var number = (double)decimal.Parse(match.Groups[1].Value);

        if (string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return number;
        }

        return match.Groups[2].Value switch
        {
            "in" => (decimal)_vm.Units.MmToUnit(number * 25.4),
            "inch" => (decimal)_vm.Units.MmToUnit(number * 25.4),
            "mm" => (decimal)_vm.Units.MmToUnit(number),
            _ => new BindingNotification(new ArgumentException("Invalid length string"), BindingErrorType.DataValidationError),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // The numeric value comes in here
        var format = $"F{_vm.GetDecimalPlaces()}";
        return string.Format("{0:" + format + "} {1}", value, _vm.Units.Suffix);
    }
}
