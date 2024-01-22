using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using Notification.Wpf;
using NetFabric.Hyperlinq;

using EGOIST.Data;
using EGOIST.Enums;
using EGOIST.Helpers;

using LLama.Common;
using LLama;
using LLama.Native;
using LLamaSharp.KernelMemory;

using Microsoft.KernelMemory;
using Microsoft.KernelMemory.ContentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.Handlers;
using Microsoft.KernelMemory.MemoryStorage.DevTools;

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
    #endregion

    #region GenerationVariables
    [ObservableProperty]
    private GenerationState _generationState = GenerationState.None;
    [ObservableProperty]
    private TextGenerationParameters _parameters = new();
    [ObservableProperty]
    private ObservableCollection<ModelInfo> _generationModels = new();
    [ObservableProperty]
    private ModelInfo? _selectedGenerationModel;
    [ObservableProperty]
    private ModelInfo.ModelInfoWeight? _selectedGenerationWeight;

    private LLamaWeights? GenerationModel;
    private LLamaEmbedder? GenerationModelEmbedder;
    private ModelParams? GenerationModelParameters;
    private IKernelMemory? GenerationMemory;
    private CancellationTokenSource? GenerationCancelToken;
    #endregion

    #region ChatVariables
    [ObservableProperty]
    private ObservableCollection<ChatSession> _chatSessions = new();
    [ObservableProperty]
    private ChatSession? _selectedChatSession;
    [ObservableProperty]
    private string _chatUserInput = string.Empty;

    internal ScrollViewer? ChatContainerView;
    #endregion

    #region CompletionVariables
    [ObservableProperty]
    private ObservableCollection<CompletionSession> _completionSessions = new();
    private CompletionSession? _selectedCompletionSession;
    public CompletionSession SelectedCompletionSession
    {
        get => _selectedCompletionSession;
        set
        {
            _selectedCompletionSession = value;
            CompletionText = value != null ? _selectedCompletionSession.Content : string.Empty;
            OnPropertyChanged(nameof(SelectedCompletionSession));
        }
    }

    private string _completionText = string.Empty;
    public string CompletionText
    {
        get => SelectedCompletionSession != null ? SelectedCompletionSession.Content : _completionText;
        set
        {
            if (SelectedCompletionSession != null)
            {
                SelectedCompletionSession.Content = value;
                OnPropertyChanged(nameof(SelectedCompletionSession.Content));
            }

            _completionText = value;
            UpdateCompletionChanges();
            OnPropertyChanged(nameof(CompletionText));
        }
    }
    [ObservableProperty]
    private string _completionStatics = string.Empty;
    [ObservableProperty]
    private ModelInfo.TextConfiguration _completionPrompt = new() { SystemPrompt = "I want you to act as a storyteller. You will come up with entertaining stories that are engaging, imaginative and captivating for the audience." };

    internal static string[]? WordsDictionary;
    internal Wpf.Ui.Controls.TextBox? CompletionTextEditor;
    internal Popup? CompletionSuggestionPopup;
    internal ListView? CompletionSuggestionList;
    #endregion

    #region MemoryVariables

    [ObservableProperty]
    private ObservableCollection<MemorySource> _memoriesPaths = new();

    [ObservableProperty]
    private MemorySource? _selectedMemory;

    [ObservableProperty]
    private string _memoryUserInput = string.Empty;
    
    internal ScrollViewer? MemoryContainerView;

    #endregion

    #region InitlizingMethods
    public void OnStartup()
    {
        Extensions.onSaveData += SaveData;
        Extensions.onLoadData += LoadData;
        AppConfig.Instance.ConfigSavedEvent += LoadData;

        GenerationState = GenerationState.None;
        Thread thread = new(UIHandler);
        thread.Start();
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
        Extensions.SaveData();
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

        var dataPath = $"{Directory.GetCurrentDirectory()}\\EGOIST_Data.bin";
        if (!File.Exists(dataPath))
            return;

        var dataEncrypted = File.ReadAllBytes(dataPath);
        var data = Extensions.Decrypt<SaveInfo>(dataEncrypted);
        if (data == null)
            return;

        _chatSessions = data.ChatSessions;
        CompletionSessions = data.CompletionSessions;
        MemoriesPaths = data.MemoriesPaths;
        ChatUserInput = data.ChatUserInput;
        CompletionText = data.CompletionText;
        MemoryUserInput = data.MemoryUserInput;
    }

    private void SaveData()
    {
        var data = SaveInfo.Instance;
        data.ChatSessions = _chatSessions;
        data.CompletionSessions = CompletionSessions;
        data.MemoriesPaths = MemoriesPaths;
        data.ChatUserInput = ChatUserInput;
        data.CompletionText = CompletionText;
        data.MemoryUserInput = MemoryUserInput;

        var dataEncrypted = Extensions.Encrypt(data);
        File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\EGOIST_Data.bin", dataEncrypted);
    }
    #endregion

    #region GenerationMethods
    [RelayCommand]
    public void GenerationSwitchModels()
    {
        new Thread(() =>
        {
            try
            {
                if (SelectedGenerationModel == null)
                    return;

                Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model {SelectedGenerationModel.Name} Started loading", Type = NotificationType.Information }, areaName: "NotificationArea");
                var isGpu = AppConfig.Instance.Device == Device.GPU;
                NativeLibraryConfig.Instance.WithCuda(isGpu).WithAvx(NativeLibraryConfig.AvxLevel.Avx512).WithAutoFallback(true);

                var modelPath = $"{AppConfig.Instance.ModelsPath}\\{SelectedGenerationModel.Type.RemoveSpaces()}\\{SelectedGenerationModel.Name.RemoveSpaces()}\\{SelectedGenerationWeight.Weight.RemoveSpaces()}.{SelectedGenerationWeight.Extension.ToLower().RemoveSpaces()}";
                GenerationModelParameters = new ModelParams(modelPath)
                {
                    ContextSize = 4096,
                    GpuLayerCount = isGpu ? 41 : 0
                };
                GenerationModel = LLamaWeights.LoadFromFile(GenerationModelParameters);
                GenerationModelEmbedder = new(GenerationModel, GenerationModelParameters);

                GenerationMemory = new KernelMemoryBuilder()
                    .WithLLamaSharpTextEmbeddingGeneration(new LLamaSharpTextEmbeddingGenerator(GenerationModelEmbedder))
                    .WithLLamaSharpTextGeneration(new LlamaSharpTextGenerator(GenerationModel, GenerationModelEmbedder.Context))
                    .WithSearchClientConfig(new SearchClientConfig { MaxMatchesCount = 1, AnswerTokens = 100 })
                    .With(new TextPartitioningOptions { MaxTokensPerParagraph = 300, MaxTokensPerLine = 100, OverlappingTokens = 30 })
                    .WithSimpleFileStorage(new SimpleFileStorageConfig { StorageType = FileSystemTypes.Disk })
                    .WithSimpleVectorDb(new SimpleVectorDbConfig { StorageType = FileSystemTypes.Disk })
                    .Build<MemoryServerless>();

                Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model loaded Successfully", Type = NotificationType.Information }, areaName: "NotificationArea");

            }
            catch (Exception ex)
            {
                Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Model Switching to {SelectedGenerationModel.Name} Failed, Exception: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea");
            }
        }).Start();
    }

    [RelayCommand]
    public void GenerationUnloadModel()
    {
        GenerationModel?.Dispose();
        GenerationState = GenerationState.None;
        SelectedGenerationModel = null;
        Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model Unloaded", Type = NotificationType.Information }, areaName: "NotificationArea");
    }
    #endregion

    #region ChatMethods

    [RelayCommand]
    private void ChatCreate()
    {
        if (SelectedGenerationModel == null)
        {
            Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
            return;
        }

        // Initialize a llama chat session
        var context = GenerationModel.CreateContext(GenerationModelParameters);
        var Executor = new InteractiveExecutor(context);
        
        // TODO: Support Function Calling whenever Semantic Kernel support it for LLamaSharp
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
        Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Chat {SelectedChatSession.SessionName} Deleted", Type = NotificationType.None }, areaName: "NotificationArea");

        foreach (ChatLog log in SelectedChatSession.ChatMessages.Values)
        {
            log.Messages.Clear();
        }
        SelectedChatSession.ChatMessages.Clear();
        ChatSessions.Remove(SelectedChatSession);
        SelectedChatSession = null;
    }

    [RelayCommand]
    private async void MessageSend()
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
                Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
                return;
            }
            if (SelectedChatSession == null)
                ChatCreate();

            if (SelectedChatSession?.Executor == null)
                SelectedChatSession.Executor = new InteractiveExecutor(GenerationModel.CreateContext(GenerationModelParameters));

            GenerationState = GenerationState.Started;

            var userMessage = SelectedChatSession?.AddMessage("User", ChatUserInput);
            var aiMessage = SelectedChatSession?.AddMessage("Assistant", string.Empty);

            var Prompt = SelectedGenerationModel.TextConfig.Prompt(ChatUserInput);
            ChatUserInput = string.Empty;

            GenerationCancelToken = new();
            var MessageParams = new InferenceParams()
            {
                MaxTokens = Parameters.MaxTokens,
                Temperature = Parameters.Randomness,
                TopP = Parameters.RandomnessBooster,
                TopK = (int)(Parameters.OptimalProbability * 100),
                FrequencyPenalty = Parameters.FrequencyPenalty,
                AntiPrompts = new List<string> { "User:" }
            };

            IAsyncEnumerable<string>? tokens = SelectedChatSession?.Executor.InferAsync(Prompt, MessageParams, GenerationCancelToken.Token);
            await Parallel.ForEachAsync(tokens, (token, cancelToken) =>
            {
                var filteredToken = token;
                foreach (string blackToken in SelectedGenerationModel.TextConfig.BlackList)
                {
                    if (token.Contains(blackToken))
                    {
                        int index = token.IndexOf(blackToken);
                        filteredToken = filteredToken.Remove(index, blackToken.Length);
                        break;
                    }
                }

                aiMessage.Message += filteredToken;
                return new ValueTask();
            });

            ChatContainerView.ScrollToBottom();
            GenerationState = GenerationState.Finished;
            GenerationCancelToken.Dispose();
        }
    }

    [RelayCommand]
    private static void MessageCopy(ChatMessage message)
    {
        Clipboard.SetText(message.Message);
        Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Message Copied to Clipboard", Type = NotificationType.None }, areaName: "NotificationArea");
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

        if (GenerationState == GenerationState.Started || CompletionTextEditor == null)
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
            Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
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
        Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Session {SelectedCompletionSession.SessionName} Deleted", Type = NotificationType.None }, areaName: "NotificationArea");

        SelectedCompletionSession.Executor?.Context.Dispose();
        CompletionSessions.Remove(SelectedCompletionSession);
        SelectedCompletionSession = null;
    }

    [RelayCommand]
    private async void CompletionGenerate()
    {
        if (SelectedGenerationModel == null)
        {
            Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
            return;
        }
        if (GenerationState == GenerationState.Started)
        {
            GenerationCancelToken.Cancel();
            return;
        }
        if (SelectedCompletionSession == null)
            CompletionCreate();

        if (SelectedCompletionSession?.Executor == null)
            SelectedCompletionSession.Executor = new StatelessExecutor(GenerationModel, GenerationModelParameters);

        GenerationState = GenerationState.Started;
        var Prompt = SelectedGenerationModel.TextConfig.Prompt(CompletionText, CompletionPrompt);

        GenerationCancelToken = new();
        var MessageParams = new InferenceParams()
        {
            MaxTokens = Parameters.MaxTokens,
            Temperature = Parameters.Randomness,
            TopP = Parameters.RandomnessBooster,
            TopK = (int)(Parameters.OptimalProbability * 100),
            FrequencyPenalty = Parameters.FrequencyPenalty,
            AntiPrompts = new List<string> { "User:" }
        };

        IAsyncEnumerable<string>? tokens = SelectedCompletionSession?.Executor.InferAsync(Prompt, MessageParams, GenerationCancelToken.Token);
        await Parallel.ForEachAsync(tokens, (token, cancelToken) =>
        {
            var filteredToken = token;
            foreach (string blackToken in SelectedGenerationModel.TextConfig.BlackList)
            {
                if (token.Contains(blackToken))
                {
                    int index = token.IndexOf(blackToken);
                    filteredToken = filteredToken.Remove(index, blackToken.Length);
                    break;
                }
            }

            CompletionText += filteredToken;
            return new ValueTask();
        });

        GenerationState = GenerationState.Finished;
        GenerationCancelToken.Dispose();
    }

    #endregion

    #region MemoryMethods

    [RelayCommand]
    private async Task MemorySourceAdd()
    {
        var docCollections = new ComboBox
        {
            IsEditable = true,
            ItemsSource = MemoriesPaths,
            DisplayMemberPath = "Collection",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var docPathText = new Wpf.Ui.Controls.TextBox
        {
            Text = string.Empty,
            PlaceholderText = "Document path...",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var docPathDialog = new System.Windows.Controls.Button
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new SymbolIcon { Symbol = Wpf.Ui.Common.SymbolRegular.DocumentAdd24 },
                    new System.Windows.Controls.TextBlock { Text = "Browse" }
                }
            }
        };

        docPathDialog.Click += (sender, e) =>
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Documents|*.pdf;*.html;*.htm;*.md;*.txt;*.json|" +
                         "MS Office Files|*.docx;*.doc;*.xlsx;*.xls;*.pptx;*.ppt|" +
                         "Images|*.jpg;*.jpeg;*.png;*.tiff|" +
                         "All Supported Files|*.docx;*.doc;*.xlsx;*.xls;*.pptx;*.ppt;*.pdf;*.html;*.htm;*.md;*.txt;*.json;*.jpg;*.jpeg;*.png;*.tiff"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                docPathText.Text = openFileDialog.FileName;
        };

        var importBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "Memory",
            Content = new StackPanel { Width = 400, Children = { new System.Windows.Controls.TextBlock { Text = "Import Document", FontWeight = FontWeights.Bold, FontSize = 32, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 50) } , docCollections, docPathText, docPathDialog } },
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        var result = await importBox.ShowDialogAsync();
        var docName = Path.GetFileName(docPathText.Text);

        if (result != Wpf.Ui.Controls.MessageBoxResult.Primary || string.IsNullOrEmpty(docCollections.Text) || string.IsNullOrEmpty(docPathText.Text))
            return;

        var existingItem = MemoriesPaths.FirstOrDefault(item => item.Collection == docCollections.Text);
        if (existingItem != null)
            existingItem.Documents.Add(docName);
        else
        {
            existingItem = new MemorySource { Collection = docCollections.Text };
            existingItem.Documents.Add(docName);
            MemoriesPaths.Add(existingItem);
        }

        try
        {
            var memoryProgress = new Snackbar(Extensions.SnackbarArea)
            {
                Title = "Importing Memory",
                Icon = new SymbolIcon { Symbol = Wpf.Ui.Common.SymbolRegular.DocumentAdd24 },
                Timeout = TimeSpan.FromDays(1),
                Content = new StackPanel
                {
                    Children =
                    {
                        new System.Windows.Controls.TextBlock { Text = $"Document {docName} is importing now" },
                        new ProgressBar { Margin = new Thickness(5,15,5,5), IsIndeterminate = true }
                    }
                }
            };
            Extensions.SnackbarArea.AddToQue(memoryProgress);
            existingItem.IsLoaded = false;
            await GenerationMemory.ImportDocumentAsync(new Document(docCollections.Text).AddFile(docPathText.Text), steps: Constants.PipelineWithoutSummary);
            existingItem.IsLoaded = true;
            await Extensions.SnackbarArea.HideCurrent();
        }
        catch (Exception ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Error", Message = $"Error importing document: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea", TimeSpan.FromSeconds(60));
            return;
        }

        Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Memory {docName} has Imported", Type = NotificationType.None }, areaName: "NotificationArea", TimeSpan.FromSeconds(60));
    }

    [RelayCommand]
    private async Task MemorySourceDelete()
    {
        var docCollections = new ComboBox
        {
            ItemsSource = MemoriesPaths,
            DisplayMemberPath = "Collection",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 20)
        };


        var importBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "Memory",
            Content = new StackPanel { Width = 400, Children = { new System.Windows.Controls.TextBlock { Text = "Delete Document", FontWeight = FontWeights.Bold, FontSize = 32, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 50) } , docCollections } },
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        var result = await importBox.ShowDialogAsync();

        var memoriesCollection = docCollections.Text;
        if (result != Wpf.Ui.Controls.MessageBoxResult.Primary || string.IsNullOrEmpty(memoriesCollection))
            return;


        var existingItem = MemoriesPaths.FirstOrDefault(item => item.Collection == memoriesCollection);
        if (existingItem != null)
            MemoriesPaths.Remove(existingItem);

        try
        {
            var memoryProgress = new Snackbar(Extensions.SnackbarArea)
            {
                Title = "Deleting Memory",
                Icon = new SymbolIcon { Symbol = Wpf.Ui.Common.SymbolRegular.DocumentAdd24 },
                IsShown = true,
                Content = new StackPanel
                {
                    Children =
                    {
                        new System.Windows.Controls.TextBlock { Text = $"Collection {memoriesCollection} is begin deleted now" },
                        new ProgressBar { IsIndeterminate = true }
                    }
                }
            };
            Extensions.SnackbarArea.AddToQue(memoryProgress);

            await GenerationMemory.DeleteDocumentAsync(docCollections.Text);

            await Extensions.SnackbarArea.HideCurrent();
        }
        catch (Exception ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Error", Message = $"Error deleting document: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea", TimeSpan.FromSeconds(60));
            return;
        }

        Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Memories Collection {memoriesCollection} has Deleted", Type = NotificationType.None }, areaName: "NotificationArea", TimeSpan.FromSeconds(60));
    }

    [RelayCommand]
    private async Task MemoryAsk()
    {
        try
        {
            if (GenerationState == GenerationState.Started)
            {
                GenerationCancelToken.Cancel();
                return;
            }
            if (SelectedGenerationModel == null)
                Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Text Generation Model isn't loaded yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");
            if (SelectedMemory == null)
                Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Memory isn't selected yet.", Type = NotificationType.Warning }, areaName: "NotificationArea");

            if (string.IsNullOrEmpty(MemoryUserInput) || SelectedMemory == null || !SelectedMemory.IsLoaded || SelectedGenerationModel == null)
                return;

            GenerationState = GenerationState.Started;
            var userMessage = new ChatMessage { Sender = "User", Message = MemoryUserInput };
            var memoryMessage = new ChatMessage { Sender = "EGOIST", Message = "Gathering information from memories." };
            SelectedMemory.Messages.Add(userMessage);
            SelectedMemory.Messages.Add(memoryMessage);

            GenerationCancelToken = new();
            MemoryUserInput = string.Empty;
            MemoryContainerView?.ScrollToBottom();

            var answer = await GenerationMemory.AskAsync(userMessage.Message, filter: new MemoryFilter().ByDocument(SelectedMemory.Collection), cancellationToken: GenerationCancelToken.Token);
            memoryMessage.Message = answer.Result;
            GenerationState = GenerationState.Finished;
            GenerationCancelToken.Dispose();
        }
        catch (Exception ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Text Generation", Message = $"Asking Memory Failed, Exception: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea", TimeSpan.FromSeconds(60));
        }
    }

    #endregion
}

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
public partial class ChatSession : ObservableObject, INotifyPropertyChanged
{
    public string SessionName { get; set; }

