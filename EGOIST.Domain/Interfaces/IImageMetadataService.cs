namespace EGOIST.Domain.Interfaces;

/// <summary>
/// Defines the interface for a service that extracts metadata from images, specifically focusing on character data.
/// </summary>
public interface IImageMetadataService
{
    /// <summary>
    /// Extracts character data from an image.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <returns>A task that completes with a string representing the extracted character data.</returns>
    Task<string> ExtractCharacterData(string imagePath);
}