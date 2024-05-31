using System;

namespace EGOIST.Presentation.UI.Models;

public class NavigationItem(Type type, NavigationItemType navType, string title, string icon)
{
    public string Title { get; } = title;
    public string Icon { get; } = icon;
    public Type ViewModel { get; } = type;
    public NavigationItemType NavType { get; } = navType;
}