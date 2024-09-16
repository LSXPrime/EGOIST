using EGOIST.Application.Interfaces.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace EGOIST.Infrastructure.Services.FileTypes;

public class ImageSharpImageManipulationService : IImageManipulationService
{
    public byte[] Resize(byte[] imageData, int width, int height)
    {
        using var image = Image.Load(imageData);
        image.Mutate(x => x.Resize(width, height));
        return SaveImage(image);
    }

    public byte[] Crop(byte[] imageData, int x, int y, int width, int height)
    {
        using var image = Image.Load(imageData);
        image.Mutate(context => context.Crop(new Rectangle(x, y, width, height)));
        return SaveImage(image);
    }

    public byte[] Rotate(byte[] imageData, float degrees)
    {
        using var image = Image.Load(imageData);
        image.Mutate(x => x.Rotate(degrees));
        return SaveImage(image);
    }

    public byte[] Grayscale(byte[] imageData)
    {
        using var image = Image.Load(imageData);
        image.Mutate(x => x.Grayscale());
        return SaveImage(image);
    }

    public byte[] SaveImage(byte[] image)
    {
        throw new NotImplementedException();
    }
    
    private byte[] SaveImage(Image image)
    {
        using var memoryStream = new MemoryStream();
        image.Save(memoryStream, new JpegEncoder());
        return memoryStream.ToArray();
    }
}