using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Image;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EGOIST.Application.Services.Image;

public class UpscaleService(
    ILogger<UpscaleService> logger,
    IImageHelper imageHelper,
    [FromKeyedServices("ImageModelCoreService")] IModelCoreService modelCore) : IImageService
{
    private readonly ImageModelCoreService? _imageModelCore = modelCore as ImageModelCoreService;
    
    public Task<byte[]> Imagine(ImageGenerationParameters parameters) => Task.FromResult(Array.Empty<byte>());

    public Task<byte[]> Inpaint(ImageGenerationParameters parameters, byte[] source, byte[] mask) => Task.FromResult(Array.Empty<byte>());

    public Task<byte[]> Transform(ImageGenerationParameters parameters, byte[] source) => Task.FromResult(Array.Empty<byte>());

    public async Task<byte[]> Upscale(int scale, byte[] source)
    {
        if (_imageModelCore?.UpscaleModel == null)
        {
            logger.LogWarning("Image Upscale Model isn't loaded yet.");
            return [];
        }
        
        _imageModelCore.State = GenerationState.Started;
        
        var upscaledImage = _imageModelCore.UpscaleModel is null
            ? null
            : await Task.Run(() => _imageModelCore.UpscaleModel.Upscale(imageHelper.LoadImage(source), scale));


        _imageModelCore.State = GenerationState.Finished;

        return upscaledImage != null ? imageHelper.SaveImage(upscaledImage) : [];
    }
}