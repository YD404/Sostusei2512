// DisplayManager.cs - マルチディスプレイ管理スクリプト
using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    void Start()
    {
        // 利用可能なディスプレイを確認
        Debug.Log($"ディスプレイ数: {Display.displays.Length}");
        
        // 2つ以上のディスプレイがある場合、2番目のディスプレイを有効化
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("2番目のディスプレイを有効化しました");
        }
        
        // 3つ以上のディスプレイがある場合、必要に応じて有効化
        if (Display.displays.Length > 2)
        {
            Display.displays[2].Activate();
        }
    }
}