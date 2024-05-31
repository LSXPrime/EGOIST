using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents metadata related to the CivitAI API response.
/// </summary>
public class Metadata
{
    /// <summary>
    /// The format of the model file.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; }

    /// <summary>
    /// The model weight.
    /// </summary>
    [JsonPropertyName("fp")]
    public string Fp { get; set; }

    /// <summary>
    /// The next cursor for pagination.
    /// </summary>
    [JsonPropertyName("nextCursor")]
    public string NextCursor { get; set; }

    /// <summary>
    /// The next page for pagination.
    /// </summary>
    [JsonPropertyName("nextPage")]
    public string NextPage { get; set; }

    /// <summary>
    /// The size of the model.
    /// </summary>
    [JsonPropertyName("size")]
    public string Size { get; set; }
}