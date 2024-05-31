using LLama;

namespace EGOIST.Data;

public partial class CompletionSession : ObservableObject
{
    public string SessionName { get; set; }
    [ObservableProperty]
    public string _content = string.Empty;

    #region LLamaSharp
    [NonSerialized]
    public StatelessExecutor? Executor;
    #endregion


    public CompletionSession()
    {
        SessionName = $"Completion {DateTime.Now}";
    }
}
