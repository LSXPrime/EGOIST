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
using EGOIST.Views.Pages;
using System.Text.Json;
using System.Text;
using MetadataExtractor;
using Directory = System.IO.Directory;
using MetadataExtractor.Formats.Exif;

namespace EGOIST.ViewModels.Pages;

public partial class ManagementViewModel : ObservableObject, INavigationAware
{
    #region SharedVariables
    [ObservableProperty]
    public static ObservableCollection<ModelInfo> _modelsInfo = new();
    [ObservableProperty]
    private static Dictionary<string, Visibility> _visibilityDict = new()
    {
        { "DownloadingCharacter", Visibility.Visible },
    };
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

    #region RPCharactersVariables
    [ObservableProperty]
    private RoleplayCharacter _roleplayCharacterCreator = new();
    [ObservableProperty]
    private RoleplayCharacter _roleplayCharacterPreview = new();

    [ObservableProperty]
    public static ObservableCollection<RoleplayCharacter> _roleplayCharacters = new();

    /*
    private string _roleplaySearchText = string.Empty;
    public string RoleplaySearchText
    {
        get => _roleplaySearchText;
        set
        {
            _roleplaySearchText = value;
            RoleplayCharactersFilter($"SCHAR:{value}");
            OnPropertyChanged(nameof(RoleplaySearchText));
        }
    }

    [ObservableProperty]
    private ObservableCollection<RoleplayCharacter> _roleplayCharactersFiltered = new();

    [ObservableProperty]
    private ObservableCollection<string> _roleplayCharactersTags = new();
    [ObservableProperty]
    private ObservableCollection<string> _roleplayCharactersTagsFiltered = new();
    */
    #endregion

    #region InitlizingMethods
    public void OnStartup()
    {
        Extensions.onLoadData += LoadData;
        AppConfig.Instance.ConfigSavedEvent += LoadData;
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }

    public void LoadData()
    {
        if (File.Exists("EGOIST_ModelsData.json"))
        {
            var modelsData = File.ReadAllText("EGOIST_ModelsData.json");
            ModelsInfo = JsonConvert.DeserializeObject<ObservableCollection<ModelInfo>>(modelsData);
            DownloadModelsInfo.Clear();
            DownloadModelsInfo.AddRange(ModelsInfo);
        }

        RoleplayCharacters.Clear();
        var charactersPaths = Directory.GetDirectories(AppConfig.Instance.CharactersPath);
        foreach (var charPath in charactersPaths)
        {
            var charImported = File.ReadAllText($"{charPath}\\{Path.GetFileName(charPath)}.json");
            var charData = RoleplayCharacter.FromJson(charImported);
            RoleplayCharacters.Add(charData);
        }
        /*
        RoleplayCharactersFiltered.AddRange(RoleplayCharacters);

        RoleplayCharactersTags.Clear();
        foreach (var charInfo in RoleplayCharacters)
        {
            var tag = charInfo.Tags.AsValueEnumerable().Where(x => !RoleplayCharactersTags.Contains(x));
            RoleplayCharactersTags.AddRange(tag);
        }
        */

        Task.Run(LoadModels);
    }

    private async Task LoadModels()
    {
        string apiUrl = $"https://api.github.com/repos/LSXPrime/EGOIST-Models-Catalog/contents/Models";

        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(3) };
        client.DefaultRequestHeaders.Add("User-Agent", "EGOIST");

