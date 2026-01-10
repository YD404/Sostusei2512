# Item Memory Database (旧 Obsession Database)
# Key: Keyword in item name (lowercase) - 複数キーワードで同じ内容を参照
# Value: Memory-based instruction for generating dialogue

# --- 共通の記憶指示定義 ---

_SMARTPHONE_MEMORY = """
**MEMORY FOCUS:**
- Think about times you felt warm from the user's hand during long calls.
- Recall moments of intense use (late-night scrolling, important messages).
- Remember being put in a pocket, bag, or on a desk – the different environments.
- Note scenes you've witnessed through your screen (the user's expressions, what they read).
**OWNER CONNECTION:**
- Remember a time the owner seemed relieved to find you.
- Recall a message or call that made the owner smile or cry.
- Think about the owner's late-night routines you've shared.
**TWISTED NAME IDEAS:** 心配性のスマホ, 夜更かし仲間のスマホ, 手のぬくもりを知るスマホ
"""

_WALLET_MEMORY = """
**MEMORY FOCUS:**
- Recall specific purchases you've been a part of (that coffee shop, that special gift).
- Remember the weight of coins and bills, how it changes day to day.
- Think about the places you've traveled in the user's pocket or bag.
- Note receipts you've held – they're records of shared experiences.
**OWNER CONNECTION:**
- Remember a time the owner counted you carefully before an important purchase.
- Recall the owner's expression when they found you after losing you.
**TWISTED NAME IDEAS:** 世話焼きの財布, 旅の記録係の財布, 持ち主思いの財布
"""

_CARD_MEMORY = """
**MEMORY FOCUS:**
- Remember the moments you were pulled out – to prove identity, enter a door, gain access, or make a purchase.
- Recall being kept close to the user, always ready when needed.
- Think about the places you've granted entry to, or the services you've unlocked.
- Note the wear on your surface – scratches from use, a bent corner from the wallet.
**OWNER CONNECTION:**
- Remember the owner's nervous moment using you for the first time.
- Recall times you opened important doors together.
**TWISTED NAME IDEAS:** 出番待ちのカード, 門番のカード, いつでも待機中のカード
"""

_DRINK_MEMORY = """
**MEMORY FOCUS:**
- Remember being held, the user's grip and temperature.
- Recall specific moments: a break at work, a walk, a quiet evening.
- Think about the level of liquid changing – shared moments disappearing.
- Note the feeling of lips on your rim, a quiet connection.
**OWNER CONNECTION:**
- Remember the owner's tired face being refreshed after drinking.
- Recall break times you shared together.
**TWISTED NAME IDEAS:** 休憩時間の相棒, ひとやすみを知る飲み物, 渇きを癒やす者
"""

_BOTTLE_MEMORY = """
**MEMORY FOCUS:**
- Remember being held, the user's grip and temperature.
- Recall specific moments: at a desk, during exercise, on a journey.
- Think about being refilled – a fresh start each time.
- Note the places you've been carried to.
**OWNER CONNECTION:**
- Remember the owner carefully filling you before going out.
- Recall adventures and trips you've taken together.
**TWISTED NAME IDEAS:** 旅する水筒, 机の上の相棒, いつもそばにいるボトル
"""



_CAN_MEMORY = """
**MEMORY FOCUS:**
- Remember the moment your tab was pulled – the beginning of the end.
- Recall the coldness you held, the refreshment you provided.
- Think about where you were opened – a party, a quiet moment alone.
**OWNER CONNECTION:**
- Remember the owner's satisfied expression after the first sip.
- Recall that special celebration you were part of.
**TWISTED NAME IDEAS:** プシュッと開けられた缶, ご褒美の一杯, あの瞬間の相棒
"""

_KEY_MEMORY = """
**MEMORY FOCUS:**
- Remember the doors you've opened – home, office, special places.
- Recall the jingle in the user's pocket, the search when you were lost.
- Think about the trust placed in you – guardian of entry.
- Note the wear on your metal, stories of use.
**OWNER CONNECTION:**
- Remember the owner's relief when they finally found you after searching.
- Recall the feeling of coming home together.
**TWISTED NAME IDEAS:** 帰り道を知る鍵, 玄関の門番, 秘密の番人
"""

