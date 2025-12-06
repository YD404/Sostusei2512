using UnityEngine;
using TMPro;
using System.IO;

// セリフ用TMPと、タイプライターエフェクトは必須
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(TypewriterEffectTMP))]
public class PythonMessageTMP : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField, Tooltip("メインのセリフを表示するTMP（自動取得）")]
    private TextMeshProUGUI textMessageTMP; // メインのセリフ

    [SerializeField, Tooltip("★ここに追加したCreditTextをドラッグ＆ドロップしてください")]
    public TextMeshProUGUI creditTextTMP;   // ★追加：クレジット表示用

    private TypewriterEffectTMP typewriterEffect;
    private string logFilePath;

    void Awake()
    {
        // メインのTMPは同じオブジェクトにある前提
        textMessageTMP = GetComponent<TextMeshProUGUI>();
        typewriterEffect = GetComponent<TypewriterEffectTMP>();

        logFilePath = Path.Combine(Application.streamingAssetsPath, "Message.txt");

        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        // 初期化：テキストを空にする
        if (textMessageTMP != null) textMessageTMP.text = "";
        if (creditTextTMP != null) creditTextTMP.text = ""; // クレジットも空に
    }

    // ★追加：クレジット情報をセットする関数
    // Pythonからの [[CREDIT]] ... を受け取ったらこれを呼ぶ
    public void SetCredit(string creditInfo)
    {
        Debug.Log($"[PythonMessageTMP] クレジット更新リクエスト: {creditInfo}");
        if (creditTextTMP != null)
        {
            creditTextTMP.text = creditInfo;
            Debug.Log($"[PythonMessageTMP] テキスト更新完了: {creditTextTMP.text}");
        }
    }

    // 既存：メッセージ受信関数
    public void ReceiveMessage(string messageLine)
    {
        if (textMessageTMP == null) return;

        if (!string.IsNullOrEmpty(messageLine))
        {
            textMessageTMP.text = messageLine;
            WriteToFile(messageLine);
            // ※注意: ここではまだ表示されません（StartTypewriterが呼ばれるまで）
        }
    }

    private void WriteToFile(string text)
    {
        try
        {
            File.AppendAllText(logFilePath, text + System.Environment.NewLine);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }

    public void StartTypewriter()
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.StartDisplay();
        }
    }
}