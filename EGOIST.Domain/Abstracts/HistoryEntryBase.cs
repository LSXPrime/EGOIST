using System.Globalization;
using System.Text.Json.Serialization;
using EGOIST.Domain.Entities;

namespace EGOIST.Domain.Abstracts;

[JsonDerivedType(typeof(ImageHistoryEntry), "Image")]
public class HistoryEntryBase : EntityBase
{
    private string _name = DateTime.Now.ToString("yyyyMMdd_HH-mm-ss", CultureInfo.InvariantCulture);
    private string _path = string.Empty;
    private readonly string _type = string.Empty;
    private readonly string _extension = string.Empty;
    
    public string Name { get => _name; set => Notify(ref _name, value); }
    public string Path { get => _path; set => Notify(ref _path, value); }
    public string Type { get => _type; init => Notify(ref _type, value); }
    public string Extension { get => _extension; init => Notify(ref _extension, value); }

    public HistoryEntryBase()
    {
    }

    [JsonConstructor]
    protected HistoryEntryBase(string name, string path, string type, string extension)
    {
        _name = name;
        _path = path;
        _type = type;
        _extension = extension;
    }
}