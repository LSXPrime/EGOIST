using CommunityToolkit.Mvvm.Input;

namespace EGOIST.Presentation.UI.Models;

public class PageCard
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public RelayCommand NavigateAction { get; set; }
}