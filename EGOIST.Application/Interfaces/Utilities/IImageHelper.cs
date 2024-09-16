using HPPH;

namespace EGOIST.Application.Interfaces.Utilities;

public interface IImageHelper
{
    IImage LoadImage(string path);
    IImage LoadImage(byte[] bytes);
    IImage LoadImage(Stream stream);
    byte[] SaveImage(IImage image);
    IImage<T> Convert<T>(IImage image) where T : struct, IColor;
}