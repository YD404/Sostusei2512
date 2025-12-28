"""
camera_capture.py - カメラ制御モジュール

フリッカー対策付きのカメラ撮影機能を提供。
- 複数フレームの中央値を取ることでフリッカーを軽減
- カメラの露出安定を待ってから撮影
- OBS等の仮想カメラを自動除外（macOS対応）
- OBS仮想カメラプロセスを自動終了（macOS）
"""

import cv2
import numpy as np
import logging
import subprocess
import sys

logger = logging.getLogger(__name__)

# 除外するカメラ名のキーワード（大文字小文字無視）
EXCLUDED_CAMERA_KEYWORDS = ["obs", "virtual", "screen capture"]

# 自動終了するプロセス名のパターン
KILL_PROCESS_PATTERNS = [
    "obs-studio.mac-camera-extension",
    "obs-mac-virtualcam",
]


def kill_virtual_camera_processes():
    """
    OBS仮想カメラ等の競合するプロセスを強制終了
    macOS専用
    """
    if sys.platform != "darwin":
        return
    
    for pattern in KILL_PROCESS_PATTERNS:
        try:
            # pkill -f でパターンマッチしたプロセスを終了
            result = subprocess.run(
                ["pkill", "-f", pattern],
                capture_output=True,
                text=True,
                timeout=5
            )
            if result.returncode == 0:
                logger.info(f"[[CAMERA]] 仮想カメラプロセスを終了: {pattern}")
            # returncode 1 = プロセスが見つからなかった（正常）
        except subprocess.TimeoutExpired:
            logger.warning(f"プロセス終了がタイムアウト: {pattern}")
        except Exception as e:
            logger.warning(f"プロセス終了に失敗: {pattern} - {e}")


# モジュール読み込み時にOBS仮想カメラを終了
kill_virtual_camera_processes()


def get_macos_cameras():
    """
    macOSでカメラデバイス名とインデックスのマッピングを取得
    Returns: list of dict [{"index": int, "name": str}, ...]
    """
    if sys.platform != "darwin":
        return []
    
    try:
        result = subprocess.run(
            ["system_profiler", "SPCameraDataType", "-json"],
            capture_output=True,
            text=True,
            timeout=5
        )
        import json
        data = json.loads(result.stdout)
        cameras = []
        
        if "SPCameraDataType" in data:
            for i, cam in enumerate(data["SPCameraDataType"]):
                name = cam.get("_name", f"Camera {i}")
                cameras.append({"index": i, "name": name})
                logger.info(f"macOS カメラ検出: Index {i} = {name}")
        
        return cameras
    except Exception as e:
        logger.warning(f"macOSカメラ情報の取得に失敗: {e}")
        return []


def find_physical_camera_index(exclude_keywords=EXCLUDED_CAMERA_KEYWORDS):
    """
    仮想カメラを除外して物理カメラのインデックスを検索
    
    Returns:
        int: 物理カメラのインデックス、見つからない場合は0
    """
    cameras = get_macos_cameras()
    
    if not cameras:
        # macOS以外、または情報取得失敗時はOpenCVでスキャン
        logger.info("OpenCVでカメラをスキャン中...")
        for i in range(5):
            cap = cv2.VideoCapture(i)
            if cap.isOpened():
                # 解像度とFPSで判別（OBSは通常低FPS）
                fps = cap.get(cv2.CAP_PROP_FPS)
                if fps > 10:  # OBS仮想カメラは通常5fps程度
                    logger.info(f"物理カメラ候補発見: Index {i} (FPS: {fps})")
                    cap.release()
                    return i
                cap.release()
        return 0
    
    # macOSカメラ情報を使って除外
    for cam in cameras:
        name_lower = cam["name"].lower()
        is_excluded = any(kw in name_lower for kw in exclude_keywords)
        
        if is_excluded:
            logger.info(f"仮想カメラ除外: {cam['name']} (Index {cam['index']})")
        else:
            logger.info(f"物理カメラ選択: {cam['name']} (Index {cam['index']})")
            return cam["index"]
    
    logger.warning("物理カメラが見つかりません。デフォルト(0)を使用します。")
    return 0


