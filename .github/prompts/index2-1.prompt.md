```prompt
# 多時區電子時鐘頁面 規格書（強化版）

此文件為「多時區電子時鐘」頁面的完整規格，供前端與後端工程師、測試與 UX 使用，描述互動行為、資料契約、可及性、錯誤處理與驗收準則。

## 目的
- 在首頁中央顯示一個大型即時時鐘（預設為本地時間），下方呈現多個選定時區的即時時間；使用者可點選任一時區卡片，將該時區時間「交換」至中央顯示，並顯示提示訊息「已切換至[時區名稱]時間」。

## 檔案位置（目前專案位置）
- 前端 Razor Page: `/Users/qiuzili/Demo/Demo/Pages/index2.cshtml`
- 後端 PageModel: `/Users/qiuzili/Demo/Demo/Pages/index2.cshtml.cs`
- 建議前端腳本: `wwwroot/js/index2-clock.js`（可 reuse 現有 `clock.js`）

---

## 使用者流程（簡述）
1. 使用者開啟頁面：中央顯示大時鐘（預設本地時間）；下方顯示若干時區卡（每秒更新）。
2. 使用者點選某一時區卡：
	 - 前端立即執行 UI 交換動畫（平滑過渡）。
	 - 顯示短暫 toast："已切換至[時區名稱]時間"（約 2.5 秒）。
	 - 更新資料結構：primary 與 times 陣列交換，持續每秒更新顯示。
3. 使用者可用鍵盤（Tab + Enter/Space）完成同樣操作。

---

## 視覺與互動細節
- 佈局：中央一個大型時鐘（建議字體 56–120px），下方為響應式時區卡片網格或橫向清單。
- 卡片資訊：時區名稱（例：Taipei / Asia/Taipei）、城市或簡短描述、HH:MM:SS 時刻，整張卡片皆可點擊。
- 訊息（toast）：置於右上或中央上方，短暫顯示並可關閉。
- 動畫：交換動作用 transform + opacity，時長 ≈ 200–300ms。
- 無障礙：卡片要能被聚焦（tabindex），並用 aria-pressed / role="button" 傳達可互動性；切換時在 `aria-live="polite"` 的隱藏區域播報提示。

---

## 資料契約（前端/後端）
- TimezoneItem（物件）欄位建議：
	- id: string （IANA 時區 ID，如 "Asia/Taipei"）
	- label: string （短名，如 "Taipei"）
	- displayName: string （顯示用名，例："台北（TPE）"）
	- isLocal: boolean （是否為本地時間）
	- meta?: object （可選額外欄位，如城市座標、描述）

範例 JSON（初始 payload）
```json
{
	"primary": { "id": "Asia/Taipei", "label": "Taipei", "displayName": "台北", "isLocal": true },
	"times": [
		{ "id": "America/Los_Angeles", "label": "Los Angeles", "displayName": "洛杉磯", "isLocal": false },
		{ "id": "Europe/London", "label": "London", "displayName": "倫敦", "isLocal": false }
	]
}
```

前端函式介面建議（API）：
- initIndex2Clock(initialPayload)
- swapPrimaryWith(tzId)
- onPrimaryChanged(callback) // 回呼給整個應用（若需持久化）

後端（`index2.cshtml.cs`）建議：
- OnGet(): 傳回初始 `Primary` 與 `Times` 清單。使用 TimeZoneInfo（或 IANA 轉換）產生清單與 label。
- Optional: OnPostPersistPrimary(string tzId) — 若要將使用者偏好儲存到 Cookie 或 DB，提供 API。

實作建議：前端以 Intl.DateTimeFormat 處理時區與 DST，避免手動計算偏移：
```js
new Intl.DateTimeFormat('zh-TW', { timeZone: 'Asia/Taipei', hour: '2-digit', minute: '2-digit', second: '2-digit' }).format(date)
```

---

## 邊界情境與錯誤處理
- JavaScript 被停用：SSR 顯示靜態時間快照，並顯示提示『啟用 JavaScript 以獲得即時更新與切換功能』。
- 無效或不支援的時區 ID：忽略該卡並在 console/log 記錄錯誤；提示 UI 顯示簡短錯誤標記。
- 大量時區（數百筆）：採用分頁或虛擬滾動以節省 DOM 與記憶體。
- 效能：只使用一個 setInterval（1000ms）更新所有顯示，避免每卡片一個 timer。
- 持久化失敗（網路）：前端 fallback 用 localStorage，並顯示非阻斷式 toast 通知。

---

## 測試方案（建議）
- 後端（C# 單元測試）：
	- 測試 OnGet 回傳格式與欄位。
	- 測試持久化 API 錯誤與成功情境。
- 前端（JS 單元測試）：
	- swapPrimaryWith() 的陣列交換邏輯。
	- 時間格式化 util（使用 Intl）輸出驗證。
- E2E（Playwright 或 Selenium）：
	- 驗證初始中央為本地時間。
	- 點選下方時區卡後，中央與清單交換，且顯示 toast 與 aria-live 播報。
	- 鍵盤操作 (Tab + Enter/Space) 能觸發交換。

測試案例範例（端對端）：
- 步驟：載入頁面 → 點選洛杉磯卡 → 等待交換動畫結束 → 檢查中央顯示為洛杉磯時間 → 檢查 toast 文本為「已切換至洛杉磯時間」。

---

## 驗收準則（Acceptance Criteria）
- 中央時鐘正確顯示時間並每秒更新（跨裝置、跨時區一致）。
- 點選任一下方時區在 300ms 內開始視覺交換，且 toast 在 3 秒內出現並自動消失。 
- 鍵盤操作能完成同樣行為（無障礙通過）。
- 使用 Intl 或相容函式庫時，DST 處理正確。
- 無 JavaScript 時呈現可理解的降級內容。

---

## 最小交付契約（交給工程師即可直接實作）
- `index2.cshtml` DOM 要求：
	- 主時鐘區塊 id：`#primary-clock`
	- 時區卡容器 id：`#timezone-list`，每張卡使用 class `.timezone-card` 並帶 `data-tz-id`。
	- toast 容器 id：`#toast-container`
	- aria live 區塊 id：`#announce` 並設定 `aria-live="polite"`。
- `index2-clock.js` 介面：
	- export function initIndex2Clock(initialPayload)
	- export function swapPrimaryWith(tzId)

---

## 風險與備選方案
- 風險：部分舊版瀏覽器不支援 Intl.timeZone，備選：按需載入 polyfill 或使用 moment-timezone。
- 若需支援大量使用者偏好儲存，請評估後端 DB 與快取需求。

---

## 附註（供交付時一併提供）
- 建議提供一張 UX mockup（交換前後的視覺圖）。
- 提供初始 JSON payload 範例與 API spec（若實作持久化）。

---

完成：此檔可直接作為需求/規格交付給前端/後端與測試團隊。

```
