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
        /// <param name="prompt">The text prompt describing the desired image.</param>
        /// <param name="negative">Optional negative prompt to exclude elements from the image.</param>
        /// <returns>A task that completes with the generated image data.</returns>
        Task<byte[]> Imagine(string prompt, string negative = "");

        /// <summary>
        /// Inpaints an image based on a prompt and a mask.
        /// </summary>
        /// <param name="prompt">The text prompt describing the desired inpainting.</param>
        /// <param name="negative">Optional negative prompt to exclude elements from the inpainted region.</param>
        /// <param name="source">The source image data.</param>
        /// <param name="mask">The mask image data, where white pixels indicate the region to inpaint.</param>
        /// <returns>A task that completes with the inpainted image data.</returns>
        Task<byte[]> Inpaint(string prompt, string negative, byte[] source, byte[] mask);

        /// <summary>
        /// Transforms an image based on a prompt.
        /// </summary>
        /// <param name="prompt">The text prompt describing the desired image transformation.</param>
        /// <param name="negative">Optional negative prompt to exclude elements from the transformed image.</param>
        /// <param name="source">The source image data.</param>
        /// <returns>A task that completes with the transformed image data.</returns>
        Task<byte[]> Transform(string prompt, string negative, byte[] source);
    }
}