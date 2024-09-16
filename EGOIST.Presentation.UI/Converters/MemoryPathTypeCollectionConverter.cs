using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;

namespace EGOIST.Presentation.UI.Converters;

public class MemoryPathTypeCollectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ObservableCollection<Citation> citations || parameter is not string type)
            return new ObservableCollection<Citation>();
        
        return type.ToLowerInvariant() switch
        {
            "file" => citations.Where(x => Uri.TryCreate(x.Path, UriKind.Absolute, out var uri) && uri.IsFile).ToObservableCollection(),
            "url" => citations.Where(x => Uri.TryCreate(x.Path, UriKind.Absolute, out var uri) && !uri.IsFile && !uri.AbsoluteUri.Contains("youtube.com")).ToObservableCollection(),
            "youtube" => citations.Where(x => Uri.TryCreate(x.Path, UriKind.Absolute, out var uri) && !uri.IsFile && uri.AbsoluteUri.Contains("youtube.com")).ToObservableCollection(),
            _ => citations
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
