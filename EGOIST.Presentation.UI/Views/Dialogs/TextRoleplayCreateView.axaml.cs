using Avalonia.Controls;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Dialogs;

namespace EGOIST.Presentation.UI.Views.Dialogs;

public partial class TextRoleplayCreateView : UserControl
{
    public TextRoleplayCreateView(TextRoleplayCreateViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }
    
    public TextRoleplayCreateView()
    {
        InitializeComponent();
    }
}