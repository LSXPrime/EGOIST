using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class CompletionSession : BaseEntity
{
    private string _sessionName = $"Completion {DateTime.Now}";
    private string _content = string.Empty;

    public string SessionName { get => _sessionName; set => Notify(ref _sessionName, value); }
    public string Content { get => _content; set => Notify(ref _content, value); }

    [NonSerialized]
    public IInference? Executor;
}