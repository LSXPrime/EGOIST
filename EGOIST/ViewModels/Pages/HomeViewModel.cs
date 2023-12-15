using EGOIST.Views.Pages;
using EGOIST.Views.Windows;
using Wpf.Ui.Controls;

namespace EGOIST.ViewModels.Pages;

public partial class HomeViewModel : ObservableObject
{
    [RelayCommand]
    private void NavigateTo(string navTarget)
    {
        var mainWindow = App.GetService<MainWindow>();
        switch (navTarget)
        {
            case "Navigation_ImageGeneration":
                mainWindow.NavigationView.Navigate(typeof(VoicePage));
                break;
            case "Navigation_TextGeneration":
                mainWindow.NavigationView.Navigate(typeof(VoicePage));
                break;
            case "Navigation_VoiceGeneration":
                mainWindow.NavigationView.Navigate(typeof(VoicePage));
                break;
        }
    }
}
