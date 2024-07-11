using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Services.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Presentation.UI.Models;
using EGOIST.Presentation.UI.Services;
using EGOIST.Presentation.UI.ViewModels.Pages;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;
using FluentIcons.Common;

namespace EGOIST.Presentation.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        CurrentNavigationItem = NavigationItems.First(vm => vm.Children.Any(c => c.ViewModel == typeof(HomePageViewModel))).Children.First(c => c.ViewModel == typeof(HomePageViewModel));
        NavigateTo(CurrentNavigationItem.ViewModel);
        
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

    [ObservableProperty]
    private NavigationItem _currentNavigationItem;

    private ObservableCollection<NavigationItemGroup> NavigationItems { get; } =
    [
        new NavigationItemGroup(
            null,
            [
                new NavigationItem(typeof(HomePageViewModel), NavigationItemType.Main, "Home", Symbol.Home.ToString())
            ]),
        new NavigationItemGroup(
            new NavigationItem(typeof(TextPageViewModel), NavigationItemType.Main, "Text", Symbol.Textbox.ToString()),
            [
                new NavigationItem(typeof(ChatPageViewModel), NavigationItemType.Sub, "Chat", Symbol.Chat.ToString()),
                new NavigationItem(typeof(CompletionPageViewModel), NavigationItemType.Sub, "Completion", Symbol.Pen.ToString()),
                new NavigationItem(typeof(MemoryPageViewModel), NavigationItemType.Sub, "Memory", Symbol.Record.ToString()),
                new NavigationItem(typeof(RoleplayPageViewModel), NavigationItemType.Sub, "Roleplay", Symbol.Person.ToString())
            ])
    ];
    
    private readonly Dictionary<Type, (NavigationItemType NavType, Action<NavigationItemType, Dictionary<string, object>?> Action)> _navigationActions = new()
    {
        { typeof(HomePageViewModel), (NavigationItemType.Main, (navType, parameters) => NavigationService.NavigateTo<HomePageViewModel>(parameters: parameters, type: navType)) },
        { typeof(TextPageViewModel), (NavigationItemType.Main, (navType, parameters) => NavigationService.NavigateTo<TextPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(ChatPageViewModel), (NavigationItemType.Sub, (navType, parameters) => NavigationService.NavigateTo<ChatPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(CompletionPageViewModel), (NavigationItemType.Sub, (navType, parameters) => NavigationService.NavigateTo<CompletionPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(MemoryPageViewModel), (NavigationItemType.Sub, (navType, parameters) => NavigationService.NavigateTo<MemoryPageViewModel>(parameters: parameters, type: navType)) },
        { typeof(RoleplayPageViewModel), (NavigationItemType.Sub, (navType, parameters) => NavigationService.NavigateTo<RoleplayPageViewModel>(parameters: parameters, type: navType)) }
    };
    
    public void NavigateTo(Type? navType)
    {
        if (navType == null || !_navigationActions.TryGetValue(navType, out var navData)) 
            return;
        
        var (navItemType, action) = navData; 

        var targetItem = NavigationItems
            .SelectMany(group => group.Children)
            .FirstOrDefault(item => item.ViewModel == navType);

        if (targetItem?.NavType == NavigationItemType.Sub)
        {
            var parentItem = NavigationItems
                .FirstOrDefault(group => group.Children.Contains(targetItem))?
                .Parent;

            if (parentItem != null && _navigationActions.TryGetValue(parentItem.ViewModel, out var parentNavData))
            {
                parentNavData.Action.Invoke(parentNavData.NavType, null);
            }
        }

        action.Invoke(navItemType, null);
    }

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