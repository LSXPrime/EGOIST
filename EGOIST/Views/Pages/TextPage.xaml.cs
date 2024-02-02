using EGOIST.Data;
using EGOIST.ViewModels.Pages;
using Wpf.Ui.Controls;
using EGOIST.Helpers;

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

        InitializeComponent();

        viewModel.ChatContainerView = ChatContainerView;
        viewModel.RoleplayContainerView = RoleplayContainerView;
        viewModel.MemoryContainerView = MemoryContainerView;
        viewModel.CompletionTextEditor = CompletionTextEditor;
        viewModel.CompletionSuggestionPopup = CompletionSuggestionPopup;
        viewModel.CompletionSuggestionList = CompletionSuggestionList;
    }

    [RelayCommand]
    private void ChangeTab(string index)
    {
        Tabs.SelectedIndex = int.Parse(index);
    }

    private void CompletionSuggestionHandler(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case System.Windows.Input.Key.Escape:
                CompletionSuggestionPopup.IsOpen = false;
                break;
            case System.Windows.Input.Key.Tab:
                e.Handled = true;
                int lastWordIndex = CompletionTextEditor.Text.LastIndexOf(' ');
                if (lastWordIndex >= 0)
                {
                    string newText = $"{CompletionTextEditor.Text[..(lastWordIndex + 1)]}{CompletionSuggestionList.SelectedItem}";
                    CompletionTextEditor.Text = newText;
                    CompletionTextEditor.CaretIndex = CompletionTextEditor.Text.Length;
                }
                CompletionSuggestionPopup.IsOpen = false;
                break;
            case System.Windows.Input.Key.Up:
                if (CompletionSuggestionPopup.IsOpen)
                    e.Handled = true;
                CompletionSuggestionList.SelectedIndex += CompletionSuggestionList.SelectedIndex > 0 ? -1 : 0;
                CompletionSuggestionList.ScrollIntoView(CompletionSuggestionList.SelectedItem);
                break;
            case System.Windows.Input.Key.Down:
                if (CompletionSuggestionPopup.IsOpen)
                    e.Handled = true;
                CompletionSuggestionList.SelectedIndex += CompletionSuggestionList.SelectedIndex < CompletionSuggestionList.Items.Count ? 1 : 0;
                CompletionSuggestionList.ScrollIntoView(CompletionSuggestionList.SelectedItem);
                break;
        }
    }
}
