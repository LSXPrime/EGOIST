using EGOIST.Domain.Interfaces;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace EGOIST.Infrastructure.Services.FileTypes;

/// <summary>
/// Provides services for extracting character data from image metadata.
/// </summary>
public class ImageMetadataService (ILogger<ImageMetadataService> logger): IImageMetadataService
{
    /// <summary>
    /// Extracts character data from the metadata of an image file.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <returns>A task that completes with the extracted character data as a string.</returns>
    public async Task<string> ExtractCharacterData(string imagePath)
    {
        var extension = Path.GetExtension(imagePath).ToLowerInvariant();

        switch (extension)
        {
            case ".webp":
                return await ExtractFromWebP(imagePath);
            case ".png":
                return await ExtractFromPng(imagePath);
            default:
                throw new NotSupportedException($"Image format '{extension}' not supported for metadata extraction.");
        }
    }

    /// <summary>
    /// Extracts character data from the metadata of a PNG image file.
    /// </summary>
    /// <param name="imagePath">The path to the PNG image file.</param>
    /// <returns>A task that completes with the extracted character data as a string.</returns>
    private async Task<string> ExtractFromPng(string imagePath)
    {
        try
        {
            var imageData = await File.ReadAllBytesAsync(imagePath, CancellationToken.None);

            var num = 8;
            while (num < imageData.Length)
            {
                var chunkLength = BitConverter.ToInt32(imageData, num);
                num += 8 + chunkLength;

                if (Encoding.ASCII.GetString(imageData, num - 8, 4) == "tEXt")
                {
                    return Encoding.UTF8.GetString(imageData, num - chunkLength, chunkLength);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting metadata from PNG image: {ImagePath}", imagePath);
            throw;
        }
    }

    /// <summary>
    /// Extracts character data from the metadata of a WebP image file.
    /// </summary>
    /// <param name="imagePath">The path to the WebP image file.</param>
    /// <returns>A task that completes with the extracted character data as a string.</returns>
    private Task<string> ExtractFromWebP(string imagePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(imagePath);

            var exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

            if (exifIfd0Directory == null) return Task.FromResult<string>(null);
            var description = exifIfd0Directory.GetDescription(37510);
            if (description != "Undefined")
            {
                return Task.FromResult(description);
            }
            else if (exifIfd0Directory.TryGetInt32(37510, out var characterData))
            {
                return Task.FromResult(characterData.ToString());
            }
            else
            {
                try
                {
                    JsonDocument.Parse(description);
                    return Task.FromResult(description);
                }
                catch
                {
                    var parts = description.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var byteValues = Array.ConvertAll(parts, byte.Parse);
                    return Task.FromResult(Encoding.UTF8.GetString(byteValues));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting metadata from WebP image: {ImagePath}", imagePath);
            throw;
        }
    }
}