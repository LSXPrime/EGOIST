using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Domain.Entities;

namespace EGOIST.Presentation.UI.Views.Controls;

[TemplatePart("PART_TogglePaneBtn", typeof(Button))]
public class SessionOptionsControl : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SessionOptionsControl, string>(nameof(Title), "Options");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsOpen), defaultValue: false);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<TextGenerationParameters> ParametersProperty =
        AvaloniaProperty.Register<SessionOptionsControl, TextGenerationParameters>(nameof(Parameters));

    public TextGenerationParameters Parameters
    {
        get => GetValue(ParametersProperty);
        set => SetValue(ParametersProperty, value);
    }

    public static readonly StyledProperty<TextPromptParameters> PromptFormatProperty =
        AvaloniaProperty.Register<SessionOptionsControl, TextPromptParameters>(nameof(PromptFormat));

    public TextPromptParameters PromptFormat
    {
        get => GetValue(PromptFormatProperty);
        set => SetValue(PromptFormatProperty, value);
    }
    
    public TextPromptParameters? PromptFormatBinding
    {
        get => PromptFormat;
        set
        {
            if (value == null)
                return;
            
            PromptFormat.Name = value.Name;
            PromptFormat.Content = value.Content;
            PromptFormat.PromptPrefix = value.PromptPrefix;
            PromptFormat.PromptSuffix = value.PromptSuffix;
            PromptFormat.SystemPrefix = value.SystemPrefix;
            PromptFormat.SystemSuffix = value.SystemSuffix;
            PromptFormat.SystemPrompt = value.SystemPrompt;
            PromptFormat.BlackList = value.BlackList;
        }
    }


    public static readonly StyledProperty<TextModelParameters> ModelParametersProperty =
        AvaloniaProperty.Register<SessionOptionsControl, TextModelParameters>(nameof(ModelParameters));

    public TextModelParameters ModelParameters
    {
        get => GetValue(ModelParametersProperty);
        set => SetValue(ModelParametersProperty, value);
    }
    
    public static readonly StyledProperty<object> PromptTemplatesProperty =
        AvaloniaProperty.Register<SessionsListControl, object>(nameof(PromptTemplates));

    public object PromptTemplates
    {
        get => GetValue(PromptTemplatesProperty);
        set => SetValue(PromptTemplatesProperty, value);
    }
    
    public static readonly StyledProperty<RelayCommand> SavePromptProperty =
        AvaloniaProperty.Register<SessionsListControl, RelayCommand>(nameof(SavePrompt));

    public RelayCommand SavePrompt
    {
        get => GetValue(SavePromptProperty);
        set => SetValue(SavePromptProperty, value);
    }

    private Button? TogglePaneBtn { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        TogglePaneBtn = e.NameScope.Find<Button>("PART_TogglePaneBtn");
        if (TogglePaneBtn != null)
        {
            TogglePaneBtn.Click += (_, _) => { IsOpen = !IsOpen; };
        }
    }
    
    public SessionOptionsControl()
    {
        DataContext = this;
    }
}
