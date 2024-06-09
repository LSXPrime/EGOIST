using Avalonia.Controls;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;

namespace EGOIST.Presentation.UI.Views.Pages.Text;

public partial class ChatPageView : UserControl
{
    public ChatPageView(ChatPageViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
    public ChatPageView()
    {
        InitializeComponent();
    }
}