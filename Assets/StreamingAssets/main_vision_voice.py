import time
import os
import json
import random
import logging
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

# Import Clients
from ollama_client import OllamaClient
from gemini_client import GeminiClient
from voice_client import VoiceClient

# --- Configuration & Constants ---
WATCHED_EXTENSIONS = {'.jpg', '.jpeg', '.png'}
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
CAPTURE_DIR = os.path.join(SCRIPT_DIR, "capture")
VOICE_DIR = os.path.join(SCRIPT_DIR, "voice")
CONFIG_FILE = os.path.join(SCRIPT_DIR, "config.json")

# Ensure directories exist
os.makedirs(CAPTURE_DIR, exist_ok=True)
os.makedirs(VOICE_DIR, exist_ok=True)

# Configure Logging
import sys
logging.basicConfig(
    level=logging.INFO,
    format='[[%(levelname)s]] %(message)s',
    handlers=[logging.StreamHandler(sys.stdout)] # Explicitly print to stdout for Unity
)
logger = logging.getLogger(__name__)

# --- Load Config ---
def load_config():
    try:
        with open(CONFIG_FILE, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        logger.error(f"Failed to load config.json: {e}")
        return {}

config_data = load_config()
VOICE_VARIANTS = config_data.get("VOICE_VARIANTS", {})
PERSONALITY_PROMPTS = config_data.get("PERSONALITY_PROMPTS", {})
PSYCHOLOGICAL_TRIGGERS = config_data.get("PSYCHOLOGICAL_TRIGGERS", [])

# --- Initialize Clients ---
try:
    ollama_client = OllamaClient(model_name='qwen2.5vl:7b')
    gemini_client = GeminiClient(model_name='gemini-flash-latest')
    voice_client = VoiceClient()
    logger.info("Clients initialized successfully (Hybrid Mode).")
except Exception as e:
    logger.critical(f"Failed to initialize clients: {e}")
    exit(1)

# --- Logic Helper Functions ---
def determine_tone(score):
    try:
        score = int(score)
    except:
        score = 3
        
    if score <= 2:
        return "Tone: Fresh, Polite, Curious (New Item). Act as if you just met the world."
    elif score >= 4:
        return "Tone: Wise, Intimate, Nostalgic (Old Item). Act as if you have known the user for years."
    else:
        return "Tone: Friendly, Casual (Used Item). Act as a reliable partner."

def determine_personality_id(analysis_data):
    pid = analysis_data.get("personality_id", "observer")
    item_name = analysis_data.get("item_name", "Object")
    item_lower = item_name.lower()
    
    # Force override based on keywords
    if any(x in item_lower for x in ['phone', 'smart', 'watch', 'tablet', 'screen']): pid = 'external_brain'
    elif any(x in item_lower for x in ['bottle', 'water', 'drink', 'food', 'snack', 'candy', 'medicine']): pid = 'lifeline'
    elif any(x in item_lower for x in ['wallet', 'key', 'card', 'money', 'coin', 'purse']): pid = 'gatekeeper'
    elif any(x in item_lower for x in ['pen', 'pencil', 'note', 'book', 'laptop', 'camera']): pid = 'muse'
    elif any(x in item_lower for x in ['earphone', 'headphone', 'plush', 'toy', 'tissue', 'handkerchief', 'cigarette']): pid = 'sanctuary'
    elif any(x in item_lower for x in ['cosmetic', 'makeup', 'mirror', 'glass', 'ring', 'necklace', 'jewelry']): pid = 'mask'
    
    return pid

def process_image(image_path):
    if os.path.basename(image_path).startswith('.'):
        return

    try:
        time.sleep(1.0) # Wait for file write 
        filename = os.path.basename(image_path)
        logger.info(f"[[STATE_START]] Processing {filename}")
        
        # 1. Image Analysis (OLLAMA - Local)
        # -----------------------------------
        analysis_data = ollama_client.analyze_image(image_path)
        
        if not analysis_data:
            logger.warning("Analysis failed. Using default fallback.")
            analysis_data = {"item_name": "Object", "description": "Unknown", "item_condition": "Unknown", "condition_score": 3, "personality_id": "observer"}
        
        # 2. Logic: Tone & Personality
        # ----------------------------
        score = analysis_data.get("condition_score", 3)
        tone_instruction = determine_tone(score)
        
        pid = determine_personality_id(analysis_data)
        
        if pid not in VOICE_VARIANTS: pid = "observer"
        voice_settings = random.choice(VOICE_VARIANTS[pid])
        logger.info(f"[[CREDIT]] COERIOINK: {voice_settings['name']} (Role: {pid})")

        # 3. Text Generation (GEMINI - Cloud)
        # -----------------------------------
        trigger = random.choice(PSYCHOLOGICAL_TRIGGERS)
        prompt_text = PERSONALITY_PROMPTS.get(pid, "")
        
        speech_text = gemini_client.generate_dialogue(
            analysis_data, 
            prompt_text, 
            tone_instruction, 
            trigger
        )
        
        logger.info(f"[[MESSAGE]] {speech_text}")

        if len(speech_text) < 2:
            logger.error("Speech text too short/empty. Skipping TTS.")
            return

        # 4. Audio Synthesis (COEIROINK - Local)
        # --------------------------------------
        audio_data = voice_client.synthesis(
            speech_text, 
            voice_settings['uuid'], 
            voice_settings['style']
        )

        if audio_data:
            base_name = os.path.splitext(filename)[0]
            wav_filename = f"{base_name}.wav"
            wav_path = os.path.join(VOICE_DIR, wav_filename)
            
            try:
                with open(wav_path, "wb") as f:
                    f.write(audio_data)
                logger.info(f"[[STATE_COMPLETE]] Saved videos to {wav_filename}")
            except Exception as e:
                logger.error(f"File Write Failed: {e}")
        else:
            logger.error("[[STATE_COMPLETE]] Audio Gen Failed")

    except Exception as e:
        logger.error(f"Processing Failed: {e}")
        import traceback
        traceback.print_exc()

# --- Watcher Class ---
class ImageHandler(FileSystemEventHandler):
    def __init__(self):
        self.last_processed = {}

    def on_created(self, event):
        if event.is_directory: return
        filename = os.path.basename(event.src_path)
        ext = os.path.splitext(filename)[1].lower()
        
        if ext in WATCHED_EXTENSIONS:
            # Debounce: Ignore if processed in the last 2 seconds
            now = time.time()
            if filename in self.last_processed:
                if now - self.last_processed[filename] < 2.0:
                    logger.info(f"Skipping duplicate event for {filename}")
                    return
            
            self.last_processed[filename] = now
            
            # Additional small wait to ensure file write is fully done
            time.sleep(1.0) 
            process_image(event.src_path)

if __name__ == "__main__":
    print("--- Hybrid AI Object Voice System (v7.0: OllamaVision + GeminiText) ---")
    logger.info(f"[[SYSTEM]] Monitoring: {CAPTURE_DIR}")
    
    event_handler = ImageHandler()
    observer = Observer()
    observer.schedule(event_handler, CAPTURE_DIR, recursive=False)
    observer.start()

    try:
        while True: time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()
