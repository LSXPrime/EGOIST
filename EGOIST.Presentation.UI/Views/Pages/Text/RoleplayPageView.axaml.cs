using Avalonia.Controls;
using Avalonia.Interactivity;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;

namespace EGOIST.Presentation.UI.Views.Pages.Text;

public partial class RoleplayPageView : UserControl
{
    public RoleplayPageView(RoleplayPageViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
    
    public RoleplayPageView()
    {
        InitializeComponent();
    }

    private void SplitViewPaneToggle_OnClick(object? sender, RoutedEventArgs e)
    {
        SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
    }
}