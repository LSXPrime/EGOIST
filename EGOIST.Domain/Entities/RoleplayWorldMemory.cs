using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class RoleplayWorldMemory : EntityBase
{
    private string _name = string.Empty;
    private string _content = string.Empty;
    private string _author = string.Empty;
    private DateTime _creationDate = DateTime.Now;

    public string Name { get => _name; set => Notify(ref _name, value); }
    public string Content { get => _content; set => Notify(ref _content, value); }
    public string Author { get => _author; set => Notify(ref _author, value); }
    public DateTime CreationDate { get => _creationDate; set => Notify(ref _creationDate, value); }
    public ObservableCollection<string> Tags { get; set; } = [];
    
    [field: NonSerialized]
    public bool IsRetrieved { get; set; }
}