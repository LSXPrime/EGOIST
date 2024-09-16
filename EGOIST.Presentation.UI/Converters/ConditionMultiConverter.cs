using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using YoutubeExplode;

namespace EGOIST.Presentation.UI.Converters;

/// <summary>
/// A multipurpose converter that converts a value based on a specified parameter.
/// </summary>
public class ConditionMultiConverter : IValueConverter
{
    /// <summary>
    /// Converts a value based on the specified parameter.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">The parameter that determines the conversion logic. 
    /// Possible values are:
    ///  - "bitmap": Converts the value to a BitmapImage.
    /// </param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>The converted value, or the original value if the parameter is not recognized.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null)
            return value;

        var parameterString = parameter.ToString()!.ToLowerInvariant();
        return parameterString switch
        {
            "bitmap" => ConvertToBitmap(value),
        //    "subtitleslanguages" => ConvertToSubtitlesLanguage(value?.ToString()).Result,
            _ => value
        };
    }

    /// <summary>
    /// Converts the value to a BitmapImage.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted BitmapImage, or the original value if the conversion is not possible.</returns>
    private static object? ConvertToBitmap(object? value)
    {
        return value switch
        {
            string parameterString => new Bitmap(parameterString),
            Stream parameterStream => parameterStream.Length > 0 ? new Bitmap(parameterStream) : null,
            byte[] parameterBytes => parameterBytes.Length > 0 ? new Bitmap(new MemoryStream(parameterBytes)) : null,
            _ => value
        };
    }

    /// <summary>
    /// Converts a value back to its source type. Not implemented.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">The parameter used in the original conversion.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Fetch subtitles languages from the specified url.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static async Task<List<string>> ConvertToSubtitlesLanguage(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return [];

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.IsFile ||
            !uri.AbsoluteUri.Contains("youtube.com")) return [];
        try
        {
            var youtube = new YoutubeClient();
            var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(url);
            return trackManifest.Tracks.Select(track => track.Language.Code.ToUpperInvariant()).Distinct()
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}