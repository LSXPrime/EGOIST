using Wpf.Ui.Controls;
using System.Collections.ObjectModel;
using EGOIST.Data;
using EGOIST.Enums;
using Notification.Wpf;
using System.Windows.Controls;
using System.ComponentModel;
using NetFabric.Hyperlinq;
using LLama.Common;
using LLama;
using LLama.Native;
using EGOIST.Helpers;
using System.Windows.Controls.Primitives;
using System.Reflection;
using System.IO;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Services;

namespace EGOIST.ViewModels.Pages;
public partial class TextViewModel : ObservableObject, INavigationAware
{
    #region SharedVariables
    private bool _isInitialized = false;
    [ObservableProperty]
    private static Dictionary<string, Visibility> _visibilityDict = new()
    {
        { "inchat", Visibility.Visible },
        { "SendMessage", Visibility.Visible },
        { "pauseaudio", Visibility.Visible },
        { "stopaudio", Visibility.Visible },
        { "generatebutton", Visibility.Visible },
        { "resetaudiobutton", Visibility.Visible },
        { "transcribebutton", Visibility.Visible },
        { "transcribestarted", Visibility.Visible },
        { "transcribefinished", Visibility.Visible },
    };
    [ObservableProperty]
    private Dictionary<string, string> _switchableIcons = new()
    {
        {"SendMessage", "Send24" }
    };

    private readonly NotificationManager notification = new();
    #endregion

    #region GenerationVariables
    [ObservableProperty]
    private GenerationState _generationState = GenerationState.None;
    [ObservableProperty]
    private TextGenerationParameters _parameters = new();
    [ObservableProperty]
    private ObservableCollection<ModelInfo> _generationModels = new();
    [ObservableProperty]
    private ModelInfo _selectedGenerationModel;
    [ObservableProperty]
    private ModelInfo.ModelInfoWeight _selectedGenerationWeight;

    private LLamaWeights GenerationModel;
    private ModelParams GenerationModelParameters;
    private CancellationTokenSource GenerationCancelToken;
    #endregion

    #region ChatVariables
    [ObservableProperty]
    private ObservableCollection<ChatSession> _chatSessions = new();
    [ObservableProperty]
    private ChatSession _selectedChatSession;
    [ObservableProperty]
    private string _chatUserInput = string.Empty;

    internal ScrollViewer ChatContainerView;
    #endregion

    #region CompletionVariables
    [ObservableProperty]
    private ObservableCollection<CompletionSession> _completionSessions = new();
    [ObservableProperty]
    private CompletionSession _selectedCompletionSession;

    private string _completionText = string.Empty;
    public string CompletionText
    {
        get => _completionText;
        set
        {
            _completionText = value;
            UpdateCompletionChanges();
            OnPropertyChanged(nameof(CompletionText));
        }
    }
    [ObservableProperty]
    private string _completionStatics = string.Empty;
    [ObservableProperty]
    private (string, string, string) _completionPrompt = new("I want you to act as a storyteller. You will come up with entertaining stories that are engaging, imaginative and captivating for the audience.", string.Empty, string.Empty);

    internal static string[] WordsDictionary;
    internal Wpf.Ui.Controls.TextBox CompletionTextEditor;
    internal Popup CompletionSuggestionPopup;
    internal ListView CompletionSuggestionList;
    #endregion

