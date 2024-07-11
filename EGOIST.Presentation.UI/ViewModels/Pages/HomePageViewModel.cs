using System.Collections.Generic;
using System.Threading.Tasks;
using EGOIST.Presentation.UI.Interfaces.Navigation;

namespace EGOIST.Presentation.UI.ViewModels.Pages;

public class HomePageViewModel : ViewModelBase, INavigationAware
{
    public override string Title => "Home";

    public Task Initialize(Dictionary<string, object>? parameters) => Task.CompletedTask;

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;
}