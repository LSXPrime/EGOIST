using System.Collections.ObjectModel;
using EGOIST.Domain.Entities;

namespace EGOIST.Domain.Interfaces;

public interface ISession
{
    string Name { get; set; }
    bool IsLoaded { get; set; }
    string? ToString();
}

public interface ISession<TMessage> : ISession where TMessage : IMessage
{
    ObservableCollection<TMessage> Messages { get; set; }
    TMessage AddMessage(string user, string message, Citation[]? citations = null);
}