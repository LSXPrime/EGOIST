using Avalonia.Controls;
using Avalonia.Threading;
using EGOIST.Presentation.UI.ViewModels.Dialogs;

namespace EGOIST.Presentation.UI.Views.Dialogs;

public partial class TextMemoryCreateView : UserControl
{
    public TextMemoryCreateView(TextMemoryCreateViewModel viewModel)
    {
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
    }
    
    public TextMemoryCreateView()
    {
        InitializeComponent();
    }
}