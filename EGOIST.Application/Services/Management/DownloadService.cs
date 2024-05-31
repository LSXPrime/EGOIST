using System.Collections.ObjectModel;
using System.Text.Json;
using EGOIST.Application.Services.Utilities;
using EGOIST.Application.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Management;

public class DownloadService
{
    public ObservableCollection<DownloadInfo> CurrentDownloads { get; set; }

    private readonly IFileSystemService _fileSystemService;
    private readonly INetworkService _networkService;
    private readonly ILogger _logger;

    public DownloadService(IFileSystemService fileSystemService, INetworkService networkService, ILogger logger)
    {
        _fileSystemService = fileSystemService;
        _networkService = networkService;
        _logger = logger;
    }

    public async Task DownloadModel(ModelInfoWeight weight, ModelInfo modelInfo, Action<DownloadInfo> onProgressChanged)
    {
        var downloadInfo = new DownloadInfo
        {
            Link = weight.Link,
            Name = $"{modelInfo.Name} - {weight.Weight}",
            LocalPath = Path.Combine(AppConfig.Instance.ModelsPath, modelInfo.Type.RemoveSpaces(),
                                     modelInfo.Name.RemoveSpaces(), $"{weight.Weight.RemoveSpaces()}.{weight.Extension.ToLower().RemoveSpaces()}"),
            TotalBytes = 0,
            DownloadedBytes = 0,
            Status = DownloadStatus.Pending,
            Model = modelInfo,
            CancellationTokenSource = new CancellationTokenSource()
        };

        _logger.LogInformation($"Model {downloadInfo.Name} started downloading.");

        CurrentDownloads.Add(downloadInfo);
        _ = Task.Run(() => HandleDownload(downloadInfo));
    }

    private async Task HandleDownload(DownloadInfo downloadInfo)
    {
        try
        {
            downloadInfo.TotalBytes = await _networkService.GetFileSize(downloadInfo.Link);

            bool resumeDownload = false;

            if (_fileSystemService.FileExists(downloadInfo.LocalPath))
            {
                downloadInfo.DownloadedBytes = _fileSystemService.GetFileSize(downloadInfo.LocalPath);
                if (downloadInfo.DownloadedBytes < downloadInfo.TotalBytes)
                    resumeDownload = true;
                else
                {
                    downloadInfo.Status = DownloadStatus.Completed;
                    _logger.LogInformation($"Model {downloadInfo.Name} already downloaded.");
                    return;
                }
            }
            else
            {
                _fileSystemService.CreateDirectory(Path.GetDirectoryName(downloadInfo.LocalPath));
                string configPath = Path.Combine(Path.GetDirectoryName(downloadInfo.LocalPath), "egoist_config.json");
                await _fileSystemService.WriteAllTextAsync(configPath, JsonSerializer.Serialize(downloadInfo.Model));
            }

            downloadInfo.Status = DownloadStatus.Downloading;

            await _networkService.DownloadFile(downloadInfo.Link, downloadInfo.LocalPath, downloadInfo.DownloadedBytes,
                progress =>
                {
                    downloadInfo.DownloadedBytes = progress;
                }, downloadInfo.CancellationTokenSource.Token);

            downloadInfo.Status = DownloadStatus.Completed;
            _logger.LogInformation($"Model {downloadInfo.Name} finished downloading.");
        }
        catch (OperationCanceledException)
        {
            downloadInfo.Status = DownloadStatus.Paused;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading model {downloadInfo.Name}");
            throw;
        }
    }

    public void ResumeDownload(DownloadInfo downloadInfo)
    {
        if (downloadInfo.Status == DownloadStatus.Paused)
        {
            downloadInfo.CancellationTokenSource = new CancellationTokenSource();
            downloadInfo.Status = DownloadStatus.Pending;
        }
    }

    public void PauseDownload(DownloadInfo downloadInfo)
    {
        downloadInfo.CancellationTokenSource.Cancel();
        downloadInfo.Status = DownloadStatus.Paused;
    }

    public void CancelDownload(DownloadInfo downloadInfo)
    {
        downloadInfo.CancellationTokenSource.Cancel();
        downloadInfo.Status = DownloadStatus.Canceled;
        if (_fileSystemService.FileExists(downloadInfo.LocalPath))
            _fileSystemService.DeleteFile(downloadInfo.LocalPath);
    }
}
