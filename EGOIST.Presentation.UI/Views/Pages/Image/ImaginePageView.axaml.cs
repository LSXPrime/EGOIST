using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Pages.Image;

namespace EGOIST.Presentation.UI.Views.Pages.Image;

public partial class ImaginePageView : UserControl
{
    public ImaginePageView(ImaginePageViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }
    public ImaginePageView()
    {
        InitializeComponent();
    }
}