using Avalonia.Controls;
using Avalonia.Interactivity;
using EGOIST.Presentation.UI.ViewModels.Dialogs;

namespace EGOIST.Presentation.UI.Views.Dialogs;

public partial class TextRoleplayWorldMemoryView : UserControl
{
    public TextRoleplayWorldMemoryView(TextRoleplayWorldMemoryViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
    
    public TextRoleplayWorldMemoryView()
    {
        InitializeComponent();
    }

    private void WorldSelectionSwitcher_OnClick(object? sender, RoutedEventArgs e)
    {
        WorldSelect.IsVisible = !WorldSelect.IsVisible;
        WorldCreate.IsVisible = !WorldCreate.IsVisible;
    }
}