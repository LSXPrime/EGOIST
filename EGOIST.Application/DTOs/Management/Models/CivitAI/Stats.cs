using System.Text.Json.Serialization;

namespace EGOIST.Application.DTOs.Management.Models.CivitAI;

/// <summary>
/// Represents statistics related to a model or model version on CivitAI.
/// </summary>
public class Stats
{
    /// <summary>
    /// The number of comments on the model or model version.
    /// </summary>
    [JsonPropertyName("commentCount")]
    public int CommentCount { get; set; }

    /// <summary>
    /// The number of downloads of the model or model version.
    /// </summary>
    [JsonPropertyName("downloadCount")]
    public int DownloadCount { get; set; }

    /// <summary>
    /// The number of times the model or model version has been favorited.
    /// </summary>
    [JsonPropertyName("favoriteCount")]
    public int FavoriteCount { get; set; }

    /// <summary>
    /// The average rating of the model or model version.
    /// </summary>
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    /// <summary>
    /// The number of ratings for the model or model version.
    /// </summary>
    [JsonPropertyName("ratingCount")]
    public int RatingCount { get; set; }

    /// <summary>
    /// The number of thumbs down the model or model version has received.
    /// </summary>
    [JsonPropertyName("thumbsDownCount")]
    public int ThumbsDownCount { get; set; }

    /// <summary>
    /// The number of thumbs up the model or model version has received.
    /// </summary>
    [JsonPropertyName("thumbsUpCount")]
    public int ThumbsUpCount { get; set; }

    /// <summary>
    /// The number of times the model or model version has been tipped.
    /// </summary>
    [JsonPropertyName("tippedAmountCount")]
    public int TippedAmountCount { get; set; }
}