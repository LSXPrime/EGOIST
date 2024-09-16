using System.Net;
using System.Net.Http.Headers;
using DuckDuckGoSearch;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Domain.Entities;
using ReverseMarkdown;
using Serilog;
using YoutubeExplode;

namespace EGOIST.Infrastructure.Services.Network;

/// <summary>
/// Provides network operations using an `HttpClient`.
/// </summary>
public class NetworkService : INetworkService
{
    private readonly HttpClient _httpClient = new();

    private readonly Converter _htmlToMarkdownConverter = new(new Config
    {
        UnknownTags = Config.UnknownTagsOption.Bypass,
        RemoveComments = true,
        TableWithoutHeaderRowHandling = Config.TableWithoutHeaderRowHandlingOption.EmptyRow,
        SmartHrefHandling = true
    });

    /// <summary>
    /// Searches Web for results based on a given query.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="numResults">The maximum number of results to return.</param>
    /// <param name="getPageContent">Whether to get the page content or not.</param>
    /// <returns>A collection of <see cref="Citation"/> objects representing the search results.</returns>
    public async Task<IEnumerable<Citation>> SearchAsync(string query, int numResults = 5, bool getPageContent = true)
    {
        try
        {
            var results = await DDGClient.SearchAsync(WebUtility.UrlEncode(query));
            var takeCount = results.Count < numResults ? results.Count : numResults;
            var citations = await Task.WhenAll(results.Take(takeCount).Select(async result => new Citation
            {
                Collection = "Web Search",
                Title = result.Title,
                Path = result.Link,
                Content = (getPageContent ? await GetPageContent(new Uri(result.Link)) : result.Description)!
            }));


            return citations;
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Failed to search for {Query}", query);
            return [];
        }
    }

    /// <summary>
    /// Downloads a file from a URL to a specified path.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="destinationPath">The path to save the downloaded file.</param>
    /// <param name="startByte">The starting byte for resuming a partial download.</param>
    /// <param name="progressCallback">A callback function to report download progress.</param>
    /// <param name="cancellationToken">A token to cancel the download operation.</param>
    /// <returns>A task that completes when the file download is finished.</returns>
    public async Task DownloadFile(string url, string destinationPath, long startByte, Action<long>? progressCallback,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Set range header for resuming downloads
        if (startByte > 0)
        {
            request.Headers.Range = new RangeHeaderValue(startByte, null);
        }

        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

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
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

        while (bytesRead > 0)
        {
            totalBytesRead += bytesRead;
            progressCallback?.Invoke(totalBytesRead);

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            bytesRead = await stream.ReadAsync(buffer, cancellationToken);
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

    /// <summary>
    /// Gets the content of a web page at the given url as markdown.
    /// </summary>
    /// <param name="url">The url of the web page.</param>
    /// <returns>A task that completes with the markdown content of the web page.</returns>
    public async Task<string> GetPageContent(Uri url)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(url);
            return _htmlToMarkdownConverter.Convert(html);
        }
        catch (Exception e)
        {
            Log.Logger.Fatal(e, "Failed to get content for {Url}", url);
            return string.Empty;
        }
    }

    /// <summary>
    /// Converts html to markdown.
    /// </summary>
    /// <param name="htmlContent">The html content of the web page.</param>
    /// <returns>A task that completes with the markdown content of the web page.</returns>
    public Task<string> GetPageContent(string htmlContent)
    {
        return Task.FromResult(_htmlToMarkdownConverter.Convert(htmlContent));
    }

    public async Task<string> GetTranscript(string provider, string url)
    {
        return provider switch
        {
            "youtube" => await GetYoutubeTranscript(url),
            _ => await GetPageContent(new Uri(url))
        };
    }

    private static async Task<string> GetYoutubeTranscript(string url)
    {
        try
        {
            var separatedUrl = url.Split(":::");
            if (separatedUrl.Length != 2)
                return string.Empty;
            var youtube = new YoutubeClient();
            var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(separatedUrl[0]);
            if (trackManifest.Tracks.Count == 0) 
                return string.Empty;
            var trackInfo = trackManifest.GetByLanguage(separatedUrl[1]);
            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
            return string.Join(" ", track.Captions.Select(caption => caption.Text));
        }
        catch (Exception e)
        {
            Log.Logger.Fatal(e, "Failed to get transcript for {Url}", url);
            return string.Empty;
        }
    }
}