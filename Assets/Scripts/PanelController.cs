using UnityEngine;

/// <summary>
/// UI Panelの表示/非表示の切り替え機能のみを担当するスクリプト。
/// 「いつ」切り替えるかは、他のスクリプト（フロー制御用）が決定する。
/// </summary>
public class PanelController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject panelWaiting;
    [SerializeField] private GameObject panelScanning;
    [SerializeField] private GameObject panelScanComplete;
    [SerializeField] private GameObject panelMessage;
    [SerializeField] private GameObject panelEnd;

    [Header("Skip Settings")]
    [Tooltip("ScanComplete Panelの表示をスキップするか")]
    [SerializeField] private bool skipScanComplete = false;
    [Tooltip("End Panelの表示をスキップするか")]
    [SerializeField] private bool skipEnd = false;

    //void Start()
    //{
    //    // 初期状態として待機画面のみ表示
    //    ShowWaitingPanel();
    //}

    /// <summary>
    /// 全ての管理対象パネルを非表示にする（内部処理用）
    /// </summary>
    private void HideAllPanels()
    {
        panelWaiting.SetActive(false);
        panelScanning.SetActive(false);
        panelScanComplete.SetActive(false);
        panelMessage.SetActive(false);
        panelEnd.SetActive(false);
    }

    // --- ここから下は、他のスクリプトから呼び出して使用する ---

    /// <summary>
    /// 待機 (Waiting) Panel のみを表示する
    /// </summary>
    public void ShowWaitingPanel()
    {
        HideAllPanels();
        panelWaiting.SetActive(true);
    }

    /// <summary>
    /// スキャン中 (Scanning) Panel のみを表示する
    /// </summary>
    public void ShowScanningPanel()
    {
        HideAllPanels();
        panelScanning.SetActive(true);
    }

    /// <summary>
    /// スキャン完了 (ScanComplete) Panel を表示する。
    /// スキップ設定が有効な場合は表示せず false を返す。
    /// </summary>
    /// <returns>パネルが表示された場合は true、スキップされた場合は false</returns>
    public bool ShowScanCompletePanel()
    {
        HideAllPanels();
        if (skipScanComplete)
        {
            return false; // スキップ
        }

        panelScanComplete.SetActive(true);
        return true; // 表示
    }

    /// <summary>
    /// メッセージ (Message) Panel のみを表示する
    /// </summary>
    public void ShowMessagePanel()
    {
        HideAllPanels();
        panelMessage.SetActive(true);
    }

    /// <summary>
    /// 終了 (End) Panel を表示する。
    /// スキップ設定が有効な場合は表示せず false を返す。
    /// </summary>
    /// <returns>パネルが表示された場合は true、スキップされた場合は false</returns>
    public bool ShowEndPanel()
    {
        HideAllPanels();
        if (skipEnd)
        {
            return false; // スキップ
        }

        panelEnd.SetActive(true);
        return true; // 表示
    }
}