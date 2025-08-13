# 多時區電子時鐘規格（2025.08 現況版）

## 目標
- 建立一個多時區電子時鐘網頁，設計參考 https://time.is/zh_tw/，強調美觀、清晰、現代感。

## 檔案位置
- 前端：Demo/Pages/index2.cshtml（Razor 頁面，含 HTML/CSS/JS）
- 後端：Demo/Pages/index2.cshtml.cs（Razor PageModel，預留 API 擴充）

## 功能現況

### 1. 畫面設計
- 網頁中央顯示本地時間電子時鐘：
	- 時間格式：hh:mm:ss（24 小時制，數字大且清晰，動態跳秒）
	- 日期格式：yyyy:MM:dd（顯示於時間下方，字體略小但明顯）
- 下方區域（footer）顯示多個指定時區的當前時間：
	- 紐約（America/New_York）、倫敦（Europe/London）、東京（Asia/Tokyo）、沙烏地阿拉伯（Asia/Riyadh）
	- 每個時區顯示格式同本地時間，並標註城市名稱
- UI 採用漸層、陰影、圓角、分隔區塊，並確保時間資訊易於辨識
- 響應式設計，支援桌面與行動裝置

### 2. 技術規格
- 前端以 JavaScript（clock.js）動態更新所有時鐘，每秒更新（setInterval）
- 各時區時間皆由前端根據瀏覽器本地時間 + IANA 時區偏移計算（未呼叫後端 API）
- 日期與時間皆有前導零（如 09:05:03）
- 若時區計算失敗，於下方顯示錯誤提示（#clock-error）
- 可擴充更多時區，HTML 結構已預留彈性

### 3. 後端設計
- Razor PageModel 僅負責頁面初始化，尚未實作多時區 API
- 已預留 OnGetWorldTimes() 方法，未來可由後端提供世界時區時間資訊

### 4. 其他
- 頁面載入即顯示正確時間，並持續即時更新
- 所有文字、數字皆清晰易讀，避免過度裝飾
- CSS/JS 檔案分離，易於維護與擴充
