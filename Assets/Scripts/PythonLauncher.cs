using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

public class PythonLauncher : MonoBehaviour, IDisposable
{
    [Header("Dependencies")]
    [Tooltip("結果を通知するFlowManager")]
    [SerializeField] private FlowManager flowManager;

    private Process pythonProcess;
    private string pythonExecutablePath = "/opt/homebrew/bin/python3.11";
    private static readonly Queue<string> resultQueue = new Queue<string>();

    // Start() で起動する
    void Start()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            UnityEngine.Debug.LogWarning("Pythonプロセスは既に実行中です。");
            return;
        }

        string scriptPath = Path.Combine(Application.streamingAssetsPath, "main_vision_voice.py");
        string workingDirectory = Path.Combine(Application.streamingAssetsPath);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutablePath,
            Arguments = scriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {
            pythonProcess = new Process();
            pythonProcess.StartInfo = startInfo;

            // 標準出力のイベントハンドラ
            pythonProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    lock (resultQueue)
                    {
                        resultQueue.Enqueue(args.Data);
                    }
                }
            };
            // 標準エラーのイベントハンドラ
            pythonProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    UnityEngine.Debug.LogError("Python Error: " + args.Data);
                }
            };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            UnityEngine.Debug.Log($"Pythonプロセスを開始しました: {scriptPath}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Pythonプロセスの起動に失敗: {e.Message}");
            // 起動失敗時もFlowManagerに通知
            if (flowManager != null)
            {
                flowManager.OnPythonError("起動失敗: " + e.Message);
            }
        }
    }

    // public void ExecuteScript() は Start() に統合されたため削除

    void Update()
    {
        // メインスレッドでキューを処理
        while (resultQueue.Count > 0)
        {
            string line = null;
            lock (resultQueue)
            {
                if (resultQueue.Count > 0)
                {
                    line = resultQueue.Dequeue();
                }
            }

            if (line != null)
            {
                // FlowManagerが設定されていれば、受信データを渡す
                if (flowManager != null)
                {
                    flowManager.OnPythonMessageReceived(line);
                }
                else
                {
                    // FlowManagerがない場合は、元のデバッグログを出す
                    LogPythonOutput(line);
                }
            }
        }
    }

    // 元のデバッグログ処理（FlowManagerがない場合のフォールバック）
    private void LogPythonOutput(string line)
    {
        if (line.Contains("処理中"))
        {
            UnityEngine.Debug.Log("「処理中」を受信しました。");
            UnityEngine.Debug.Log(line);
        }
        else if (line.Contains("結果"))
        {
            UnityEngine.Debug.Log("「結果」を受信しました。");
            UnityEngine.Debug.Log(line);
        }
        else
        {
            UnityEngine.Debug.Log("Python (Other): " + line);
        }
    }

    void OnApplicationQuit()
    {
        KillProcess();
    }

    void OnDestroy()
    {
        KillProcess();
    }
    public void Dispose()
    {
        KillProcess();
    }
    private void KillProcess()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            try
            {
                pythonProcess.Kill();
                UnityEngine.Debug.Log("Pythonプロセスを終了しました。");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Pythonプロセスの終了に失敗: {e.Message}");
            }
            pythonProcess.Dispose();
            pythonProcess = null;
        }
    }
}