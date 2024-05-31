namespace EGOIST.Domain.Enums;

/// <summary>
/// Represents the different possible states of a download process.
/// </summary>
public enum DownloadStatus : short
{
    /// <summary>
    /// The download is queued and waiting to start.
    /// </summary>
    Pending,

    /// <summary>
    /// The download is currently in progress.
    /// </summary>
    Downloading,

    /// <summary>
    /// The download has been paused temporarily.
    /// </summary>
    Paused,

    /// <summary>
    /// The download has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The download has been canceled before completion.
    /// </summary>
    Canceled
}