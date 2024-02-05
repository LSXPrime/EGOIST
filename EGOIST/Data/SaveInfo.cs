using System.Collections.ObjectModel;
using EGOIST.ViewModels.Pages;

namespace EGOIST.Data;

public class SaveInfo
{
    public ObservableCollection<ChatSession> ChatSessions = new();
    public ObservableCollection<CompletionSession> CompletionSessions = new();
    public ObservableCollection<MemorySource> MemoriesPaths = new();
    public ObservableCollection<RoleplaySession> RoleplaySessions = new();
    public string ChatUserInput = string.Empty;
    public string CompletionText = string.Empty;
    public string MemoryUserInput = string.Empty;


    private static SaveInfo? instance;
    public static SaveInfo Instance => instance ??= new SaveInfo();
}
