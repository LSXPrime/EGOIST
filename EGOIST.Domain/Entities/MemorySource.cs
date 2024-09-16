using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Interfaces;

namespace EGOIST.Domain.Entities;

public class MemorySource : EntityBase, ISession<ChatMessage>
{
    private string _collection = string.Empty;
    private bool _isLoaded;
    private ObservableCollection<Citation> _documents = [];
    private int _resultsCount = 3;
    private double _minSimilarity = 0.5;

    public string Name
    {
        get => _collection;
        set => Notify(ref _collection, value);
    }
    
    public bool IsLoaded
    {
        get => _isLoaded;
        set => Notify(ref _isLoaded, value);
    }

    public ObservableCollection<Citation> Documents
    {
        get => _documents;
        set => Notify(ref _documents, value);
    }
    
    public int ResultsCount
    {
        get => _resultsCount;
        set => Notify(ref _resultsCount, value);
    }
    
    public double MinSimilarity
    {
        get => _minSimilarity;
        set => Notify(ref _minSimilarity, value);
    }


    public ObservableCollection<ChatMessage> Messages { get; set; } = [];

    public ChatMessage AddMessage(string user, string message, Citation[]? citations = null)
    {
        throw new NotImplementedException();
    }
}