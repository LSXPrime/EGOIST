using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Domain.Entities;

namespace EGOIST.Presentation.UI.Views.Controls;

[TemplatePart("PART_TogglePaneBtn", typeof(Button))]
[TemplatePart("PART_UnloadMemoryModelBtn", typeof(Button))]
[TemplatePart("PART_MemoryModelComboBox", typeof(ComboBox))]
[TemplatePart("PART_MemoryModelWeightComboBox", typeof(ComboBox))]
[TemplatePart("PART_MemoryModelWeightContainer", typeof(StackPanel))]
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
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsOpen), defaultValue: true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsParametersVisibleProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsParametersVisible), defaultValue: true);

    public bool IsParametersVisible
    {
        get => GetValue(IsParametersVisibleProperty);
        set => SetValue(IsParametersVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsParametersExpandedProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsParametersExpanded), defaultValue: false);

    public bool IsParametersExpanded
    {
        get => GetValue(IsParametersExpandedProperty);
        set => SetValue(IsParametersExpandedProperty, value);
    }


    public static readonly StyledProperty<bool> IsPromptVisibleProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsPromptVisible), defaultValue: true);

    public bool IsPromptVisible
    {
        get => GetValue(IsPromptVisibleProperty);
        set => SetValue(IsPromptVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsPromptExpandedProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsPromptExpanded), defaultValue: false);

    public bool IsPromptExpanded
    {
        get => GetValue(IsPromptExpandedProperty);
        set => SetValue(IsPromptExpandedProperty, value);
    }


    public static readonly StyledProperty<bool> IsModelVisibleProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsModelVisible), defaultValue: true);

    public bool IsModelVisible
    {
        get => GetValue(IsModelVisibleProperty);
        set => SetValue(IsModelVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsModelExpandedProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsModelExpanded), defaultValue: false);

    public bool IsModelExpanded
    {
        get => GetValue(IsModelExpandedProperty);
        set => SetValue(IsModelExpandedProperty, value);
    }

    public static readonly StyledProperty<bool> IsMemoryVisibleProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsMemoryVisible), defaultValue: false);

    public bool IsMemoryVisible
    {
        get => GetValue(IsMemoryVisibleProperty);
        set => SetValue(IsMemoryVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsMemoryExpandedProperty =
        AvaloniaProperty.Register<SessionOptionsControl, bool>(nameof(IsMemoryExpanded), defaultValue: false);

    public bool IsMemoryExpanded
    {
        get => GetValue(IsMemoryExpandedProperty);
        set => SetValue(IsMemoryExpandedProperty, value);
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

    public static readonly StyledProperty<IRelayCommand> SavePromptProperty =
        AvaloniaProperty.Register<SessionsListControl, IRelayCommand>(nameof(SavePrompt));

    public IRelayCommand SavePrompt
    {
        get => GetValue(SavePromptProperty);
        set => SetValue(SavePromptProperty, value);
    }


    public static readonly StyledProperty<bool> MemoryEnabledProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, bool>(nameof(MemoryEnabled),
            defaultBindingMode: BindingMode.TwoWay);

    public bool MemoryEnabled
    {
        get => GetValue(MemoryEnabledProperty);
        set => SetValue(MemoryEnabledProperty, value);
    }

    public static readonly StyledProperty<int> MemoryChunkSizeProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, int>(nameof(MemoryChunkSize),
            defaultBindingMode: BindingMode.TwoWay);

    public int MemoryChunkSize
    {
        get => GetValue(MemoryChunkSizeProperty);
        set => SetValue(MemoryChunkSizeProperty, value);
    }

    public static readonly StyledProperty<int> MemoryOverlappingProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, int>(nameof(MemoryOverlapping),
            defaultBindingMode: BindingMode.TwoWay);

    public int MemoryOverlapping
    {
        get => GetValue(MemoryOverlappingProperty);
        set => SetValue(MemoryOverlappingProperty, value);
    }

    public static readonly StyledProperty<string> MemoryChunkSeparatorProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, string>(nameof(MemoryChunkSeparator),
            defaultBindingMode: BindingMode.TwoWay);

    public string MemoryChunkSeparator
    {
        get => GetValue(MemoryChunkSeparatorProperty);
        set => SetValue(MemoryChunkSeparatorProperty, value);
    }

    public static readonly StyledProperty<object> MemoryModelsProperty =
        AvaloniaProperty.Register<MessageInputOptionsControl, object>(nameof(MemoryModels));

    public object MemoryModels
    {
        get => GetValue(MemoryModelsProperty);
        set => SetValue(MemoryModelsProperty, value);
    }


    public static readonly StyledProperty<IRelayCommand> LoadMemoryModelCommandProperty =
        AvaloniaProperty.Register<SessionsListControl, IRelayCommand>(nameof(LoadMemoryModelCommand));

    public IRelayCommand LoadMemoryModelCommand
    {
        get => GetValue(LoadMemoryModelCommandProperty);
        set => SetValue(LoadMemoryModelCommandProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> DisposeMemoryModelCommandProperty =
        AvaloniaProperty.Register<SessionsListControl, IRelayCommand>(nameof(DisposeMemoryModelCommand));

    public IRelayCommand DisposeMemoryModelCommand
    {
        get => GetValue(DisposeMemoryModelCommandProperty);
        set => SetValue(DisposeMemoryModelCommandProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> RefreshMemoryModelsCommandProperty =
        AvaloniaProperty.Register<SessionsListControl, IRelayCommand>(nameof(RefreshMemoryModelsCommand));

    public IRelayCommand RefreshMemoryModelsCommand
    {
        get => GetValue(RefreshMemoryModelsCommandProperty);
        set => SetValue(RefreshMemoryModelsCommandProperty, value);
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var togglePaneBtn = e.NameScope.Find<Button>("PART_TogglePaneBtn");
        var unloadMemoryModelBtn = e.NameScope.Find<Button>("PART_UnloadMemoryModelBtn");
        var models = e.NameScope.Find<ComboBox>("PART_MemoryModelComboBox");
        var modelWeights = e.NameScope.Find<ComboBox>("PART_MemoryModelWeightComboBox");
        var modelWeightContainer = e.NameScope.Find<StackPanel>("PART_MemoryModelWeightContainer");

        if (togglePaneBtn != null) togglePaneBtn.Click += (_, _) => { IsOpen = !IsOpen; };

        if (unloadMemoryModelBtn != null)
            unloadMemoryModelBtn.Click += (_, _) =>
            {
                if (models != null)
                    models.SelectedItem = null;
                if (modelWeights != null)
                    modelWeights.SelectedItem = null;

                DisposeMemoryModelCommand.Execute(null);
            };

        if (models != null)
            models.SelectionChanged += (_, _) =>
            {
                if (modelWeightContainer != null)
                    modelWeightContainer.IsVisible = models.SelectedItem is ModelInfo { Weights.Count: > 0 };
                
                if (modelWeights == null) return;
                if (modelWeights.SelectedItem != null)
                    DisposeMemoryModelCommand.Execute(null);

                modelWeights.SelectedItem = null;
            };

        if (modelWeights != null)
            modelWeights.SelectionChanged += (_, _) =>
            {
                if (unloadMemoryModelBtn != null)
                    unloadMemoryModelBtn.IsVisible = modelWeights.SelectedItem != null;

                if (models is { SelectedItem: ModelInfo model } &&
                    modelWeights is { SelectedItem: ModelInfoWeight weight })
                    LoadMemoryModelCommand.Execute((model, weight));
            };
    }

    public SessionOptionsControl()
    {
        DataContext = this;
    }
}