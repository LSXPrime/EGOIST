using EGOIST.Application.DTOs.Voice;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EGOIST.Application.Interfaces.Voice
{
    /// <summary>
    /// Defines the interface for a voice service, providing methods for audio generation and transcription.
    /// </summary>
    public interface IVoiceService
    {
        /// <summary>
        /// Generates audio from a text prompt using a voice model.
        /// </summary>
        /// <param name="prompt">The text prompt for the audio generation.</param>
        /// <param name="speaker">The speaker's voice data, if applicable.</param>
        /// <returns>A task that completes with the generated audio data.</returns>
        Task<byte[]> Generate(string prompt, byte[] speaker);

        /// <summary>
        /// Transcribes audio using a speech recognition model.
        /// </summary>
        /// <param name="dto">The voice transcribe DTO containing the audio data and options.</param>
        /// <returns>An asynchronous enumerable of transcribed text segments.</returns>
        IAsyncEnumerable<string> Generate(VoiceTranscribeDto dto);
    }
}