using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;


public class ModelInfoWeight : EntityBase
{
    private string _extension = string.Empty;
    private string _weight = string.Empty;
    private string _link = string.Empty;
    private long _size;
    private Dictionary<string, string> _metadata = [];

    public string Extension { get => _extension; set => Notify(ref _extension, value); }
    public string Weight { get => _weight; set => Notify(ref _weight, value); }
    public string Link { get => _link; set => Notify(ref _link, value); }
    public long Size { get => _size; set => Notify(ref _size, value); }
    public Dictionary<string, string> Metadata { get => _metadata; set => Notify(ref _metadata, value); }
}