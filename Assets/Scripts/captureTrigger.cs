using UnityEngine;
using System.Collections; // コルーチンを使用するために必要

public class captureTrigger : MonoBehaviour
{
    [SerializeField]
    private WebCamCapture captureModule;

    public KeyCode captureKey = KeyCode.Space; // キャプチャに使用するキー
    public float captureDelay = 5f; // 遅延時間（秒）

    // 既に遅延実行が要求されているか（連打防止用）
    private bool isWaitingForCapture = false;

    void Update()
    {
        // 指定されたキーが押され、かつ現在待機中でない場合
        if (Input.GetKeyDown(captureKey) && !isWaitingForCapture)
        {
            if (captureModule != null)
            {
                // コルーチンを開始して遅延処理を行う
                StartCoroutine(DelayedCapture());
            }
            else
            {
                Debug.LogWarning("captureModule が設定されていません。");
            }
        }
    }

    // 遅延後にキャプチャを実行するコルーチン
    private IEnumerator DelayedCapture()
    {
        // 待機状態にする（連打を無視するため）
        isWaitingForCapture = true;
        Debug.Log($"{captureDelay}秒後にキャプチャを実行します...");

        // 指定された秒数だけ待機
        yield return new WaitForSeconds(captureDelay);

        // キャプチャメソッドを呼び出す
        captureModule.CaptureAndSave();
        Debug.Log("キャプチャを実行しました。");

        // 待機状態を解除（次のキー入力に備える）
        isWaitingForCapture = false;
    }
}