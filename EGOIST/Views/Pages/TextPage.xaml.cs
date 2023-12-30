using System.Windows.Input;
using EGOIST.Data;
using EGOIST.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace EGOIST.Views.Pages;
public partial class TextPage : INavigableView<TextViewModel>
{
    public TextViewModel ViewModel { get; }
    public SystemInfo Info { get; set; }

    public TextPage(TextViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        Info = SystemInfo.Instance;
        Info.Montitor();

        InitializeComponent();

        viewModel.ChatContainerView = ChatContainerView;
    }

    [RelayCommand]
    private void ChangeTab(string index)
    {
        Tabs.SelectedIndex = int.Parse(index);
    }
}
