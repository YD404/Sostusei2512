using UnityEngine;
using System.Collections; // コルーチンを使用するために必要

public class captureTrigger : MonoBehaviour
{
    [SerializeField]
    private WebCamCapture captureModule;

    [SerializeField]
    private FlowManager flowManager; // ★追加: キャプチャ時にScanningに切り替え

    public KeyCode captureKey = KeyCode.Space; // キャプチャに使用するキー

    // 既に遅延実行が要求されているか（連打防止用）
    private bool isWaitingForCapture = false;

    void Update()
    {
        // 指定されたキーが押され、かつ現在待機中でない場合
        if (Input.GetKeyDown(captureKey) && !isWaitingForCapture)
        {
            if (captureModule != null)
            {
                // 即座にキャプチャ実行
                // 連打防止のため簡易的にフラグ制御をするなら、一瞬だけ待つか、
                // あるいはPython側の処理が終わるまで入力を受け付けない制御が必要かもしれないが、
                // ここでは「即座に」という要望通り遅延なしで実行する。
                // ただし、連続実行を防ぐならコルーチンで1フレーム待つ程度にするか、
                // そのまま実行してすぐにフラグを戻す。
                
                // 今回はシンプルに即時実行
                Debug.Log("キャプチャを実行します...");
                captureModule.CaptureAndSave();
                Debug.Log("キャプチャを実行しました。");

                // ★追加: キャプチャ実行と同時にScanningに切り替え
                if (flowManager != null)
                {
                    flowManager.NotifyScanStart();
                }
            }
            else
            {
                Debug.LogWarning("captureModule が設定されていません。");
            }
        }
    }
}