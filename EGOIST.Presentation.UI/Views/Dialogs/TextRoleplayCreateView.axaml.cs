using Avalonia.Controls;
using EGOIST.Presentation.UI.ViewModels.Dialogs;

namespace EGOIST.Presentation.UI.Views.Dialogs;

public partial class TextRoleplayCreateView : UserControl
{
    public TextRoleplayCreateView(TextRoleplayCreateViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
    
    public TextRoleplayCreateView()
    {
        InitializeComponent();
    }
}