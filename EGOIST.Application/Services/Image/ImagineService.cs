using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Image;
using EGOIST.Application.Interfaces.Utilities;
using EGOIST.Domain.Entities;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Image;

public class ImagineService(
    ILogger<ImagineService> logger,
    [FromKeyedServices("ImageModelCoreService")] IModelCoreService modelCore,
    IImageHelper imageHelper) : IImageService
{
    private readonly ImageModelCoreService? _imageModelCore = modelCore as ImageModelCoreService;

    /// <summary>
    /// Generates an image from a text prompt using Stable Diffusion.
    /// </summary>
    /// <param name="parameters">Optional parameters for the image generation including prompts, sampling, and other settings.</param>
    /// <returns>A task that completes with the generated image data.</returns>
    public async Task<byte[]> Imagine(ImageGenerationParameters parameters)
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

                return _imageModelCore.Model.TextToImage(parameters.Prompt, param);
            });

        _imageModelCore.State = GenerationState.Finished;

        return stableDiffusionImage != null ? imageHelper.SaveImage(stableDiffusionImage) : [];
    }

    /// <summary>
    /// Inpaints an image based on a prompt and a mask.
    /// </summary>
    /// <param name="parameters">Optional parameters for the image generation including prompts, sampling, and other settings.</param>
    /// <param name="source">The source image data.</param>
    /// <param name="mask">The mask image data, where white pixels indicate the region to inpaint.</param>
    /// <returns>A task that completes with the inpainted image data.</returns>
    public Task<byte[]> Inpaint(ImageGenerationParameters parameters, byte[] source, byte[] mask) => Task.FromResult(Array.Empty<byte>());

    /// <summary>
    /// Transforms an image based on a prompt.
    /// </summary>
    /// <param name="parameters">Optional parameters for the image generation including prompts, sampling, and other settings.</param>
    /// <param name="source">The source image data.</param>
    /// <returns>A task that completes with the transformed image data.</returns>
    public Task<byte[]> Transform(ImageGenerationParameters parameters, byte[] source) => Task.FromResult(Array.Empty<byte>());

    public Task<byte[]> Upscale(int scale, byte[] source) => Task.FromResult(Array.Empty<byte>());
}