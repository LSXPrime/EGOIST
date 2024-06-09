using System.Collections.ObjectModel;

namespace EGOIST.Domain.Interfaces;

public interface ISession
{
    string SessionName { get; set; }
    string ToString();
}

public interface ISession<TMessage> : ISession where TMessage : IMessage
{
    ObservableCollection<TMessage> Messages { get; set; }
    TMessage AddMessage(string user, string message);
}