using System.Collections.ObjectModel;
using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class RoleplayWorld : EntityBase
{
    private string _name = string.Empty;

    public string Name { get => _name; set => Notify(ref _name, value); }
    public ObservableCollection<RoleplayWorldMemory> Memories { get; set; } = [];
}