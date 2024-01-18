using System.IO;
using System.IO.Compression;
using System.Net.Http;
using NAudio.Wave;
using MessagePack;
using Wpf.Ui.Controls;
using System.Collections.ObjectModel;
using EGOIST.Data;
using EGOIST.Enums;
using EGOIST.Helpers;
using System.Windows.Forms;
using Whisper.net;
using NAudio.Wave.SampleProviders;
using Notification.Wpf;
using NetFabric.Hyperlinq;

namespace EGOIST.ViewModels.Pages;
public partial class VoiceViewModel : ObservableObject, INavigationAware
{
    #region SharedVariables
    private bool _isInitialized = false;
    [ObservableProperty]
    private Dictionary<string, Visibility> _visibilityDict = new()
    {
        { "playbackpanel", Visibility.Visible },
        { "playaudio", Visibility.Visible },
        { "pauseaudio", Visibility.Visible },
        { "stopaudio", Visibility.Visible },
        { "generatebutton", Visibility.Visible },
        { "resetaudiobutton", Visibility.Visible },
        { "transcribebutton", Visibility.Visible },
        { "transcribestarted", Visibility.Visible },
        { "transcribefinished", Visibility.Visible },
    };
    private readonly NotificationManager notification = new();
    #endregion

    #region GenerationVariables
    [ObservableProperty]
    private GenerationState _cloneState = GenerationState.None;
    [ObservableProperty]
    private ObservableCollection<string> _generationLanaguages = new() { "EN", "ES", "FR", "DE", "IT", "PT", "PL", "TR", "RU", "NL", "CS", "AR", "ZH-CN", "JA,HU", "KO" };
    [ObservableProperty]
    private ObservableCollection<string> _generationVoicePaths = new();
    [ObservableProperty]
    private Dictionary<string, List<ModelInfo>> _generationModels = new();
    [ObservableProperty]
    private ModelInfo _selectedCloneModel;
    [ObservableProperty]
    private string _selectedLanguage = string.Empty;
    [ObservableProperty]
    private string _selectedVoice = string.Empty;
    [ObservableProperty]
    private string _textToGenerate = string.Empty;
    [ObservableProperty]
    private float _speechRate = 1f;

    #endregion

    #region TranscribeVariables
    [ObservableProperty]
    private GenerationState _transcribeState = GenerationState.None;
    [ObservableProperty]
    private ModelInfo _selectedTranscribeModel;
    [ObservableProperty]
    private ModelInfo.ModelInfoWeight _selectedTranscribeWeight;
    [ObservableProperty]
    private string _selectedTranscribeAudio = string.Empty;
    [ObservableProperty]
    private string _selectedTranscribeType = "SRT";
    [ObservableProperty]
    private string _transcribeAudioResult = string.Empty;
    [ObservableProperty]
    private string[] _transcribeTypes = { "TXT", "SRT" };

    private WhisperFactory? TranscribeModel;
    private WhisperProcessor? TranscribeProcessor;
    #endregion

    #region PlaybackVariables
    private PlaybackState PlaybackState;
    private string _lastGeneratedAudio = string.Empty;
    private AudioFileReader? audioFile;
    private WaveOutEvent? outputDevice;
    [ObservableProperty]
    private double _playbackProgress;
    #endregion

    #region RecordingVariables
    private WaveInEvent? recordingDevice;
    private List<byte> recordedAudioData = new();
    private System.Threading.Timer? recordTimer;

    [ObservableProperty]
    private ObservableCollection<string> _availableMicrophones;
    [ObservableProperty]
    private ObservableCollection<string> _recordingOptions = new();
    [ObservableProperty]
    public ObservableCollection<double> _waveformData = new();

    [ObservableProperty]
    private string _selectedMicrophone = string.Empty;

    [ObservableProperty]
    private string _selectedRecordingOption = string.Empty;

    [ObservableProperty]
    public TimeSpan _recordingTime = TimeSpan.Zero;

    [ObservableProperty]
    private bool _recordingAmplify = false;
    [ObservableProperty]
    private float _recordingAmplifyFactor = 1.0f;

    [ObservableProperty]
    private string _lastRecordedAudio = string.Empty;


    #endregion

