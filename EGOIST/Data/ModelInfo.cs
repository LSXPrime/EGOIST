using Newtonsoft.Json;
using EGOIST.Helpers;
using NetFabric.Hyperlinq;
using System.ComponentModel;

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
    public List<ModelInfoWeight> Weights { get; set; }
    public string DownloadRepo { get; set; }
    public TextConfiguration TextConfig { get; set; } 
    public List<ModelInfoWeight> Downloaded => Weights.AsValueEnumerable().Where(x => System.IO.File.Exists($"{AppConfig.Instance.ModelsPath}/{Type.RemoveSpaces()}/{Name.RemoveSpaces()}/{x.Weight.RemoveSpaces()}.{x.Extension.ToLower().RemoveSpaces()}")).ToList();

    public class ModelInfoWeight
    {
        public string Extension{ get; set; }
        public string Weight { get; set; }
        public string Link { get; set; }
        public string Size { get; set; }
    }

    public class TextConfiguration : INotifyPropertyChanged
    {
        public string PromptPrefix { get; set; }
        public string PromptSuffix { get; set; }
        public string SystemPrefix { get; set; }
        public string SystemSuffix { get; set; }
        public string SystemPrompt { get; set; }
        public List<string> BlackList { get; set; }


        public string Prompt(string prompt) => $"{SystemPrefix}{SystemPrompt}{SystemSuffix}{PromptPrefix}{prompt}{PromptSuffix}";

        public string Prompt(string prompt, string system) => $"{SystemPrefix}{system}{SystemSuffix}{PromptPrefix}{prompt}{PromptSuffix}";
        public string Prompt(string prompt, string system, string prefix, string suffix) => $"{SystemPrefix}{(string.IsNullOrEmpty(system) ? SystemPrompt : system) }{SystemSuffix}{(string.IsNullOrEmpty(prefix) ? PromptPrefix : prefix)}{prompt}{(string.IsNullOrEmpty(suffix) ? PromptSuffix : suffix)}";
        public string Prompt(string prompt, TextConfiguration configuration) => $"{SystemPrefix}{(string.IsNullOrEmpty(configuration.SystemPrompt) ? SystemPrompt : configuration.SystemPrompt)}{SystemSuffix}{(string.IsNullOrEmpty(configuration.PromptPrefix) ? PromptPrefix : configuration.PromptPrefix)}{prompt}{(string.IsNullOrEmpty(configuration.PromptSuffix) ? PromptSuffix : configuration.PromptSuffix)}";
        // TODO: Filtering not working, It's not returning anything
        public async IAsyncEnumerable<string> Filter(IAsyncEnumerable<string> tokens, CancellationToken cancellationToken)
        {
            HashSet<string> blackSet = new(BlackList);

            await foreach (string token in tokens.WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                string current = token;
                bool containsBlacklisted = false;

                foreach (string blackToken in blackSet)
                {
                    if (current.Contains(blackToken))
                    {
                        containsBlacklisted = true;
                        current = current.Replace(blackToken, " ");
                        break;
                    }
                }

                if (containsBlacklisted)
                    yield return current;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static ModelInfo FromJson(string json) => JsonConvert.DeserializeObject<ModelInfo>(json);

    public static string ToJson(ModelInfo modelInfo) => JsonConvert.SerializeObject(modelInfo, Formatting.Indented);
}
