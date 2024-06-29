using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using EGOIST.Presentation.UI.Models;
using EGOIST.Presentation.UI.ViewModels;
using FluentAvalonia.UI.Controls;

namespace EGOIST.Presentation.UI.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is not NavigationViewItem nvItem) return;
        var type = Type.GetType($"EGOIST.Presentation.UI.ViewModels.Pages.{nvItem.Tag}");
        ViewModel.NavigateTo(type);
    }
}