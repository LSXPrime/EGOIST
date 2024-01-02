using EGOIST.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace EGOIST.Views.Pages;

public partial class ManagementPage : INavigableView<ManagementViewModel>
{
    public ManagementViewModel ViewModel { get; }

    public ManagementPage(ManagementViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
