using EGOIST.Application.DTOs.Voice;
using EGOIST.Application.Interfaces.Voice;
using EGOIST.Domain.Enums;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace EGOIST.Application.Services.Voice
{
    public class TranscribeService : IVoiceService
    {
        private readonly ILogger<TranscribeService> _logger;
        private readonly GenerationService _generation;

        public TranscribeService(ILogger<TranscribeService> logger)
        {
            _logger = logger;
            _generation = GenerationService.Instance;
        }

        /// <summary>
        /// Transcribes audio using Whisper.net.
        /// </summary>
        /// <param name="dto">The voice transcribe DTO containing the audio data.</param>
        /// <returns>An asynchronous enumerable of transcribed text segments.</returns>
        public async IAsyncEnumerable<string> Generate(VoiceTranscribeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            await using var audioReader = new WaveFileReader(new MemoryStream(dto.File));

            // get Whisper transcriber
            var transcriber = _generation.TranscribeProcessor;
            // Set the language
            transcriber?.ChangeLanguage(dto.Language);

            // Transcribe the audio
            await foreach (var segment in transcriber?.ProcessAsync(audioReader, CancellationToken.None)!)
            {
                yield return segment.Text;
            }
        }

        /// <summary>
        /// Generates audio using the selected voice model.
        /// </summary>
        /// <param name="prompt">The text prompt for the audio generation.</param>
        /// <param name="speaker">The speaker's voice data, if applicable.</param>
        /// <returns>A task that completes with the generated audio data.</returns>
        public async Task<byte[]> Generate(string prompt, byte[] speaker) => await Task.FromResult(Array.Empty<byte>());
    }
}