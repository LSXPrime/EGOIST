using Avalonia.Controls;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Pages;

namespace EGOIST.Presentation.UI.Views.Pages;

public partial class SettingsPageView : UserControl
{
    public SettingsPageView(SettingsPageViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }

    public SettingsPageView() => InitializeComponent();
}