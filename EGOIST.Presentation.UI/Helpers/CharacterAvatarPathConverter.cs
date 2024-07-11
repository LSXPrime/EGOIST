using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;

namespace EGOIST.Presentation.UI.Helpers;

public class CharacterAvatarPathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var avatar = value switch
        {
            RoleplayCharacter character => $@"{character.Name}\{character.Avatar}",
            string => value.ToString(),
            _ => ""
        };

        var path = Path.Combine(AppConfig.Instance.CharactersPath, avatar!);

        return string.IsNullOrEmpty(avatar) ? null : Path.Exists(path) ? new Bitmap(path) : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
