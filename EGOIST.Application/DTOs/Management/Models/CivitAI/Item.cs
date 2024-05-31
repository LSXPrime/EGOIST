using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents a model item returned by the CivitAI API.
/// </summary>
public class Item
{
    /// <summary>
    /// A list of strings indicating if commercial use is allowed.
    /// </summary>
    [JsonPropertyName("allowCommercialUse")]
    public List<string> AllowCommercialUse { get; set; }

    /// <summary>
    /// Indicates if derivatives of the model are allowed.
    /// </summary>
    [JsonPropertyName("allowDerivatives")]
    public bool AllowDerivatives { get; set; }

    /// <summary>
    /// Indicates if using a different license for derivatives is allowed.
    /// </summary>
    [JsonPropertyName("allowDifferentLicense")]
    public bool AllowDifferentLicense { get; set; }

    /// <summary>
    /// Indicates if using the model without credit is allowed.
    /// </summary>
    [JsonPropertyName("allowNoCredit")]
    public bool AllowNoCredit { get; set; }

    /// <summary>
    /// Cosmetic information about the model.
    /// </summary>
    [JsonPropertyName("cosmetic")]
    public object Cosmetic { get; set; }

    /// <summary>
    /// The creator of the model.
    /// </summary>
    [JsonPropertyName("creator")]
    public Creator Creator { get; set; }

    /// <summary>
    /// The description of the model.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// The ID of the model.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// A list of model versions associated with the model.
    /// </summary>
    [JsonPropertyName("modelVersions")]
    public List<ModelVersion> ModelVersions { get; set; }

    /// <summary>
    /// The name of the model.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Indicates if the model is NSFW.
    /// </summary>
    [JsonPropertyName("nsfw")]
    public bool Nsfw { get; set; }

    /// <summary>
    /// The NSFW level of the model.
    /// </summary>
    [JsonPropertyName("nsfwLevel")]
    public int NsfwLevel { get; set; }

    /// <summary>
    /// Statistics about the model.
    /// </summary>
    [JsonPropertyName("stats")]
    public Stats Stats { get; set; }

    /// <summary>
    /// A list of tags associated with the model.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// The type of the model.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }
}