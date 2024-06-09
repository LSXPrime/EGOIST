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
    
    public ObservableCollection<NavigationItemGroup> NavigationItems { get; } =
    [
        new NavigationItemGroup(
            null,
            [
                new(typeof(HomePageViewModel), NavigationItemType.Main, "Home", Symbol.Home.ToString())
            ]),
        new NavigationItemGroup(
            new NavigationItem(typeof(TextPageViewModel), NavigationItemType.Main, "Text", Symbol.Textbox.ToString()),
            [
                new NavigationItem(typeof(ChatPageViewModel), NavigationItemType.Sub, "Chat", Symbol.Chat.ToString()),
                new NavigationItem(typeof(CompletionPageViewModel), NavigationItemType.Sub, "Completion", Symbol.Pen.ToString())
            ])
    ];

    private readonly Dictionary<Type, Action<NavigationItemType, Dictionary<string, object>?>> _navigationActions = new()
    {
        { typeof(HomePageViewModel), (navType, parameters) => NavigationService.NavigateTo<HomePageViewModel>(parameters: parameters, type: navType) },
        { typeof(TextPageViewModel), (navType, parameters) => NavigationService.NavigateTo<TextPageViewModel>(parameters: parameters, type: navType) },
        { typeof(ChatPageViewModel), (navType, parameters) => NavigationService.NavigateTo<ChatPageViewModel>(parameters: parameters, type: navType) },
        { typeof(CompletionPageViewModel), (navType, parameters) => NavigationService.NavigateTo<CompletionPageViewModel>(parameters: parameters, type: navType) },
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
            Debug.WriteLine($"Parent: {mainItem?.Title}");
            if (mainItem != null && _navigationActions.TryGetValue(mainItem.ViewModel, out var main))
            {
                main.Invoke(mainItem.NavType, null);
                Debug.WriteLine($"Parent Navigated: {mainItem.Title}");
            }
        }

        if (!_navigationActions.TryGetValue(item?.ViewModel!, out var action)) return;
        action.Invoke(item!.NavType, null);
        Debug.WriteLine($"Item Navigated: {item.Title}");
    }

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [ObservableProperty]
    private SystemInfo _systemInfo = new();
}


/*
 *                             <ListBox ItemsSource="{Binding NavigationItems}" SelectedItem="{Binding CurrentNavigationItem}">
                                <ListBox.Styles>
                                    <Style Selector="ListBoxItem">
                                        <Setter Property="Padding" Value="16,8"></Setter>
                                        <Setter Property="Background" Value="Transparent"></Setter>
                                    </Style>
                                </ListBox.Styles>
                                <ListBox.ItemTemplate>
                                    <DataTemplate DataType="{x:Type models:NavigationItem}">
                                        <StackPanel Spacing="25" Orientation="Horizontal">
                                            <icons:SymbolIcon Symbol="{Binding Icon}" FontSize="24" />
                                            <TextBlock Text="{Binding Title}" FontSize="24" />
                                        </StackPanel>
                                    </DataTemplate>
                                </ListBox.ItemTemplate>
                            </ListBox>
*/

/*
																<u:Divider Width="5" Height="25" IsVisible="{Binding ., Converter={StaticResource EqualBoolConverter}, ConverterParameter={Binding #NavChildren.SelectedItem}}" Orientation="Vertical" />


                            <ListBox ItemsSource="{Binding NavigationItems}" >
                                <ListBox.Styles>
                                    <Style Selector="ListBoxItem">
                                        <Setter Property="Padding" Value="0,0,0,0"></Setter>
                                        <Setter Property="Background" Value="Transparent"></Setter>
                                    </Style>

                                </ListBox.Styles>
                                <ListBox.ItemTemplate>
                                    <DataTemplate>
                                        <StackPanel>
                                            <ToggleButton HorizontalAlignment="Stretch" HorizontalContentAlignment="Left" IsChecked="{Binding Key.Visible}" Command="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).NavigateToCommand}" CommandParameter="{Binding Key}" Background="Transparent" Foreground="White" >
                                                <StackPanel Spacing="25" Orientation="Horizontal" HorizontalAlignment="Left">
                                                    <icons:SymbolIcon Symbol="{Binding Key.Icon}" FontSize="24" />
                                                    <TextBlock Text="{Binding Key.Title}" FontSize="24" />
                                                </StackPanel>
                                            </ToggleButton>
                                            <icons:SymbolIcon Margin="22.5,-10,0,0" Symbol="ArrowDown" FontSize="8" HorizontalAlignment="Left" IsVisible="{Binding !Key.Visible}" />

                                            <u:Divider Margin="0,5,0,0" />

                                            <ListBox ItemsSource="{Binding Value}" SelectedItem="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).CurrentNavigationItem}" IsVisible="{Binding Key.Visible}" >
                                                <ListBox.Styles>
                                                    <Style Selector="ListBoxItem">
                                                        <Setter Property="Padding" Value="20,8"></Setter>
                                                    </Style>
													<Style Selector="ListBoxItem:selected">
														<Setter Property="Width" Value="500"></Setter>
														<Setter Property="Background" Value="Transparent"/>
														<Setter Property="HorizontalAlignment" Value="Left"/>
													</Style>
                                                </ListBox.Styles>
                                                <ListBox.ItemTemplate>
                                                    <DataTemplate DataType="{x:Type models:NavigationItem}">
                                                        <MenuItem Command="{Binding $parent[Window].((vm:MainWindowViewModel)DataContext).NavigateToCommand}" CommandParameter="{Binding}" Background="Transparent" HorizontalAlignment="Left" Padding="0,0,20,0" >
                                                            <StackPanel Spacing="25" Orientation="Horizontal">
                                                                <icons:SymbolIcon Symbol="{Binding Icon}" FontSize="16" />
                                                                <TextBlock Text="{Binding Title}" FontSize="16" />
                                                            </StackPanel>
                                                        </MenuItem>
                                                    </DataTemplate>
                                                </ListBox.ItemTemplate>
                                            </ListBox>
                                        </StackPanel>
                                    </DataTemplate>
                                </ListBox.ItemTemplate>
                            </ListBox>

*/