        try
        {

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ModelsInfo = new ObservableCollection<ModelInfo>();

                using HttpClient client2 = new() { Timeout = TimeSpan.FromSeconds(3) };
                var files = JsonConvert.DeserializeObject<List<GithubFileInfo>>(jsonResponse);
                foreach (var file in files)
                {
                    var fileResponse = await client.GetAsync(file.download_url);
                    if (fileResponse.IsSuccessStatusCode)
                    {
                        string fileContent = await fileResponse.Content.ReadAsStringAsync();
                        ModelInfo modelInfo = ModelInfo.FromJson(fileContent);
                        ModelsInfo.Add(modelInfo);
                    }
                }

                // Keep offline copy
                var json = JsonConvert.SerializeObject(ModelsInfo);
                File.WriteAllText("EGOIST_ModelsData.json", json);
                DownloadModelsInfo.Clear();
                DownloadModelsInfo.AddRange(ModelsInfo);
                Extensions.Notify(new NotificationContent { Title = "Management", Message = $"Models Fetching Successed", Type = NotificationType.None }, areaName: "NotificationArea");
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

    #region RPCharactersMethods

    /*
    [RelayCommand]
    private void RoleplayCharactersFilter(string tag)
    {
        if (!string.IsNullOrEmpty(tag))
        {
            if (tag.StartsWith("SCHAR:"))
            {
                tag = tag[6..];
                RoleplayCharactersFiltered = new ObservableCollection<RoleplayCharacter>(RoleplayCharacters.Where(x => x.Name.Contains(tag) && x.Tags.Intersect(RoleplayCharactersTagsFiltered).Any()));
            }
            else
            {
                if (RoleplayCharactersTagsFiltered.Contains(tag))
                    RoleplayCharactersTagsFiltered.Remove(tag);
                else
                    RoleplayCharactersTagsFiltered.Add(tag);

                RoleplayCharactersFiltered = new ObservableCollection<RoleplayCharacter>(RoleplayCharacters.Where(x=> x.Tags.Intersect(RoleplayCharactersTagsFiltered).Any()));
            }
        }
        else
        {
            RoleplayCharactersFiltered.Clear();
            RoleplayCharactersFiltered.AddRange(RoleplayCharacters);
        }

        OnPropertyChanged(nameof(RoleplayCharactersFiltered));
    }
    */

    [RelayCommand]
    public async Task RPCharactersCreate()
    {
        var charPath = $"{AppConfig.Instance.CharactersPath}\\{RoleplayCharacterCreator.Name}";
        Directory.CreateDirectory(charPath);
        if (File.Exists(RoleplayCharacterCreator.Avatar))
        {
            File.Copy(RoleplayCharacterCreator.Avatar, $"{charPath}\\{RoleplayCharacterCreator.Name}{Path.GetExtension(RoleplayCharacterCreator.Avatar)}", true);
            RoleplayCharacterCreator.Avatar = $"{RoleplayCharacterCreator.Name}{Path.GetExtension(RoleplayCharacterCreator.Avatar)}";
        }
        else
            RoleplayCharacterCreator.Avatar = $"{RoleplayCharacterCreator.Name}.webp";
        var createdCharacter = RoleplayCharacterCreator.ToJson();
        await File.WriteAllTextAsync($"{charPath}\\{RoleplayCharacterCreator.Name}.json", createdCharacter);
        Extensions.Notify("Management", $"Character {RoleplayCharacterCreator.Name} Created Successfully");
        RPCharactersReset();
        LoadData();
    }

    [RelayCommand]
    public void RPCharactersReset() => RoleplayCharacterCreator = RoleplayCharacterPreview = new();

    [RelayCommand]
    public async Task RPCharactersImport()
    {
        var openFileDialog = new System.Windows.Forms.OpenFileDialog { Filter = "Text|*.json;*.txt;*.log|" + "Images|*.png;*.webp|" + "All Supported Files|*.json;*.txt;*.log;*.png;*.webp" };
        if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            return;
        
        var charPath = openFileDialog.FileName;
        var charFormat = Path.GetExtension(charPath);
        var charParsed = string.Empty;
        if (new[] { ".json", ".txt", ".log" }.Contains(Path.GetExtension(charPath)))
        {
            charParsed = await File.ReadAllTextAsync(charPath);
            var charData = RoleplayCharacter.FromJson(charParsed);
            if (charData != null)
                RoleplayCharacterCreator = charData;
        }
        else if (new[] { ".webp", ".png" }.Contains(Path.GetExtension(charPath)))
        {
            switch (charFormat)
            {
                case ".webp":
                    try
                    {
                        var directories = ImageMetadataReader.ReadMetadata(charPath);

                        var exifDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                        var description = exifDirectory?.GetDescription(ExifDirectoryBase.TagUserComment);

                        if (description == "Undefined" && exifDirectory != null && exifDirectory.TryGetInt32(ExifDirectoryBase.TagUserComment, out int value))
                        {
                            charParsed = value.ToString();
                            break;
                        }

                        try
                        {
                            JsonDocument.Parse(description);
                            charParsed = description;
                            break;
                        }
                        catch
                        {
                            var byteArr = Array.ConvertAll(description.Split(','), byte.Parse);
                            charParsed = Encoding.UTF8.GetString(byteArr);
                        }
                    }
                    catch (Exception ex)
                    {
                        Extensions.Notify("Management",  $"Character Importing Failed, Exception: {ex.Message}", NotificationType.Error);
                    }
                    break;
                case ".png":
                    try
                    {
                        var buffer = File.ReadAllBytes(charPath);
                        int index = 8;
                        while (index < buffer.Length)
                        {
                            int chunkLength = BitConverter.ToInt32(buffer, index);
                            index += 8 + chunkLength;
                            if (Encoding.ASCII.GetString(buffer, index - 8, 4) == "tEXt")
                                charParsed = Encoding.UTF8.GetString(buffer, index - chunkLength, chunkLength);
                        }
                    }
                    catch (Exception ex)
                    {
                        Extensions.Notify("Management", $"Character Importing Failed, Exception: {ex.Message}", NotificationType.Error);
                    }
                    break;
            }

            var charData = JsonConvert.DeserializeObject<RoleplayCharacter>(charParsed, new RoleplayCharacterTavenAIConverter());
            charData.Avatar = charPath;
            if (charData != null)
                RoleplayCharacterCreator = charData;
        }
    }

    [RelayCommand]
    public void RPCharactersPreviewEdit()
    {
        RoleplayCharacterCreator = RoleplayCharacterPreview;
        App.GetService<ManagementPage>().Tabs.SelectedIndex = 3;
    }

    [RelayCommand]
    public void RPCharactersPreviewDelete()
    {
        var charPath = $"{AppConfig.Instance.CharactersPath}\\{RoleplayCharacterPreview.Name}";
        if (!string.IsNullOrEmpty(RoleplayCharacterPreview.Name) && Directory.Exists(charPath))
            Directory.Delete(charPath, true);

        RoleplayCharacterPreview = new();
    }

    #endregion
}

public class GithubFileInfo
{
    public string name { get; set; }
    public string path { get; set; }
    public string sha { get; set; }
    public int size { get; set; }
    public string url { get; set; }
    public string html_url { get; set; }
    public string git_url { get; set; }
    public string download_url { get; set; }
    public string type { get; set; }
}
