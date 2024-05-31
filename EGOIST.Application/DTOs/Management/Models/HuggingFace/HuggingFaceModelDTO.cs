using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.HuggingFace;

/// <summary>
/// Represents a Hugging Face model, containing information like name, tags, and weights.
/// </summary>
public class HuggingFaceModelDto
{
    /// <summary>
    /// The date and time the model was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The number of times the model has been downloaded.
    /// </summary>
    [JsonPropertyName("downloads")]
    public int Downloads { get; set; }

    /// <summary>
    /// The name of the library used to create the model.
    /// </summary>
    [JsonPropertyName("library_name")]
    public string Library { get; set; }

    /// <summary>
    /// The number of likes the model has received.
    /// </summary>
    [JsonPropertyName("likes")]
    public int Likes { get; set; }

    /// <summary>
    /// The unique identifier of the model.
    /// </summary>
    [JsonPropertyName("modelId")]
    public string ModelId { get; set; }

    /// <summary>
    /// The name of the model.
    /// </summary>
    [JsonPropertyName("id")]
    public string Name { get; set; }

    /// <summary>
    /// The pipeline tag associated with the model, indicating its purpose.
    /// </summary>
    [JsonPropertyName("pipeline_tag")]
    public string PipelineTag { get; set; }

    /// <summary>
    /// Indicates whether the model is private or public.
    /// </summary>
    [JsonPropertyName("private")]
    public bool Private { get; set; }

    /// <summary>
    /// A list of tags associated with the model.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// The unique identifier of the model.
    /// </summary>
    [JsonPropertyName("_id")]
    public string Uid { get; set; }

    /// <summary>
    /// A list of sibling models, representing different weights or variations of the model.
    /// </summary>
    [JsonPropertyName("siblings")]
    public List<Sibling> Weights { get; set; }

    /// <summary>
    /// Represents a sibling model, typically a different weight or variation of the main model.
    /// </summary>
    public class Sibling
    {
        /// <summary>
        /// The relative file name of the sibling model.
        /// </summary>
        [JsonPropertyName("rfilename")]
        public string Name { get; set; }
    }
}