using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Domain.Interfaces;
using EGOIST.Presentation.UI.Interfaces.Interactions;
using EGOIST.Presentation.UI.Services;
using FluentIcons.Common;

namespace EGOIST.Presentation.UI.Views.Controls;

[TemplatePart("PART_TogglePaneBtn", typeof(Button))]
[TemplatePart("PART_SessionsList", typeof(ListBox))]
public class SessionsListControl : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SessionsListControl, string>(nameof(Title), "Sessions");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<object> SessionsProperty =
        AvaloniaProperty.Register<SessionsListControl, object>(nameof(Sessions));

    public object Sessions
    {
        get => GetValue(SessionsProperty);
        set => SetValue(SessionsProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> MainActionProperty =
        AvaloniaProperty.Register<SessionsListControl, IRelayCommand>(nameof(MainAction));

    public IRelayCommand MainAction
    {
        get => GetValue(MainActionProperty);
        set => SetValue(MainActionProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> SubActionProperty =
        AvaloniaProperty.Register<SessionsListControl, IRelayCommand>(nameof(SubAction));

    public IRelayCommand SubAction
    {
        get => GetValue(SubActionProperty);
        set => SetValue(SubActionProperty, value);
    }

    public static readonly StyledProperty<IRelayCommand> ThirdActionProperty =
        AvaloniaProperty.Register<SessionsListControl, IRelayCommand>(nameof(ThirdAction));

    public IRelayCommand ThirdAction
    {
        get => GetValue(ThirdActionProperty);
        set => SetValue(ThirdActionProperty, value);
    }

    public static readonly StyledProperty<Symbol> MainActionIconProperty =
        AvaloniaProperty.Register<SessionsListControl, Symbol>(nameof(MainActionIcon), Symbol.Add);

    public Symbol MainActionIcon
    {
        get => GetValue(MainActionIconProperty);
        set => SetValue(MainActionIconProperty, value);
    }

    public static readonly StyledProperty<Symbol> SubActionIconProperty =
        AvaloniaProperty.Register<SessionsListControl, Symbol>(nameof(SubActionIcon), Symbol.Delete);

    public Symbol SubActionIcon
    {
        get => GetValue(SubActionIconProperty);
        set => SetValue(SubActionIconProperty, value);
    }

    public static readonly StyledProperty<Symbol> ThirdActionIconProperty =
        AvaloniaProperty.Register<SessionsListControl, Symbol>(nameof(ThirdActionIcon), Symbol.Stop);

    public Symbol ThirdActionIcon
    {
        get => GetValue(ThirdActionIconProperty);
        set => SetValue(ThirdActionIconProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<SessionsListControl, bool>(nameof(IsOpen), defaultValue: true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsSessionSelectedProperty =
        AvaloniaProperty.Register<SessionsListControl, bool>(nameof(IsSessionSelected), defaultValue: false);

    public bool IsSessionSelected
    {
        get => GetValue(IsSessionSelectedProperty);
        set => SetValue(IsSessionSelectedProperty, value);
    }

    private Button? TogglePaneBtn { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        TogglePaneBtn = e.NameScope.Find<Button>("PART_TogglePaneBtn");
        if (TogglePaneBtn != null)
        {
            TogglePaneBtn.Click += (_, _) => { IsOpen = !IsOpen; };
        }

        var sessionList = e.NameScope.Find<ListBox>("PART_SessionsList");
        if (sessionList != null)
        {
            sessionList.SelectionChanged += (_, _) =>
            {
                /*
                if (sessionList.SelectedItem == null && _selectedSession != null && Sessions is IEnumerable<ISession> sessions)
                {
                    sessions = sessions.ToArray();
                    var session = sessions.FirstOrDefault(x => x.Name == _selectedSession.Name);
                    if (session != null)
                    {
                        sessionList.SelectedItem = session;
                        return;
                    }
                }
                */

                Dispatcher.UIThread.Post(() => IsSessionSelected = sessionList.SelectedItem is ISession);
                
                if (NavigationService.Current.Sub is ITextViewModel textViewModel)
                    textViewModel.Select(sessionList.SelectedItem! as ISession);

                /*
                await Dispatcher.UIThread.InvokeAsync(
                    () => { SelectedSession = (ISession)sessionList.SelectedItem!; });
                */
            };
        }

        base.OnApplyTemplate(e);
    }
}