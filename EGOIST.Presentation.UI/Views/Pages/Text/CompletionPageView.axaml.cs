using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.ViewModels.Pages.Text;
using NetFabric.Hyperlinq;

namespace EGOIST.Presentation.UI.Views.Pages.Text;

public partial class CompletionPageView : UserControl
{
    private readonly string[]? _wordsDictionary = [];
    private CompletionPageViewModel ViewModel => (CompletionPageViewModel)DataContext!;
    private int _searchIndex = -1;

    // Property to bind the search text
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            // Reset the search index when search text changes
            _searchIndex = -1;
        }
    }
    
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
            default:
                UpdateCompletionChanges();
                break;
        }
    }
    
    private void UpdateCompletionChanges()
    {
        var content = CompletionTextEditor.Text;
        if (string.IsNullOrEmpty(content))
            return;
        var completionLineCount = content!.Split('\n').Length;
        var completionWord = content.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var completionCharacterCount = content.Length;

        ViewModel.CompletionStatics = $"L: {completionLineCount} || W: {completionWord.Length} || C: {completionCharacterCount}";

    //    Debug.WriteLine($"UserInput: {ViewModel.Content}");
   //     Debug.WriteLine($"Lines: {completionLineCount}, Words: {completionWord.Length}, Characters: {completionCharacterCount}");
        if (ViewModel.State == GenerationState.Started || CompletionTextEditor == null || completionWord.Length == 0)
            return;

        var searchText = completionWord[^1].ToLower();
        var filteredList = _wordsDictionary?.AsValueEnumerable().Where(word => word.StartsWith(searchText)).ToArray();
        if (CompletionTextEditor.IsFocused && filteredList?.Length > 0)
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
    
    // Method to perform the search operation
    private void Search(object? sender, RoutedEventArgs e)
    {
        // Get the current text in the TextBox
        var text = CompletionTextEditor.Text;

        // If text is null or empty, do nothing
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        
        // If search text is empty, do nothing
        if (string.IsNullOrEmpty(_searchText))
        {
            return;
        }

        // Increment the search index to find the next occurrence
        _searchIndex = (_searchIndex + 1) % text!.Length;
        
        // Find the next occurrence of the search text starting from the current search index
        var startIndex = text.IndexOf(_searchText, _searchIndex, StringComparison.OrdinalIgnoreCase);
    //    Debug.WriteLine($"Start index: {startIndex}, Search index: {_searchIndex}, Search text: {_searchText}");

        // If search text is found
        if (startIndex != -1)
        {
            // Update the search index to the end of the current search result
            _searchIndex = startIndex + _searchText.Length;
            
            // Select the search text
            CompletionTextEditor.SelectionStart = startIndex;
            CompletionTextEditor.SelectionEnd = _searchIndex;
            
            // Scroll to the selected text
            CompletionTextEditor.CaretIndex = startIndex;
            
   //         Debug.WriteLine($"Selected index: {CompletionTextEditor.CaretIndex}, Selection start: {CompletionTextEditor.SelectionStart}, Selection end: {CompletionTextEditor.SelectionEnd}, Search index: {_searchIndex}, Selected text: {CompletionTextEditor.SelectedText}");
//            CompletionTextEditor.ScrollToCaret();
        }
        // If the search text is not found, reset the search index
        else
        {
            _searchIndex = -1;
        }
    }
}