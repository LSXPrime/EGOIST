using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EGOIST.Presentation.UI.Converters;

/// <summary>
/// A value converter that retrieves the names of values in an enum.
/// </summary>
/// <remarks>
/// This converter takes an enum value as input and returns an array of strings representing the names of all values in that enum.
/// </remarks>
public class EnumValuesConverter : IValueConverter
{
    /// <summary>
    /// Converts an enum value to an array of enum value names.
    /// </summary>
    /// <param name="value">The enum value to convert.</param>
    /// <param name="targetType">The target type of the conversion, not used.</param>
    /// <param name="parameter">Optional parameter, not used.</param>
    /// <param name="culture">The culture information, not used.</param>
    /// <returns>An array of strings representing the names of all values in the enum.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Check if the input value is an enum.
        if (value != null && value.GetType().IsEnum)
            return Enum.GetNames(value.GetType()); // Get the names of all values in the enum.

        // Return null if the input value is not an enum.
        return null;
    }

    /// <summary>
    /// Converts a value back, which is not supported by this converter.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">The parameter for the conversion.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>Throws a `NotImplementedException`.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}