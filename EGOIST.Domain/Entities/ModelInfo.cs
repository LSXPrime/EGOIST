using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class ModelInfo : BaseEntity
{
    private string _name = string.Empty;
    private string _backend = string.Empty;
    private string _type = string.Empty;
    private string _task = string.Empty;
    private string _architecture = string.Empty;
    private string _parameters = string.Empty;
    private string _updateDate = string.Empty;
    private string _description = string.Empty;
    private string _downloadRepo = string.Empty;
    private TextPromptParameters? _textConfig;
    private Dictionary<string, string> _metadata = [];

    public string Name { get => _name; set => Notify(ref _name, value); }
    public string Backend { get => _backend; set => Notify(ref _backend, value); }
    public string Type { get => _type; set => Notify(ref _type, value); }
    public string Task { get => _task; set => Notify(ref _task, value); }
    public string Architecture { get => _architecture; set => Notify(ref _architecture, value); }
    public string Parameters { get => _parameters; set => Notify(ref _parameters, value); }
    public string UpdateDate { get => _updateDate; set => Notify(ref _updateDate, value); }
    public string Description { get => _description; set => Notify(ref _description, value); }
    public ObservableCollection<ModelInfoWeight>? Weights { get; set; } = new();
    public string DownloadRepo { get => _downloadRepo; set => Notify(ref _downloadRepo, value); }
    public TextPromptParameters? TextConfig { get => _textConfig; set => Notify(ref _textConfig, value); }
    public Dictionary<string, string> Metadata { get => _metadata; set => Notify(ref _metadata, value); }
}