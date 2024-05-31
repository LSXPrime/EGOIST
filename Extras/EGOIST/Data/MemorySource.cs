using System.Collections.ObjectModel;

namespace EGOIST.Data;

public partial class MemorySource : ObservableObject
{
    [ObservableProperty]
    private string _collection = string.Empty;
    [ObservableProperty]
    private ObservableCollection<string> _documents = new();
    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();
    [ObservableProperty]
    private bool _isLoaded;
}