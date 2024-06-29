using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        SystemInfoService.Instance.SetSystemInfo(ref _systemInfo);
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
                new NavigationItem(typeof(MemoryPageViewModel), NavigationItemType.Sub, "Memory", Symbol.Record.ToString())
            ])
    ];

    private readonly Dictionary<Type, Action<NavigationItemType, Dictionary<string, object>?>> _navigationActions = new()
    {
        { typeof(HomePageViewModel), (navType, parameters) => NavigationService.NavigateTo<HomePageViewModel>(parameters: parameters, type: navType) },
        { typeof(TextPageViewModel), (navType, parameters) => NavigationService.NavigateTo<TextPageViewModel>(parameters: parameters, type: navType) },
        { typeof(ChatPageViewModel), (navType, parameters) => NavigationService.NavigateTo<ChatPageViewModel>(parameters: parameters, type: navType) },
        { typeof(CompletionPageViewModel), (navType, parameters) => NavigationService.NavigateTo<CompletionPageViewModel>(parameters: parameters, type: navType) },
        { typeof(MemoryPageViewModel), (navType, parameters) => NavigationService.NavigateTo<MemoryPageViewModel>(parameters: parameters, type: navType) },
    };

    partial void OnCurrentNavigationItemChanged(NavigationItem value)
    {
        NavigateTo(value);
    }

    [RelayCommand]
    private void NavigateTo(NavigationItem? item)
    {

        if (item?.NavType == NavigationItemType.Sub)
        {
            var mainItem = NavigationItems.First(x => x.Children.Contains(item)).Parent;
            if (mainItem != null && _navigationActions.TryGetValue(mainItem.ViewModel, out var main))
            {
                main.Invoke(mainItem.NavType, null);
            }
        }

        if (!_navigationActions.TryGetValue(item?.ViewModel!, out var action)) return;
        action.Invoke(item!.NavType, null);
    }
    
    public void NavigateTo(Type? navType)
    {
        var item = NavigationItems
            .SelectMany(x => x.Children)
            .FirstOrDefault(x => x.ViewModel == navType);
        
        if (item?.NavType == NavigationItemType.Sub)
        {
            var mainItem = NavigationItems.First(x => x.Children.Contains(item)).Parent;
            if (mainItem != null && _navigationActions.TryGetValue(mainItem.ViewModel, out var main))
            {
                main.Invoke(mainItem.NavType, null);
            }
        }

        if (!_navigationActions.TryGetValue(item?.ViewModel!, out var action)) return;
        action.Invoke(item!.NavType, null);
    }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [ObservableProperty]
    private SystemInfo _systemInfo = new();

    public override string Title { get; } = "EGOIST";
}