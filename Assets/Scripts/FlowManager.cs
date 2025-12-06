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

    // 各状態の固定表示時間（秒）
    private const float STATE_DURATION = 10.0f;

    private enum FlowState
    {
        Waiting,
        Scanning,
        ScanComplete,
        Message,
        End
    }

    private FlowState currentState;

    void Start()
    {
        if (pythonLauncher == null)
        {
            Debug.LogError("PythonLauncherがInspectorで設定されていません！");
            return;
        }

        ChangeState(FlowState.Waiting);
    }

    private void ChangeState(FlowState newState)
    {
        CancelInvoke();
        currentState = newState;
        Debug.Log($"--- FlowState 変更: {currentState} ---");

        switch (currentState)
        {
            case FlowState.Waiting:
                panelController.ShowWaitingPanel();
                break;

            case FlowState.Scanning:
                panelController.ShowScanningPanel();
                break;

            case FlowState.ScanComplete:
                bool displayed = panelController.ShowScanCompletePanel();
                // パネル表示の有無にかかわらず次へ進むロジック
                float delay = displayed ? STATE_DURATION : 0.1f;
                Invoke(nameof(OnCompleteFinished), delay);
                break;

            case FlowState.Message:
                panelController.ShowMessagePanel();
                if (pythonMessageDisplay != null)
                {
                    pythonMessageDisplay.StartTypewriter();
                }
                Invoke(nameof(OnMessageFinished), STATE_DURATION);
                break;

            case FlowState.End:
                bool endDisplayed = panelController.ShowEndPanel();
                float endDelay = endDisplayed ? STATE_DURATION : 0.1f;
                Invoke(nameof(OnEndFinished), endDelay);
                break;
        }
    }

    // --- Pythonからのメッセージ受信部 ---

    public void OnPythonMessageReceived(string line)
    {
        Debug.Log($"[Python受信] {line}");

        // 1. 開始タグ [[STATE_START]]
        if (line.Contains("[[STATE_START]]"))
        {
            if (currentState == FlowState.Waiting)
            {
                Debug.Log("タグ検知: 開始 -> Scanningへ");
                ChangeState(FlowState.Scanning);
            }
        }
        // ★追加部分: クレジットタグ [[CREDIT]]
        // メッセージよりも先に来るので、ここで受け取ってセットしておく
        else if (line.Contains("[[CREDIT]]"))
        {
            // タグを除去して、中身（例: "CV: ディアちゃん"）を取り出す
            string creditBody = line.Replace("[[CREDIT]]", "").Trim();
            Debug.Log($"[FlowManager] クレジット受信確認: {creditBody}");
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

            if (pythonMessageDisplay != null)
            {
                pythonMessageDisplay.ReceiveMessage(messageBody);
            }
        }
        // 3. 完了タグ [[STATE_COMPLETE]]
        else if (line.Contains("[[STATE_COMPLETE]]"))
        {
            if (currentState == FlowState.Scanning)
            {
                Debug.Log("タグ検知: 完了 -> ScanCompleteへ");
                ChangeState(FlowState.ScanComplete);
            }
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