using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class MemorySource : EntityBase
{
    private string _collection = string.Empty;
    private bool _isLoaded;

    public string Collection { get => _collection; set => Notify(ref _collection, value); }
    public ObservableCollection<string> Documents { get; set; } = [];
    public ObservableCollection<ChatMessage> Messages { get; set; } = [];
    public bool IsLoaded { get => _isLoaded; set => Notify(ref _isLoaded, value); }
}