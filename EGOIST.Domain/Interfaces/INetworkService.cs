namespace EGOIST.Domain.Interfaces;

/// <summary>
/// Defines the interface for a network service, providing methods for file downloading and size retrieval.
/// </summary>
public interface INetworkService
{
    /// <summary>
    /// Downloads a file from a given URL to a specified destination path.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destinationPath">The path to save the downloaded file.</param>
    /// <param name="startByte">The starting byte for resuming a download (if applicable).</param>
    /// <param name="progressCallback">An optional callback to report download progress.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the download operation.</param>
    /// <returns>A task that completes when the file download is finished or canceled.</returns>
    Task DownloadFile(string url, string destinationPath, long startByte = 0, Action<long>? progressCallback = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the size of a file located at the given URL.
    /// </summary>
    /// <param name="url">The URL of the file to check.</param>
    /// <returns>A task that completes with the size of the file in bytes.</returns>
    Task<long> GetFileSize(string url);
}