namespace EGOIST.Data;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _sender = string.Empty;
    [ObservableProperty]
    private string _message = string.Empty;
    [ObservableProperty]
    private bool _isEditable;
}
