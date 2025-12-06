#include <Keyboard.h>

const int sensorPin = 2; 
int lastSensorState = HIGH;

void setup() {
  pinMode(sensorPin, INPUT);
  
  // キーボード開始
  Keyboard.begin();
  
  // デバッグ用シリアル開始
  Serial.begin(9600);
}

void loop() {
  int currentSensorState = digitalRead(sensorPin);

  // 検知した瞬間 (HIGH -> LOW)
  if (currentSensorState == LOW && lastSensorState == HIGH) {
    
    // 1. シリアルモニタに表示 (確認用)
    Serial.println("SPACE送信");
    
    // 2. スペースキー入力
    Keyboard.press(' ');
    delay(50);
    Keyboard.release(' ');
  }

  lastSensorState = currentSensorState;
  delay(200); 
}