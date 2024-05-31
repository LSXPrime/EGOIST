using EGOIST.Domain.Interfaces;
using System.Net.Http.Headers;

namespace EGOIST.Infrastructure.Services.Network;

/// <summary>
/// Provides network operations using an `HttpClient`.
/// </summary>
public class NetworkService : INetworkService
{
    private readonly HttpClient _httpClient = new();
    

    /// <summary>
    /// Downloads a file from a URL to a specified path.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destinationPath">The path to save the downloaded file.</param>
    /// <param name="startByte">The starting byte for resuming a partial download.</param>
    /// <param name="progressCallback">A callback function to report download progress.</param>
    /// <param name="cancellationToken">A token to cancel the download operation.</param>
    /// <returns>A task that completes when the file download is finished.</returns>
    public async Task DownloadFile(string url, string destinationPath, long startByte, Action<long>? progressCallback, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Set range header for resuming downloads
        if (startByte > 0)
        {
            request.Headers.Range = new RangeHeaderValue(startByte, null);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Create the file and write the downloaded data
        await using var fileStream = File.OpenWrite(destinationPath);

        // Skip already downloaded bytes
        if (startByte > 0)
        {
            fileStream.Seek(startByte, SeekOrigin.Begin);
        }

        var totalBytesRead = 0L;
        var buffer = new byte[4096];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        while (bytesRead > 0)
        {
            totalBytesRead += bytesRead;
            progressCallback?.Invoke(totalBytesRead);

            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the file size of a resource at a given URL using a HEAD request.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <returns>A task that completes with the file size in bytes.</returns>
    public async Task<long> GetFileSize(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        if (!response.Content.Headers.TryGetValues("Content-Length", out var contentLengthValues)) return 0;
        return long.TryParse(contentLengthValues.FirstOrDefault(), out var fileSize) ? fileSize : 0;
    }
}