    #region InitlizingMethods
    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        LoadData();
        AppConfig.Instance.ConfigSavedEvent += LoadData;
        _isInitialized = true;
        PlaybackState = PlaybackState.Stopped;
        CloneState = GenerationState.None;
        Thread thread = new Thread(UIHandler);
        thread.Start();
    }

    private void UIHandler()
    {
        while (true)
        {
            VisibilityDict["playbackpanel"] = CloneState == GenerationState.Finished ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["playaudio"] = PlaybackState == PlaybackState.Playing ? Visibility.Hidden : Visibility.Visible;
            VisibilityDict["pauseaudio"] = PlaybackState == PlaybackState.Playing ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["stopaudio"] = PlaybackState != PlaybackState.Stopped ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["generatebutton"] = CloneState != GenerationState.Started ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["resetaudiobutton"] = CloneState == GenerationState.Finished ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["transcribebutton"] = TranscribeState != GenerationState.Started ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["transcribestarted"] = TranscribeState != GenerationState.None ? Visibility.Visible : Visibility.Hidden;
            VisibilityDict["transcribefinished"] = TranscribeState == GenerationState.Finished ? Visibility.Visible : Visibility.Hidden;
            OnPropertyChanged(nameof(VisibilityDict));

            Thread.Sleep(100);
        }
    }

    private void LoadData()
    {
        GenerationModels.Clear();
        GenerationVoicePaths.Clear();

        var models = ManagementViewModel._modelsInfo.AsValueEnumerable().Where(x => x.Type.Contains("Voice") && x.Downloaded.Count > 0).ToList();
        GenerationModels = new()
        {
            { "Clone", models.AsValueEnumerable().Where(x => x.Task.Contains("Clone")).ToList() },
            { "Generate", models.AsValueEnumerable().Where(x => x.Task.Contains("Generate")).ToList() },
            { "Transcribe", models.AsValueEnumerable().Where(x => x.Task.Contains("Transcribe")).ToList() }
        };

        if (Directory.Exists(AppConfig.Instance.VoicesPath))
        {
            var voices = Directory.GetDirectories(AppConfig.Instance.VoicesPath);
            GenerationVoicePaths = new ObservableCollection<string>(voices.Select(f => Path.GetFileName(f)));
        }

        AvailableMicrophones = new(RecordingAvailableMicrophones());
    }
    #endregion

    #region GenerationMethods
    [RelayCommand]
    public async Task GenerationSwitchModels(string task)
    {
        try
        {
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Model Switching to {SelectedCloneModel.Name} Started", Type = NotificationType.Information }, areaName: "NotificationArea");

        //    var modelPath = $"{AppConfig.Instance.ModelsPath}\\{SelectedCloneModel.Type.RemoveSpaces()}\\{SelectedCloneModel.Name.RemoveSpaces()}\\{SelectedCloneModel.Weights[0].Weight.RemoveSpaces()}.{SelectedCloneModel.Weights[0].Extension.ToLower().RemoveSpaces()}";

            using var client = new HttpClient();
            // Send the POST request to the backend
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{AppConfig.Instance.ApiUrl}/switch_model")
            {
                Content = new MultipartFormDataContent
                {
                    { new StringContent(SelectedCloneModel.Name), "selected_model" },
                    { new StringContent(SelectedCloneModel.Type), "model_type" },
                    { new StringContent(SelectedCloneModel.Task), "model_task" },
                    { new StringContent(SelectedCloneModel.Backend), "backend_type" }
                }
            };
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Model Switched to {SelectedCloneModel.Name} Successfully", Type = NotificationType.Success }, areaName: "NotificationArea");
            else
            {
                // Handle failure response
                Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Model Switching to {SelectedCloneModel.Name} Failed, Error: {response.StatusCode}", Type = NotificationType.Error }, areaName: "NotificationArea");
                SelectedCloneModel = null;
            }

        }
        catch (Exception ex)
        {
            // Handle exceptions (if needed)
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Model Switching to {SelectedCloneModel.Name} Failed, Exception: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea");
        }
    }

    [RelayCommand]
    public async Task GenerateAudio()
    {
        try
        {
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Voice Cloning Process Started", Type = NotificationType.Information }, areaName: "NotificationArea");
            CloneState = GenerationState.Started;
            var voiceDataPath = $"{AppConfig.Instance.VoicesPath}\\{SelectedVoice}\\speaker.wav";
            var voiceData = File.ReadAllBytes(voiceDataPath);
            byte[] compressedvoiceData;
            using (var compressedStream = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(compressedStream, CompressionMode.Compress))
                {
                    compressionStream.Write(voiceData, 0, voiceData.Length);
                }

                compressedvoiceData = compressedStream.ToArray();
            }

            // Prepare the data to send to the backend
            var data = new MultipartFormDataContent
            {
                { new StringContent(SelectedCloneModel.Name), "selected_model" },
                { new StringContent(TextToGenerate), "text_to_generate" },
                { new StringContent(SelectedLanguage.ToLower()), "language" },
                { new ByteArrayContent(compressedvoiceData), "voice_to_clone", Path.GetFileName(voiceDataPath) }
            };

            using var client = new HttpClient();
            // Send the POST request to the backend
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{AppConfig.Instance.ApiUrl}/synthesize");
            request.Content = data;
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = "Voice Cloning Process Completed Successfully", Type = NotificationType.Success }, areaName: "NotificationArea");

                // Handle successful response
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic config = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
                string audioData = config.audio ?? string.Empty;

                // Decode the base64-encoded data to get the serialized audio
                var serializedAudio = Convert.FromBase64String(audioData);

                // Decompress the data
                byte[] decompressedAudioData;
                using (var decompressedStream = new MemoryStream())
                {
                    using (var compressedDataStream = new MemoryStream(serializedAudio))
                    using (var decompressionStream = new DeflateStream(compressedDataStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedStream);
                    }
                    decompressedAudioData = decompressedStream.ToArray();
                }

                // Deserialize the received audio using MessagePack
                var deserializedAudio = MessagePackSerializer.Deserialize<byte[]>(decompressedAudioData);

                // Save the audio to a temporary file
                _lastGeneratedAudio = $"{AppConfig.Instance.ResultsPath}\\VoiceGeneration\\Audio-{DateTime.Now:yyyy-MM-dd-HH-mm_ss}.wav";
                File.WriteAllBytes(_lastGeneratedAudio, deserializedAudio);
            }
            else
            {
                // Handle failure response
                Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Failed to receive audio. Status code: {response.StatusCode}", Type = NotificationType.Error }, areaName: "NotificationArea");
            }
            CloneState = GenerationState.Finished;
        }
        catch (Exception ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Failed to receive audio. Exception: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea");
            CloneState = GenerationState.None;
        }
    }

    [RelayCommand]
    private void ResetGeneration()
    {
        CloneState = GenerationState.None;
        PlaybackState = PlaybackState.Stopped;
        TextToGenerate = string.Empty;
        if (outputDevice != null)
            StopAudio();
    }
    #endregion

    #region TranscribeMethods

    [RelayCommand]
    public void TranscribeSwitchModels(string parameter)
    {
        try
        {
            if (parameter.Equals("unload"))
            {
                TranscribeModel?.Dispose();
                TranscribeProcessor?.Dispose();
                ResetTranscribe();
                SelectedTranscribeModel = null;
                Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Audio Transcribe Model Unloaded", Type = NotificationType.Information }, areaName: "NotificationArea");
                return;
            }
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Audio Transcribe Model {SelectedTranscribeModel} Started loading", Type = NotificationType.Information }, areaName: "NotificationArea");
            Thread thread = new(SwitchMethodBG);
            thread.Start();
        }
        catch (Exception ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Model Switching to {SelectedTranscribeModel} Failed, Exception: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea");
        }

        void SwitchMethodBG()
        {
            var modelPath = $"{AppConfig.Instance.ModelsPath}\\{SelectedTranscribeModel.Type.RemoveSpaces()}\\{SelectedTranscribeModel.Name.RemoveSpaces()}\\{SelectedTranscribeWeight.Weight.RemoveSpaces()}.{SelectedTranscribeWeight.Extension.ToLower().RemoveSpaces()}";

            var isGpu = AppConfig.Instance.Device == Device.GPU;
            TranscribeModel = WhisperFactory.FromPath(modelPath, false, null, false, isGpu);
            TranscribeProcessor = TranscribeModel.CreateBuilder().WithLanguage("auto").Build();
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Audio Transcribe Model loaded Successfully", Type = NotificationType.Information }, areaName: "NotificationArea");
        }
    }

    [RelayCommand]
    private void TranscribeAudioBrowser()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Audio Files (*.wav; *.mp3)|*.wav;*.mp3";
        DialogResult result = openFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            SelectedTranscribeAudio = openFileDialog.FileName;
        }
    }

    [RelayCommand]
    private async Task TranscribeAudio()
    {
        try
        {
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Audio Transcribe Process Started", Type = NotificationType.Information }, areaName: "NotificationArea");
            TranscribeState = GenerationState.Started;
            using var fileStream = File.OpenRead(SelectedTranscribeAudio);
            using var wavStream = new MemoryStream();

            if (Path.GetExtension(SelectedTranscribeAudio).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                using var mp3FileReader = new Mp3FileReader(SelectedTranscribeAudio);
                // Create a WaveFormatConversionStream to convert MP3 to WAV
                var pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3FileReader);

                // Resample the PCM stream to 16kHz
                var resampler = new WdlResamplingSampleProvider(pcmStream.ToSampleProvider(), 16000);

                // Write the resampled WAV data to a memory stream
                WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());
                wavStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                using var reader = new WaveFileReader(fileStream);
                    // Only 16kHz sample rate is supported in Whisper.Net workaround
                var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
                WaveFileWriter.WriteWavFileToStream(wavStream, resampler.ToWaveProvider16());
                wavStream.Seek(0, SeekOrigin.Begin);
            }

            TranscribeAudioResult = string.Empty;

            var results = TranscribeProcessor?.ProcessAsync(wavStream);
            var index = 1; // incase of SRT
            string previousEndTime = "00:00:00,000"; // incase of SRT, Track previous end time for relative timestamp calculation

            await foreach (var result in results)
            {
                switch (SelectedTranscribeType)
                {
                    case "TXT":
                        TranscribeAudioResult += result.Text;
                        break;
                    case "SRT":
                        // Convert to SRT format
                        var startTimeStr = previousEndTime;
                        var endTimeStr = result.End.ToString(@"hh\:mm\:ss\,fff");

                        // Build SRT string with relative timestamps
                        TranscribeAudioResult += $"{index}\n{startTimeStr} --> {endTimeStr}\n{result.Text}\n\n";

                        // Update previous end time for next iteration
                        previousEndTime = endTimeStr; 
                        index++;
                        break;
                }
            }

            TranscribeState = GenerationState.Finished;
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Audio Transcribe Process Finished", Type = NotificationType.Information }, areaName: "NotificationArea");
        }
        catch (Exception ex)
        {
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Audio Transcribe Process Failed, Exception: {ex.Message}", Type = NotificationType.Error }, areaName: "NotificationArea");
            ResetTranscribe();
        }
    }

    [RelayCommand]
    private void ResetTranscribe()
    {
        SelectedTranscribeAudio = string.Empty;
        TranscribeAudioResult = string.Empty;
        TranscribeState = GenerationState.None;
    }

    [RelayCommand]
    private void TranscribeSave()
    {
        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Text & SubRip Subtitle Files (*.txt,*.srt)|*.txt,*.srt|All Files (*.*)|*.*";
        saveFileDialog.FileName = $"transcribe_result{DateTime.Now:yyyy-MM-dd-HH-mm_ss}.{SelectedTranscribeType.ToLower()}";

        DialogResult result = saveFileDialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            File.WriteAllText(saveFileDialog.FileName, TranscribeAudioResult);
            Extensions.Notify(new NotificationContent { Title = "Voice Generation", Message = $"Audio Transcribe Text Saved, Path:  {saveFileDialog.FileName}", Type = NotificationType.Information }, areaName: "NotificationArea");

        }
    }
    #endregion

    #region PlaybackMethods
    [RelayCommand]
    private void PlayAudio()
    {
        outputDevice?.Dispose();

        audioFile = new AudioFileReader(_lastGeneratedAudio);
        outputDevice = new WaveOutEvent();
        outputDevice.Init(audioFile);
        outputDevice.Play();
        PlaybackState = PlaybackState.Playing;

        // Update progress
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.Tick += (s, e) => { PlaybackProgress = audioFile.Position / audioFile.Length * 100; };
        timer.Start();
    }

    [RelayCommand]
    private void PauseAudio()
    {
        if (outputDevice != null)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Pause();
                PlaybackState = PlaybackState.Paused;
            }
            else if (outputDevice.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Play();
                PlaybackState = PlaybackState.Playing;
            }
        }
    }

    [RelayCommand]
    private void StopAudio()
    {
        if (outputDevice != null)
        {
            outputDevice.Stop();
            outputDevice.Dispose();
            audioFile?.Dispose();
            PlaybackProgress = 0;
            PlaybackState = PlaybackState.Stopped;
        }
    }
    #endregion

    #region RecordingMethods

    public static List<string> RecordingAvailableMicrophones()
    {
        List<string> microphones = new List<string>();
        for (var i = 0; i < WaveIn.DeviceCount; i++)
        {
            WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(i);
            microphones.Add(deviceInfo.ProductName);
        }
        return microphones;
    }

    [RelayCommand]
    public void RecordingStart()
    {
        recordingDevice = new WaveInEvent();
        recordingDevice.DeviceNumber = AvailableMicrophones.IndexOf(SelectedMicrophone);
        recordingDevice.DataAvailable += Recording_DataAvailable;
        recordingDevice.StartRecording();
        recordTimer = new System.Threading.Timer(_ =>
        {
            RecordingTime += TimeSpan.FromSeconds(1);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    [RelayCommand]
    public void RecordingPause()
    {
        recordingDevice?.StopRecording();
    }

    [RelayCommand]
    public void RecordingStop()
    {
        RecordingSave();
        WaveformData.Clear();
        recordTimer?.Dispose();
        recordingDevice?.StopRecording();
        recordingDevice?.Dispose();
        recordingDevice = null;
    }

    private void Recording_DataAvailable(object sender, WaveInEventArgs e)
    {
        if (RecordingAmplify)
        {
            var amplifiedData = RecordingAmplifier(e.Buffer);
            recordedAudioData.AddRange(amplifiedData);
        }
        else
            recordedAudioData.AddRange(e.Buffer);

        var buffer = e.Buffer;
        var bytesRecorded = e.BytesRecorded / 2; // 16-bit audio, so 2 bytes per sample

        for (var i = 0; i < bytesRecorded; i += 2) // Assuming 16-bit samples
        {
            var sample = BitConverter.ToInt16(buffer, i);
            var sample32 = sample / 32768f; // Normalize to [-1.0, 1.0]
            WaveformData.Add(sample32);
        }
    }

    private byte[] RecordingAmplifier(byte[] buffer)
    {
        var samples = new float[buffer.Length / 2];
        for (var i = 0; i < buffer.Length / 2; i++)
        {
            var sample = BitConverter.ToInt16(buffer, i * 2);
            samples[i] = sample * RecordingAmplifyFactor;
        }

        var amplifiedData = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            var amplifiedSample = (short)Math.Clamp(samples[i], short.MinValue, short.MaxValue);
            var bytes = BitConverter.GetBytes(amplifiedSample);
            Buffer.BlockCopy(bytes, 0, amplifiedData, i * 2, 2);
        }

        return amplifiedData;
    }

    private async Task RecordingSave()
    {
        var VoiceName = new Wpf.Ui.Controls.TextBox
        {
            Text = string.Empty,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        var VoiceNameR = new Wpf.Ui.Controls.MessageBox
        {
            Title = "Save Voice as",
            Content = VoiceName,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        var result = await VoiceNameR.ShowDialogAsync();

        if (result != Wpf.Ui.Controls.MessageBoxResult.Primary)
            return;

        // Save recorded data to a file
        var _outputDir = $"{AppConfig.Instance.VoicesPath}\\{VoiceName.Text}";
        Directory.CreateDirectory(_outputDir);

        using (var waveWriter = new WaveFileWriter($"{_outputDir}//Speaker.wav", recordingDevice?.WaveFormat))
        {
            waveWriter.Write(recordedAudioData.ToArray(), 0, recordedAudioData.Count);
        }

        recordedAudioData.Clear();

        GenerationVoicePaths.Clear();
        // Reload the voices list
        if (Directory.Exists(AppConfig.Instance.VoicesPath))
        {
            var voices = Directory.GetDirectories(AppConfig.Instance.VoicesPath);
            GenerationVoicePaths = new ObservableCollection<string>(voices.Select(f => Path.GetFileName(f)));
        }

    }


    #endregion
}
