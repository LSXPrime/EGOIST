using System.IO;
using System.Runtime.InteropServices;
using EGOIST.Application.Interfaces.Utilities;
using HPPH;
using HPPH.SkiaSharp;
using SkiaSharp;

namespace EGOIST.Presentation.UI.Services.Utilities;

public class ImageHelper : IImageHelper
{
    public IImage LoadImage(string path)
    {
        using var image = SKImage.FromEncodedData(path);
        return image.ToImage();
    }

    public IImage LoadImage(byte[] bytes)
    {
        using var image = SKImage.FromEncodedData(bytes);
        return image.ToImage();
    }

    public IImage LoadImage(Stream stream)
    {
        using var image = SKImage.FromEncodedData(stream);
        return image.ToImage();
    }

    public byte[] SaveImage(IImage image)
    {
        using var skImage = image.ToSKImage();
        var bytes = skImage.Encode(SKEncodedImageFormat.Png, 100);
        return bytes.ToArray();
    }

    public IImage<T> Convert<T>(IImage image) where T : struct, IColor
    {
        var bitmap = image.ToSKBitmap();
        return Image<T>.Create(MemoryMarshal.Cast<SKColor, T>(bitmap.Pixels), bitmap.Width, bitmap.Height);
    }
}