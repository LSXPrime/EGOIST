using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;
using NetFabric.Hyperlinq;

namespace EGOIST.Presentation.UI.Views.Pages.Text;

public partial class CompletionPageView : UserControl
{
    private readonly string[]? _wordsDictionary;

    public CompletionPageView(CompletionPageViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream($"{assembly.FullName}.Assets.dictionary.txt");
        if (stream == null) return;
        using StreamReader reader = new(stream);
        _wordsDictionary = reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    }
    public CompletionPageView()
    {
        InitializeComponent();
    }

    private void CompletionTextEditor_SuggestionHandler(object? _, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                CompletionSuggestionPopup.IsOpen = false;
                break;
            case Key.Tab:
                e.Handled = true;
                var lastWordIndex = CompletionTextEditor.Text!.LastIndexOf(' ');
                if (lastWordIndex >= 0)
                {
                    var newText = $"{CompletionTextEditor.Text[..(lastWordIndex + 1)]}{CompletionSuggestionList.SelectedItem}";
                    CompletionTextEditor.Text = newText;
                    CompletionTextEditor.CaretIndex = CompletionTextEditor.Text.Length;
                }
                CompletionSuggestionPopup.IsOpen = false;
                break;
            case Key.Up:
                if (CompletionSuggestionPopup.IsOpen)
                    e.Handled = true;
                CompletionSuggestionList.SelectedIndex += CompletionSuggestionList.SelectedIndex > 0 ? -1 : 0;
                CompletionSuggestionList.ScrollIntoView(CompletionSuggestionList.SelectedItem!);
                break;
            case Key.Down:
                if (CompletionSuggestionPopup.IsOpen)
                    e.Handled = true;
                CompletionSuggestionList.SelectedIndex += CompletionSuggestionList.SelectedIndex < CompletionSuggestionList.Items.Count ? 1 : 0;
                CompletionSuggestionList.ScrollIntoView(CompletionSuggestionList.SelectedItem!);
                break;
        }
    }
    
    private void UpdateCompletionChanges()
    {
        var vm = (CompletionPageViewModel)DataContext!;
        var completionLineCount = vm.UserInput.Split('\n').Length;
        var completionWord = vm.UserInput.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var completionCharacterCount = vm.UserInput.Length;

        vm.CompletionStatics = $"L: {completionLineCount} || W: {completionWord.Length} || C: {completionCharacterCount}";

        if (vm.State == GenerationState.Started || CompletionTextEditor == null)
            return;

        var searchText = completionWord[^1].ToLower();
        var filteredList = _wordsDictionary?.AsValueEnumerable().Where(word => word.StartsWith(searchText)).ToArray()!;
        if (CompletionTextEditor.IsFocused && filteredList.Length > 0)
        {
            CompletionSuggestionList.ItemsSource = filteredList;
            CompletionSuggestionList.SelectedItem = filteredList[0];
            //var caretPosition = CompletionTextEditor.GetRectFromCharacterIndex(CompletionTextEditor.CaretIndex);
            //var point = CompletionTextEditor.PointToScreen(new Point(caretPosition.Right, caretPosition.Bottom));
            //CompletionSuggestionPopup.PlacementRect = new Rect(point.X, point.Y + caretPosition.Height, 0, 0);
            CompletionSuggestionPopup.IsOpen = true;
        }
        else
            CompletionSuggestionPopup.IsOpen = false;
    }


}