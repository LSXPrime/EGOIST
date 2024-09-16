using System.Globalization;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class CompletionSession : EntityBase, ISession
{
    private string _sessionName = $"Completion {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture)}";
    private string _content = string.Empty;
    [NonSerialized]
    private bool _isLoaded;

    public string Name { get => _sessionName; set => Notify(ref _sessionName, value); }
    public string Content { get => _content; set => Notify(ref _content, value); }
    
    public bool IsLoaded
    {
        get => _isLoaded;
        set => Notify(ref _isLoaded, value);
    }

    [NonSerialized]
    public IInference? Executor;
}