using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents a file associated with a model version on CivitAI.
/// </summary>
public class File
{
    /// <summary>
    /// The download URL for the file.
    /// </summary>
    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; }

    /// <summary>
    /// The ID of the file.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Metadata related to the file.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Indicates if this is the primary file for the model version.
    /// </summary>
    [JsonPropertyName("primary")]
    public bool Primary { get; set; }

    /// <summary>
    /// The size of the file in kilobytes.
    /// </summary>
    [JsonPropertyName("sizeKB")]
    public double SizeKB { get; set; }

    /// <summary>
    /// The type of the file.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }
}