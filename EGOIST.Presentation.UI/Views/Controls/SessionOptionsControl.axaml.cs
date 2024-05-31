using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
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

    public static readonly StyledProperty<TextModelParameters> ModelParametersProperty =
        AvaloniaProperty.Register<SessionOptionsControl, TextModelParameters>(nameof(ModelParameters));

    public TextModelParameters ModelParameters
    {
        get => GetValue(ModelParametersProperty);
        set => SetValue(ModelParametersProperty, value);
    }

    private Button? TogglePaneBtn { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        TogglePaneBtn = e.NameScope.Find<Button>("PART_TogglePaneBtn");
        if (TogglePaneBtn != null)
        {
            TogglePaneBtn.Click += (sender, args) => { IsOpen = !IsOpen; };
        }
    }
    
    public SessionOptionsControl()
    {
        DataContext = this;
    }
}
