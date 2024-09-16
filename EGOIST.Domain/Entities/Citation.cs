using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

/// <summary>
/// Represents a reference to a local file or a search result.
/// </summary>
public class Citation : EntityBase
{
    private string _collection = string.Empty;
    private string _title = string.Empty;
    private string _path = string.Empty;
    private string _content = string.Empty;
    private float _relevance;
    
    /// <summary>
    /// The name of the collection the cited item belongs to.
    /// </summary>
    public string Collection
    {
        get => _collection;
        set => Notify(ref _collection, value);
    }
    
    /// <summary>
    /// The title or name of the cited item.
    /// </summary>
    public string Title
    {
        get => _title;
        set => Notify(ref _title, value);
    }

    /// <summary>
    /// The local file path or the URL of the cited item.
    /// </summary>
    public string Path
    {
        get => _path;
        set => Notify(ref _path, value);
    }

    /// <summary>
    /// The content of the cited item. 
    /// </summary>
    public string Content
    {
        get => _content;
        set => Notify(ref _content, value);
    }
    
    /// <summary>
    /// The relevance of the cited item.
    /// </summary>
    public float Relevance
    {
        get => _relevance;
        set => Notify(ref _relevance, value);
    }
}