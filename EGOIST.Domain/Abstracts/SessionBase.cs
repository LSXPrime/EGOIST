using System.Collections.ObjectModel;
using System.Text;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Abstracts;

public abstract class SessionBase<TMessage>(string sessionName) : BaseEntity, ISession<TMessage>
    where TMessage : BaseEntity, IMessage, new()
{
    public string SessionName
    {
        get => sessionName;
        init => Notify(ref sessionName, value);
    }

    public ObservableCollection<TMessage> Messages { get; set; } = [];

    public TMessage AddMessage(string user, string message)
    {
        var messageInput = new TMessage();
        messageInput.GetType().GetProperty("Sender")!.SetValue(messageInput, user);
        messageInput.GetType().GetProperty("Message")!.SetValue(messageInput, message);
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