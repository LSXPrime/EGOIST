using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EGOIST.Presentation.UI.Helpers
{
    /// <summary>
    /// A value converter that checks a condition and returns a value based on the result.
    /// </summary>
    /// <remarks>
    /// This converter takes a value as input and checks if it matches a specified condition.
    /// It returns one value if the condition is true and another value if it's false.
    /// </remarks>
    public class ConditionCheckConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value based on a condition.
        /// </summary>
        /// <param name="value">The value to check against the condition.</param>
        /// <param name="targetType">The target type of the conversion, not used.</param>
        /// <param name="parameter">A string containing the condition, true value, and false value, separated by "|" (pipe character). Example: "condition|trueValue|falseValue".</param>
        /// <param name="culture">The culture information, not used.</param>
        /// <returns>The true value if the condition is met, otherwise the false value.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string conditionString)
            {
                throw new ArgumentException("Parameter is not a valid condition string.");
            }

            // Split the condition string into parts.
            var parts = conditionString.Split('|', StringSplitOptions.RemoveEmptyEntries);

            // Check if the condition is met.
            if (parts.Length >= 3)
            {
                var conditionValue = parts[0];
                var trueValue = parts[1];
                var falseValue = parts[2];

                // Compare the value to the condition.
                var inputValue = value?.GetType() == typeof(string) ? (string)value : value?.ToString();
                return inputValue == conditionValue ? trueValue : falseValue;
            }

            throw new ArgumentException("Invalid condition string format. Expected 'condition|trueValue|falseValue'.");
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
}