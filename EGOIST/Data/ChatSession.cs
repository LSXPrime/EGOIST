using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using LLama;

namespace EGOIST.Data;

public partial class ChatSession : ObservableObject, INotifyPropertyChanged
{
    public string SessionName { get; set; }

    [ObservableProperty]
    private Dictionary<int, ObservableCollection<ChatMessage>> _chatMessages = new();
    public ObservableCollection<ChatMessage> Messages => ChatMessages[CurrentLog];


    #region Unused due LLama tokenizer/embeddings limitions
    [ObservableProperty]
    public int _currentLog = 0;
    public string CurrentLogSTR => $"{CurrentLog} / {Edits}";
    public int Edits => ChatMessages.Count - 1;
    public bool Edited => Edits > 0;
    #endregion

    #region LLamaSharp
    [NonSerialized]
    public StatefulExecutorBase? Executor;
    #endregion


    public ChatSession()
    {
        SessionName = $"Chat {DateTime.Now}";
        ChatMessages.Add(0, new());
    }

    public ChatMessage AddMessage(string user, string message)
    {
        var messageInput = new ChatMessage { Sender = user, Message = message };
        Messages.Add(messageInput);

        return messageInput;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        foreach (var message in Messages)
        {
            stringBuilder.AppendLine($"{message.Sender}: {message.Message}");
        }

        return stringBuilder.ToString();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
