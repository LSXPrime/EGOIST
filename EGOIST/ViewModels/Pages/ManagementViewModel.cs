using System.IO;
using Wpf.Ui.Controls;
using Newtonsoft.Json;
using Notification.Wpf;
using System.Collections.ObjectModel;
using NetFabric.Hyperlinq;
using System.Net.Http;
using System.Net;
using EGOIST.Helpers;
using EGOIST.Data;
using EGOIST.Enums;

namespace EGOIST.ViewModels.Pages;

public partial class ManagementViewModel : ObservableObject, INavigationAware
{
    #region SharedVariables
    private bool _isInitialized = false;
    [ObservableProperty]
    public static ObservableCollection<ModelInfo> _modelsInfo = new();
    private readonly NotificationManager notification = new();
    #endregion

    #region OverviewVariables
    [ObservableProperty]
    private Dictionary<string, ObservableCollection<string>> _filters = new()
    {
        { "Text", new ObservableCollection<string> { "Chat", "Completion", "Coding" } },
        { "Image", new ObservableCollection<string> { "Generate", "Inpaint", "Manipulate" } },
        { "Voice", new ObservableCollection<string> { "Clone", "Generate", "Transcribe" } },
        { "Video", new ObservableCollection<string> { "Generate", "Inpaint", "Remix" } }
    };

