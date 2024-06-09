using Avalonia.Data.Converters;
using System;
using System.Globalization;
using FluentIcons.Common;

namespace EGOIST.Presentation.UI.Helpers;

/// <summary>
/// Converts a string condition to a FluentIcons symbol.
/// </summary>
public class ConditionIconConverter : IValueConverter
{
    /// <summary>
    /// Converts a string condition to a FluentIcons symbol.
    /// </summary>
    /// <param name="value">The string condition to convert.</param>
    /// <param name="targetType">The type of the target object.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>The corresponding FluentIcons symbol.</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not a valid condition string.</exception>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string conditionString)
        {
            throw new ArgumentException("Parameter is not a valid condition string.");
        }

        return conditionString.ToLower() switch
        {
            "system" => Symbol.System,
            "user" => Symbol.Person,
            "assistant" or "bot" or "ai" => Symbol.Bot,
            _ => Symbol.Warning
        };
    }

    /// <summary>
    /// Not implemented, as conversion from FluentIcons symbol to string condition is not required.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The type of the target object.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>Not implemented.</returns>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}