class CameraCapture:
    """フリッカー対策付きのカメラキャプチャクラス"""
    
    def __init__(self, camera_index=None, width=1280, height=720, auto_detect=True):
        """
        Args:
            camera_index: カメラデバイスのインデックス（Noneで自動検出）
            width: キャプチャ幅
            height: キャプチャ高さ
            auto_detect: Trueの場合、仮想カメラを除外して物理カメラを自動検出
        """
        if camera_index is None and auto_detect:
            self.camera_index = find_physical_camera_index()
            logger.info(f"カメラ自動検出: Index {self.camera_index}")
        else:
            self.camera_index = camera_index if camera_index is not None else 0
            
        self.width = width
        self.height = height
        self.cap = None
        self._is_initialized = False
    
    def initialize(self):
        """カメラを初期化"""
        if self._is_initialized:
            return True
        
        logger.info(f"カメラ初期化中... (index={self.camera_index})")
        self.cap = cv2.VideoCapture(self.camera_index)
        
        if not self.cap.isOpened():
            logger.error("カメラを開けませんでした")
            return False
        
        # カメラ設定
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, self.width)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, self.height)
        
        # 実際の解像度を取得
        actual_w = int(self.cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        actual_h = int(self.cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        fps = self.cap.get(cv2.CAP_PROP_FPS)
        logger.info(f"カメラ初期化完了: {actual_w}x{actual_h} @ {fps}fps")
        
        self._is_initialized = True
        return True

    
    def capture_with_stabilization(self, warmup_frames=5, capture_frames=5):
        """
        フリッカー対策付きの撮影
        
        Args:
            warmup_frames: 露出安定のために捨てるフレーム数
            capture_frames: 中央値計算に使うフレーム数
        
        Returns:
            numpy.ndarray: キャプチャした画像（BGR形式）
        """
        if not self._is_initialized:
            if not self.initialize():
                return None
        
        logger.info(f"撮影開始: ウォームアップ{warmup_frames}フレーム, 撮影{capture_frames}フレーム")
        
        # 1. ウォームアップ（露出安定待ち）
        for i in range(warmup_frames):
            ret, _ = self.cap.read()
            if not ret:
                logger.warning(f"ウォームアップフレーム{i}の取得失敗")
        
        # 2. 複数フレームを取得
        frames = []
        for i in range(capture_frames):
            ret, frame = self.cap.read()
            if ret:
                frames.append(frame.astype(np.float32))
            else:
                logger.warning(f"キャプチャフレーム{i}の取得失敗")
        
        if len(frames) == 0:
            logger.error("フレームを取得できませんでした")
            return None
        
        # 3. 中央値を計算（フリッカー除去）
        # 中央値は外れ値に強く、蛍光灯のフリッカーに効果的
        median_frame = np.median(np.stack(frames), axis=0).astype(np.uint8)
        
        logger.info(f"撮影完了: {len(frames)}フレームから合成")
        return median_frame
    
    def capture_single(self):
        """
        単純な1フレームキャプチャ（高速だがフリッカー対策なし）
        """
        if not self._is_initialized:
            if not self.initialize():
                return None
        
        ret, frame = self.cap.read()
        if ret:
            return frame
        return None
    
    def release(self):
        """カメラリソースを解放"""
        if self.cap is not None:
            self.cap.release()
            self._is_initialized = False
            logger.info("カメラリソースを解放しました")
    
    def __enter__(self):
        self.initialize()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        self.release()


# モジュールテスト用
if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO, format='%(message)s')
    
    print("=== カメラキャプチャテスト ===")
    
    with CameraCapture() as cam:
        print("\n1. フリッカー対策キャプチャ...")
        frame = cam.capture_with_stabilization()
        if frame is not None:
            cv2.imwrite("test_stabilized.jpg", frame)
            print(f"   保存完了: test_stabilized.jpg ({frame.shape})")
        
        print("\n2. 単純キャプチャ...")
        frame = cam.capture_single()
        if frame is not None:
            cv2.imwrite("test_single.jpg", frame)
            print(f"   保存完了: test_single.jpg ({frame.shape})")
    
    print("\n=== テスト完了 ===")
