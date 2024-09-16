using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class TextGenerationParameters : EntityBase
{
    private int _maxTokens;
    private float _randomness;
    private float _randomnessBooster;
    private float _optimalProbability;
    private float _frequencyPenalty;
    

    public int MaxTokens { get => _maxTokens; set => Notify(ref _maxTokens, value); }
    public float Randomness { get => _randomness; set => Notify(ref _randomness, value); }
    public float RandomnessBooster { get => _randomnessBooster; set => Notify(ref _randomnessBooster, value); }
    public float OptimalProbability { get => _optimalProbability; set => Notify(ref _optimalProbability, value); }
    public float FrequencyPenalty { get => _frequencyPenalty; set => Notify(ref _frequencyPenalty, value); }
    
    public TextGenerationParameters(int maxTokens, float randomness, float randomnessBooster, float optimalProbability, float frequencyPenalty)
    {
        MaxTokens = maxTokens;
        Randomness = randomness;
        RandomnessBooster = randomnessBooster;
        OptimalProbability = optimalProbability;
        FrequencyPenalty = frequencyPenalty;
    }
    
    public TextGenerationParameters(bool useDefaults = false)
    {
        if (!useDefaults) return;
        MaxTokens = 1200;
        Randomness = 0.7f;
        RandomnessBooster = 0.3f;
        OptimalProbability = 0.2f;
        FrequencyPenalty = 1f;
    }
}