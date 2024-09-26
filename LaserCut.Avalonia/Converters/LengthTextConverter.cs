

using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data;
using Avalonia.Data.Converters;
using LaserCut;
using LaserCut.Avalonia.ViewModels;

public class LengthTextConverter : IValueConverter
{
    private readonly UnitViewModel _units;
    private readonly Regex _pattern;

    public LengthTextConverter(UnitViewModel units)
    {
        _units = units;
        _pattern = new Regex(@"^\s*(\d*\.?\d*)\s*([A-Za-z]*)");
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
        var match = _pattern.Match(str);

        var number = (double)decimal.Parse(match.Groups[1].Value);

        if (string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return number;
        }

        return match.Groups[2].Value switch
        {
            "in" => (decimal)_units.MmToUnit(number * 25.4),
            "inch" => (decimal)_units.MmToUnit(number * 25.4),
            "mm" => (decimal)_units.MmToUnit(number),
            _ => new BindingNotification(new ArgumentException("Invalid length string"), BindingErrorType.DataValidationError),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // The numeric value comes in here
        var format = _units.Unit switch
        {
            LengthUnit.Millimeter => "0.00",
            LengthUnit.Inch => "0.000",
            _ => "0.000"
        };

        return string.Format("{0:" + format + "} {1}", value, _units.Suffix);
    }
}