    #region InitlizingMethods
    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        LoadData();
        AppConfig.Instance.ConfigSavedEvent += LoadData;
        _isInitialized = true;
        GenerationState = GenerationState.None;
        Thread thread = new(UIHandler);
        thread.Start();
    }

    private void UIHandler()
    {
        while (true)
        {
            VisibilityDict["inchat"] = SelectedChatSession != null ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["incompletion"] = SelectedCompletionSession != null ? Visibility.Visible : Visibility.Hidden;
            SwitchableIcons["SendMessage"] = GenerationState != GenerationState.Started ? "Send24" : "Stop24";

            OnPropertyChanged(nameof(VisibilityDict));
            OnPropertyChanged(nameof(SwitchableIcons));
            Thread.Sleep(1000);
        }
    }

    private void LoadData()
    {
        GenerationModels.Clear();

        var models = ManagementViewModel._modelsInfo.AsValueEnumerable().Where(x => x.Type.Contains("Text") && x.Downloaded.Count > 0).ToList();
        GenerationModels = new(models);

        var resourceName = "EGOIST.Assets.dictionary.txt";
        Assembly assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);
        WordsDictionary = reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    }
    #endregion

    #region GenerationMethods
    [RelayCommand]
    public void GenerationSwitchModels(string task)
    {
        try
        {
            if (task.Contains("unload"))
            {
                GenerationModel?.Dispose();
                GenerationState = GenerationState.None;
                SelectedGenerationModel = null;
                notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model Unloaded", Type = NotificationType.Information }, areaName: "NotificationArea");
                return;
            }
            notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model {SelectedGenerationModel.Name} Started loading", Type = NotificationType.Information }, areaName: "NotificationArea");
            Thread thread = new(SwitchMethodBG);
            thread.Start();
        }
        catch (Exception ex)
        {
            notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Model Switching to {SelectedGenerationModel.Name} Failed, Exception: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea");
        }

        void SwitchMethodBG()
        {
            var isGpu = AppConfig.Instance.Device == Device.GPU;
            NativeLibraryConfig.Instance.WithCuda(isGpu).WithAvx(NativeLibraryConfig.AvxLevel.Avx512).WithAutoFallback(true);

            // Load model
            var modelPath = $"{AppConfig.Instance.ModelsPath}\\{SelectedGenerationModel.Type.RemoveSpaces()}\\{SelectedGenerationModel.Name.RemoveSpaces()}\\{SelectedGenerationWeight.Weight.RemoveSpaces()}.{SelectedGenerationWeight.Extension.ToLower().RemoveSpaces()}";
            GenerationModelParameters = new ModelParams(modelPath)
            {
                ContextSize = 4096,
                GpuLayerCount = isGpu ? 41 : 0
            };
            GenerationModel = LLamaWeights.LoadFromFile(GenerationModelParameters);

            notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model loaded Successfully", Type = NotificationType.Information }, areaName: "NotificationArea");
        }
    }
    #endregion

    #region ChatMethods

    [RelayCommand]
    private void ChatCreate()
    {
        if (SelectedGenerationModel == null)
        {
            notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
            return;
        }

        // Initialize a llama chat session
        var context = GenerationModel.CreateContext(GenerationModelParameters);
        var Executor = new InteractiveExecutor(context);

        // TODO: Change History Handeling to EGOIST instead LLamaSharp

        // Create a new chat session and add it to ChatSessions
        var newSession = new ChatSession
        {
            Executor = Executor
        };
        ChatSessions.Add(newSession);
        SelectedChatSession = newSession;
    }

    [RelayCommand]
    private void ChatDelete()
    {
        notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Chat {SelectedChatSession.SessionName} Deleted", Type = NotificationType.None }, areaName: "NotificationArea");

        foreach (ChatLog log in SelectedChatSession.ChatMessages.Values)
        {
            log.Messages.Clear();
        }
        SelectedChatSession.ChatMessages.Clear();
        ChatSessions.Remove(SelectedChatSession);
        SelectedChatSession = null;
    }

    [RelayCommand]
    private void MessageSend()
    {
        if (GenerationState == GenerationState.Started)
        {
            GenerationCancelToken.Cancel();
            return;
        }

        if (!string.IsNullOrEmpty(ChatUserInput))
        {
            if (SelectedGenerationModel == null)
            {
                notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
                return;
            }
            if (SelectedChatSession == null)
                ChatCreate();

            GenerationState = GenerationState.Started;

            SelectedChatSession?.AddMessage("User", ChatUserInput);
            SelectedChatSession?.AddMessage("Assistant", string.Empty);

            var Prompt = SelectedGenerationModel.TextConfig.Prompt(ChatUserInput);

            GenerationCancelToken = new();
            Thread thread = new(() => MessageSendBG(Prompt));
            thread.Start();
            ChatUserInput = string.Empty;
        }

        // Scroll to the last item in Chat Container
        ChatContainerView.ScrollToBottom();
    }

    async void MessageSendBG(string UserMessage)
    {
        var MessageParams = new InferenceParams()
        {
            MaxTokens = Parameters.MaxTokens,
            Temperature = Parameters.Randomness,
            TopP = Parameters.RandomnessBooster,
            TopK = (int)(Parameters.OptimalProbability * 100),
            FrequencyPenalty = Parameters.FrequencyPenalty,
            AntiPrompts = new List<string> { "User:" }
        };

        var aiResponse = SelectedChatSession?.Messages.Last();

        IAsyncEnumerable<string> tokens = SelectedChatSession?.Executor.InferAsync(UserMessage, MessageParams, GenerationCancelToken.Token);
        await foreach (var token in tokens)
        {
            var filteredToken = token;
            foreach (string blackToken in SelectedGenerationModel.TextConfig.BlackList)
            {
                if (token.Contains(blackToken))
                {
                    filteredToken = filteredToken.Replace(blackToken, " ");
                    break;
                }
            }

            aiResponse.Message += filteredToken;
        }

        GenerationState = GenerationState.Finished;
        GenerationCancelToken.Dispose();
    }

    [RelayCommand]
    private void MessageCopy(ChatMessage message)
    {
        Clipboard.SetText(message.Message);
        notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Message Copied to Clipboard", Type = NotificationType.None }, areaName: "NotificationArea");
    }

    [RelayCommand]
    private void MessageEdit(ChatMessage editedMessage)
    {
        // Will be commentend for now, Llama.cpp & LlamaSharp limitations
        /*
        if (editedMessage.IsEditable == true)
        {
            var updatedLog = new ChatLog() { ID = SelectedChatSession.Edits + 1 };

            int index = SelectedChatSession.Messages.IndexOf(editedMessage);
            if (index != -1)
                for (int i = 0; i <= index; i++)
                    updatedLog.Messages.Add(SelectedChatSession.Messages[i]);

            SelectedChatSession.ChatMessages.Add(updatedLog.ID, updatedLog);
            SelectedChatSession.CurrentLog = updatedLog.ID;
            var message = new ChatMessage { Sender = "Assistant", Message = string.Empty };
            SelectedChatSession?.Messages.Add(message);

            GenerationCancelToken = new();
            Thread thread = new Thread(() => MessageSendBG(message, editedMessage, ChatContainerView));
            thread.Start();

            SelectedChatSession.OnPropertyChanged(nameof(SelectedChatSession.Messages));
        }

        editedMessage.IsEditable = !editedMessage.IsEditable;
        // Scroll to the last item in Chat Container
        ChatContainerView.ScrollToBottom();
        */
    }

    [RelayCommand]
    private void MessageSlide(string parameter)
    {
        SelectedChatSession.CurrentLog += int.Parse(parameter);
        SelectedChatSession.OnPropertyChanged(nameof(SelectedChatSession.Messages));
    }

    #endregion

    #region CompletionMethods

    private void UpdateCompletionChanges()
    {
        var CompletionLineCount = CompletionText.Split('\n').Length;
        var CompletionWord = CompletionText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var CompletionCharacterCount = CompletionText.Length;

        CompletionStatics = $"L: {CompletionLineCount} || W: {CompletionWord.Length} || C: {CompletionCharacterCount}";

        if (GenerationState == GenerationState.Started)
            return;

        string searchText = CompletionWord[^1].ToLower();
        string[] filteredList = WordsDictionary.AsValueEnumerable().Where(word => word.StartsWith(searchText)).ToArray();
        if (CompletionTextEditor.IsFocused && filteredList.Length > 0)
        {
            CompletionSuggestionList.ItemsSource = filteredList;
            CompletionSuggestionList.SelectedItem = filteredList[0];
            var caretPosition = CompletionTextEditor.GetRectFromCharacterIndex(CompletionTextEditor.CaretIndex);
            var point = CompletionTextEditor.PointToScreen(new Point(caretPosition.Right, caretPosition.Bottom));
            CompletionSuggestionPopup.PlacementRectangle = new Rect(point.X, point.Y + caretPosition.Height, 0, 0);
            CompletionSuggestionPopup.IsOpen = true;
        }
        else
            CompletionSuggestionPopup.IsOpen = false;
    }

    [RelayCommand]
    private async Task CompletionSearch()
    {
        var searchText = new Wpf.Ui.Controls.TextBox
        {
            Text = CompletionTextEditor.SelectedText,
            PlaceholderText = "Search keyword",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        var replaceText = new Wpf.Ui.Controls.TextBox
        {
            Text = string.Empty,
            PlaceholderText = "Replacement keyword",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        var searchBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "Search / Replace",
            Content = new StackPanel { Children = { searchText, replaceText } },
            PrimaryButtonText = "Search",
            SecondaryButtonText = "Replace",
            CloseButtonText = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        var result = await searchBox.ShowDialogAsync();

        if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
        {
            if (!string.IsNullOrEmpty(searchText.Text))
            {
                int cursorIndex = CompletionTextEditor.SelectionStart + CompletionTextEditor.SelectionLength;
                int index = CompletionText.IndexOf(searchText.Text, cursorIndex);

                if (index != -1)
                {
                    CompletionTextEditor.Focus();

                    CompletionTextEditor.SelectionStart = index;
                    CompletionTextEditor.SelectionLength = searchText.Text.Length;
                }
                else
                    await new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Search / Replace",
                        Content = "No results found",
                        CloseButtonText = "Close",
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center
                    }.ShowDialogAsync();
            }
        }
        else if (result == Wpf.Ui.Controls.MessageBoxResult.Secondary)
        {
            if (!string.IsNullOrEmpty(searchText.Text))
                CompletionText = CompletionText.Replace(searchText.Text, replaceText.Text);
        }
    }

    [RelayCommand]
    private void CompletionCreate()
    {
        if (SelectedGenerationModel == null)
        {
            notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
            return;
        }

        // Initialize a llama completion session
        var newSession = new CompletionSession
        {
            Executor = new StatelessExecutor(GenerationModel, GenerationModelParameters)
        };
        CompletionSessions.Add(newSession);
        SelectedCompletionSession = newSession;
    }

    [RelayCommand]
    private void CompletionDelete()
    {
        notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Session {SelectedCompletionSession.SessionName} Deleted", Type = NotificationType.None }, areaName: "NotificationArea");

        SelectedCompletionSession.Executor.Context.Dispose();
        CompletionSessions.Remove(SelectedCompletionSession);
        SelectedCompletionSession = null;
    }

    [RelayCommand]
    private async Task CompletionGenerate()
    {
        if (SelectedGenerationModel == null)
        {
            notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
            return;
        }
        if (GenerationState == GenerationState.Started)
        {
            GenerationCancelToken.Cancel();
            return;
        }
        if (SelectedCompletionSession == null)
            CompletionCreate();

        GenerationState = GenerationState.Started;
        var Prompt = (!string.IsNullOrEmpty(CompletionPrompt.Item1) || !string.IsNullOrEmpty(CompletionPrompt.Item2) || !string.IsNullOrEmpty(CompletionPrompt.Item3)) ? SelectedGenerationModel.TextConfig.Prompt(CompletionText, CompletionPrompt.Item1, CompletionPrompt.Item2, CompletionPrompt.Item3) : SelectedGenerationModel.TextConfig.Prompt(CompletionText);

        GenerationCancelToken = new();
        Thread thread = new(() => CompletionGenerateBG(Prompt));
        thread.Start();

        async void CompletionGenerateBG(string prompt)
        {
            var MessageParams = new InferenceParams()
            {
                MaxTokens = Parameters.MaxTokens,
                Temperature = Parameters.Randomness,
                TopP = Parameters.RandomnessBooster,
                TopK = (int)(Parameters.OptimalProbability * 100),
                FrequencyPenalty = Parameters.FrequencyPenalty,
                AntiPrompts = new List<string> { "User:" }
            };

            IAsyncEnumerable<string> tokens = SelectedCompletionSession?.Executor.InferAsync(prompt, MessageParams, GenerationCancelToken.Token);
            await foreach (var token in tokens)
            {
                var filteredToken = token;
                foreach (string blackToken in SelectedGenerationModel.TextConfig.BlackList)
                {
                    if (token.Contains(blackToken))
                    {
                        filteredToken = filteredToken.Replace(blackToken, " ");
                        break;
                    }
                }

                CompletionText += filteredToken;
            }

            GenerationState = GenerationState.Finished;
            GenerationCancelToken.Dispose();
        }
    }

    #endregion

    #region InteractionMethods

    private void InteractData()
    {
    }

    #endregion
}

