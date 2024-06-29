using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class MemorySource : EntityBase, ISession<ChatMessage>
{
    private string _collection = string.Empty;
    private bool _isLoaded;
    public string Name
    {
        get => _collection;
        set => Notify(ref _collection, value);
    }

    public ObservableCollection<string> Documents { get; set; } = [];
    public ObservableCollection<ChatMessage> Messages { get; set; } = [];
    public ChatMessage AddMessage(string user, string message)
    {
        var messageInput = new ChatMessage { Sender = user, Message = message };
        Messages.Add(messageInput);
        return messageInput;
    }

    public bool IsLoaded { get => _isLoaded; set => Notify(ref _isLoaded, value); }
}