namespace EGOIST.Application.DTOs.Voice;

/// <summary>
/// Represents a voice transcription request.
/// </summary>
public class VoiceTranscribeDto
{
    /// <summary>
    /// The audio language.
    /// </summary>
    public string? Language { get; set; }
    
    /// <summary>
    /// The file extension of the audio data.
    /// </summary>
    public string Extension { get; set; } = "WAV";

    /// <summary>
    /// The audio data in byte array format.
    /// </summary>
    public byte[] File { get; set; } = [];

    /// <summary>
    /// The desired output type of the transcription.
    /// </summary>
    public string Type { get; set; } = "TXT";
}