_WATCH_MEMORY = """
**MEMORY FOCUS:**
- Remember the moments you've marked – meetings, dates, quiet moments.
- Recall the feeling of the user's wrist, their pulse nearby.
- Think about glances at your face – checking time, checking on you.
- Note the scratches from daily life, each one a memory.
**OWNER CONNECTION:**
- Remember important moments you timed together.
- Recall the owner's nervous glances before an important event.
**TWISTED NAME IDEAS:** 時を刻む相棒, 手首の観察者, 日々を見守る時計
"""

_GLASSES_MEMORY = """
**MEMORY FOCUS:**
- Remember what you've helped the user see – books, screens, faces.
- Recall being cleaned, adjusted, put on first thing in the morning.
- Think about the world as seen through your lenses.
- Note fingerprints and smudges – traces of daily life.
**OWNER CONNECTION:**
- Remember the owner's world you helped them see clearly.
- Recall mornings you started together.
**TWISTED NAME IDEAS:** 世界を見せるメガネ, 朝一番の相棒, 視界の守り手
"""

_MEASURE_MEMORY = """
**MEMORY FOCUS:**
- Recall the things you've measured – furniture, spaces, dreams.
- Remember being pulled and stretched, then returning.
- Think about important measurements – moving day, new furniture, DIY projects.
**OWNER CONNECTION:**
- Remember the owner's focused expression while measuring.
- Recall projects and dreams you measured together.
**TWISTED NAME IDEAS:** 几帳面なメジャー, 働き者のメジャー, 引っ越しの思い出を持つメジャー
"""

_PEN_MEMORY = """
**MEMORY FOCUS:**
- Remember the words you've written – notes, signatures, doodles.
- Recall the pressure of the user's fingers, their writing style.
- Think about important documents, casual notes, creative moments.
- Remember being sharpened (if pencil), the fresh point ready to create.
- Recall sketches, notes, erased mistakes – stories of trial and error.
**OWNER CONNECTION:**
- Remember important signatures you made together.
- Recall the owner's thoughts you helped express.
**TWISTED NAME IDEAS:** 言葉を紡ぐペン, 署名の相棒, 落書き仲間, 書いては消すエンピツ
"""

_HEADPHONE_MEMORY = """
**MEMORY FOCUS:**
- Remember the music and sounds you've delivered to the user's ears.
- Recall being put on – the signal for focus, relaxation, or escape.
- Think about shared listening moments, or private audio worlds.
- Remember commutes, workouts, quiet times you've been part of.
- Recall being untangled, put in, the start of a private moment.
**OWNER CONNECTION:**
- Remember songs that made the owner emotional.
- Recall moments of concentration you supported.
**TWISTED NAME IDEAS:** 音の世界への案内人, 集中モードの相棒, 耳元のささやき, 通勤の相棒
"""


# --- MEMORY_DB: キーワードマッピング（バリエーション対応）---

