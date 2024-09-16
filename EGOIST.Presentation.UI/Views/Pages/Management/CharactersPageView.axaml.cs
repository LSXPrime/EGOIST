using Avalonia.Controls;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Pages;

namespace EGOIST.Presentation.UI.Views.Pages.Management;

public partial class CharactersPageView : UserControl
{
    public CharactersPageView(SettingsPageViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }

    public CharactersPageView() => InitializeComponent();
}