using EGOIST.Application.DTOs.Management.Models.CivitAI;
using EGOIST.Application.DTOs.Management.Models.HuggingFace;
using EGOIST.Domain.Entities;

namespace EGOIST.Application.Utilities;

public static class RepositoryMapper
{
    public static ModelInfo ToModelInfo(this HuggingFaceModelDto model) => new()
    {
        Name = model.Name,
        Backend = model.Tags.Contains("gguf") ? (model.PipelineTag.Equals("text-generation") || model.Tags.Any(t => t.Contains("text-generation")) ? "llama.cpp" : "whisper.cpp") : model.Library,
        Type = model.PipelineTag.HuggingFaceGetTypeAndTask().Type,
        Task = model.PipelineTag.HuggingFaceGetTypeAndTask().Task,
        Architecture = string.Empty,
        Parameters = string.Empty,
        UpdateDate = model.CreatedAt.ToString(),
        Description = string.Empty,
        Weights = new(model.Weights.Select(w => new ModelInfoWeight
        {
            Extension = Path.GetExtension(w.Name),
            Weight = w.Name.GetModelWeight(),
            Link = $"https://huggingface.co/{model.Name}/resolve/main/{w.Name}",
            Size = new HttpClient().GetAsync("fileUrl").Result.Content.Headers.ContentLength ?? 0
        }).ToList()),
        DownloadRepo = $"https://huggingface.co/{model.Name}",
    };

    public static ModelInfo ToModelInfo(this ModelVersion version) => new()
    {
        Name = version.Name,
        Backend = "stable-diffusion.cpp",
        Type = "Image",
        Task = version.BaseModel.Equals("Inpainting") ? "Inpaint" : "Generate",
        Architecture = version.BaseModel,
        Parameters = "",
        UpdateDate = version.PublishedAt.ToString(),
        Description = version.Description,
        Weights = new(version.Files.Select(w => new ModelInfoWeight
        {
            Extension = Path.GetExtension(w.Name),
            Weight = w.Name.GetModelWeight(),
            Link = w.DownloadUrl,
            Size = (long)(w.SizeKB * 1024)
        }).ToList()),
        DownloadRepo = $"https://civitai.com/model-versions/{version.Id}"
    };

    public static IEnumerable<ModelInfo> ToModelsInfo(this IEnumerable<CivitAIModelDTO> models) => models.SelectMany(model => model.Items.SelectMany(item => item.ModelVersions.Select(version => new ModelInfo
    {
        Name = item.Name + " - " + version.Name,
        Backend = "stable-diffusion.cpp",
        Type = "Image",
        Task = version.BaseModel.Equals("Inpainting") ? "Inpaint" : "Generate",
        Architecture = version.BaseModel,
        Parameters = "",
        UpdateDate = version.PublishedAt.ToString(),
        Description = item.Description,
        Weights = new(version.Files.Select(w => new ModelInfoWeight
        {
            Extension = Path.GetExtension(w.Name),
            Weight = w.Name.GetModelWeight(),
            Link = w.DownloadUrl,
            Size = (long)(w.SizeKB * 1024)
        }).ToList()),
        DownloadRepo = $"https://civitai.com/models/{item.Id}"
    })));
}
