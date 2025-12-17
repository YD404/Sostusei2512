using UnityEngine;
using TMPro;

/// <summary>
/// サブディスプレイ用アーカイブコントローラー。
/// ステート切り替えではなく、ログエントリを蓄積していく方式。
/// FlowManagerからAddLogEntry()を呼び出してメッセージを追加する。
/// </summary>
public class SubPanelController : MonoBehaviour
{
    [Header("Archive Container")]
    [Tooltip("ログアイテムを追加するコンテナ（ScrollViewのContent等）")]
    [SerializeField] private Transform contentContainer;

    [Header("Log Item Prefab")]
    [Tooltip("ログエントリのプレハブ（TextMeshProUGUIを持つ）")]
    [SerializeField] private GameObject logItemPrefab;

    [Header("Fallback Text (仮置き)")]
    [Tooltip("プレハブ未設定時に直接テキストを追加するTMP")]
    [SerializeField] private TextMeshProUGUI fallbackText;

    [Header("Status / Log Display")]
    [Tooltip("Pythonログを表示するLogBufferDisplay（推奨）")]
    [SerializeField] private LogBufferDisplay logBufferDisplay;

    [Tooltip("フォールバック: ステータス表示用TMP（LogBufferDisplay未設定時）")]
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        if (contentContainer == null && fallbackText == null)
        {
            Debug.LogWarning("SubPanelController: ContentContainerまたはFallbackTextを設定してください。");
        }

        // 初期化時にfallbackTextをクリア
        if (fallbackText != null)
        {
            fallbackText.text = "";
        }
    }

    /// <summary>
    /// ログエントリを追加する。FlowManagerから呼び出される。
    /// </summary>
    /// <param name="message">メッセージ本文</param>
    /// <param name="credit">クレジット情報（CV名など）</param>
    public void AddLogEntry(string message, string credit = "")
    {
        Debug.Log($"[SubPanelController] ログ追加: {message} / Credit: {credit}");

        // プレハブが設定されている場合
        if (logItemPrefab != null && contentContainer != null)
        {
            GameObject newItem = Instantiate(logItemPrefab, contentContainer);
            
            // プレハブ内のTMPを取得してテキスト設定
            TextMeshProUGUI tmp = newItem.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                string displayText = string.IsNullOrEmpty(credit) 
                    ? message 
                    : $"{message}\n<size=70%><color=#888888>{credit}</color></size>";
                tmp.text = displayText;
            }

            // TODO: スライドインアニメーション等の演出を追加
        }
        // フォールバック: 直接テキストに追記
        else if (fallbackText != null)
        {
            string entry = string.IsNullOrEmpty(credit) 
                ? message 
                : $"{message} ({credit})";
            
            // 改行して追記（上に新しいログ）
            if (string.IsNullOrEmpty(fallbackText.text))
            {
                fallbackText.text = entry;
            }
            else
            {
                fallbackText.text = entry + "\n\n" + fallbackText.text;
            }
        }
        else
        {
            Debug.LogWarning("[SubPanelController] ログを表示する手段がありません。ContentContainerまたはFallbackTextを設定してください。");
        }
    }

    /// <summary>
    /// アーカイブをクリアする（デバッグ用）
    /// </summary>
    public void ClearArchive()
    {
        if (contentContainer != null)
        {
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (fallbackText != null)
        {
            fallbackText.text = "";
        }

        Debug.Log("[SubPanelController] アーカイブをクリアしました。");
    }

    /// <summary>
    /// ステータス（Pythonログ）を更新する。FlowManagerから呼び出される。
    /// LogBufferDisplayが設定されている場合はそちらにログを追加する。
    /// </summary>
    /// <param name="status">表示するログ文字列（空文字でクリア）</param>
    public void SetStatus(string status)
    {
        // 空文字の場合はクリア
        if (string.IsNullOrEmpty(status))
        {
            if (logBufferDisplay != null) logBufferDisplay.ClearLogs();
            if (statusText != null) statusText.text = "";
            return;
        }

        // LogBufferDisplayが設定されている場合はそちらを優先
        if (logBufferDisplay != null)
        {
            logBufferDisplay.AddLog(status);
        }
        // フォールバック: 直接TMPに設定
        else if (statusText != null)
        {
            statusText.text = status;
        }
    }
}
