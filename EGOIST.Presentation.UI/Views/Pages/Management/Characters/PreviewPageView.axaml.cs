using Avalonia.Controls;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Pages;
using EGOIST.Presentation.UI.ViewModels.Pages.Management.Characters;

namespace EGOIST.Presentation.UI.Views.Pages.Management.Characters;

public partial class PreviewPageView : UserControl
{
    public PreviewPageView(PreviewPageViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }

    public PreviewPageView() => InitializeComponent();
}