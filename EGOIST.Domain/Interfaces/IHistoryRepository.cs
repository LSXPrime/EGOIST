using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Interfaces;

public interface IHistoryRepository
{
    Task<IEnumerable<HistoryEntryBase>> GetAllHistory(Dictionary<string, string> parameters);
    Task<HistoryEntryBase?> GetHistoryEntry(Dictionary<string, string> parameters);
    Task<HistoryEntryBase> AddHistoryEntry(HistoryEntryBase entry, byte[]? data);
    Task DeleteHistoryEntry(HistoryEntryBase entry);
}