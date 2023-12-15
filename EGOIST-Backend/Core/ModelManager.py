import os
import io
import zlib
import tempfile
import torch
import torchaudio
import asyncio
import msgpack
import base64
import soundfile
from TTS.api import TTS
from pathlib import Path
from dotenv import load_dotenv
from fastapi import FastAPI, File, UploadFile, HTTPException
from transformers import AutoModel, AutoProcessor, AutoTokenizer

load_dotenv()

# Path to the directory containing the models
models_path = os.getenv("MODELS_PATH")

if models_path is None:
    # Handle case where models_path is not set properly
    raise ValueError("MODELS_PATH environment variable is not set.")

class ModelManager:

    # Voice Generation Managament Class
    class ModelManager_Voice:
        def __init__(self):
            self.current_model_voice = None
            self.current_model_voice_processor = None
                
            self.backend_type = "transformers"
            self.device = "cuda:0" if torch.cuda.is_available() else "cpu"
            self.current_models = self.load_models()

        def load_models(self):
            models = {}
            model_tasks = ["clone", "transcribe"]

            for model_task in model_tasks:
                models[model_task] = {}
                model_task_path = Path(models_path) / "VoiceGeneration" / model_task

                if model_task_path.exists() and model_task_path.is_dir():
                    # List directories within the current model task directory
                    subdirectories = [item.name for item in model_task_path.iterdir() if item.is_dir()]
                    models[model_task] = subdirectories

            return models

        def switch_model(self, selected_model, model_task, backend_type):
            if model_task in self.current_models and selected_model in self.current_models[model_task]:
                model_path = Path(models_path) / "VoiceGeneration" / model_task / selected_model

                # Unloading previous models and freeing memory
                if self.current_model_voice != None:
                    del self.current_model_voice
                if self.current_model_voice_processor != None:
                    del self.current_model_voice_processor

                if torch.cuda.is_available():
                    torch.cuda.empty_cache()
                    torch.cuda.ipc_collect()

                self.backend_type = backend_type
                if backend_type == "transformers":
                    print(f"{selected_model} Will be loaded by Transformers")
                    self.current_model_voice = AutoModel.from_pretrained(model_path).to(self.device)
                    self.current_model_voice_processor = AutoProcessor.from_pretrained(model_path).to(self.device)
                elif backend_type == "tts":
                    print(f"{selected_model} Will be loaded by TTS")
                    self.current_model_voice = TTS(model_path=model_path, config_path=Path(model_path) / "config.json").to(self.device)
                    self.current_model_voice_processor = None
                elif backend_type == "none":
                    print(f"Voice Generation Model {selected_model} got unloaded")
                    return {"message": "Model Unloaded"}
                else:
                    return {"message": f"{self.backend_type} is an Invalid backend type"}

                return {"message": f"Switched to {selected_model} successfully"}
            else:
                return {"message": f"Model {selected_model} with Task {model_task} and type {backend_type} not found or invalid type"}

        async def synthesize(self, selected_model, text_to_generate, language, voice_to_clone: UploadFile = File(...)):
            try:
                if (self.current_model_voice == None):
                    raise HTTPException(status_code=500, detail=f"Voice Generation Model {selected_model} isn't loaded yet")

                # Process the audio sample (you may need additional audio preprocessing here)
                compressed_data = await voice_to_clone.read()
                audio_bytes = zlib.decompress(compressed_data)
                # Logic to process the audio sample goes here
                if self.backend_type == "transformers":
                    # Synthesize text to audio / no clone with transformers
                    input_ids = self.current_model_voice_processor(text_to_generate, return_tensors="pt").input_ids
                    with torch.no_grad():
                        output = self.current_model_voice.generate(input_ids)
                    result = output[0].detach().cpu().numpy()
                elif self.backend_type == "tts":
                    # Save audio_bytes to a temporary WAV file
                    if "bark".upper() in selected_model.upper():
                        # For the "bark" model, save the WAV file to TempSpeakerPath
                        temp_speaker_path = tempfile.mkdtemp(prefix="TempSpeakerPath_")
                        temp_speaker_filename = "Speaker.wav"
                        temp_speaker_full_path = os.path.join(temp_speaker_path, temp_speaker_filename)
                        temp_voice_dir = os.path.dirname(temp_speaker_path)

                        # Write audio bytes to Speaker.wav inside TempSpeakerPath
                        with open(temp_speaker_full_path, "wb") as temp_wav_file:
                            temp_wav_file.write(audio_bytes)

                        
                        result = self.current_model_voice.tts(text=text_to_generate, speaker=temp_speaker_path, voice_dir=temp_voice_dir, language=language)
                        # Clean up the temporary file after usage
                        os.unlink(temp_voice_dir)
                    else:
                        # For other models, save the WAV file normally using NamedTemporaryFile
                        with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as temp_wav_file:
                            temp_wav_file.write(audio_bytes)
                            temp_wav_file_path = temp_wav_file.name
                
                        # Use the temporary WAV file path as speaker_wav input
                        result = self.current_model_voice.tts(text=text_to_generate, speaker_wav=temp_wav_file_path, language=language)

                        # Clean up the temporary file after usage
                        os.unlink(temp_wav_file_path)

                
                # Save audio data as a temporary WAV file
                with io.BytesIO() as wav_io:
                    # Save the audio data as a WAV file in memory (you can adjust the parameters as needed)
                    soundfile.write(wav_io, result, samplerate=44100, format='WAV')

                    # Get the WAV audio data as bytes
                    wav_io.seek(0)
                    wav_bytes = wav_io.read()

                # Encode the audio WAV data in MessagePack format
                audio_msgpack = msgpack.packb(wav_bytes, use_bin_type=True)
                compressed_audio = zlib.compress(audio_msgpack, level=zlib.Z_BEST_COMPRESSION)
                audio_base64 = base64.b64encode(compressed_audio).decode('utf-8')

                return {'audio': audio_base64}
    
            except Exception as e:
                print(f"Exception Detected: {str(e)}")
                raise HTTPException(status_code=500, detail=str(e))

            

    voiceModel = ModelManager_Voice()