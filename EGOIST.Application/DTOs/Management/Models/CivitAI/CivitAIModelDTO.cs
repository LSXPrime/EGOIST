using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents a CivitAI model response, containing metadata and a list of model items.
/// </summary>
public class CivitAIModelDTO
{
    /// <summary>
    /// A list of model items returned by the CivitAI API.
    /// </summary>
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }

    /// <summary>
    /// Metadata related to the API response.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }
}