public partial class CompletionSession : ObservableObject
{
    public string SessionName { get; set; }
    [ObservableProperty]
    public string _content = string.Empty;

    #region LLamaSharp
    public StatelessExecutor Executor;
    #endregion


    public CompletionSession()
    {
        SessionName = $"Completion {DateTime.Now}";
    }
}
public partial class ChatSession : ObservableObject, INotifyPropertyChanged
{
    public string SessionName { get; set; }

    [ObservableProperty]
    public Dictionary<int, ChatLog> _chatMessages = new();
    public ObservableCollection<ChatMessage> Messages => ChatMessages[_currentLog].Messages;

    [ObservableProperty]
    public int _currentLog = 0;
    public string CurrentLogSTR => string.Format("{0} / {1}", CurrentLog, Edits);
    public int Edits => ChatMessages.Count - 1;
    public bool Edited => Edits > 0;
    public IKernelBuilder KernelBuilder;

    #region LLamaSharp
    public StatefulExecutorBase Executor;
    #endregion


    public ChatSession()
    {
        SessionName = $"Chat {DateTime.Now}";
        ChatMessages.Add(0, new ChatLog());
    }

    public void AddMessage(string user, string message)
    {
        var userInput = new ChatMessage { Sender = user, Message = message };
        Messages.Add(userInput);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class ChatLog : ObservableObject
{
    [ObservableProperty]
    public int _ID = 0;
    [ObservableProperty]
    public ObservableCollection<ChatMessage> _messages = new();
}

public partial class ChatMessage : ObservableObject
{
    public string Sender { get; set; }
    [ObservableProperty]
    public string _message;
    [ObservableProperty]
    public bool _isEditable;
}