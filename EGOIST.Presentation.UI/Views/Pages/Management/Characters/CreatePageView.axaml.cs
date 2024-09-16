using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Presentation.UI.ViewModels.Pages.Management.Characters;

namespace EGOIST.Presentation.UI.Views.Pages.Management.Characters;

public partial class CreatePageView : UserControl
{
    private readonly IFileSystemService _fileSystemService;
    private CreatePageViewModel ViewModel => (DataContext as CreatePageViewModel)!;
    
    public CreatePageView(CreatePageViewModel viewModel, IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        Dispatcher.UIThread.Invoke(() => DataContext = viewModel);
        InitializeComponent();
        AvatarImageBorder.AddHandler(DragDrop.DropEvent, SourceImageDrop);
    }

    public CreatePageView() => InitializeComponent();

    private void SourceImageDrop(object? sender, DragEventArgs e)
    {
        var draggedFile = e.Data.GetFiles()!.FirstOrDefault();
        if (draggedFile != null)
            ViewModel.Avatar = _fileSystemService.ReadAllBytes(draggedFile.Path.AbsolutePath);
    }
}