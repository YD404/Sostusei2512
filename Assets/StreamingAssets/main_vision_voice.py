import time
import os
import json
import random
import logging
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

# Import Clients
from ollama_client import OllamaClient
from deepseek_client import DeepSeekClient # CHANGED from GeminiClient
from voice_client import VoiceClient
import item_obsessions

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
# ------------------------------------------------------------------
# 2. Initialize Clients
# ------------------------------------------------------------------
try:
    ollama_client = OllamaClient()
    deepseek_client = DeepSeekClient() # CHANGED
    voice_client = VoiceClient()
    logger.info("Clients initialized successfully (Hybrid Mode: Ollama + DeepSeek).")
except Exception as e:
    logger.critical(f"Failed to initialize clients: {e}")
    exit(1)

# --- Logic Helper Functions ---
# --- Logic Helper Functions ---
def determine_persona(analysis_data):
    """
    Determines persona based on the 5-step priority logic.
    Returns (persona_id, role_name_jp)
    """
    state_str = analysis_data.get("state", "Normal").lower()
    shape_str = analysis_data.get("shape", "Other").lower()
    is_machine = analysis_data.get("is_machine", False)
    
    # 1. Old / Dirty -> Old Man (ご長寿)
    if any(x in state_str for x in ["old", "dirty", "broken"]):
        return "lifeline", "ご長寿" # Mapped to deepest male voice available (Lifeline usually has diverse voices or use specific UUID)

    # 2. Sharp / Machine+Black -> Chuuni (中二病)
    # Note: Color is not currently extracted by Ollama in the new prompt, assuming Shape/Machine is enough or add color check back if needed.
    # For now, using Shape=Sharp OR Machine=True logic slightly loosely for Chuuni if not Old.
    if "sharp" in shape_str:  
        return "gatekeeper", "中二病" # Mapped to cool male voice

    # 3. Machine -> Tsundere (ツンデレ)
    if is_machine:
        return "mask", "ツンデレ" # Mapped to sharp female

    # 4. Round -> Yandere (ヤンデレ)
    if "round" in shape_str:
        return "sanctuary", "ヤンデレ" # Mapped to whisper/soft female

    # 5. Default -> Gal (ギャル)
    return "external_brain", "ギャル" # Mapped to energetic female

def get_voice_uuid(persona_id):
    """
    Maps specific Persona IDs to specific preferred Voice UUIDs/Styles from config.
    """
    # This mapping attempts to pick the best voice from valid config categories
    # "lifeline" (Old) -> 九州そら (Style 685839222 is deep? Actual check needed, defaulting to first valid)
    # "gatekeeper" (Chuuni) -> 剣崎雌雄 (Male)
    # "mask" (Tsundere) -> No.7 (Female)
    # "sanctuary" (Yandere) -> 春日部つむぎ (Soft Female)
    # "external_brain" (Gal) -> 虚音イフ (Female)
    
    variants = VOICE_VARIANTS.get(persona_id, [])
    if not variants:
        # Fallback to any existant
        all_keys = list(VOICE_VARIANTS.keys())
        if all_keys: variants = VOICE_VARIANTS[all_keys[0]]
        
    if variants:
        return random.choice(variants) # Random variation within the persona category
    return None

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
        logger.info(f"[[OLLAMA ANALYSIS]] Data: {json.dumps(analysis_data, ensure_ascii=False)}")
        
        # 2. Logic: Persona & Prompt Construction
        # ----------------------------
        persona_id, role_name = determine_persona(analysis_data)
        
        # Construct Context Parts
        item_name = analysis_data.get("item_name", "Object")
        is_machine_str = str(analysis_data.get("is_machine", False))
        shape_val = analysis_data.get("shape", "Unknown")
        state_val = analysis_data.get("state", "Normal")
        user_app = analysis_data.get("user_appearance", "None")

        # Get Obsession Instruction
        obsession_instruction = item_obsessions.get_obsession_instruction(item_name)
        
        context_str = (
            f"Context: Machine={is_machine_str}, Shape={shape_val}, State={state_val}.\n"
            f"User Appearance: {user_app}"
        )
        
        # Select Random Topic
        import prompts 
        topic = random.choice(prompts.TOPIC_LIST)
        
        # Voice Selection
        voice_settings = get_voice_uuid(persona_id)
        if voice_settings:
            logger.info(f"[[CREDIT]] COERIOINK: {voice_settings['name']} (Role: {role_name})")
        else:
            logger.warning("[[CREDIT]] No voice settings found.")

        # 3. Text Generation (DeepSeek - Cloud)
        # -----------------------------------
        full_text = deepseek_client.generate_dialogue(
            item_name,
            context_str,
            topic,
            obsession_instruction
        )
        
        logger.info(f"[[DEEPSEEK RAW]] {full_text}")

        # Parse Output "Dialogue by Role"
        # Expected: "Some text... by RoleName"
        import re
        match = re.search(r'(.*)(?:\s+by\s+)(.*)', full_text, re.DOTALL)
        
        if match:
            speech_text = match.group(1).strip()
            # role_suffix = match.group(2).strip() # Unused but good for debug
        else:
            # Fallback if format is broken
            speech_text = full_text.split('by')[0].strip()

        logger.info(f"[[MESSAGE]] {speech_text}")

        if len(speech_text) < 2:
            logger.error("Speech text too short/empty. Skipping TTS.")
            return

        # 4. Audio Synthesis (COEIROINK - Local)
        # --------------------------------------
        if voice_settings:
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
    print("--- Hybrid AI Object Voice System (v8.0: OllamaVision + DeepSeekText) ---")
    logger.info("--- Hybrid AI Object Voice System (v8.0: OllamaVision + DeepSeekText) ---")
    
    event_handler = ImageHandler()
    observer = Observer()
    observer.schedule(event_handler, CAPTURE_DIR, recursive=False)
    observer.start()

    try:
        while True: time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()
