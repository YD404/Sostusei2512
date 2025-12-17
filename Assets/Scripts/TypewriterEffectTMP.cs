using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffectTMP : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public float delay = 0.05f;

    private string fullText;
    private Coroutine typewriterCoroutine;

    void Awake()
    {
        if (tmpText == null)
        {
            tmpText = GetComponent<TextMeshProUGUI>();
        }
    }

    // void Start() { StartDisplay(); } // ← この自動実行ロジックは削除またはコメントアウト

    /// <summary>
    /// 外部から呼ばれ、現在のtmpText.textを元にタイプライター処理を開始する
    /// </summary>
    public void StartDisplay()
    {
        if (tmpText == null) return;

        // 既存のコルーチンを停止
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // ★重要: このメソッドが呼ばれた瞬間のテキストを全文として取得
        fullText = tmpText.text;
        Debug.Log($"[TypewriterEffect] StartDisplay: TextLength={fullText.Length}, Text='{fullText}'");

        // 表示を確実に更新
        tmpText.ForceMeshUpdate();

        // 表示をリセット
        tmpText.maxVisibleCharacters = 0;

        // コルーチンを開始
        typewriterCoroutine = StartCoroutine(ShowText());
    }

    private IEnumerator ShowText()
    {
        int totalChars = fullText.Length;

        for (int i = 0; i <= totalChars; i++)
        {
            tmpText.maxVisibleCharacters = i;

            if (i >= totalChars)
            {
                break;
            }

            yield return new WaitForSeconds(delay);
        }
    }
}