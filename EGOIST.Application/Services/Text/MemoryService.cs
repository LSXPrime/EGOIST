using System.Collections.ObjectModel;
using EGOIST.Application.Interfaces.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;

namespace EGOIST.Application.Services.Text;

public class MemoryService(ILogger<CompletionService> _logger) : ITextService
{
    public ObservableCollection<MemorySource> MemoriesPaths = [];

    public MemorySource SelectedMemory;

    private readonly GenerationService _generation = GenerationService.Instance;


    public async Task Create(string collectionPath)
    {
        var separatedText = collectionPath.Split(',');
        if (separatedText.Length != 2 )
            throw new Exception("Collection or path is empty or null");

        var docName = Path.GetFileName(separatedText[1]);
        var memorySource = MemoriesPaths.FirstOrDefault(item => item.Collection == separatedText[0]);
        if (memorySource != null)
            memorySource.Documents.Add(docName);
        else
        {
            memorySource = new MemorySource { Collection = separatedText[0] };
            memorySource.Documents.Add(docName);
            MemoriesPaths.Add(memorySource);
        }

        try
        {
            memorySource.IsLoaded = false;
            await _generation.Memory.ImportDocumentAsync(new Document(separatedText[0]).AddFile(separatedText[1]), steps: Constants.PipelineWithoutSummary);
            memorySource.IsLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing document");
            throw; 
        }
    }

    public async Task Delete(string collection)
    {
        var existingItem = MemoriesPaths.FirstOrDefault(item => item.Collection == collection);
        if (existingItem != null)
            MemoriesPaths.Remove(existingItem);

        try
        {
            await _generation.Memory.DeleteDocumentAsync(collection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document");
            throw; 
        }
    }

    public async Task Generate(string prompt, TextGenerationParameters? generationParameters = null, TextPromptParameters? promptParameters = null)
    {
        try
        {
            if (_generation.State == GenerationState.Started)
            {
                _generation.CancelToken.Cancel();
                return;
            }
            if (_generation.SelectedGenerationModel == null)
            {
                _logger.LogWarning("Text Generation Model isn't loaded yet.");
                return;
            }
            if (SelectedMemory == null)
            {
                _logger.LogWarning("Memory isn't selected yet.");
                return;
            }

            if (string.IsNullOrEmpty(prompt) || SelectedMemory == null || !SelectedMemory.IsLoaded || _generation.SelectedGenerationModel == null)
                return;

            _generation.State = GenerationState.Started;
            var userMessage = new ChatMessage { Sender = "User", Message = prompt };
            var memoryMessage = new ChatMessage { Sender = "EGOIST", Message = "Gathering information from memories." };
            SelectedMemory.Messages.Add(userMessage);
            SelectedMemory.Messages.Add(memoryMessage);

            _generation.CancelToken = new();
            prompt = string.Empty;

            var answer = await _generation.Memory?.AskAsync(userMessage.Message, filter: new MemoryFilter().ByDocument(SelectedMemory.Collection), cancellationToken: _generation.CancelToken.Token)!;
            memoryMessage.Message = answer.Result;
            _generation.State = GenerationState.Finished;
            _generation.CancelToken.Dispose();
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Asking Memory Failed");
        }
    }
}