using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Models;

namespace EGOIST.Presentation.UI.ViewModels.Pages;

public class HomePageViewModel : ViewModelBase, INavigationAware
{
    public override string Title => "Home";
    public List<PageCard> PageCards { get; set; } = [];


    public Task Initialize(Dictionary<string, object>? parameters)
    {
        PageCards = CreatePageCards(NavigationData.Items);   
        return Task.CompletedTask;
    }

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo() => Task.CompletedTask;


    private List<PageCard> CreatePageCards(NavigationItemGroup[] navigationItems)
    {
        return (from @group in navigationItems from item in @group.Children where NavigationData.Actions.ContainsKey(item.ViewModel) && item.NavType == NavigationItemType.Sub select new PageCard { Title = item.Title, Description = item.Description, Icon = item.Icon, NavigateAction = new RelayCommand(() => NavigationData.NavigateTo(item.ViewModel)) }).ToList();
    }
}