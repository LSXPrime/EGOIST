using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Domain.Interfaces;
using FluentIcons.Common;

namespace EGOIST.Presentation.UI.Views.Controls;

[TemplatePart("PART_TogglePaneBtn", typeof(Button))]
public class SessionsListControl : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SessionsListControl, string>(nameof(Title), "Sessions");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
     public static readonly StyledProperty<ObservableCollection<ISession<IMessage>>> SessionsProperty =
        AvaloniaProperty.Register<SessionsListControl, ObservableCollection<ISession<IMessage>>>(nameof(Sessions), []);

    public ObservableCollection<ISession<IMessage>> Sessions
    {
        get => GetValue(SessionsProperty);
        set => SetValue(SessionsProperty, value);
    }

    public static readonly StyledProperty<ISession<IMessage>> SelectedSessionProperty =
        AvaloniaProperty.Register<SessionsListControl, ISession<IMessage>>(nameof(SelectedSession));

    public ISession<IMessage> SelectedSession
    {
        get => GetValue(SelectedSessionProperty);
        set => SetValue(SelectedSessionProperty, value);
    }

    public static readonly StyledProperty<RelayCommand> MainActionProperty =
        AvaloniaProperty.Register<SessionsListControl, RelayCommand>(nameof(MainAction));

    public RelayCommand MainAction
    {
        get => GetValue(MainActionProperty);
        set => SetValue(MainActionProperty, value);
    }

    public static readonly StyledProperty<RelayCommand> SubActionProperty =
        AvaloniaProperty.Register<SessionsListControl, RelayCommand>(nameof(SubAction));

    public RelayCommand SubAction
    {
        get => GetValue(SubActionProperty);
        set => SetValue(SubActionProperty, value);
    }

    public static readonly StyledProperty<Symbol> MainActionIconProperty =
        AvaloniaProperty.Register<SessionsListControl, Symbol>(nameof(MainActionIcon), Symbol.Home);

    public Symbol MainActionIcon
    {
        get => GetValue(MainActionIconProperty);
        set => SetValue(MainActionIconProperty, value);
    }

    public static readonly StyledProperty<Symbol> SubActionIconProperty =
        AvaloniaProperty.Register<SessionsListControl, Symbol>(nameof(SubActionIcon), Symbol.Home);

    public Symbol SubActionIcon
    {
        get => GetValue(SubActionIconProperty);
        set => SetValue(SubActionIconProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SessionsListControl, bool>(nameof(IsOpen), defaultValue: false);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private Button? TogglePaneBtn { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        TogglePaneBtn = e.NameScope.Find<Button>("PART_TogglePaneBtn");
        if (TogglePaneBtn != null)
        {
            TogglePaneBtn.Click += TogglePane;
        }
    }

    private void TogglePane(object? sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
    }
}