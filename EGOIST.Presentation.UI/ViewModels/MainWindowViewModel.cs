using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Presentation.UI.Models;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.ViewModels.Pages;

namespace EGOIST.Presentation.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        var currentNavigationItem = NavigationData.Items.First(vm => vm.Children.Any(c => c.ViewModel == typeof(HomePageViewModel))).Children.First(c => c.ViewModel == typeof(HomePageViewModel));
        NavigationData.NavigateTo(currentNavigationItem.ViewModel);
        
        SystemInfoService.Instance.OnUpdate += (_, info) => SystemInfo = info;
        NavigationService.OnNavigation += _ =>
        {
            NavigationPath = string.Join(" - ", new[] {
                NavigationService.Current.Main.Title, 
                NavigationService.Current.Sub?.Title, 
                NavigationService.Current.Nested?.Peek().Title 
            }.Where(s => !string.IsNullOrEmpty(s)));
        };
    }

    [ObservableProperty]
    private bool _isPaneOpen;

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [ObservableProperty]
    private SystemInfo? _systemInfo;

    [ObservableProperty] 
    private string _navigationPath = "Home";

    public override string Title => "EGOIST";
}