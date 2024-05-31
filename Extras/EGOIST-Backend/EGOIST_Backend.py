import time
start_time = time.time()

import os
import torch
import torchaudio
import asyncio
import msgpack
import base64
import uvicorn
from Core.ModelManager import ModelManager
from TTS.api import TTS
from pathlib import Path
from dotenv import load_dotenv
from fastapi import FastAPI, File, UploadFile, Form
from transformers import AutoModel, AutoTokenizer
from TTS.tts.configs.xtts_config import XttsConfig
from TTS.tts.models.xtts import Xtts
from TTS.tts.configs.bark_config import BarkConfig
from TTS.tts.models.bark import Bark


load_dotenv()
app = FastAPI(
    title="EGOIST AI Hub - Backend",
    description="EGOIST backend server solution",
    version="0.0.1",
)
# Instantiate the ModelManager class
model_manager = ModelManager()

@app.post("/switch_model")
async def switch_model(selected_model: str = Form(...), model_type: str = Form(...), model_task: str = Form(...), backend_type: str = Form(...)):
    if model_type == "voice_generation":
        return model_manager.voiceModel.switch_model(selected_model, model_task, backend_type)
    elif model_type == "text_generation":
        return model_manager.voiceModel.switch_model(selected_model, model_task, backend_type)
    elif model_type == "image_generation":
        return model_manager.voiceModel.switch_model(selected_model, model_task, backend_type)
    else:
        return {"message": f"{model_type} isn't supported yet."}


@app.post("/synthesize")
async def synthesize(selected_model: str = Form(...), text_to_generate: str = Form(...), language: str = Form(...), voice_to_clone: UploadFile = File(...)):
    return await model_manager.voiceModel.synthesize(selected_model, text_to_generate, language, voice_to_clone)


if __name__ == "__main__":
    print("--- EGOIST Loaded in %s seconds ---" % int(time.time() - start_time))
    uvicorn.run("EGOIST_Backend:app", reload=True, host=os.getenv("HOST_IP"), port=int(os.getenv("HOST_PORT")))