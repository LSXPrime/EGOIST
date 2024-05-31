using EGOIST.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace EGOIST.Views.Pages;
public partial class VoicePage : INavigableView<VoiceViewModel>
{
    public VoiceViewModel ViewModel { get; }

    public VoicePage(VoiceViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
