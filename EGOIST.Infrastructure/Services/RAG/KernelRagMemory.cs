using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Text;
using LLama;
using LLamaSharp.KernelMemory;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.KernelMemory.ContentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;

namespace EGOIST.Infrastructure.Services.RAG;

public class KernelRagMemory(ILogger<KernelRagMemory> logger) : IRagMemory
{
    private const string DefaultCollection = "Default";
    private IKernelMemory? Memory { get; set; }

    private LLamaEmbedder? Embedder { get; set; }

    public Task InitializeAsync(params object[] args)
    {
        if (args[0] is not TextModelCoreService generation)
        {
            logger.LogWarning("Kernel Memory initialization failed. Model or parameters are null.");
            return Task.CompletedTask;
        }
        
        Embedder = new LLamaEmbedder(generation.Model!, generation.ModelParameters!);

        Memory = new KernelMemoryBuilder()
            .WithLLamaSharpTextEmbeddingGeneration(new LLamaSharpTextEmbeddingGenerator(Embedder))
            .WithLLamaSharpTextGeneration(new LlamaSharpTextGenerator(generation.Model!, Embedder.Context))
            .WithSearchClientConfig(new SearchClientConfig { MaxMatchesCount = 1, AnswerTokens = 100 })
            .With(new TextPartitioningOptions { MaxTokensPerParagraph = 300, MaxTokensPerLine = 100, OverlappingTokens = 30 })
            .WithSimpleFileStorage(new SimpleFileStorageConfig { StorageType = FileSystemTypes.Disk })
            .WithSimpleVectorDb(new SimpleVectorDbConfig { StorageType = FileSystemTypes.Disk })
            .Build();
        
        logger.LogInformation("Kernel Memory initialized as retrieval augmented generation service.");
        return Task.CompletedTask;
    }

    public Task DisposeAsync(params object[] args)
    {
        Embedder?.Dispose();
        Embedder = null;
        Memory = null;
        return Task.CompletedTask;
    }

    public async Task SaveAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the default collection if the key doesn't specify one.
            var collection = string.IsNullOrEmpty(key) ? DefaultCollection : key;
            await Memory?.ImportDocumentAsync(new Document(collection).AddFile(value), cancellationToken: cancellationToken)!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving memory item with key: {Key}", key);
            throw;
        }
    }

    public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the default collection if the key doesn't specify one.
            var collection = DefaultCollection;
            var question = key;

            // Check if the key includes a collection.
            var parts = key.Split(":", 2);
            if (parts.Length > 1)
            {
                collection = parts[0];
                question = parts[1];
            }

            var result = await Memory?.AskAsync(question,
                 filter: new MemoryFilter().ByDocument(collection), cancellationToken: cancellationToken)!;

            return result.Result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving memory item with key: {Key}", key);
            throw;
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