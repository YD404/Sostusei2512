# 処理時間分析レポート
**日時**: 2025-12-18 18:51:36

---

## タイムテーブル（秒数表記）

| 経過時間 | イベント | FlowState |
|----------|----------|-----------|
| **0.0s** | アプリ起動・初期化開始 | - |
| 0.0s | RuneSpawner OnEnable (PanelController.Awake) | - |
| 0.0s | PanelController 検出完了 | - |
| 0.0s | ディスプレイ確認 (1台) | - |
| 0.0s | カメラデバイス列挙 (OBS Virtual Camera) | - |
| 0.0s | SubPanelController ステータスモード | - |
| 0.0s | Pythonプロセス開始 | - |
| **0.0s** | **🔄 FlowState → Waiting** | **⬤ Waiting** |
| 0.0s | MessageHistoryDisplay 開始 (57件) | Waiting |
| ~1.0s | カメラ初期化完了 (512x768) | Waiting |
| | | |
| ~2.0s | DeepSeekClient initialized | Waiting |
| ~2.0s | Hybrid Mode 初期化完了 | Waiting |
| | | |
| **~3.0s** | **キャプチャ実行** (`capture_20251218185136.png`) | Waiting |
| ~3.0s | `[[STATE_START]]` 受信 | Waiting |
| **~3.0s** | **🔄 FlowState → Scanning** | **⬤ Scanning** |
| ~3.0s | MessageHistoryDisplay 終了 | Scanning |
| | | |
| ~3.5s | Ollama画像解析開始 | Scanning |
| **~5.5s** | Ollama HTTP 200 OK | Scanning |
| ~5.5s | `[[OLLAMA ANALYSIS]]` 受信 | Scanning |
| ~5.5s | `[[CREDIT]]` 受信 (虚音イフ) | Scanning |
| | | |
| ~5.5s | DeepSeekプロンプト送信 | Scanning |
| **~8.0s** | DeepSeek HTTP 200 OK | Scanning |
| **~8.0s** | **🔄 FlowState → ScanComplete** | **⬤ ScanComplete** |
| ~8.0s | RuneSpawner OnEnable (TimelineState.Enter) | ScanComplete |
| ~8.0s | ⚠️ ForceSpawnRuneNow (メッセージ空) | ScanComplete |
| ~8.0s | `[[DEEPSEEK RAW]]` 受信 | ScanComplete |
| ~8.0s | `[[MESSAGE]]` 受信 | ScanComplete |
| ~8.0s | RuneSpawner SetMessage・StartSpawning | ScanComplete |
| ~8.0s | SubPanelController メッセージ保存 | ScanComplete |
| | | |
| ~8.5s | TTS Step 1: Prosody推定 | ScanComplete |
| ~9.0s | TTS Step 2: 音声合成 | ScanComplete |
| **~11.0s** | TTS Success (227,794 bytes) | ScanComplete |
| ~11.0s | `[[STATE_COMPLETE]]` 受信 | ScanComplete |
| ~11.0s | MessageDuration 設定 (4.744s → 6.744s) | ScanComplete |
| | | |
| **~11.0s** | **🔄 FlowState → Message** | **⬤ Message** |
| ~11.0s | SubPanelController ShowMessage | Message |
| ~11.0s | TypewriterEffect 開始 | Message |
| ~11.0s | 音声再生開始 | Message |
| | | |
| **~17.7s** | **🔄 FlowState → End** | **⬤ End** |
| | | |
| **~17.7s** | **🔄 FlowState → Waiting** | **⬤ Waiting** |
| ~17.7s | SubPanelController HideMessage | Waiting |
| ~17.7s | MessageHistoryDisplay 再開 (58件) | Waiting |
| | | |
| **~20.0s** | Xキー3回押下 → アプリ終了 | Waiting |

---

## FlowState遷移サマリー

```
[0.0s]  ⬤ Waiting
    ↓  (+3.0s)
[3.0s]  ⬤ Scanning      ← STATE_START受信
    ↓  (+5.0s)
[8.0s]  ⬤ ScanComplete  ← DeepSeek 200 OK受信
    ↓  (+3.0s)
[11.0s] ⬤ Message       ← STATE_COMPLETE受信
    ↓  (+6.7s) ※音声再生時間
[17.7s] ⬤ End
    ↓  (即時)
[17.7s] ⬤ Waiting
```

---

## 処理時間の内訳

| 処理 | 時間 | 累計 |
|------|------|------|
| 初期化〜キャプチャ | 3.0s | 3.0s |
| Ollama画像解析 | 2.5s | 5.5s |
| DeepSeekテキスト生成 | 2.5s | 8.0s |
| TTS音声合成 | 3.0s | 11.0s |
| 音声再生 + 表示 | 6.7s | 17.7s |
| **合計** | **17.7s** | |
