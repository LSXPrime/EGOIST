using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EGOIST.Presentation.UI.Converters;

public class DictionaryValueConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [string key, IDictionary dictionary] || !dictionary.Contains(key))
            return null;

        return dictionary[key];
    }
}