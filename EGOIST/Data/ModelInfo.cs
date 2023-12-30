using Newtonsoft.Json;
using EGOIST.Helpers;
using NetFabric.Hyperlinq;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace EGOIST.Data;

public class ModelInfo
{
    public string Name { get; set; }
    public string Backend { get; set; }
    public string Type { get; set; }
    public string Task { get; set; }
    public string Architecture { get; set; }
    public string UpdateDate { get; set; }
    public string Description { get; set; }
    public string Checkpoint { get; set; }
    public List<ModelInfoWeight> Weights { get; set; }
    public string DownloadRepo { get; set; }
    public TextConfiguration TextConfig { get; set; } 
    public List<ModelInfoWeight> Downloaded => Weights.AsValueEnumerable().Where(x => System.IO.File.Exists($"{AppConfig.Instance.ModelsPath}/{Type.RemoveSpaces()}/{Name.RemoveSpaces()}/{x.Weight.RemoveSpaces()}.{x.Extension.RemoveSpaces()}")).ToList();

    public class ModelInfoWeight
    {
        public string Extension{ get; set; }
        public string Weight { get; set; }
        public string Link { get; set; }
        public string Size { get; set; }
    }

    public class TextConfiguration
    {
        public string PromptPrefix { get; set; }
        public string PromptSuffix { get; set; }
        public string SystemPrefix { get; set; }
        public string SystemSuffix { get; set; }
        public string SystemPrompt { get; set; }
        public List<string> BlackList { get; set; }

        public string Prompt(string prompt)
        {
            return $"{SystemPrefix}{SystemPrompt}{SystemSuffix}{PromptPrefix}{prompt}{PromptSuffix}";
        }

        public string Prompt(string prompt, string system)
        {
            return $"{SystemPrefix}{system}{SystemSuffix}{PromptPrefix}{prompt}{PromptSuffix}";
        }

        public async IAsyncEnumerable<string> Filter(IAsyncEnumerable<string> tokens)
        {
            await foreach (string token in tokens)
            {
                string current = string.Join("", token);
                if (BlackList.Any(current.Contains))
                {
                    string blackToken = BlackList.First(current.Contains);
                    yield return current.Replace(blackToken, " ");
                }
            }
        }
    }

    public static ModelInfo FromJson(string json)
    {
        return JsonConvert.DeserializeObject<ModelInfo>(json);
    }

    public static string ToJson(ModelInfo modelInfo)
    {
        return JsonConvert.SerializeObject(modelInfo, Formatting.Indented);
    }
}
