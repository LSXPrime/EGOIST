using System.IO;
using Wpf.Ui.Controls;
using System.Collections.ObjectModel;
using EGOIST.Data;
using EGOIST.Enums;
using Newtonsoft.Json.Linq;
using Notification.Wpf;
using System.Windows.Controls;
using System.ComponentModel;
using NetFabric.Hyperlinq;
using LLama.Common;
using LLama;
using LLama.Native;
using LLama.Abstractions;
using System.Text;
using EGOIST.Helpers;

namespace EGOIST.ViewModels.Pages;
public partial class TextViewModel : ObservableObject, INavigationAware
{
    #region SharedVariables
    private bool _isInitialized = false;
    [ObservableProperty]
    private Dictionary<string, Visibility> _visibilityDict = new()
    {
        { "inchat", Visibility.Visible },
        { "playaudio", Visibility.Visible },
        { "pauseaudio", Visibility.Visible },
        { "stopaudio", Visibility.Visible },
        { "generatebutton", Visibility.Visible },
        { "resetaudiobutton", Visibility.Visible },
        { "transcribebutton", Visibility.Visible },
        { "transcribestarted", Visibility.Visible },
        { "transcribefinished", Visibility.Visible },
    };
    private readonly NotificationManager notification = new();
    internal ScrollViewer ChatContainerView;
    #endregion

    #region GenerationVariables
    [ObservableProperty]
    private GenerationState _generationState = GenerationState.None;

    [ObservableProperty]
    private TextGenerationParameters _parameters = new();

    [ObservableProperty]
    private ObservableCollection<ChatSession> _chatSessions = new();

    [ObservableProperty]
    private ChatSession _selectedChatSession;

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _generationModels = new();

    [ObservableProperty]
    private string _userInput = string.Empty;
    
    [ObservableProperty]
    private ModelInfo _selectedGenerationModel;

    [ObservableProperty]
    private ModelInfo.ModelInfoWeight _selectedGenerationWeight;

    [ObservableProperty]
    private string _textToGenerate = string.Empty;

    private LLamaWeights GenerationModel;
    private ModelParams GenerationModelParameters;
    private CancellationToken ChatCancelToken;
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
            VisibilityDict["playaudio"] = GenerationState == GenerationState.Started ? Visibility.Hidden : Visibility.Visible;
            VisibilityDict["pauseaudio"] = GenerationState == GenerationState.Started ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["stopaudio"] = GenerationState != GenerationState.Finished ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["generatebutton"] = GenerationState != GenerationState.Started ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["resetaudiobutton"] = GenerationState == GenerationState.Finished ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["transcribebutton"] = GenerationState != GenerationState.Started ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["transcribestarted"] = GenerationState != GenerationState.None ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["transcribefinished"] = GenerationState == GenerationState.Finished ? Visibility.Visible : Visibility.Hidden;
            OnPropertyChanged(nameof(VisibilityDict));

            Thread.Sleep(1000);
        }
    }

    private void LoadData()
    {
        GenerationModels.Clear();

        var models = ManagementViewModel._modelsInfo.AsValueEnumerable().Where(x => x.Type.Contains("Text") && x.Downloaded.Count > 0).ToList();
        GenerationModels = new(models);
    }
    #endregion

    #region GenerationMethods
    [RelayCommand]
    public void GenerationSwitchModels(string task)
    {
        try
        {
            if (task.Equals("unload"))
            {
                GenerationModel?.Dispose();
                ResetGeneration();
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
            var modelPath = $"{AppConfig.Instance.ModelsPath}\\{SelectedGenerationModel.Type.RemoveSpaces()}\\{SelectedGenerationModel.Name.RemoveSpaces()}\\{SelectedGenerationWeight.Weight.RemoveSpaces()}";
            GenerationModelParameters = new ModelParams(modelPath)
            {
                ContextSize = 4096,
                GpuLayerCount = isGpu ? 41 : 0
            };
            GenerationModel = LLamaWeights.LoadFromFile(GenerationModelParameters);

            notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model loaded Successfully", Type = NotificationType.Information }, areaName: "NotificationArea");
        }
    }

    [RelayCommand]
    private void ResetGeneration()
    {
        GenerationState = GenerationState.None;
        TextToGenerate = string.Empty;
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

        // TODO: Use InstructExecutor for Instruct Models
        // TODO: Change History Handeling to EGOIST instead LLamaSharp

        // Create a new chat session and add it to ChatSessions
        var newSession = new ChatSession
        {
            Executor = Executor,
            Inference = new LLama.ChatSession((InteractiveExecutor)Executor)
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
        if (!string.IsNullOrEmpty(UserInput))
        {
            if (SelectedGenerationModel == null)
            {
                notification.Show(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
                return;
            }

            if (SelectedChatSession == null)
                ChatCreate();

            SelectedChatSession?.AddMessage("User", UserInput);
            SelectedChatSession?.AddMessage("Assistant", string.Empty);


            ChatCancelToken = new();
            Thread thread = new(() => MessageSendBG(UserInput));
            thread.Start();
            UserInput = string.Empty;
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

        IAsyncEnumerable<string> tokens = SelectedChatSession?.Executor.InferAsync(UserMessage, MessageParams, ChatCancelToken);
        await foreach (string token in SelectedGenerationModel.TextConfig.Filter(tokens).WithCancellation(ChatCancelToken))
        {
            aiResponse.Message += token;
        }
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

            ChatCancelToken = new();
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
}

public partial class ChatSession : ObservableObject, INotifyPropertyChanged
{
    public string SessionName { get; set; }
    public LLama.ChatSession Inference;

    [ObservableProperty]
    public Dictionary<int, ChatLog> _chatMessages = new();
    public ObservableCollection<ChatMessage> Messages => ChatMessages[_currentLog].Messages;

    public int _currentLog = 0;
    public int CurrentLog
    {
        get => _currentLog;
        set
        {
            if (_currentLog != value)
            {
                _currentLog = value;

                // Create a copy of ChatHistory, Llama.cpp & LlamaSharp limitations
                LlamaHistory.Messages.Clear();
                foreach (var message in Messages)
                {
                    LlamaHistory.AddMessage((AuthorRole)Enum.Parse(typeof(AuthorRole), message.Sender), message.Message);
                }

                OnPropertyChanged(nameof(CurrentLog));
            }
        }
    }
    public string CurrentLogSTR => string.Format("{0} / {1}", CurrentLog, Edits);
    public int Edits => ChatMessages.Count - 1;
    public bool Edited => Edits > 0;

    #region LLamaSharp
    public ChatHistory LlamaHistory = new();
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
        LlamaHistory.AddMessage((AuthorRole)Enum.Parse(typeof(AuthorRole), user), message);
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