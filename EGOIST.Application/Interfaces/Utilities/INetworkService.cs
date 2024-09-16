using EGOIST.Domain.Entities;

namespace EGOIST.Application.Interfaces.Utilities;

/// <summary>
/// Defines the interface for a network service, providing methods for file downloading and size retrieval.
/// </summary>
public interface INetworkService
{
    /// <summary>
    /// Searches Web for results based on a given query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="numResults">The maximum number of results to return.</param>
    /// <param name="getPageContent">Whether to get the page content.</param>
    /// <returns>A collection of <see cref="Citation"/> objects representing the search results.</returns>
    Task<IEnumerable<Citation>> SearchAsync(string query, int numResults = 5, bool getPageContent = true);
    
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
    
    /// <summary>
    /// Gets the content of a web page at the given url as markdown.
    /// </summary>
    /// <param name="url">The url of the web page.</param>
    /// <returns>A task that completes with the markdown content of the web page.</returns>
    Task<string> GetPageContent(Uri url);
    
    /// <summary>
    /// Converts html to markdown.
    /// </summary>
    /// <param name="htmlContent">The html content of the web page.</param>
    /// <returns>A task that completes with the markdown content of the web page.</returns>
    Task<string> GetPageContent(string htmlContent);
    
    /// <summary>
    /// Gets the video transcript of the given provider and url.
    /// </summary>
    /// <param name="provider">The provider of the transcript.</param>
    /// <param name="url">The url of the transcript.</param>
    /// <returns>A task that completes with the transcript of the given provider and url.</returns>
    Task<string> GetTranscript(string provider, string url);
}