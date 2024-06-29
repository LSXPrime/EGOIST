using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class CompletionSession : EntityBase, ISession
{
    private string _sessionName = $"Completion {DateTime.Now}";
    private string _content = string.Empty;

    public string Name { get => _sessionName; set => Notify(ref _sessionName, value); }
    public string Content { get => _content; set => Notify(ref _content, value); }

    [NonSerialized]
    public IInference? Executor;
}