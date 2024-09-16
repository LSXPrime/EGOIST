using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using EGOIST.Presentation.UI.Services;

namespace EGOIST.Presentation.UI.Views.Controls;

[TemplatePart("PART_TogglePaneBtn", typeof(Button))]
public class ImageGenerationOptionsControl : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ImageGenerationOptionsControl, string>(nameof(Title), "Options");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ImageGenerationOptionsControl, bool>(nameof(IsOpen), defaultValue: true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<ImageGenerationParameters> ParametersProperty =
        AvaloniaProperty.Register<ImageGenerationOptionsControl, ImageGenerationParameters>(nameof(Parameters),
            new ImageGenerationParameters());

    public ImageGenerationParameters Parameters
    {
        get => GetValue(ParametersProperty);
        set
        {
            SetValue(ParametersProperty, value);
            Sampler = value.Sampler.ToString();
        }
    }

    
    public string Sampler
    {
        get => Parameters.Sampler.ToString();
        set => Parameters.Sampler = (ImageGenerationSampler)Enum.Parse(typeof(ImageGenerationSampler), value);
    }
    
    public static readonly StyledProperty<bool> RandomizedSeedProperty =
        AvaloniaProperty.Register<ImageGenerationOptionsControl, bool>(nameof(RandomizedSeed), defaultValue: false);

    public bool RandomizedSeed
    {
        get => GetValue(RandomizedSeedProperty);
        set => SetValue(RandomizedSeedProperty, value);
    }

    private Button? TogglePaneBtn { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        TogglePaneBtn = e.NameScope.Find<Button>("PART_TogglePaneBtn");
        if (TogglePaneBtn != null) TogglePaneBtn.Click += (_, _) => { IsOpen = !IsOpen; };
        var controlNetImageBorder = e.NameScope.Find<Border>("ControlNetImageBorder");
        controlNetImageBorder?.AddHandler(DragDrop.DropEvent, ContrControlNetDropImage);
        var randomizedSeedBtn = e.NameScope.Find<ToggleButton>("RandomizedSeedBtn");
        if (randomizedSeedBtn != null) randomizedSeedBtn.IsCheckedChanged += ToggleRandomSeed;
    }

    public ImageGenerationOptionsControl()
    {
        DataContext = this;
        _fileSystemService = Design.IsDesignMode ? null! : Ioc.Default.GetRequiredService<IFileSystemService>();
    //    RandomizeSeed = new RelayCommand(() => Parameters.Seed = new Random().Next());
        ControlNetLoadImage = new RelayCommand(() => Task.Run(async () =>
        {
            var path = (await DialogService.OpenFileDialogAsync(false, new List<FilePickerFileType>
            {
                new("Images")
                {
                    Patterns = new List<string>
                        { "*.bmp", "*.gif", "*.jpeg", "*.jpg", "*.pbm", "*.png", "*.tiff", "*.tga", "*.webp" },
                    AppleUniformTypeIdentifiers = new List<string>
                        { "public.bmp", "public.gif", "public.jpeg", "public.png", "public.tiff" },
                    MimeTypes = new List<string>
                        { "image/bmp", "image/gif", "image/jpeg", "image/png", "image/tiff", "image/tga", "image/webp" }
                },
            })).FirstOrDefault();

            if (!string.IsNullOrEmpty(path))
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    Parameters.ControlNetParameters.ControlNetImage =
                        await _fileSystemService.ReadAllBytesAsync(path);
                });
        }));

        ControlNetDeleteImage = new RelayCommand(() => Parameters.ControlNetParameters.ControlNetImage = null);
    }

    private readonly IFileSystemService _fileSystemService;
    public RelayCommand ControlNetLoadImage { get; }
    public RelayCommand ControlNetDeleteImage { get; }

    private void ContrControlNetDropImage(object? sender, DragEventArgs e)
    {
        var draggedFile = e.Data.GetFiles()!.FirstOrDefault();
        if (draggedFile != null)
        {
            Parameters.ControlNetParameters.ControlNetImage =
                _fileSystemService.ReadAllBytes(draggedFile.Path.AbsolutePath);
        }
    }

    private void ToggleRandomSeed(object? sender, RoutedEventArgs e)
    {
        RandomizedSeed = !RandomizedSeed;
        Parameters.Seed = RandomizedSeed ? -1 : 0;

    }
}