using Avalonia.Controls;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Pages;

namespace EGOIST.Presentation.UI.Views.Pages;

public partial class ImagePageView : UserControl
{
    public ImagePageView(ImagePageViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }
    
    public ImagePageView() => InitializeComponent();
}