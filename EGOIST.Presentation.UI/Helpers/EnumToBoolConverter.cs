using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace EGOIST.Presentation.UI.Helpers
{
    /// <summary>
    /// A value converter that converts an enum value to a boolean, based on a comparison with a specified enum value.
    /// </summary>
    /// <remarks>
    /// This converter takes an enum value as input and compares it to a specified enum value provided as a parameter.
    /// It returns true if the input value matches the specified enum value, otherwise false.
    /// </remarks>
    public class EnumToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts an enum value to a boolean, indicating whether it matches the specified enum value.
        /// </summary>
        /// <param name="value">The enum value to convert.</param>
        /// <param name="targetType">The target type of the conversion, not used.</param>
        /// <param name="parameter">The name of the enum value to compare against, as a string.</param>
        /// <param name="culture">The culture information, not used.</param>
        /// <returns>True if the input value matches the specified enum value, otherwise false.</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string enumValueName)
            {
                throw new ArgumentException("Enum parameter isn't a string.");
            }

            if (value == null || !Enum.IsDefined(value.GetType(), value)) return false;
            var targetEnumValue = Enum.Parse(value.GetType(), enumValueName);
            return targetEnumValue.Equals(value);

        }

        /// <summary>
        /// Converts a boolean value back to the specified enum value.
        /// </summary>
        /// <param name="value">The boolean value to convert back.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">The name of the enum value to convert to, as a string.</param>
        /// <param name="culture">The culture information, not used.</param>
        /// <returns>The enum value represented by the specified enum value name.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is not string enumValueName)
            {
                throw new ArgumentException("Enum parameter isn't a string.");
            }

            return Enum.Parse(targetType, enumValueName);
        }
    }
}