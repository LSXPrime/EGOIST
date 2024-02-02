using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using LLama;
using NetFabric.Hyperlinq;
using Newtonsoft.Json;

namespace EGOIST.Data;

public partial class RoleplaySession : ObservableObject, INotifyPropertyChanged
{
    public string SessionName { get; set; }
    public string UserRoleName = "User";
    [ObservableProperty]
    private List<RoleplayCharacterEXT> _characters = new();
    [ObservableProperty]
    private ObservableCollection<RoleplayMessage> _messages = new();
    public RoleplayMessage? LastMessage;

    public RoleplaySession()
    {
        SessionName = $"Roleplay {DateTime.Now}";
    }

    public RoleplayMessage AddMessage(RoleplayCharacter user, string message)
    {
        var messageInput = new RoleplayMessage { Sender = user, Message = message };
        LastMessage = messageInput;
        Messages.Add(messageInput);

        return messageInput;
    }

    public List<RoleplayMessage> GetMissedMessages(RoleplayCharacter character)
    {
        var senderMessages = Messages.Where(m => m.Sender == character).ToList();

        if (senderMessages.Count > 0)
        {
            var lastMessage = senderMessages.Last();
            var messagesAfterLast = Messages.SkipWhile(m => m != lastMessage).Skip(1).ToList();
            return messagesAfterLast;
        }

        return null;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var message in Messages)
        {
            stringBuilder.AppendLine($"{message.Sender?.Name ?? UserRoleName}: {message.Message}");
        }

        return stringBuilder.ToString();
    }

    public string ToString(List<RoleplayMessage> roleplayMessages)
    {
        if (roleplayMessages == null)
            return string.Empty;

        var stringBuilder = new StringBuilder();
        foreach (var message in roleplayMessages)
            stringBuilder.Append($"{(message.Sender?.Name ?? UserRoleName)}: {message.Message}\n");

        stringBuilder.Append($"{UserRoleName}: ");
        return stringBuilder.ToString();
    }


    public string ToString(RoleplayCharacter character, bool exampleDialogue = false)
    {
        var stringBuilder = new StringBuilder();

        var messages = exampleDialogue
            ? character.ExampleDialogue.Select(msg => new { msg.Sender, msg.Message })
            : Messages.Where(x => x.Sender == character).Select(msg => new { Sender = msg.Sender?.Name ?? UserRoleName, msg.Message });

        foreach (var message in messages)
        {
            stringBuilder.AppendLine($"{message.Sender}: {message.Message}");
        }

        return stringBuilder.ToString();
    }
}

public partial class RoleplayMessage : ObservableObject
{
    [ObservableProperty]
    private RoleplayCharacter? _sender;
    [ObservableProperty]
    private string _message = string.Empty;
}

public partial class RoleplayCharacterEXT : ObservableObject
{
    [ObservableProperty]
    private RoleplayCharacter? _character;
    [JsonIgnore]
    public StatefulExecutorBase Executor { get; set; }
}