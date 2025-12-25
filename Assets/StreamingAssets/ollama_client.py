import ollama
import json
import re
import logging
import base64
import os
import prompts

# Configure basic logging
logger = logging.getLogger(__name__)

class OllamaClient:
    def __init__(self, model_name="qwen2.5vl:7b"):
        self.model_name = model_name

    def extract_json(self, text):
        try:
            match = re.search(r'```json\s*(\{.*?\})\s*```', text, re.DOTALL)
            if match: return json.loads(match.group(1))
            match = re.search(r'\{.*\}', text, re.DOTALL)
            if match: return json.loads(match.group(0))
            return None
        except Exception:
            return None

    def analyze_image(self, image_path: str) -> dict:
        """
        Analyzes the image using local Ollama (Vision Model).
        Note: Image preprocessing (CLAHE, background removal) is now done
              in main_vision_voice.py before saving to capture/
        """
        if not os.path.exists(image_path):
            logger.error(f"Image not found: {image_path}")
            return None

        logger.info(f"Analyzing image (Local Ollama): {os.path.basename(image_path)}")

        try:
            # 画像をそのまま読み込み（前処理済み）
            with open(image_path, "rb") as f:
                image_data = base64.b64encode(f.read()).decode("utf-8")

            analysis_response = ollama.chat(
                model=self.model_name,
                messages=[{
                    "role": "user",
                    "content": prompts.ANALYSIS_PROMPT,
                    "images": [image_data]
                }],
                options={
                    "temperature": 0.1,      # より保守的な回答
                    "num_predict": 512,      # CoT用に十分なトークン数
                    "top_p": 0.9             # 確率分布の絞り込み
                }
            )
            
            content = analysis_response['message']['content']
            analysis_data = self.extract_json(content)
            
            if not analysis_data:
                logger.warning("Local Analysis JSON parsing failed. Using default.")
                return {
                    "is_machine": False, 
                    "shape": "Other", 
                    "state": "Normal", 
                    "item_name": "Unknown Object", 
                    "user_appearance": "None"
                }
                
            return analysis_data

        except Exception as e:
            logger.error(f"Local Image Analysis Failed: {e}")
            return None