    private string _selectedType = string.Empty;
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            _selectedType = value;
            FiltersApply(true);
            OnPropertyChanged(nameof(SelectedType));
        }
    }

    private string _selectedTask = string.Empty;
    public string SelectedTask
    {
        get => _selectedTask;
        set
        {
            _selectedTask = value;
            FiltersApply(true);
            OnPropertyChanged(nameof(SelectedTask));
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            FiltersApply(true);
            OnPropertyChanged(nameof(SearchText));
        }
    }

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _downloadModelsInfo = new();
    #endregion

    #region DownloadsVariables
    [ObservableProperty]
    private ObservableCollection<DownloadInfo> _downloadsInfo = new();

    private Task? downloadHandlerTask;
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
    }

    public void LoadData()
    {
        string modelsData = File.ReadAllText("Models_Data.json");
        ModelsInfo = JsonConvert.DeserializeObject<ObservableCollection<ModelInfo>>(modelsData);
        DownloadModelsInfo.Clear();
        DownloadModelsInfo.AddRange(ModelsInfo);

        Task.Run(LoadModels);
    }

    private async Task LoadModels()
    {
        //  string apiUrl = $"https://api.github.com/repos/LSXPrime/EGOIST-Models-Catalog/contents/Configs";
        string apiUrl = "http://120.0.0.1";

        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(3) };
        client.DefaultRequestHeaders.Add("User-Agent", "EGOIST");

        try
        {

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                Extensions.Notify(new NotificationContent { Title = "Management", Message = $"Models , A7a", Type = NotificationType.Error }, areaName: "NotificationArea");

                string jsonResponse = await response.Content.ReadAsStringAsync();

                ModelsInfo = new ObservableCollection<ModelInfo>();

                // Can be easily achived in one line by using dynamic json, but as Game Developer, I hate LINQ
                int startIndex = jsonResponse.IndexOf("{\"name\":");
                while (startIndex != -1)
                {
                    int endIndex = jsonResponse.IndexOf("{\"name\":", startIndex + 1);
                    if (endIndex == -1)
                        endIndex = jsonResponse.LastIndexOf('}');

                    string fileData = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);

                    // Check if the file is of type "file" and ends with ".json"
                    if (fileData.Contains("\"type\":\"file\"") && fileData.Contains("\"name\":\"") && fileData.Contains(".json\""))
                    {
                        // Extract the name and download_url
                        int nameStart = fileData.IndexOf("\"name\":\"") + 8;
                        int nameEnd = fileData.IndexOf(".json\"", nameStart) + 5;
                        string fileName = fileData[nameStart..nameEnd];

                        int downloadUrlStart = fileData.IndexOf("\"download_url\":\"") + 16;
                        int downloadUrlEnd = fileData.IndexOf("\",\"", downloadUrlStart);
                        string fileUrl = fileData[downloadUrlStart..downloadUrlEnd];

                        HttpResponseMessage fileResponse = await client.GetAsync(fileUrl);

                        if (fileResponse.IsSuccessStatusCode)
                        {
                            string fileContent = await fileResponse.Content.ReadAsStringAsync();
                            ModelInfo modelInfo = ModelInfo.FromJson(fileContent);
                            ModelsInfo.Add(modelInfo);
                        }
                    }

                    startIndex = jsonResponse.IndexOf("{\"name\":", endIndex + 1);
                }

                // Keep offline copy
                var json = JsonConvert.SerializeObject(ModelsInfo);
                File.WriteAllText("Models_Data.json", json);
                DownloadModelsInfo.Clear();
                DownloadModelsInfo.AddRange(ModelsInfo);
            }
        }
        catch (TaskCanceledException ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Management", Message = $"Models Fetching Timeout, Check Internet Connection", Type = NotificationType.Error }, areaName: "NotificationArea");
        }
        catch (HttpRequestException ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Management", Message = $"Models Fetching Failed, Check Internet Connection", Type = NotificationType.Error }, areaName: "NotificationArea");
        }
    }
    #endregion

    #region DownloadsMethods

    [RelayCommand]
    private void FiltersApply(bool? apply)
    {
        if (apply == true)
        {

            // P.S Why not Linq, performance baby, on every single edit
            /*
            // P.S Linq for clean code, but I might remove it based on Performance
            DownloadModelsInfo = new ObservableCollection<ModelInfo>(ModelsInfo.Where(item =>
            item.Type.ToLower().Contains(SelectedType.ToLower()) ||
            item.Task.ToLower().Contains(SelectedTask.ToLower()) ||
            item.Name.ToLower().Contains(SearchText.ToLower())));
            */
            

            DownloadModelsInfo.Clear();

            // Apply Type filter if selected
            if (!string.IsNullOrEmpty(SelectedType) && Filters.ContainsKey(SelectedType))
            {
                foreach (var item in ModelsInfo)
                {
                    if (item.Type.ToLower().Contains(SelectedType.ToLower()))
                    {
                        DownloadModelsInfo.Add(item);
                    }
                }
            }
            else
            {
                DownloadModelsInfo.AddRange(ModelsInfo);
            }

            // Apply Task filter if selected
            if (!string.IsNullOrEmpty(SelectedTask))
            {
                for (int i = DownloadModelsInfo.Count - 1; i >= 0; i--)
                {
                    if (!DownloadModelsInfo[i].Task.ToLower().Contains(SelectedTask.ToLower()))
                    {
                        DownloadModelsInfo.RemoveAt(i);
                    }
                }
            }

            // Apply SearchText filter
            if (!string.IsNullOrEmpty(SearchText))
            {
                for (int i = DownloadModelsInfo.Count - 1; i >= 0; i--)
                {
                    if (!DownloadModelsInfo[i].Name.ToLower().Contains(SearchText.ToLower()))
                    {
                        DownloadModelsInfo.RemoveAt(i);
                    }
                }
            }
        }
        else
        {
            DownloadModelsInfo.Clear();
            DownloadModelsInfo.AddRange(ModelsInfo);
            SelectedType = SelectedTask = SearchText = string.Empty;
        }

        OnPropertyChanged(nameof(DownloadModelsInfo));
    }

    [RelayCommand]
    public void DownloadModel(ModelInfo.ModelInfoWeight weight)
    {
        var existingDownload = DownloadsInfo.FirstOrDefault(item => item.Link.Equals(weight.Link));
        if (existingDownload == null)
        {
            var modelInfo = ModelsInfo.FirstOrDefault(item => item.Weights.Contains(weight));
            var downloadInfo = new DownloadInfo
            {
                Link = weight.Link,
                Name = $"{modelInfo.Name} - {weight.Weight}",
                LocalPath = $"{AppConfig.Instance.ModelsPath}/{modelInfo.Type.RemoveSpaces()}/{modelInfo.Name.RemoveSpaces()}/{weight.Weight.RemoveSpaces()}.{weight.Extension.ToLower().RemoveSpaces()}",
                TotalBytes = 0,
                DownloadedBytes = 0,
                Status = DownloadStatus.Pending,
                Model = modelInfo,
                CancellationTokenSource = new()
            };
            Extensions.Notify(new NotificationContent { Title = "Management", Message = $"Model {downloadInfo.Name} Started Downloading", Type = NotificationType.Notification }, areaName: "NotificationArea");

            DownloadsInfo.Add(downloadInfo);
            if (downloadHandlerTask == null || downloadHandlerTask.IsCompleted)
                downloadHandlerTask = HandleDownloads();
        }
        else
            Extensions.Notify(new NotificationContent { Title = "Management", Message = $"Model Weight {weight.Weight} is Already Downloading", Type = NotificationType.Warning }, areaName: "NotificationArea");
    }

    public async Task HandleDownloads()
    {
        var downloadTasks = new List<Task>();

        while (DownloadsInfo.Any(item => item.Status == DownloadStatus.Pending))
        {
            var pendingDownloads = DownloadsInfo.Where(item => item.Status == DownloadStatus.Pending).ToList();

            foreach (var pendingDownload in pendingDownloads)
            {
                pendingDownload.TotalBytes = pendingDownload.TotalBytes > 0 ? pendingDownload.TotalBytes : await Task.Run(async () =>
                {
                    var request = WebRequest.Create(pendingDownload.Link);
                    request.Method = "HEAD";

                    using var response = await request.GetResponseAsync();
                    if (response is HttpWebResponse webResponse && webResponse.StatusCode == HttpStatusCode.OK)
                        return webResponse.ContentLength;

                    return 0;
                });

                bool resumeDownload = false;
                

                if (File.Exists(pendingDownload.LocalPath))
                {
                    pendingDownload.DownloadedBytes = new FileInfo(pendingDownload.LocalPath).Length;
                    if (pendingDownload.DownloadedBytes < pendingDownload.TotalBytes)
                        resumeDownload = true;
                    else
                    {
                        pendingDownload.Status = DownloadStatus.Completed;
                        continue;
                    }
                }
                else
                {
                    string directory = Path.GetDirectoryName(pendingDownload.LocalPath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    var json = JsonConvert.SerializeObject(pendingDownload.Model);
                    File.WriteAllText($"{directory}\\egoist_config.json", json);
                }

                pendingDownload.Status = DownloadStatus.Downloading;
                downloadTasks.Add(Task.Run(async () =>
                {
                    using var client = new WebClient();
                    client.DownloadProgressChanged += (sender, args) => pendingDownload.DownloadedBytes = args.BytesReceived;
                    client.DownloadFileCompleted += (sender, args) => // I don't know why not working yet
                    {
                        pendingDownload.Status = DownloadStatus.Completed;
                        DownloadsInfo.Remove(pendingDownload);
                    };

                    try
                    {
                        using var fileStream = new FileStream(pendingDownload.LocalPath, resumeDownload ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Write);

                        if (resumeDownload)
                            client.Headers.Add("Range", $"bytes={pendingDownload.DownloadedBytes}-");

                        using var data = await client.OpenReadTaskAsync(pendingDownload.Link);
                        var buffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = await data.ReadAsync(buffer, 0, buffer.Length, pendingDownload.CancellationTokenSource.Token)) > 0)
                        {
                            pendingDownload.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                            await fileStream.WriteAsync(buffer, 0, bytesRead, pendingDownload.CancellationTokenSource.Token);
                            pendingDownload.DownloadedBytes += bytesRead;
                        }

                        // A workarount for client.DownloadFileCompleted
                        if (pendingDownload.DownloadedBytes == pendingDownload.TotalBytes)
                        {
                            pendingDownload.Status = DownloadStatus.Completed;
                            DownloadsInfo.Remove(pendingDownload);
                            Extensions.Notify(new NotificationContent { Title = "Management", Message = $"Model {pendingDownload.Name} Finished Downloading", Type = NotificationType.Notification }, areaName: "NotificationArea");
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        // Handle cancellation
                        pendingDownload.Status = DownloadStatus.Paused;

                    }
                    catch (Exception ex)
                    {
                    }
                }, pendingDownload.CancellationTokenSource.Token));
            }

            await Task.WhenAny(Task.WhenAll(downloadTasks), Task.Delay(1000)); // Adjust the delay duration as needed
            downloadTasks.RemoveAll(task => task.IsCompleted || task.IsFaulted || task.IsCanceled);
        }
    }

    [RelayCommand]
    public void ResumePauseDownload(DownloadInfo downloadInfo)
    {
        if (downloadInfo.Status == DownloadStatus.Paused)
        {
            downloadInfo.CancellationTokenSource = new CancellationTokenSource();
            downloadInfo.Status = DownloadStatus.Pending;
        }
        else
        {
            downloadInfo.CancellationTokenSource.Cancel();
            downloadInfo.Status = DownloadStatus.Paused;
        }
    }

    [RelayCommand]
    public void CancelDownload(DownloadInfo downloadInfo)
    {
        downloadInfo.CancellationTokenSource.Cancel();
        downloadInfo.Status = DownloadStatus.Canceled;

        if (File.Exists(downloadInfo.LocalPath))
            File.Delete(downloadInfo.LocalPath);

        DownloadsInfo.Remove(downloadInfo);
        downloadInfo = null;
    }

    #endregion
}
