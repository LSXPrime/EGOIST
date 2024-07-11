using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using EGOIST.Presentation.UI.Interfaces.Navigation;
using EGOIST.Presentation.UI.Models;
using EGOIST.Presentation.UI.ViewModels;
using EGOIST.Presentation.UI.ViewModels.Pages;

namespace EGOIST.Presentation.UI.Services;

public static class NavigationService
{
    public static readonly CurrentPage Current = new();
    
    public delegate void NavigationEventHandler(object? sender);
    public static event NavigationEventHandler? OnNavigation;
    

    public static void NavigateTo<TViewModel>(Dictionary<string, object>? parameters = null, NavigationItemType type = NavigationItemType.Main) where TViewModel : ViewModelBase
    {
        var viewModel = Design.IsDesignMode ? Activator.CreateInstance<TViewModel>() : Ioc.Default.GetService<TViewModel>();
        if (viewModel is INavigationAware navigationAware)
        {
            navigationAware.Initialize(parameters);
            navigationAware.OnNavigatedTo();
        }

        if (viewModel is ViewModelBase vm)
            switch (type)
            {
                case NavigationItemType.Main:
                    Current.Main = vm;
                    break;
                case NavigationItemType.Sub:
                    Current.Sub = vm;
                    break;
                case NavigationItemType.Nested:
                    Current.Nested ??= new Stack<ViewModelBase>();
                    Current.Nested.Push(vm);
                    break;
            }
        else
            throw new ArgumentException($"No navigation item found for ViewModel type {typeof(TViewModel)}");
        
        OnNavigation?.Invoke(viewModel);
    }
}

public partial class CurrentPage : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _main = new HomePageViewModel();
    [ObservableProperty]
    private ViewModelBase? _sub;
    [ObservableProperty]
    private Stack<ViewModelBase>? _nested;
}