using System;
using Avalonia.Controls;
using EGOIST.Presentation.UI.ViewModels;
using FluentAvalonia.UI.Controls;

namespace EGOIST.Presentation.UI.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;
    
    public MainWindow()
    {
        InitializeComponent();
        
        /*
        ClientSizeProperty.Changed.Subscribe(size =>
        {
            const double aspectRatio = 16.0 / 9.0;
            ClientSize = Math.Abs(size.NewValue.Value.Width - size.OldValue.Value.Width) > 0.01f 
                ? new Size(size.NewValue.Value.Width, (int)(size.NewValue.Value.Width / aspectRatio)) 
                : new Size((int)(size.NewValue.Value.Height * aspectRatio), size.NewValue.Value.Height);
        });
        */
    }

    private void NavView_OnSelectionChanged(object? _, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is not NavigationViewItem nvItem) return;
        var type = Type.GetType($"EGOIST.Presentation.UI.ViewModels.Pages.{nvItem.Tag}");
        ViewModel.NavigateTo(type);
    }
    
    
    /*
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        const double aspectRatio = 16.0 / 9.0;
        var newWidth = Math.Max(1280, e.NewSize.Width);
        var newHeight = Math.Max(720, e.NewSize.Height);
        
        ClientSize = e.WidthChanged
            ? new Size(newWidth, (int)(newWidth / aspectRatio))
            : new Size((int)(newHeight * aspectRatio), newHeight);
    }
    */
    
}