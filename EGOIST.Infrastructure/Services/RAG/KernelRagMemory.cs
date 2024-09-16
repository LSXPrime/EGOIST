using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Text;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using LLamaSharp.KernelMemory;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.KernelMemory.Context;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Citation = EGOIST.Domain.Entities.Citation;

namespace EGOIST.Infrastructure.Services.RAG;

public class KernelRagMemory(ILogger<KernelRagMemory> logger, INetworkService networkService) : IRagMemory
{
    private const string DefaultCollection = "Default";
    private MemoryServerless? Memory { get; set; }

    public Task InitializeAsync(params object[] args)
    {
        if (args[0] is not TextModelCoreService generation || generation.Model == null ||
            generation.ModelParameters == null)
        {
            logger.LogWarning("Kernel Memory initialization failed. Model or parameters are null.");
            return Task.CompletedTask;
        }

        Memory = new KernelMemoryBuilder()
            .WithLLamaSharpTextEmbeddingGeneration(new LLamaSharpTextEmbeddingGenerator(
                new LLamaSharpConfig(generation.ModelParameters.ModelPath)
                {
                    ContextSize = generation.ModelParameters.ContextSize, Seed = generation.ModelParameters.Seed,
                    GpuLayerCount = generation.ModelParameters.GpuLayerCount
                }, generation.Model))
            .WithoutTextGenerator()
            .WithSearchClientConfig(new SearchClientConfig { MaxMatchesCount = 3, AnswerTokens = 512 })
            .With(new TextPartitioningOptions
                { MaxTokensPerParagraph = 512, MaxTokensPerLine = 50, OverlappingTokens = 30 })
            .WithSimpleFileStorage(new SimpleFileStorageConfig
                { StorageType = FileSystemTypes.Disk, Directory = AppConfig.Instance.Parameters.MemoriesPath })
            .WithSimpleVectorDb(new SimpleVectorDbConfig
                { StorageType = FileSystemTypes.Disk, Directory = AppConfig.Instance.Parameters.MemoriesPath })
            .Build<MemoryServerless>();

        logger.LogInformation("Kernel Memory initialized as retrieval augmented generation service.");
        return Task.CompletedTask;
    }

    public Task DisposeAsync(params object[] args) => Task.CompletedTask;

    public async Task SaveAsync(string key, IEnumerable<string> paths, Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (Memory == null)
            return;

        try
        {
            // Use the default collection if the key doesn't specify one.
            key = string.IsNullOrEmpty(key) ? DefaultCollection : key.Trim();

            var documents = new List<Document>();
            var pages = new Dictionary<string, string>();
            foreach (var path in paths)
            {
                if (!Uri.TryCreate(path, UriKind.Absolute, out var uri))
                {
                    pages.Add(path, path);
                    continue;
                }

                if (uri.IsFile)
                    documents.Add(new Document().AddFile(path).AddTag("Path", path));

                else if (uri.AbsoluteUri.Contains("youtube.com"))
                {
                    var subtitles = await networkService.GetTranscript("youtube", uri.AbsoluteUri);
                    if (string.IsNullOrEmpty(subtitles))
                        continue;

                    pages.TryAdd(path, subtitles);
                }
                else
                    pages.TryAdd(path, await networkService.GetPageContent(uri));
            }

            var context = new RequestContext();
            if (parameters != null)
                context.SetArgs(new Dictionary<string, object?>
                {
                    { Constants.CustomContext.Partitioning.MaxTokensPerParagraph, int.Parse(parameters["ChunkSize"]) },
                    { Constants.CustomContext.Partitioning.OverlappingTokens, int.Parse(parameters["ChunkOverlap"]) },
                    { Constants.CustomContext.Partitioning.ChunkHeader, parameters["ChunkSeparator"] }
                });

            foreach (var document in documents)
                await Memory.ImportDocumentAsync(document, index: key, context: context,
                    cancellationToken: cancellationToken);

            foreach (var page in pages.Where(page => !string.IsNullOrEmpty(page.Value)))
                await Memory.ImportTextAsync(page.Value, index: key, context: context, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving memory item with key: {Key}", key);
            throw;
        }
    }

    public async Task<Citation[]> GetAsync(string query, IEnumerable<MemorySource> paths,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (Memory == null)
            return [];

        try
        {
            var collections = paths.Select(source => source.Name).Distinct().ToArray();

            var results = await Task.WhenAll(collections.Select(collection => Memory.SearchAsync(query, index: collection,
                minRelevance: double.TryParse(parameters?["Similarity"], out var similarity) ? similarity : 0,
                limit: int.TryParse(parameters?["ChunksCount"], out var limit) ? limit : -1,
                cancellationToken: cancellationToken)));

            if (results.All(x => x.NoResult))
                return [];

            var citations = results.SelectMany(result => result.Results)
                .SelectMany(document => document.Partitions.Select(partition => new Citation
                {
                    Collection = document.DocumentId,
                    Title = document.SourceName,
                    Path = Uri.TryCreate(document.DocumentId, UriKind.Absolute, out var uri) ? uri.AbsoluteUri : document.SourceUrl ?? document.Link,
                    Content = partition.Text,
                    Relevance = partition.Relevance
                })).ToArray();
            
            return citations;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving memory item with key: {query}", query);
            return [];
        }
    }

    public Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the default collection if the key doesn't specify one.
            var collection = DefaultCollection;
            var documentKey = key;

            // Check if the key includes a collection.
            var parts = key.Split(":", 2);
            if (parts.Length > 1)
            {
                collection = parts[0];
                documentKey = parts[1];
            }

            await Memory?.DeleteDocumentAsync(collection, documentKey, cancellationToken)!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing memory item with key: {Key}", key);
            throw;
        }
    }
}