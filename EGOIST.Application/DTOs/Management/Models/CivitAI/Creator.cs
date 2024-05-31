using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents a creator of a model on CivitAI.
/// </summary>
public class Creator
{
    /// <summary>
    /// The URL of the creator's profile image.
    /// </summary>
    [JsonPropertyName("image")]
    public string Image { get; set; }

    /// <summary>
    /// The username of the creator.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }
}