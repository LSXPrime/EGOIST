using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents a version of a CivitAI model.
/// </summary>
public class ModelVersion
{
    /// <summary>
    /// The availability status of the model version.
    /// </summary>
    [JsonPropertyName("availability")]
    public string Availability { get; set; }

    /// <summary>
    /// The base model used for training.
    /// </summary>
    [JsonPropertyName("baseModel")]
    public string BaseModel { get; set; }

    /// <summary>
    /// The type of the base model.
    /// </summary>
    [JsonPropertyName("baseModelType")]
    public string BaseModelType { get; set; }

    /// <summary>
    /// The description of the model version.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// The download URL for the model version.
    /// </summary>
    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; }

    /// <summary>
    /// A list of files associated with the model version.
    /// </summary>
    [JsonPropertyName("files")]
    public List<File> Files { get; set; }

    /// <summary>
    /// The ID of the model version.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// A list of images associated with the model version.
    /// </summary>
    [JsonPropertyName("images")]
    public List<Image> Images { get; set; }

    /// <summary>
    /// The index of the model version.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// The ID of the model.
    /// </summary>
    [JsonPropertyName("modelId")]
    public int ModelId { get; set; }

    /// <summary>
    /// The name of the model version.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// The NSFW level of the model version.
    /// </summary>
    [JsonPropertyName("nsfwLevel")]
    public int NsfwLevel { get; set; }

    /// <summary>
    /// The date and time the model version was published.
    /// </summary>
    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Statistics about the model version.
    /// </summary>
    [JsonPropertyName("stats")]
    public Stats Stats { get; set; }

    /// <summary>
    /// A list of words used to train the model version.
    /// </summary>
    [JsonPropertyName("trainedWords")]
    public List<string> TrainedWords { get; set; }
}