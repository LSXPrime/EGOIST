using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Enums;

namespace EGOIST.Domain.Entities;

public class DownloadInfo : BaseEntity
{
    private string _name = string.Empty;
    private string _localPath = string.Empty;
    private string _link = string.Empty;
    private long _totalBytes = 0;
    private long _downloadedBytes = 0;
    private DownloadStatus _status = DownloadStatus.Pending;
    private ModelInfo? _model;

    public string Name { get => _name; set => Notify(ref _name, value); }
    public string LocalPath { get => _localPath; set => Notify(ref _localPath, value); }
    public string Link { get => _link; set => Notify(ref _link, value); }
    public long TotalBytes { get => _totalBytes; set => Notify(ref _totalBytes, value); }
    public long DownloadedBytes { get => _downloadedBytes; set => Notify(ref _downloadedBytes, value); }
    public DownloadStatus Status { get => _status; set => Notify(ref _status, value); }
    public ModelInfo? Model { get => _model; set => Notify(ref _model, value); }
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public string Progress => $"{Status} - ({DownloadedBytes / (1024 * 1024)}MB / {TotalBytes / (1024 * 1024)}MB) - {(double)(DownloadedBytes / TotalBytes) * 100}%";
}