using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using EGOIST.Presentation.UI.ViewModels.Pages;

namespace EGOIST.Presentation.UI.Views.Pages;

public partial class TextPageView : UserControl
{
    public TextPageView()
    {
//        DataContext = Ioc.Default.GetService<TextPageViewModel>();
        InitializeComponent();
    }
}