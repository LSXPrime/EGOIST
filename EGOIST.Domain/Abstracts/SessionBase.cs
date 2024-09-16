using System.Collections.ObjectModel;
using System.Text;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Abstracts;

public abstract class SessionBase<TMessage>(string sessionName) : EntityBase, ISession<TMessage>
    where TMessage : EntityBase, IMessage, new()
{
    public string Name
    {
        get => sessionName;
        set => Notify(ref sessionName, value);
    }
    
    [NonSerialized]
    private bool _isLoaded;
    public bool IsLoaded
    {
        get => _isLoaded;
        set => Notify(ref _isLoaded, value);
    }

    public ObservableCollection<TMessage> Messages { get; set; } = [];

    public virtual TMessage AddMessage(string user, string message, Citation[]? citations = null)
    {
        var messageInput = new TMessage
        {
            Message = message,
            Citations = citations ?? []
        };
        messageInput.GetType().GetProperty("Sender")!.SetValue(messageInput, user);
   //     messageInput.GetType().GetProperty("Message")!.SetValue(messageInput, message);
        Messages.Add(messageInput);

        return messageInput;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var message in Messages)
        {
            var sender = message.GetType().GetProperty("Sender")!.GetValue(message)!.ToString();
            var msg = message.GetType().GetProperty("Message")!.GetValue(message)!.ToString();
            stringBuilder.AppendLine($"{sender}: {msg}");
        }

        return stringBuilder.ToString();
    }
}