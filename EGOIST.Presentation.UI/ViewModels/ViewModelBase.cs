using CommunityToolkit.Mvvm.ComponentModel;

namespace EGOIST.Presentation.UI.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public abstract string Title { get; }
}