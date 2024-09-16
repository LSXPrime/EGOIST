using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Image;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Image;

public class TransformService(
    ILogger<ImagineService> logger,
    IImageHelper imageHelper,
    [FromKeyedServices("ImageModelCoreService")] IModelCoreService modelCore) : IImageService
{
    private readonly ImageModelCoreService? _imageModelCore = modelCore as ImageModelCoreService;

    public Task<byte[]> Imagine(ImageGenerationParameters parameters) => Task.FromResult(Array.Empty<byte>());

    public Task<byte[]> Inpaint(ImageGenerationParameters parameters, byte[] source, byte[] mask) => Task.FromResult(Array.Empty<byte>());

    /// <summary>
    /// Transforms an image based on a prompt.
    /// </summary>
    /// <param name="parameters">Optional parameters for the image generation including prompts, sampling, and other settings.</param>
    /// <param name="source">The source image data.</param>
    /// <returns>A task that completes with the transformed image data.</returns>
    public async Task<byte[]> Transform(ImageGenerationParameters parameters, byte[] source)
    {
        if (_imageModelCore?.SelectedGenerationModel == null)
        {
            logger.LogWarning("Text Generation Model isn't loaded yet.");
            return [];
        }

        _imageModelCore.State = GenerationState.Started;

        var stableDiffusionImage = _imageModelCore.Model is null
            ? null
            : await Task.Run(() =>
            {
                var param = new DiffusionParameter
                {
                    NegativePrompt = parameters.NegativePrompt,
                    Width = parameters.Width,
                    Height = parameters.Height,
                    CfgScale = parameters.CfgScale,
                    SampleSteps = parameters.Steps,
                    Seed = parameters.Seed,
                    SampleMethod = (Sampler)(short)parameters.Sampler,
                    ClipSkip = parameters.ClipSkip,
                    Strength = parameters.Strength
                };
                
                if (parameters.ControlNetParameters is { IsEnabled: true, ControlNetImage: not null })
                {
                    param.ControlNet.Image =
                        imageHelper.LoadImage(parameters.ControlNetParameters.ControlNetImage);
                    param.ControlNet.Strength = parameters.ControlNetParameters.ControlNetStrength;
                }
                
                return _imageModelCore.Model.ImageToImage(parameters.Prompt, imageHelper.LoadImage(source),
                    param);
            });


        _imageModelCore.State = GenerationState.Finished;

        return stableDiffusionImage != null ? imageHelper.SaveImage(stableDiffusionImage) : [];
    }

    public Task<byte[]> Upscale(int scale, byte[] source) => Task.FromResult(Array.Empty<byte>());
}