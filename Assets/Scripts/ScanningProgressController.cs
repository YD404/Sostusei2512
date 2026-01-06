using UnityEngine;

/// <summary>
/// Scanning状態中の進捗段階に応じて表示要素を切り替えるコントローラー。
/// 時間ベースで自動的にフェーズを進行する。
/// </summary>
public class ScanningProgressController : MonoBehaviour
{
    [Header("Phase Display Objects")]
    [Tooltip("Phase 1: 撮影中の表示要素")]
    [SerializeField] private GameObject phase1Object;

    [Tooltip("Phase 2: 分析中の表示要素")]
    [SerializeField] private GameObject phase2Object;

    [Tooltip("Phase 3: 生成中の表示要素")]
    [SerializeField] private GameObject phase3Object;

    [Header("Time Settings")]
    [Tooltip("Phase 2 に切り替わるまでの秒数")]
    [SerializeField] private float phase2Delay = 2.0f;

    [Tooltip("Phase 3 に切り替わるまでの秒数（Phase 1 開始から）")]
    [SerializeField] private float phase3Delay = 5.0f;

    private int currentPhase = 0;

    /// <summary>
    /// 現在のフェーズを取得
    /// </summary>
    public int CurrentPhase => currentPhase;

    private void OnEnable()
    {
        // Scanning状態に入るたびにPhase 1にリセット＆タイマー開始
        ShowPhase(1);
        StartPhaseTimers();
    }

    private void OnDisable()
    {
        // Scanning終了時にタイマーをキャンセル
        CancelInvoke();
    }

    /// <summary>
    /// 時間ベースでフェーズを進行させるタイマーを開始
    /// </summary>
    private void StartPhaseTimers()
    {
        CancelInvoke(); // 既存のタイマーをクリア
        Invoke(nameof(GoToPhase2), phase2Delay);
        Invoke(nameof(GoToPhase3), phase3Delay);
    }

    private void GoToPhase2()
    {
        if (currentPhase < 2)
        {
            ShowPhase(2);
        }
    }

    private void GoToPhase3()
    {
        if (currentPhase < 3)
        {
            ShowPhase(3);
        }
    }

    /// <summary>
    /// 指定したフェーズの表示要素のみを表示する
    /// </summary>
    /// <param name="phase">表示するフェーズ (1-3)</param>
    public void ShowPhase(int phase)
    {
        if (phase < 1 || phase > 3)
        {
            Debug.LogWarning($"[ScanningProgress] 無効なフェーズ: {phase}");
            return;
        }

        currentPhase = phase;

        // 全フェーズを非表示にしてから指定フェーズのみ表示
        if (phase1Object != null) phase1Object.SetActive(phase == 1);
        if (phase2Object != null) phase2Object.SetActive(phase == 2);
        if (phase3Object != null) phase3Object.SetActive(phase == 3);

        string phaseName = phase switch
        {
            1 => "Capturing",
            2 => "Analyzing",
            3 => "Generating",
            _ => "Unknown"
        };

        Debug.Log($"[ScanningProgress] Phase {phase}: {phaseName}");
    }

    /// <summary>
    /// Phase 1にリセットする
    /// </summary>
    public void Reset()
    {
        ShowPhase(1);
        StartPhaseTimers();
    }
}
