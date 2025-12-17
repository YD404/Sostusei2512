using UnityEngine;

/// <summary>
/// 体験フローの状態(シーケンス)を管理する。
/// </summary>
public class FlowManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PanelController panelController;
    [SerializeField] private PythonLauncher pythonLauncher;
    [SerializeField] private PythonMessageTMP pythonMessageDisplay;

    [Header("Sub Display (Optional)")]
    [Tooltip("サブディスプレイ用コントローラー。未設定でも動作します。")]
    [SerializeField] private SubPanelController subPanelController;

    [Header("Rune Effect (Optional)")]
    [Tooltip("ScanComplete時のルーンエフェクト。未設定でも動作します。")]
    [SerializeField] private RuneSpawner runeSpawner;
    [Tooltip("チェックON: Pythonからの全ログを使用 / OFF: [[MESSAGE]]のみ使用")]
    [SerializeField] private bool useAllPythonLogsForRune = false;

    // 各状態の固定表示時間（秒）
    private const float STATE_DURATION = 10.0f;
    private float pendingMessageDuration = -1.0f; // 音声からリクエストされた時間（未適用なら正の値）

    private enum FlowState
    {
        Waiting,
        Scanning,
        ScanComplete,
        Message,
        End
    }

    private FlowState currentState;

    // アーカイブ用: 現在のメッセージとクレジットを保持
    private string currentMessage = "";
    private string currentCredit = "";
    
    // RuneSpawner用: 取得前に受信したメッセージをバッファ
    private string pendingRuneMessage = null;

    void Start()
    {
        if (pythonLauncher == null)
        {
            Debug.LogError("PythonLauncherがInspectorで設定されていません！");
            return;
        }

        // ★追加: pythonMessageDisplay が未設定の場合、PanelController から取得
        if (pythonMessageDisplay == null && panelController != null)
        {
            // PanelController の Start が先に呼ばれている必要があるため、
            // 1フレーム待ってから取得を試みる
            StartCoroutine(TryGetMessageDisplayDelayed());
        }

        ChangeState(FlowState.Waiting);
    }

    private System.Collections.IEnumerator TryGetMessageDisplayDelayed()
    {
        yield return null; // 1フレーム待機
        
        if (pythonMessageDisplay == null && panelController != null)
        {
            pythonMessageDisplay = panelController.MessageDisplay;
            if (pythonMessageDisplay != null)
            {
                Debug.Log("[FlowManager] PanelController から PythonMessageTMP を自動取得しました。");
            }
        }

        // ★追加: RuneSpawner も PanelController から取得
        if (runeSpawner == null && panelController != null)
        {
            runeSpawner = panelController.RuneSpawnerDisplay;
            if (runeSpawner != null)
            {
                Debug.Log("[FlowManager] PanelController から RuneSpawner を自動取得しました。");
                
                // バッファにメッセージがあれば送信
                if (!string.IsNullOrEmpty(pendingRuneMessage))
                {
                    Debug.Log($"[FlowManager] バッファのメッセージをRuneSpawnerに送信: {pendingRuneMessage}");
                    runeSpawner.SetMessage(pendingRuneMessage);
                    pendingRuneMessage = null;
                }
            }
        }
    }

    /// <summary>
    /// 外部（MessageVoicePlayerなど）からMessage状態の表示時間を指定する
    /// 音声長さ + 2.0秒 とする
    /// </summary>
    /// <param name="clipLength">音声クリップの秒数</param>
    public void SetMessageDuration(float clipLength)
    {
        float newDuration = clipLength + 2.0f;
        Debug.Log($"[FlowManager] MessageDuration 更新リクエスト: 音声{clipLength}s -> 表示{newDuration}s");

        if (currentState == FlowState.Message)
        {
            // すでにMessage中なら、タイマーをリセットして時間を再設定
            CancelInvoke(nameof(OnMessageFinished));
            Invoke(nameof(OnMessageFinished), newDuration);
            Debug.Log($"[FlowManager] 現在のMessage状態の残り時間を {newDuration}s に延長しました。");
        }
        else
        {
            // まだMessageになっていないなら、次回の遷移で使うように保存
            pendingMessageDuration = newDuration;
        }
    }

    private void ChangeState(FlowState newState)
    {
        CancelInvoke();
        currentState = newState;
        Debug.Log($"--- FlowState 変更: {currentState} ---");

        // Scanning開始時に古いpendingDurationをクリア（念のため）
        if (currentState == FlowState.Scanning)
        {
            pendingMessageDuration = -1.0f;
        }

        switch (currentState)
        {
            case FlowState.Waiting:
                panelController.ShowWaitingPanel();
                // サブディスプレイ: ステータスをクリア
                if (subPanelController != null) subPanelController.SetStatus("");
                break;

            case FlowState.Scanning:
                panelController.ShowScanningPanel();
                // ステータスはPythonログで更新される
                break;

            case FlowState.ScanComplete:
                bool displayed = panelController.ShowScanCompletePanel();
                // ステータスはPythonログで更新される
                // パネル表示の有無にかかわらず次へ進むロジック
                float delay = displayed ? STATE_DURATION : 0.1f;
                Invoke(nameof(OnCompleteFinished), delay);
                break;

            case FlowState.Message:
                panelController.ShowMessagePanel();
                // ステータスをクリア（ログのみ表示）
                if (subPanelController != null) subPanelController.SetStatus("");
                if (pythonMessageDisplay != null)
                {
                    pythonMessageDisplay.StartTypewriter();
                }

                // 時間決定ロジック
                float duration = STATE_DURATION; // デフォルト 10s
                if (pendingMessageDuration > 0)
                {
                    duration = pendingMessageDuration;
                    pendingMessageDuration = -1.0f; // 消費
                    Debug.Log($"[FlowManager] 音声に基づく指定時間({duration}s)を適用します。");
                }
                
                Invoke(nameof(OnMessageFinished), duration);
                break;

            case FlowState.End:
                // ★ Handover Phase: サブディスプレイにログを追加
                if (subPanelController != null && !string.IsNullOrEmpty(currentMessage))
                {
                    subPanelController.AddLogEntry(currentMessage, currentCredit);
                    subPanelController.SetStatus("");  // ステータスをクリア
                    // 送信後にクリア
                    currentMessage = "";
                    currentCredit = "";
                }

                bool endDisplayed = panelController.ShowEndPanel();
                float endDelay = endDisplayed ? 5.0f : 0.1f;
                Invoke(nameof(OnEndFinished), endDelay);
                break;
        }
    }

    // --- Pythonからのメッセージ受信部 ---

    public void OnPythonMessageReceived(string line)
    {
        Debug.Log($"[Python受信] {line}");

        // ★デバッグ用: 全ログをRuneSpawnerに転送
        if (useAllPythonLogsForRune)
        {
            if (runeSpawner != null)
            {
                runeSpawner.SetMessage(line);
            }
            else
            {
                // まだ取得されていない場合はバッファに保存
                pendingRuneMessage = line;
                Debug.Log($"[FlowManager] RuneSpawner未取得のためバッファに保存: {line}");
            }
        }

        // 1. 開始タグ or ログ検知
        if (line.Contains("[[STATE_START]]") || line.Contains("Analyzing image (Local Ollama):"))
        {
            if (currentState == FlowState.Waiting)
            {
                Debug.Log($"検知: 開始({line}) -> Scanningへ");
                ChangeState(FlowState.Scanning);
            }
            // サブディスプレイにPythonログを転送
            if (subPanelController != null) subPanelController.SetStatus(line);
        }
        // ★追加部分: クレジットタグ [[CREDIT]]
        // メッセージよりも先に来るので、ここで受け取ってセットしておく
        else if (line.Contains("[[CREDIT]]"))
        {
            // タグを除去して、中身（例: "CV: ディアちゃん"）を取り出す
            string creditBody = line.Replace("[[CREDIT]]", "").Trim();
            Debug.Log($"[FlowManager] クレジット受信確認: {creditBody}");
            
            // アーカイブ用に保持
            currentCredit = creditBody;
            
            if (pythonMessageDisplay != null)
            {
                // PythonMessageTMP に新しく作った SetCredit関数 を呼ぶ
                pythonMessageDisplay.SetCredit(creditBody);
            }
        }
        // 2. メッセージタグ [[MESSAGE]]
        else if (line.Contains("[[MESSAGE]]"))
        {
            // タグを除去して、本文だけをUIに渡す
            string messageBody = line.Replace("[[MESSAGE]]", "").Trim();

            // アーカイブ用に保持
            currentMessage = messageBody;

            if (pythonMessageDisplay != null)
            {
                pythonMessageDisplay.ReceiveMessage(messageBody);
            }

            // ★追加: RuneSpawnerにもメッセージを転送
            if (runeSpawner != null)
            {
                runeSpawner.SetMessage(messageBody);
            }
        }
        // 3. 完了タグ or ログ検知
        else if (line.Contains("[[STATE_COMPLETE]]") || line.Contains("Generating dialogue (Gemini Legacy JSON)"))
        {
            // サブディスプレイにPythonログを転送
            if (subPanelController != null) subPanelController.SetStatus(line);
            
            if (currentState == FlowState.Scanning)
            {
                Debug.Log($"検知: 完了({line}) -> ScanCompleteへ");
                ChangeState(FlowState.ScanComplete);
            }
        }
        // 4. その他のPythonログもサブディスプレイに転送（Scanning/ScanComplete中のみ）
        else if (currentState == FlowState.Scanning || currentState == FlowState.ScanComplete)
        {
            if (subPanelController != null) subPanelController.SetStatus(line);
        }
    }

    public void OnPythonError(string errorMessage)
    {
        Debug.LogError($"Pythonエラー: {errorMessage}");
        // エラー時は一旦Waitingに戻す
        ChangeState(FlowState.Waiting);
    }

    // --- Invokeコールバック ---

    private void OnCompleteFinished()
    {
        ChangeState(FlowState.Message);
    }

    private void OnMessageFinished()
    {
        ChangeState(FlowState.End);
    }

    private void OnEndFinished()
    {
        ChangeState(FlowState.Waiting);
    }
}