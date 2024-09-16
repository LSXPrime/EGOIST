using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Presentation.UI.ViewModels.Pages.Image;

namespace EGOIST.Presentation.UI.Views.Pages.Image;

public partial class TransformPageView : UserControl
{
    private readonly IFileSystemService _fileSystemService;
    private TransformPageViewModel ViewModel => (DataContext as TransformPageViewModel)!;

    public TransformPageView(TransformPageViewModel viewModel, IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
        SourceImageBorder.AddHandler(DragDrop.DropEvent, SourceImageDrop);
    }

    public TransformPageView()
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