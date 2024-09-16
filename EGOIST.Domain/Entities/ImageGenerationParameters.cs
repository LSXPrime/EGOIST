using EGOIST.Domain.Abstracts;
using EGOIST.Domain.Enums;

namespace EGOIST.Domain.Entities;

public class ImageGenerationParameters : EntityBase
{
    private string _prompt = string.Empty;
    private string _negativePrompt = string.Empty;
    private float _cfgScale = 7.5f;
    private int _steps = 25;
    private ImageGenerationSampler _sampler = ImageGenerationSampler.Euler;
    private long _seed = -1;
    private float _strength = 0.75f;
    private int _clipSkip = -1;
    private int _width = 512;
    private int _height = 512;
    private ImageGenerationControlNetParameters _controlNetParameters = new();

    public string Prompt { get => _prompt; set => Notify(ref _prompt, value); }
    public string NegativePrompt { get => _negativePrompt; set => Notify(ref _negativePrompt, value); }
    public float CfgScale { get => _cfgScale; set => Notify(ref _cfgScale, value); }
    public int Steps { get => _steps; set => Notify(ref _steps, value); }
    public ImageGenerationSampler Sampler { get => _sampler; set => Notify(ref _sampler, value); }
    public long Seed { get => _seed; set => Notify(ref _seed, value); }
    public float Strength { get => _strength; set => Notify(ref _strength, value); }
    public int ClipSkip { get => _clipSkip; set => Notify(ref _clipSkip, value); }
    public int Width { get => _width; set => Notify(ref _width, value); }
    public int Height { get => _height; set => Notify(ref _height, value); }
    public ImageGenerationControlNetParameters ControlNetParameters { get => _controlNetParameters; set => Notify(ref _controlNetParameters, value); }
    

    public ImageGenerationParameters(float cfgScale, int steps, ImageGenerationSampler sampleMethod, long seed, float strength, int clipSkip, int width, int height)
    {
        CfgScale = cfgScale;
        Steps = steps;
        Sampler = sampleMethod;
        Seed = seed;
        Strength = strength;
        ClipSkip = clipSkip;
        Width = width;
        Height = height;
    }

    public ImageGenerationParameters()
    {
        CfgScale = 7.5f;
        Steps = 25;
        Sampler = ImageGenerationSampler.Euler;
        Seed = -1;
        Strength = 0.75f;
        ClipSkip = 1;
        Width = 512;
        Height = 512;
    }
}