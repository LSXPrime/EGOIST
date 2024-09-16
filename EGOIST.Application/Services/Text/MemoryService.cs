using System.Collections.ObjectModel;
using System.Globalization;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Application.Services.Management.Loaders;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class MemoryService
{
    public ObservableCollection<MemorySource> Sessions { get; set; } = [];

    public TextModelCoreService ModelCore { get; }
    private readonly TextDataLoader _dataLoader;
    private readonly ILogger<MemoryService> _logger;
    private readonly IRagMemory _ragMemory;

    public MemoryService(ILogger<MemoryService> logger,
        [FromKeyedServices("TextModelCoreScopedService")]
        IModelCoreService modelCore, IRagMemory ragMemory, TextDataLoader dataLoader)
    {
        _logger = logger;
        ModelCore = (TextModelCoreService)modelCore;
        _ragMemory = ragMemory;
        _dataLoader = dataLoader;

        ModelCore.OnSwitch += _ragMemory.InitializeAsync;
        ModelCore.OnUnload += _ragMemory.DisposeAsync;
        _ = Initialize();
    }

    private async Task Initialize()
    {
        Sessions = new ObservableCollection<MemorySource>(await _dataLoader.LoadAllSessions<MemorySource>("Memory"));
    }

    public async Task<bool> Create(string name, string[] paths, int chunkSize = 512, int overlap = 64, string chunkSeparator = "")
    {
        if (ModelCore.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return false;
        }

        var collectionName = !string.IsNullOrEmpty(name) ? name : "New Collection";
        if (paths.Length == 0)
            return false;

        var memorySource = GetOrAddSession(collectionName);

        try
        {
            memorySource.IsLoaded = false;
            ModelCore.CancelToken ??= new CancellationTokenSource();
            await _ragMemory.SaveAsync(collectionName, paths, new Dictionary<string, string>
            {
                { "ChunkSize", chunkSize.ToString(CultureInfo.InvariantCulture) },
                { "ChunkOverlap", overlap.ToString(CultureInfo.InvariantCulture) },
                { "ChunkSeparator", chunkSeparator }
            }, ModelCore.CancelToken.Token);
            memorySource.IsLoaded = true;
            await _dataLoader.SaveSession(memorySource, "Memory", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing document");
            return false;
        }

        return true;
    }

    public async Task<bool> Delete(string collection)
    {
        var existingItem = Sessions.FirstOrDefault(item => item.Name == collection);
        if (existingItem == null)
            return false;

        Sessions.Remove(existingItem);
        try
        {
            foreach (var document in existingItem.Documents)
            {
                await _ragMemory.RemoveAsync($"{collection}:{document}", ModelCore.CancelToken!.Token);
            }

            await _dataLoader.DeleteSession(collection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document");
            return false;
        }

        _logger.LogInformation("Memory {collection} Deleted", collection);

        return true;
    }

    public async Task<bool> Feed(string message, string? collection = null)
    {
        if (string.IsNullOrEmpty(message))
            return false;

        return await Feed([message], collection);
    }
    public async Task<bool> Feed(string[] messages, string? collection = null)
    {
        if (messages.Length == 0)
            return false;

        var globalMemory = GetOrAddSession(collection ?? Constants.GlobalMessagesMemory);
        globalMemory.IsLoaded = false;
        ModelCore.CancelToken ??= new CancellationTokenSource();
        await _ragMemory.SaveAsync(globalMemory.Name, messages, cancellationToken: ModelCore.CancelToken.Token);
        globalMemory.IsLoaded = true;
        return true;
    }

    public async Task<Citation[]> Generate(string userInput, MemorySource[] sessions, int chunksCount = 3,
        double similarity = 0.5, bool isGlobalMemory = false, string? globalCollection = null)
    {
        if (string.IsNullOrEmpty(userInput) || (sessions.Length == 0 && !isGlobalMemory))
            return [];

        ModelCore.CancelToken = new CancellationTokenSource();

        if (isGlobalMemory) sessions = [GetOrAddSession(globalCollection ?? Constants.GlobalMessagesMemory)];

        var answer = await _ragMemory.GetAsync(userInput, sessions, new Dictionary<string, string>
            {
                { "Similarity", similarity.ToString(CultureInfo.InvariantCulture) },
                { "ChunksCount", chunksCount.ToString() }
            },
            cancellationToken: ModelCore.CancelToken.Token);

        return answer;
    }

    public async Task Dispose()
    {
        await ModelCore.CancelToken?.CancelAsync()!;
        await ModelCore.Unload();

        ModelCore.OnSwitch -= _ragMemory.InitializeAsync;
        ModelCore.OnUnload -= _ragMemory.DisposeAsync;
    }

    private MemorySource GetOrAddSession(string sessionName)
    {
        var session = Sessions.FirstOrDefault(item => item.Name == sessionName);
        if (session == null)
        {
            session = new MemorySource { Name = sessionName };
            Sessions.Add(session);
        }

        return session;
    }
}