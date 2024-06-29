using Avalonia.Controls;
using EGOIST.Presentation.UI.ViewModels.Dialogs;

namespace EGOIST.Presentation.UI.Views.Dialogs;

public partial class TextMemoryCreateView : UserControl
{
    public TextMemoryCreateView(TextMemoryCreateViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}