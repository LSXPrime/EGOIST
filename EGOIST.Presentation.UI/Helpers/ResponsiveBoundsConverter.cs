using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EGOIST.Presentation.UI.Helpers;

/// <summary>
/// A value converter for responsive bounds calculations.
/// </summary>
/// <remarks>
/// This converter scales a target value based on the screen width, maintaining proportions.
/// The target value is typically a dimension (e.g., width or height) of a UI element.
/// </remarks>
public class ResponsiveBoundsConverter : IValueConverter
{
    /// <summary>
    /// Converts a value to a responsive bound.
    /// </summary>
    /// <param name="value">The target value to be scaled.</param>
    /// <param name="targetType">The target type of the conversion, not used.</param>
    /// <param name="parameter">Optional parameter, which can override the target value.</param>
    /// <param name="culture">The culture information, not used.</param>
    /// <returns>The scaled responsive bound.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Get the target value, using the parameter if provided.
        var targetValue = double.Parse(string.IsNullOrEmpty(parameter?.ToString()) ? value?.ToString() ?? "0" : parameter.ToString());

        // Define the reference screen width.
        const double referenceScreenWidth = 1920.0;

        // Calculate the scaled value based on the screen width.
        var scaledValue = targetValue / referenceScreenWidth * 1920;// System.Windows.SystemParameters.PrimaryScreenWidth;

        // Log the values for debugging.
    //    Debug.WriteLine($"Target {targetValue}, Screen {System.Windows.SystemParameters.PrimaryScreenWidth}, Final {scaledValue}");

        // Return the scaled value or the original value if the scaled value is zero or negative.
        return scaledValue > 0 ? scaledValue : targetValue;
    }

    /// <summary>
    /// Converts a value back, which is not supported by this converter.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">The parameter for the conversion.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>Throws a `NotImplementedException`.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}