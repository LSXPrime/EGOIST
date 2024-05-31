using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EGOIST.Presentation.UI.Helpers;
public class EqualBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return parameter is string str ? (str.StartsWith("!") ? value != null : (string)value == str) : value != parameter;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
