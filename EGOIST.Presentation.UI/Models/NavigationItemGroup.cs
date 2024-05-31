using System.Collections.ObjectModel;

namespace EGOIST.Presentation.UI.Models;

public class NavigationItemGroup(NavigationItem? parent, ObservableCollection<NavigationItem> children)
{
    public NavigationItem? Parent { get; } = parent;
    public ObservableCollection<NavigationItem> Children { get; } = children;
}
