using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents an image associated with a model version on CivitAI.
/// </summary>
public class Image
{
    /// <summary>
    /// The height of the image in pixels.
    /// </summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>
    /// The ID of the image.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// The NSFW level of the image.
    /// </summary>
    [JsonPropertyName("nsfwLevel")]
    public int NsfwLevel { get; set; }

    /// <summary>
    /// The URL of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    /// The width of the image in pixels.
    /// </summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }
}