using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Imageに鼓動のような点滅・スケーリング演出を行うスクリプト。
/// AnimationCurveで緩急を調整可能。
/// </summary>
[RequireComponent(typeof(Image))]
public class ImageHeart : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("1分あたりの鼓動回数 (BPM)")]
    [SerializeField] private float bpm = 60f;

    [Tooltip("鼓動のカーブ (横軸: 0〜1の時間, 縦軸: 0〜1の強度)")]
    [SerializeField] private AnimationCurve pulseCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),      // 開始: ゆっくりスタート
        new Keyframe(0.15f, 1f, 0f, 0f),   // ピーク: 急上昇
        new Keyframe(0.4f, 0.3f, -1f, -1f), // 少し戻る
        new Keyframe(0.5f, 0.5f, 2f, 0f),  // 2回目の軽い拍動
        new Keyframe(1f, 0f, -2f, 0f)      // 終了: ゆっくり戻る
    );

    [Header("Scale Animation")]
    [Tooltip("スケールアニメーションを有効にする")]
    [SerializeField] private bool enableScale = true;

    [Tooltip("最小スケール")]
    [SerializeField] private float minScale = 1.0f;

    [Tooltip("最大スケール")]
    [SerializeField] private float maxScale = 1.15f;

    [Header("Alpha Animation")]
    [Tooltip("透明度アニメーションを有効にする")]
    [SerializeField] private bool enableAlpha = true;

    [Tooltip("最小透明度")]
    [SerializeField] private float minAlpha = 0.5f;

    [Tooltip("最大透明度")]
    [SerializeField] private float maxAlpha = 1.0f;

    private Image targetImage;
    private RectTransform rectTransform;
    private Color originalColor;
    private Vector3 originalScale;
    private float time = 0f;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        originalColor = targetImage.color;
        originalScale = rectTransform.localScale;
    }

    private void Update()
    {
        // BPMから1拍の周期を計算 (秒)
        float beatDuration = 60f / bpm;

        // 時間を進める (0〜1にループ)
        time += Time.deltaTime / beatDuration;
        if (time >= 1f) time -= 1f;

        // カーブから値を取得 (0〜1)
        float pulse = pulseCurve.Evaluate(time);

        // スケールアニメーション
        if (enableScale)
        {
            float scale = Mathf.Lerp(minScale, maxScale, pulse);
            rectTransform.localScale = originalScale * scale;
        }

        // 透明度アニメーション
        if (enableAlpha)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
            Color c = originalColor;
            c.a = alpha;
            targetImage.color = c;
        }
    }

    /// <summary>
    /// BPMを動的に変更する
    /// </summary>
    public void SetBPM(float newBpm)
    {
        bpm = newBpm;
    }

    /// <summary>
    /// スケール範囲を動的に変更する
    /// </summary>
    public void SetScaleRange(float min, float max)
    {
        minScale = min;
        maxScale = max;
    }

    /// <summary>
    /// 透明度範囲を動的に変更する
    /// </summary>
    public void SetAlphaRange(float min, float max)
    {
        minAlpha = min;
        maxAlpha = max;
    }
}
