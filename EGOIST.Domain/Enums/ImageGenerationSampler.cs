namespace EGOIST.Domain.Enums;

public enum ImageGenerationSampler : short
{
    EulerA = 0,
    Euler = 1,
    Heun = 2,
    DPM2 = 3,
    DPMPP2SA = 4,
    DPMPP2M = 5,
    DPMPP2Mv2 = 6,
    LCM = 7
}