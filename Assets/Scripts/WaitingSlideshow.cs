using UnityEngine;

/// <summary>
/// 画像スライドショーを表示するシステム。
/// 事前に配置した複数のUI Imageオブジェクトを順番にアクティブ/非アクティブで切り替える。
/// 各画像のサイズ・位置は個別に調整可能。
/// </summary>
public class WaitingSlideshow : MonoBehaviour
{
    [Header("スライドショー設定")]
    [Tooltip("スライドショーに使用するUI画像オブジェクトの配列（事前に配置済み）")]
    [SerializeField] private GameObject[] slideObjects;

    [Tooltip("画像の切り替え間隔（秒）")]
    [SerializeField] private float slideInterval = 5.0f;

    [Header("フェード設定")]
    [Tooltip("フェード演出を使用するか")]
    [SerializeField] private bool useFade = true;

    [Tooltip("フェード時間（秒）")]
    [SerializeField] private float fadeDuration = 0.5f;

    private int currentIndex = 0;
    private float timer = 0f;

    // フェード用
    private bool isFading = false;
    private float fadeTimer = 0f;
    private bool isFadingOut = true;
    private CanvasGroup currentCanvasGroup;
    private CanvasGroup nextCanvasGroup;

    void OnEnable()
    {
        // 有効化時に初期化
        currentIndex = 0;
        timer = 0f;
        isFading = false;

        // 全てのスライドを非表示にして、最初の1枚だけ表示
        if (slideObjects != null && slideObjects.Length > 0)
        {
            for (int i = 0; i < slideObjects.Length; i++)
            {
                if (slideObjects[i] != null)
                {
                    slideObjects[i].SetActive(i == 0);
                    // CanvasGroupがあればアルファを1に
                    var cg = slideObjects[i].GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = (i == 0) ? 1f : 0f;
                }
            }
        }
    }

    void Update()
    {
        if (slideObjects == null || slideObjects.Length == 0) return;

        // 画像が1枚以下なら切り替え不要
        if (slideObjects.Length <= 1) return;

        // フェード処理中
        if (isFading)
        {
            UpdateFade();
            return;
        }

        // タイマー更新
        timer += Time.deltaTime;

        if (timer >= slideInterval)
        {
            timer = 0f;
            NextSlide();
        }
    }

    /// <summary>
    /// 次のスライドへ切り替え
    /// </summary>
    private void NextSlide()
    {
        int nextIndex = (currentIndex + 1) % slideObjects.Length;

        if (useFade)
        {
            // フェード用CanvasGroupを取得（なければ追加）
            currentCanvasGroup = GetOrAddCanvasGroup(slideObjects[currentIndex]);
            nextCanvasGroup = GetOrAddCanvasGroup(slideObjects[nextIndex]);

            // 次のスライドを表示（アルファ0で）
            slideObjects[nextIndex].SetActive(true);
            nextCanvasGroup.alpha = 0f;

            // フェード開始
            isFading = true;
            isFadingOut = true;
            fadeTimer = 0f;
        }
        else
        {
            // 即時切り替え
            slideObjects[currentIndex].SetActive(false);
            slideObjects[nextIndex].SetActive(true);
            currentIndex = nextIndex;
        }
    }

    /// <summary>
    /// フェード処理の更新
    /// </summary>
    private void UpdateFade()
    {
        fadeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(fadeTimer / fadeDuration);

        // クロスフェード: 現在をフェードアウト、次をフェードイン
        if (currentCanvasGroup != null) currentCanvasGroup.alpha = 1f - t;
        if (nextCanvasGroup != null) nextCanvasGroup.alpha = t;

        if (t >= 1f)
        {
            // フェード完了
            int nextIndex = (currentIndex + 1) % slideObjects.Length;
            slideObjects[currentIndex].SetActive(false);
            currentIndex = nextIndex;
            isFading = false;
        }
    }

    /// <summary>
    /// CanvasGroupを取得または追加
    /// </summary>
    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        if (obj == null) return null;
        
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }
        return cg;
    }
}
