using System.Text.Json.Serialization;
using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class ImageHistoryEntry : HistoryEntryBase
{
    private readonly ImageGenerationParameters _parameters = new();
    public ImageGenerationParameters Parameters { get => _parameters; init => Notify(ref _parameters, value); }
    
    public ImageHistoryEntry()
    {
    }
    
    [JsonConstructor]
    public ImageHistoryEntry(ImageGenerationParameters parameters)
    {
        Parameters = parameters;
    }
}