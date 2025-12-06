using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MessageWindowManager : MonoBehaviour
{
    [Header("参照")]
    public GameObject messageWindowPrefab;
    public Transform contentParent;

    [Header("設定")]
    public int maxLines = 50;       // 画面内の最大表示数
    public float replaceInterval = 0.1f; // 入れ替え間隔
    public Vector2 margin = new Vector2(50, 50);

    private string[] allMessages;
    private int nextMessageIndex = 0;
    private float timer;

    private Queue<GameObject> activeWindows = new Queue<GameObject>();

    void Update()
    {
        if (allMessages == null || allMessages.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= replaceInterval)
        {
            timer = 0;
            UpdateOneWindow();
        }
    }

    // ファイル読み込み時に呼ばれる
    public void UpdateMessageWindows(string[] messages)
    {
        if (messages == null) return;

        allMessages = messages.Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();

        if (allMessages.Length == 0) return;

        // 【追加】初回ロード時、まだウィンドウがなければ一気に上限まで生成する
        if (activeWindows.Count == 0)
        {
            for (int i = 0; i < maxLines; i++)
            {
                CreateNextWindow();
            }
        }
    }

    private void UpdateOneWindow()
    {
        // 1. 一番古いものを消す
        if (activeWindows.Count >= maxLines)
        {
            GameObject oldObj = activeWindows.Dequeue();
            if (oldObj != null)
            {
                var anim = oldObj.GetComponent<MessageWindowAnimation>();
                if (anim != null) anim.Dismiss();
                else Destroy(oldObj);
            }
        }

        // 2. 新しいものを1つ作る
        CreateNextWindow();
    }

    // 次のメッセージを使ってウィンドウを生成する共通処理
    private void CreateNextWindow()
    {
        if (allMessages == null || allMessages.Length == 0) return;

        string msg = allMessages[nextMessageIndex];
        CreateWindow(msg);

        // インデックスを進める（ループ）
        nextMessageIndex++;
        if (nextMessageIndex >= allMessages.Length)
        {
            nextMessageIndex = 0;
        }
    }

    private void CreateWindow(string msg)
    {
        GameObject obj = Instantiate(messageWindowPrefab, contentParent);
        activeWindows.Enqueue(obj);

        var tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = msg;

        RectTransform rect = obj.GetComponent<RectTransform>();
        RectTransform parentRect = contentParent.GetComponent<RectTransform>();
        SetRandomPosition(rect, parentRect.rect.width, parentRect.rect.height);
    }

    private void SetRandomPosition(RectTransform rect, float areaWidth, float areaHeight)
    {
        if (rect == null) return;
        float xRange = (areaWidth / 2) - margin.x;
        float yRange = (areaHeight / 2) - margin.y;
        rect.anchoredPosition = new Vector2(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange));
    }
}