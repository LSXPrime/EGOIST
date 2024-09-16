using System.Text.Json;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EGOIST.Infrastructure.Repositories;

public class LocalHistoryRepository(ILogger<LocalHistoryRepository> logger, IFileSystemService fileSystemService)
    : IHistoryRepository
{
    public async Task<IEnumerable<HistoryEntryBase>> GetAllHistory(Dictionary<string, string> parameters)
    {
        var historyPath = Path.Combine(AppConfig.Instance.Parameters.ResultsPath, parameters["Type"]);
        if (!fileSystemService.DirectoryExists(historyPath))
        {
            logger.LogWarning("Results path {historyPath} does not exist.", historyPath);
            return [];
        }

        var history = await Task.WhenAll(
            Directory
                .EnumerateFiles(historyPath, "*.json", SearchOption.AllDirectories)
                .Select(async file =>
                {
                    var json = await fileSystemService.ReadAllTextAsync(file);
                    var entry = JsonSerializer.Deserialize<HistoryEntryBase>(json);
                    if (entry != null) entry.Path = Path.Combine(historyPath, entry.Name + entry.Extension);
                    return entry;
                })
        );

        return history.Where(entry => entry != null)!;
    }

    public async Task<HistoryEntryBase?> GetHistoryEntry(Dictionary<string, string> parameters)
    {
        var historyPath = Path.Combine(AppConfig.Instance.Parameters.ResultsPath, parameters["Type"]);
        var entryPath = Path.Combine(historyPath, parameters["Name"] + ".json");
        if (!fileSystemService.DirectoryExists(historyPath) || !fileSystemService.FileExists(entryPath))
        {
            logger.LogWarning("Results path {historyPath} does not exist.", historyPath);
            return null;
        }

        var json = await fileSystemService.ReadAllTextAsync(entryPath);
        return JsonSerializer.Deserialize<HistoryEntryBase>(json);
    }

    public async Task<HistoryEntryBase> AddHistoryEntry(HistoryEntryBase entry, byte[]? data)
    {
        var historyPath = Path.Combine(AppConfig.Instance.Parameters.ResultsPath, entry.Type);
        if (!fileSystemService.DirectoryExists(historyPath)) 
            fileSystemService.CreateDirectory(historyPath);

        var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions() { WriteIndented = true });
        await fileSystemService.WriteAllTextAsync(Path.Combine(historyPath, entry.Name + ".json"), json);
        var filePath = Path.Combine(historyPath, entry.Name + entry.Extension);
        if (data != null)
        {
            await fileSystemService.WriteAllBytesAsync(filePath, data);
            entry.Path = filePath;
        }

        return entry;
    }

    public Task DeleteHistoryEntry(HistoryEntryBase entry)
    {
        var historyPath = Path.Combine(AppConfig.Instance.Parameters.ResultsPath, entry.Type);
        if (fileSystemService.FileExists(Path.Combine(historyPath, entry.Name + ".json")))
            fileSystemService.DeleteFile(Path.Combine(historyPath, entry.Name + ".json"));
        if (fileSystemService.FileExists(Path.Combine(historyPath, entry.Name + entry.Extension)))
            fileSystemService.DeleteFile(Path.Combine(historyPath, entry.Name + entry.Extension));
        
        return Task.CompletedTask;
    }
}