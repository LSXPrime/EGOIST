using System.Collections.Generic;
using EGOIST.Presentation.UI.Interfaces.Navigation;

namespace EGOIST.Presentation.UI.ViewModels.Pages;

public class HomePageViewModel : ViewModelBase, INavigationAware
{
    public new string Title => "Home";

    public void Initialize(Dictionary<string, object>? parameters) { }

    public void OnNavigatedFrom() { }

    public void OnNavigatedTo() { }
}