    [ObservableProperty]
    private Dictionary<int, ChatLog> _chatMessages = new();
    public ObservableCollection<ChatMessage> Messages => ChatMessages[CurrentLog].Messages;


    #region Unused due LLama tokenizer/embeddings limitions
    [ObservableProperty]
    public int _currentLog = 0;
    public string CurrentLogSTR => string.Format("{0} / {1}", CurrentLog, Edits);
    public int Edits => ChatMessages.Count - 1;
    public bool Edited => Edits > 0;
    #endregion

    #region LLamaSharp
    [NonSerialized]
    public StatefulExecutorBase? Executor;
    #endregion


    public ChatSession()
    {
        SessionName = $"Chat {DateTime.Now}";
        ChatMessages.Add(0, new ChatLog());
    }

    public ChatMessage AddMessage(string user, string message)
    {
        var messageInput = new ChatMessage { Sender = user, Message = message };
        Messages.Add(messageInput);

        return messageInput;
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
    private int _ID = 0;
    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();
}

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _sender = string.Empty;
    [ObservableProperty]
    private string _message = string.Empty;
    [ObservableProperty]
    private bool _isEditable;
}

public partial class MemorySource : ObservableObject
{
    [ObservableProperty]
    private string _collection = string.Empty;
    [ObservableProperty]
    private ObservableCollection<string> _documents = new();
    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();
    [ObservableProperty]
    private bool _isLoaded;
}