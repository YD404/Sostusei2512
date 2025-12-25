using UnityEngine;

/// <summary>
/// キャプチャトリガー - スペースキーでPythonにキャプチャコマンドを送信
/// Python側でカメラ撮影とフリッカー対策を行う
/// </summary>
public class captureTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PythonLauncher pythonLauncher;
    [SerializeField] private FlowManager flowManager;

    [Header("Input Settings")]
    public KeyCode captureKey = KeyCode.Space;
    
    [Header("Cooldown Settings")]
    [Tooltip("連打防止のクールダウン時間（秒）")]
    [SerializeField] private float captureCooldown = 3.0f;
    
    // 最後のキャプチャ時刻
    private float lastCaptureTime = -999f;

    void Update()
    {
        // 指定されたキーが押された場合
        if (Input.GetKeyDown(captureKey))
        {
            TriggerCapture();
        }
    }
    
    [Header("Camera Logic")]
    [Tooltip("検索するカメラ名の一部（例: VID:1133）")]
    [SerializeField] private string targetCameraKeyword = "VID:1133";
    
    // ... (フィールド定義) ...

    /// <summary>
    /// キャプチャをトリガーする（外部からも呼び出し可能）
    /// </summary>
    public void TriggerCapture()
    {
        // FlowStateがWaiting以外なら無視
        if (flowManager != null && flowManager.CurrentState != FlowManager.FlowState.Waiting)
        {
            Debug.Log($"[CaptureTrigger] Waiting以外のため無視 (現在: {flowManager.CurrentState})");
            return;
        }
        
        // クールダウンチェック
        if (Time.time - lastCaptureTime < captureCooldown)
        {
            return;
        }
        
        if (pythonLauncher == null) return;
        
        lastCaptureTime = Time.time;
        
        // カメラのインデックスを検索
        int cameraIndex = FindTargetCameraIndex();
        
        // コマンド送信 "CAPTURE <index>"
        string command = $"CAPTURE {cameraIndex}";
        Debug.Log($"[CaptureTrigger] 送信コマンド: {command} (Target: {targetCameraKeyword})");
        
        pythonLauncher.SendCommand(command);
        
        if (flowManager != null)
        {
            flowManager.NotifyScanStart();
        }
    }
    
    /// <summary>
    /// 目標のカメラ名を含むデバイスのインデックスを検索
    /// </summary>
    private int FindTargetCameraIndex()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0) return 0;
        
        for (int i = 0; i < devices.Length; i++)
        {
            // キーワードが含まれているかチェック
            if (devices[i].name.Contains(targetCameraKeyword))
            {
                Debug.Log($"[CaptureTrigger] カメラ発見: {devices[i].name} (Index {i})");
                return i;
            }
        }
        
        Debug.LogWarning($"[CaptureTrigger] キーワード '{targetCameraKeyword}' を含むカメラが見つかりません。デフォルト(0)を使用します。");
        // 見つからない場合は、外部カメラっぽいもの(Index 1)があればそれを、なければ0
        if (devices.Length > 1) return 1;
        return 0;
    }
    
    /// <summary>
    /// クールダウン残り時間を取得
    /// </summary>
    public float CooldownRemaining => Mathf.Max(0, captureCooldown - (Time.time - lastCaptureTime));
}