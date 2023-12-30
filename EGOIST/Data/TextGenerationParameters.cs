
namespace EGOIST.Data;

public partial class TextGenerationParameters : ObservableObject
{
    [ObservableProperty]
    public int _maxTokens = 1200;
    [ObservableProperty]
    public float _randomness = 0.7f;
    [ObservableProperty]
    public float _randomnessBooster = 0.3f;
    [ObservableProperty]
    public float _optimalProbability = 0.2f;
    [ObservableProperty]
    public float _frequencyPenalty = 1f;
}
