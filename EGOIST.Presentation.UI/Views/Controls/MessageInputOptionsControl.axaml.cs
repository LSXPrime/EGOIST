using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Domain.Entities;
using EGOIST.Presentation.UI.Interfaces.Interactions;
using EGOIST.Presentation.UI.Services;

namespace EGOIST.Presentation.UI.Views.Controls;

[TemplatePart("PART_MemorySourcesList", typeof(ListBox))]
public class MessageInputOptionsControl : TemplatedControl
{
    public static readonly StyledProperty<bool> MemoryEnabledProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, bool>(nameof(MemoryEnabled),
            defaultBindingMode: BindingMode.TwoWay);

    public bool MemoryEnabled
    {
        get => GetValue(MemoryEnabledProperty);
        set => SetValue(MemoryEnabledProperty, value);
    }

    public static readonly StyledProperty<int> MemoryChunksCountProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, int>(nameof(MemoryChunksCount),
            defaultBindingMode: BindingMode.TwoWay);

    public int MemoryChunksCount
    {
        get => GetValue(MemoryChunksCountProperty);
        set => SetValue(MemoryChunksCountProperty, value);
    }
    
    
    public static readonly StyledProperty<double> MemoryRelevanceProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, double>(nameof(MemoryRelevance),
            defaultBindingMode: BindingMode.TwoWay);

    public double MemoryRelevance
    {
        get => GetValue(MemoryRelevanceProperty);
        set => SetValue(MemoryRelevanceProperty, value);
    }
    
    public static readonly StyledProperty<object> MemorySourcesProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, object>(nameof(MemorySources));

    public object MemorySources
    {
        get => GetValue(MemorySourcesProperty);
        set => SetValue(MemorySourcesProperty, value);
    }

    public static readonly StyledProperty<bool> WebSearchEnabledProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, bool>(nameof(WebSearchEnabled),
            defaultBindingMode: BindingMode.TwoWay);

    public bool WebSearchEnabled
    {
        get => GetValue(WebSearchEnabledProperty);
        set => SetValue(WebSearchEnabledProperty, value);
    }

    public static readonly StyledProperty<int> WebSearchResultsCountProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, int>(nameof(WebSearchResultsCount),
            defaultBindingMode: BindingMode.TwoWay);

    public int WebSearchResultsCount
    {
        get => GetValue(WebSearchResultsCountProperty);
        set => SetValue(WebSearchResultsCountProperty, value);
    }
    
    public static readonly StyledProperty<object> CitationsProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, object>(nameof(Citations));

    public object Citations
    {
        get => GetValue(CitationsProperty);
        set => SetValue(CitationsProperty, value);
    }

    public static readonly StyledProperty<object> StateProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, object>(nameof(State));

    public object State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
    
    public static readonly StyledProperty<string> UserInputProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, string>(nameof(UserInput),
            defaultBindingMode: BindingMode.TwoWay);

    public string UserInput
    {
        get => GetValue(UserInputProperty);
        set => SetValue(UserInputProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> GenerateCommandProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, IRelayCommand>(nameof(GenerateCommand));

    public IRelayCommand GenerateCommand
    {
        get => GetValue(GenerateCommandProperty);
        set => SetValue(GenerateCommandProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> LoadAttachmentCommandProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, IRelayCommand>(nameof(LoadAttachmentCommand));

    public IRelayCommand LoadAttachmentCommand
    {
        get => GetValue(LoadAttachmentCommandProperty);
        set => SetValue(LoadAttachmentCommandProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> RemoveAttachmentCommandProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, IRelayCommand>(nameof(RemoveAttachmentCommand));

    public IRelayCommand RemoveAttachmentCommand
    {
        get => GetValue(RemoveAttachmentCommandProperty);
        set => SetValue(RemoveAttachmentCommandProperty, value);
    }

    public MessageInputOptionsControl() => DataContext = this;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var sessionList = e.NameScope.Find<ListBox>("PART_MemorySourcesList");
        if (sessionList != null)
        {
            sessionList.SelectionChanged += (_, _) =>
            {
                if (NavigationService.Current.Sub is not ITextViewModel textViewModel)
                    return;
                
                if (sessionList.SelectedItems is { Count: > 0 } )
                    textViewModel.Select(sessionList.SelectedItems.OfType<MemorySource>().ToArray());
            };
        }

        base.OnApplyTemplate(e);
    }
}