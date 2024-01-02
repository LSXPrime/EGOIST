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
        viewModel.CompletionTextEditor = CompletionTextEditor;
        viewModel.CompletionSuggestionPopup = CompletionSuggestionPopup;
        viewModel.CompletionSuggestionList = CompletionSuggestionList;
    }

    [RelayCommand]
    private void ChangeTab(string index)
    {
        Tabs.SelectedIndex = int.Parse(index);
    }

    // Why in Code-Behind, it can't be detected in ViewModel
    public void Completion_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                CompletionSuggestionPopup.IsOpen = false;
                break;
            case Key.Tab:
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
            case Key.Up:
                if (CompletionSuggestionPopup.IsOpen)
                    e.Handled = true;
                CompletionSuggestionList.SelectedIndex += CompletionSuggestionList.SelectedIndex > 0 ? -1 : 0;
                CompletionSuggestionList.ScrollIntoView(CompletionSuggestionList.SelectedItem);
                break;
            case Key.Down:
                if (CompletionSuggestionPopup.IsOpen)
                    e.Handled = true;
                CompletionSuggestionList.SelectedIndex += CompletionSuggestionList.SelectedIndex < CompletionSuggestionList.Items.Count ? 1 : 0;
                CompletionSuggestionList.ScrollIntoView(CompletionSuggestionList.SelectedItem);
                break;
        }
    }
}
