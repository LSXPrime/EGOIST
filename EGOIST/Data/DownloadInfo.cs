using EGOIST.Enums;
using EGOIST.Helpers;

namespace EGOIST.Data;

public class DownloadInfo
{
    public required string Name { get; set; }
    public required string LocalPath { get; set; }
    public required string Link { get; set; }
    public long TotalBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public DownloadStatus Status { get; set; }
    public string Progress => $"{Status} - ({DownloadedBytes.BytesToMB()}MB / {TotalBytes.BytesToMB()}MB) - {(double)(DownloadedBytes / TotalBytes) * 100}%";
    public required ModelInfo Model { get; set; }
    public required CancellationTokenSource CancellationTokenSource { get; set; }

}