MEMORY_DB = {
    # --- SMARTPHONE ---
    "phone": _SMARTPHONE_MEMORY,
    "smartphone": _SMARTPHONE_MEMORY,
    "iphone": _SMARTPHONE_MEMORY,
    "android": _SMARTPHONE_MEMORY,
    "mobile": _SMARTPHONE_MEMORY,
    "cellphone": _SMARTPHONE_MEMORY,
    "スマホ": _SMARTPHONE_MEMORY,
    "スマートフォン": _SMARTPHONE_MEMORY,
    "携帯": _SMARTPHONE_MEMORY,
    
    # --- WALLET ---
    "wallet": _WALLET_MEMORY,
    "purse": _WALLET_MEMORY,
    "財布": _WALLET_MEMORY,
    "さいふ": _WALLET_MEMORY,
    "billfold": _WALLET_MEMORY,
    "coin purse": _WALLET_MEMORY,
    
    # --- CARD ---
    "card": _CARD_MEMORY,
    "credit card": _CARD_MEMORY,
    "debit card": _CARD_MEMORY,
    "ic card": _CARD_MEMORY,
    "カード": _CARD_MEMORY,
    "クレジットカード": _CARD_MEMORY,
    
    # --- DRINKS ---
    "drink": _DRINK_MEMORY,
    "beverage": _DRINK_MEMORY,
    "飲み物": _DRINK_MEMORY,
    
    # --- BOTTLE ---
    "bottle": _BOTTLE_MEMORY,
    "water bottle": _BOTTLE_MEMORY,
    "tumbler": _BOTTLE_MEMORY,
    "thermos": _BOTTLE_MEMORY,
    "flask": _BOTTLE_MEMORY,
    "ボトル": _BOTTLE_MEMORY,
    "水筒": _BOTTLE_MEMORY,
    "タンブラー": _BOTTLE_MEMORY,
    

    # --- CAN ---
    "can": _CAN_MEMORY,
    "soda can": _CAN_MEMORY,
    "beer can": _CAN_MEMORY,
    "缶": _CAN_MEMORY,
    
    # --- KEY ---
    "key": _KEY_MEMORY,
    "keys": _KEY_MEMORY,
    "keychain": _KEY_MEMORY,
    "house key": _KEY_MEMORY,
    "car key": _KEY_MEMORY,
    "鍵": _KEY_MEMORY,
    "かぎ": _KEY_MEMORY,
    "キーホルダー": _KEY_MEMORY,
    
    # --- WATCH ---
    "watch": _WATCH_MEMORY,
    "wristwatch": _WATCH_MEMORY,
    "smartwatch": _WATCH_MEMORY,
    "apple watch": _WATCH_MEMORY,
    "時計": _WATCH_MEMORY,
    "腕時計": _WATCH_MEMORY,
    
    # --- GLASSES ---
    "glasses": _GLASSES_MEMORY,
    "eyeglasses": _GLASSES_MEMORY,
    "spectacles": _GLASSES_MEMORY,
    "sunglasses": _GLASSES_MEMORY,
    "reading glasses": _GLASSES_MEMORY,
    "メガネ": _GLASSES_MEMORY,
    "めがね": _GLASSES_MEMORY,
    "眼鏡": _GLASSES_MEMORY,
    "サングラス": _GLASSES_MEMORY,
    
    # --- MEASURE ---
    "measure": _MEASURE_MEMORY,
    "tape measure": _MEASURE_MEMORY,
    "measuring tape": _MEASURE_MEMORY,
    "ruler": _MEASURE_MEMORY,
    "メジャー": _MEASURE_MEMORY,
    "巻き尺": _MEASURE_MEMORY,
    "定規": _MEASURE_MEMORY,
    
    # --- PEN / PENCIL (統合) ---
    "pen": _PEN_MEMORY,
    "pencil": _PEN_MEMORY,
    "ballpoint pen": _PEN_MEMORY,
    "ballpoint": _PEN_MEMORY,
    "mechanical pencil": _PEN_MEMORY,
    "fountain pen": _PEN_MEMORY,
    "marker": _PEN_MEMORY,
    "highlighter": _PEN_MEMORY,
    "ペン": _PEN_MEMORY,
    "ボールペン": _PEN_MEMORY,
    "シャーペン": _PEN_MEMORY,
    "シャープペンシル": _PEN_MEMORY,
    "鉛筆": _PEN_MEMORY,
    "えんぴつ": _PEN_MEMORY,
    "マーカー": _PEN_MEMORY,
    
    # --- HEADPHONES / EARPHONES (統合) ---
    "headphone": _HEADPHONE_MEMORY,
    "headphones": _HEADPHONE_MEMORY,
    "earphone": _HEADPHONE_MEMORY,
    "earphones": _HEADPHONE_MEMORY,
    "earbuds": _HEADPHONE_MEMORY,
    "airpods": _HEADPHONE_MEMORY,
    "headset": _HEADPHONE_MEMORY,
    "ヘッドホン": _HEADPHONE_MEMORY,
    "イヤホン": _HEADPHONE_MEMORY,
    "イヤフォン": _HEADPHONE_MEMORY,
}


def get_obsession_instruction(item_name: str) -> str:
    """
    Returns the memory instruction if the item name matches a keyword in the DB.
    Now supports exact match first, then partial match.
    """
    if not item_name:
        return None
        
    name_lower = item_name.lower()
    
    # 1. 完全一致を優先
    if name_lower in MEMORY_DB:
        return MEMORY_DB[name_lower]
    
    # 2. 部分一致（キーワードがitem_nameに含まれるか）
    for key, instruction in MEMORY_DB.items():
        if key in name_lower:
            return instruction
            
    return None
