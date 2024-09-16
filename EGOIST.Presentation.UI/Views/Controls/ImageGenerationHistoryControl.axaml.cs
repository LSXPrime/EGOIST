using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;

namespace EGOIST.Presentation.UI.Views.Controls;

[TemplatePart("PART_TogglePaneBtn", typeof(Button))]
public class ImageGenerationHistoryControl : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ImageGenerationHistoryControl, string>(nameof(Title), "Options");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ImageGenerationHistoryControl, bool>(nameof(IsOpen), defaultValue: false);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<object> HistoryProperty =
        AvaloniaProperty.Register<ImageGenerationHistoryControl, object>(nameof(History));
    
    public object History
    {
        get => GetValue(HistoryProperty);
        set => SetValue(HistoryProperty, value);
    }
    
    public static readonly StyledProperty<IRelayCommand> SelectEntryCommandProperty =
        AvaloniaProperty.Register<ImageGenerationHistoryControl, IRelayCommand>(nameof(SelectEntryCommand));
    
    public IRelayCommand SelectEntryCommand
    {
        get => GetValue(SelectEntryCommandProperty);
        set => SetValue(SelectEntryCommandProperty, value);
    }
    
    public static readonly StyledProperty<IRelayCommand> DeleteEntryCommandProperty =
        AvaloniaProperty.Register<ImageGenerationHistoryControl, IRelayCommand>(nameof(DeleteEntryCommand));
    
    public IRelayCommand DeleteEntryCommand
    {
        get => GetValue(DeleteEntryCommandProperty);
        set => SetValue(DeleteEntryCommandProperty, value);
    }
    

    private Button? TogglePaneBtn { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        TogglePaneBtn = e.NameScope.Find<Button>("PART_TogglePaneBtn");
        if (TogglePaneBtn != null) TogglePaneBtn.Click += (_, _) => { IsOpen = !IsOpen; };
    }

    public ImageGenerationHistoryControl() => DataContext = this;
}