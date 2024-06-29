using EGOIST.Application.Interfaces.Core;
using EGOIST.Application.Interfaces.Image;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Image
{
    public class ImagineService(ILogger<ImagineService> logger, [FromKeyedServices("ImageModelCoreService")] IModelCoreService modelCore) : IImageService
    {
        private readonly ImageModelCoreService? _imageModelCore = modelCore as ImageModelCoreService;

        /// <summary>
        /// Generates an image from a text prompt using Stable Diffusion.
        /// </summary>
        /// <param name="prompt">The text prompt describing the desired image.</param>
        /// <param name="negative">Optional negative prompt to exclude elements from the image.</param>
        /// <returns>A task that completes with the generated image data.</returns>
        public async Task<byte[]> Imagine(string prompt, string negative = "")
        {
            if (_imageModelCore?.SelectedGenerationModel == null)
            {
                logger.LogWarning("Text Generation Model isn't loaded yet.");
                return [];
            }

            _imageModelCore.State = GenerationState.Started;

            try
            {
                var stableDiffusionImage = await Task.Run(() =>
                {
                    if (_imageModelCore.Model != null)
                    {
                        var parameters = new StableDiffusionParameter
                        {
                            NegativePrompt = negative,
                            Width = 512,
                            Height = 512,
                            CfgScale = 7.5f,
                            SampleSteps = 25,
                            Seed = Random.Shared.NextInt64(),
                            SampleMethod = Sampler.Euler_A
                        };

                        return _imageModelCore.Model.TextToImage(prompt, parameters);
                    }
                    else
                    {
                        return null;
                    }
                });

                _imageModelCore.State = GenerationState.Finished;

                return stableDiffusionImage != null ? stableDiffusionImage.Data.ToArray() : [];
            }
            finally
            {
                _imageModelCore.State = GenerationState.None;
                _imageModelCore.Model?.Dispose();
            }
        }

        /// <summary>
        /// Inpaints an image based on a prompt and a mask.
        /// </summary>
        /// <param name="prompt">The text prompt describing the desired inpainting.</param>
        /// <param name="negative">Optional negative prompt to exclude elements from the inpainted region.</param>
        /// <param name="source">The source image data.</param>
        /// <param name="mask">The mask image data, where white pixels indicate the region to inpaint.</param>
        /// <returns>A task that completes with the inpainted image data.</returns>
        public Task<byte[]> Inpaint(string prompt, string negative, byte[] source, byte[] mask)
        {
            // Implement inpainting logic using Stable Diffusion.
            // You can use the StableDiffusionModel.ImageToImage method to achieve this.
            return Task.FromResult(Array.Empty<byte>());
        }

        /// <summary>
        /// Transforms an image based on a prompt.
        /// </summary>
        /// <param name="prompt">The text prompt describing the desired image transformation.</param>
        /// <param name="negative">Optional negative prompt to exclude elements from the transformed image.</param>
        /// <param name="source">The source image data.</param>
        /// <returns>A task that completes with the transformed image data.</returns>
        public Task<byte[]> Transform(string prompt, string negative, byte[] source)
        {
            // Implement image transformation logic using Stable Diffusion.
            // You can use the StableDiffusionModel.ImageToImage method to achieve this.
            return Task.FromResult(Array.Empty<byte>());
        }
    }
}