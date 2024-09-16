using Avalonia.Controls;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;

namespace EGOIST.Presentation.UI.Views.Pages.Text;

public partial class ChatPageView : UserControl
{
    public ChatPageView(ChatPageViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }
    public ChatPageView()
    {
        InitializeComponent();
    }
}