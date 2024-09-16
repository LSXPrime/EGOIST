using EGOIST.Domain.Entities;

namespace EGOIST.Application.Interfaces.Image
{
    /// <summary>
    /// Defines the interface for an image service, providing methods for image generation, inpainting, and transformation.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Generates an image from a text prompt.
        /// </summary>
        /// <param name="parameters">Optional parameters for the image generation including prompts, sampling, and other settings.</param>
        /// <returns>A task that completes with the generated image data.</returns>
        Task<byte[]> Imagine(ImageGenerationParameters parameters);

        /// <summary>
        /// Inpaints an image based on a prompt and a mask.
        /// </summary>
        /// <param name="parameters">Optional parameters for the image generation including prompts, sampling, and other settings.</param>
        /// <param name="source">The source image data.</param>
        /// <param name="mask">The mask image data, where white pixels indicate the region to inpaint.</param>
        /// <returns>A task that completes with the inpainted image data.</returns>
        Task<byte[]> Inpaint(ImageGenerationParameters parameters, byte[] source, byte[] mask);

        /// <summary>
        /// Transforms an image based on a prompt.
        /// </summary>
        /// <param name="parameters">Optional parameters for the image generation including prompts, sampling, and other settings.</param>
        /// <param name="source">The source image data.</param>
        /// <returns>A task that completes with the transformed image data.</returns>
        Task<byte[]> Transform(ImageGenerationParameters parameters, byte[] source);

        /// <summary>
        /// Upscale an image
        /// </summary>
        /// <param name="scale">The desired scale of the image</param>
        /// <param name="source">The source image data.</param>
        /// <returns>A task that completes with the upscaled image data.</returns>
        Task<byte[]> Upscale(int scale, byte[] source);
    }
}