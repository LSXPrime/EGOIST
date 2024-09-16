using EGOIST.Domain.Enums;
using LLama.Native;
using StableDiffusion.NET;

namespace EGOIST.Application.Services.Utilities;

public class BackendService
{
    private static readonly Lazy<BackendService> _instance = new(() => new BackendService());
    public static BackendService Instance => _instance.Value;
    
    public void Initialize()
    {
        // Image Generation Backends
        Backends.CudaBackend.IsEnabled = AppConfig.Instance.Parameters.Device == Device.Cuda;
        Backends.VulkanBackend.IsEnabled = AppConfig.Instance.Parameters.Device == Device.Vulkan;

        // Text Generation Backends
        NativeLibraryConfig.All
            .WithCuda(AppConfig.Instance.Parameters.Device == Device.Cuda)
            .WithVulkan(AppConfig.Instance.Parameters.Device == Device.Vulkan)
            .WithAutoFallback();
    }
}