import os
import google.generativeai as genai
import json
import logging
from dotenv import load_dotenv
import prompts

# Configure basic logging
logger = logging.getLogger(__name__)

class GeminiClient:
    def __init__(self, model_name="gemini-flash-latest"):
        load_dotenv()
        self.api_key = os.getenv("GEMINI_API_KEY")
        if not self.api_key:
            logger.error("GEMINI_API_KEY not found in environment variables.")
            raise ValueError("GEMINI_API_KEY is missing")
        
        genai.configure(api_key=self.api_key)
        self.model = genai.GenerativeModel(model_name)
        self.model_name = model_name

    def generate_dialogue(self, analysis_data: dict, personality_prompt: str, tone_instruction: str, trigger: str) -> str:
        """
        Generates character dialogue based on context provided by external analysis.
        Returns the spoken text string.
        """
        if not analysis_data:
            return "..."
        
        # Build the structured prompt payload
        prompt_payload = {
            "role_definition": personality_prompt,
            "context": {
                "visual_description": analysis_data.get('description', 'Unknown object'),
                "item_condition": analysis_data.get('item_condition', 'Unknown condition'),
                "tone_instruction": tone_instruction
            },
            "psychological_trigger": trigger,
            "system_instructions": prompts.DIALOGUE_SYSTEM_INSTRUCTIONS,
            "required_output_format": prompts.DIALOGUE_OUTPUT_SCHEMA
        }
        
        # Convert to JSON string for the model
        prompt_text = json.dumps(prompt_payload, ensure_ascii=False, indent=2)
        
        logger.info("Generating dialogue (Gemini Legacy JSON)...")
        # logger.info(f"Prompt Payload: {prompt_text}") # Debug if needed

        try:
            response = self.model.generate_content(
                prompt_text,
                generation_config=genai.GenerationConfig(
                    response_mime_type="application/json",
                    temperature=0.8
                )
            )
            
            data = json.loads(response.text)
            
            # Extract dialogue logic
            if "dialogue" in data:
                return data["dialogue"]
            else:
                logger.warning("No 'dialogue' key in response.")
                return "..."

        except Exception as e:
            logger.error(f"Dialogue Generation Failed: {e}")
            return "..."
