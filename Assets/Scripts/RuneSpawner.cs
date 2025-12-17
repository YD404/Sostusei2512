using UnityEngine;
using System.Collections;

public class RuneSpawner : MonoBehaviour
{
    [Header("設定")]
    [TextArea] public string message = "MAGIC"; // 飛ばす文字
    public GameObject runePrefab; // 文字プレハブ
    public Transform enchantTable; // 吸い込まれる先
    
    [Header("タイミング調整")]
    public float charInterval = 0.1f; // "1文字"が出る間隔
    public float loopInterval = 2.0f; // "単語全体"を繰り返す間隔
    public bool loopEnabled = true;   // ループのオン/オフ

    void Start()
    {
        // ゲーム開始時に自動スタート
        StartCoroutine(AutoSpawnLoop());
    }

    IEnumerator AutoSpawnLoop()
    {
        do
        {
            // 1. 文字列を生成する
            yield return StartCoroutine(SpawnStringRoutine());

            // 2. 次のセットが出るまで待機
            if (loopEnabled)
            {
                yield return new WaitForSeconds(loopInterval);
            }
        } while (loopEnabled);
    }

    // 1回のメッセージ生成処理
    IEnumerator SpawnStringRoutine()
    {
        char[] characters = message.ToCharArray();

        foreach (char c in characters)
        {
            if (c == ' ') continue; // 空白は飛ばす

            // 出現位置を少しランダムに（World座標）
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            
            // Canvas配下に生成（親を指定）+ World座標で位置設定
            GameObject runeObj = Instantiate(runePrefab, spawnPos, Quaternion.identity, transform.parent);
            
            RuneBehavior runeScript = runeObj.GetComponent<RuneBehavior>();
            if (runeScript != null)
            {
                runeScript.Initialize(c.ToString(), enchantTable);
            }

            // 次の文字が出るまで少し待つ
            yield return new WaitForSeconds(charInterval);
        }
    }
}