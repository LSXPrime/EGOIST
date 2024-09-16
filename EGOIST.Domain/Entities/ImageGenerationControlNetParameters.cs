using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class ImageGenerationControlNetParameters : EntityBase
{
    public bool IsEnabled => _controlNetImage is { Length: 0 };
    
    private byte[]? _controlNetImage;
    private float _controlNetStrength = 0.9f;
    private bool _controlNetCannyPreprocess;
    private bool _controlNetCannyInverse;
    
    public byte[]? ControlNetImage { get => _controlNetImage; set => Notify(ref _controlNetImage, value); }
    public float ControlNetStrength { get => _controlNetStrength; set => Notify(ref _controlNetStrength, value); }
    public bool ControlNetCannyPreprocess { get => _controlNetCannyPreprocess; set => Notify(ref _controlNetCannyPreprocess, value); }
    public bool ControlNetCannyInverse { get => _controlNetCannyInverse; set => Notify(ref _controlNetCannyInverse, value); }
}