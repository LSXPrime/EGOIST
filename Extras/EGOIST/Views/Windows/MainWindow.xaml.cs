using EGOIST.ViewModels.Windows;
using EGOIST.Data;
using Wpf.Ui.Controls;
using EGOIST.Helpers;

namespace EGOIST.Views.Windows;
public partial class MainWindow
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService, IServiceProvider serviceProvider, ISnackbarService snackbarService, IContentDialogService contentDialogService)
    {
        AppConfig.Instance.LoadSelf();
        Wpf.Ui.Appearance.Watcher.Watch(this);

        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        navigationService.SetNavigationControl(NavigationView);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        contentDialogService.SetContentPresenter(RootContentDialog);

        NavigationView.SetServiceProvider(serviceProvider);
        Extensions.SnackbarArea = SnackbarPresenter;
        Extensions.ContentArea = RootContentDialog;
    }
}
