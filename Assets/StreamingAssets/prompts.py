
# Image Analysis Prompt (This one remains a string as it's for user content in V-LLM)
ANALYSIS_PROMPT = """
Describe the main object and the user in the image in detail.
Then, based on the object's condition and type, assign a "Personality ID".

Personality IDs:
- "external_brain": Smartphone, Watch, Tablet.
- "lifeline": Bottle, Drink, Food, Medicine.
- "gatekeeper": Wallet, Key, Money, Card.
- "muse": Pen, Notebook, Camera, Laptop.
- "sanctuary": Earphone, Plush, Handkerchief.
- "mask": Cosmetic, Mirror, Accessory, Glasses.
- "observer": Others.

Output strict JSON:
{
  "description": "Short description of visual details",
  "item_name": "Name of object",
  "item_condition": "Description of scratches, dirt, usage, or newness",
  "condition_score": "1 (New/Clean) to 5 (Old/Damaged)",
  "personality_id": "ID"
}
"""

# Dialogue Generation Instructions
# These will be embedded into the JSON payload
DIALOGUE_SYSTEM_INSTRUCTIONS = [
    "Combine the provided 'Context' with the 'Trigger'.",
    "Perform a 'Cold Reading' using the 'Specific Vague' technique.",
    "STRICTLY follow the CORE RULES defined in the Character definition.",
    "Output a short Japanese spoken line (Max 120 chars)."
]

# The Output Format Requirement
DIALOGUE_OUTPUT_SCHEMA = {
    "thought_process": "How to connect visual description with trigger",
    "dialogue": "Japanese spoken line"
}
