"""
camera_capture.py - カメラ制御モジュール

フリッカー対策付きのカメラ撮影機能を提供。
- 複数フレームの中央値を取ることでフリッカーを軽減
- カメラの露出安定を待ってから撮影
"""

import cv2
import numpy as np
import logging

logger = logging.getLogger(__name__)


class CameraCapture:
    """フリッカー対策付きのカメラキャプチャクラス"""
    
    def __init__(self, camera_index=0, width=1280, height=720):
        """
        Args:
            camera_index: カメラデバイスのインデックス
            width: キャプチャ幅
            height: キャプチャ高さ
        """
        self.camera_index = camera_index
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
        logger.info(f"カメラ初期化完了: {actual_w}x{actual_h}")
        
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
