using EGOIST.Application.Interfaces.Image;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Image
{
    public class TransformService(ILogger<TransformService> logger) : IImageService
    {
        private readonly GenerationService _generation = GenerationService.Instance;

        public Task<byte[]> Imagine(string prompt, string negative)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> Inpaint(string prompt, string negative, byte[] source, byte[] mask)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        /// <summary>
        /// Transforms an image based on a prompt.
        /// </summary>
        /// <param name="prompt">The text prompt describing the desired image transformation.</param>
        /// <param name="negative">Optional negative prompt to exclude elements from the transformed image.</param>
        /// <param name="source">The source image data.</param>
        /// <returns>A task that completes with the transformed image data.</returns>
        public async Task<byte[]> Transform(string prompt, string negative, byte[] source)
        {
            if (_generation.SelectedGenerationModel == null)
            {
                logger.LogWarning("Text Generation Model isn't loaded yet.");
                return [];
            }

            _generation.State = GenerationState.Started;

            try
            {
                var stableDiffusionImage = await Task.Run(() =>
                {
                    if (_generation.Model != null)
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

                        return _generation.Model.ImageToImage(prompt, source, parameters);
                    }
                    else
                    {
                        return null;
                    }
                });

                _generation.State = GenerationState.Finished;

                return stableDiffusionImage != null ? stableDiffusionImage.Data.ToArray() : [];
            }
            finally
            {
                _generation.State = GenerationState.None;
                _generation.Model?.Dispose();
            }
        }
    }
}