using System.Collections.ObjectModel;
using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Text;

public class MemoryService : ITextService
{
    public ObservableCollection<TextPromptParameters> PromptTemplates { get; set; }
    public ObservableCollection<MemorySource> MemoriesPaths { get; set; } = [];
    public MemorySource? SelectedMemory { get; set; }

    private readonly TextModelCoreService? _modelCore;
    private readonly ILogger<MemoryService> _logger;
    private readonly IRagMemory _ragMemory;

    public MemoryService(ILogger<MemoryService> logger, IPromptRepository<TextPromptParameters> promptRepository,
        [FromKeyedServices("TextModelCoreService")]
        IModelCoreService modelCore, IRagMemory ragMemory)
    {
        _logger = logger;
        _modelCore = modelCore as TextModelCoreService;
        _ragMemory = ragMemory;
        PromptTemplates = new ObservableCollection<TextPromptParameters>(promptRepository.GetAllTemplates(null).Result);

        _modelCore!.OnSwitch += _ragMemory.InitializeAsync;
        _modelCore!.OnUnload += _ragMemory.DisposeAsync;
    }


    public async Task<bool> Create(Dictionary<string, object>? parameter = null)
    {
        if (_modelCore?.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return false;
        }
        
        var collectionName = parameter?["Name"].ToString() ?? "New Collection";
        var pathName = parameter?["Path"].ToString();
        var docName = Path.GetFileName(pathName);
        var memorySource = MemoriesPaths.FirstOrDefault(item => item.Name == collectionName);
        if (memorySource != null)
            memorySource.Documents.Add(docName!);
        else
        {
            memorySource = new MemorySource { Name = collectionName };
            memorySource.Documents.Add(docName!);
            MemoriesPaths.Add(memorySource);
        }

        try
        {
            memorySource.IsLoaded = false;
            await _ragMemory.SaveAsync(collectionName, pathName!, _modelCore!.CancelToken!.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing document");
            return false;
        }
        finally
        {
            memorySource.IsLoaded = true;
            SelectedMemory ??= memorySource;
        }

        return true;
    }

    public async Task<bool> Delete(string collection)
    {
        var existingItem = MemoriesPaths.FirstOrDefault(item => item.Name == collection);
        if (existingItem != null)
            MemoriesPaths.Remove(existingItem);
        try
        {
            foreach (var document in existingItem?.Documents!)
            {
                await _ragMemory.RemoveAsync($"{collection}:{document}", _modelCore!.CancelToken!.Token);
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document");
            return false;
        }

        return true;
    }

    public async Task<T?> Generate<T>(string userInput, TextGenerationParameters? generationParameters = null,
        TextPromptParameters? promptParameters = null) where T : class
    {
        if (_modelCore!.State == GenerationState.Started)
        {
            await _modelCore.CancelToken!.CancelAsync();
            return null;
        }

        if (_modelCore.SelectedGenerationModel == null)
        {
            _logger.LogWarning("Text Generation Model isn't loaded yet.");
            return null;
        }

        if (SelectedMemory == null)
        {
            _logger.LogWarning("Memory isn't selected yet.");
            return null;
        }

        if (string.IsNullOrEmpty(userInput) || SelectedMemory is not { IsLoaded: true } ||
            _modelCore.SelectedGenerationModel == null)
            return null;

        _modelCore.State = GenerationState.Started;
        
        var userMessage = SelectedMemory.AddMessage("User", userInput);
        var memoryMessage = SelectedMemory.AddMessage("EGOIST", "Gathering information from memories.");

        _modelCore.CancelToken = new CancellationTokenSource();
        
        var answer = await _ragMemory.GetAsync($"{SelectedMemory.Name}:{userMessage.Message}",
            _modelCore.CancelToken.Token);

        memoryMessage.Message = answer;
        _modelCore.State = GenerationState.Finished;
        _modelCore.CancelToken.Dispose();

        return memoryMessage as T ?? null;
    }
}