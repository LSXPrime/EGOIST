using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Presentation.UI.ViewModels.Pages.Image;

namespace EGOIST.Presentation.UI.Views.Pages.Image;

public partial class UpscalePageView : UserControl
{
    private readonly IFileSystemService _fileSystemService;
    private UpscalePageViewModel ViewModel => (DataContext as UpscalePageViewModel)!;

    public UpscalePageView(UpscalePageViewModel viewModel, IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
        SourceImageBorder.AddHandler(DragDrop.DropEvent, SourceImageDrop);
    }

    public UpscalePageView()
    {
        InitializeComponent();
    }

    private void SourceImageDrop(object? sender, DragEventArgs e)
    {
        var draggedFile = e.Data.GetFiles()!.FirstOrDefault();
        if (draggedFile != null)
            ViewModel.SourceImage = _fileSystemService.ReadAllBytes(draggedFile.Path.AbsolutePath);
    }
}