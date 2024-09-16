namespace EGOIST.Application.Interfaces.Utilities;

public interface IImageManipulationService
{
    byte[] Resize(byte[] imageData, int width, int height);
    byte[] Crop(byte[] imageData, int x, int y, int width, int height);
    byte[] Rotate(byte[] imageData, float degrees);
    byte[] Grayscale(byte[] imageData);
    byte[] SaveImage(byte[] image);
}