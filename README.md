# Demo 專案

> 一個現代化的 .NET 8 Web 應用程式範本，適合快速啟動、學習與展示。

---

## 目錄

- [Demo 專案](#demo-專案)
  - [目錄](#目錄)
  - [簡介](#簡介)
  - [功能特色](#功能特色)
  - [快速開始](#快速開始)
  - [專案結構](#專案結構)
  - [首頁時鐘元件（`Pages/Index.cshtml`）](#首頁時鐘元件pagesindexcshtml)
  - [多時區電子時鐘頁面（index2.cshtml）](#多時區電子時鐘頁面index2cshtml)
  - [互動式月曆系統（index3.cshtml）](#互動式月曆系統index3cshtml)
    - [🎯 核心功能](#-核心功能)
    - [🎨 UI/UX 特色](#-uiux-特色)
    - [📝 註記功能詳解](#-註記功能詳解)
    - [🏗️ 技術架構](#️-技術架構)
    - [🔧 使用說明](#-使用說明)
  - [設定說明](#設定說明)
    - [📝 註記功能設定](#-註記功能設定)
  - [常見問題](#常見問題)

---

## 簡介

本專案為 .NET 8 建構的 Web 應用程式範本，結合 Razor Pages、靜態資源管理與現代化開發流程，適合用於學習、展示或作為新專案的起點。

## 功能特色

- 基於 .NET 8
- Razor Pages 架構
- 靜態資源（CSS/JS）管理
- 多環境設定（Development/Production）
- 友善的專案結構，易於擴充
- **首頁內建「電子時鐘」與「指針時鐘」元件**，支援即時台北時間顯示，並具備現代化美觀設計
- **多時區電子時鐘頁面**，支援世界各地時間顯示、深色模式、多語言介面
- **📝 互動式月曆系統**，具備日期選擇、導航控制和個人化註記功能
- **JSON 檔案儲存**，支援個人化資料持久化，無需資料庫設定

## 快速開始

```bash
# 1. 取得原始碼
$ git clone <your-repo-url>
$ cd Demo

# 2. 建置專案
$ dotnet build Demo/Demo.csproj

# 3. 執行專案
$ dotnet run --project Demo/Demo.csproj

# 4. 瀏覽功能
# 首頁時鐘: http://localhost:5000/
# 多時區時鐘: http://localhost:5000/index2  
# 互動式月曆: http://localhost:5000/index3
```

> [!TIP]
> 可透過 `appsettings.Development.json` 與 `appsettings.json` 進行環境參數調整。
>
> **首次使用註記功能時**，`App_Data/notes.json` 檔案會自動建立。


## 專案結構

```text
Demo/
├── Services/             # 業務服務層
│   └── NoteService.cs    # 註記功能服務（JSON 檔案 I/O）
├── Pages/                # Razor Pages 與後端程式
│   ├── Shared/           # 共用頁面元件 (Layout、部分視圖)
│   ├── Index.cshtml      # 首頁：電子時鐘與指針時鐘
│   ├── index2.cshtml     # 多時區電子時鐘頁面
│   ├── index2.cshtml.cs  # index2 頁面 Model（預留 API 擴充）
│   ├── index3.cshtml     # 互動式月曆系統
│   ├── index3.cshtml.cs  # index3 頁面 Model（月曆邏輯與註記處理）
│   └── ...
├── wwwroot/              # 靜態資源 (CSS, JS, 圖片)
├── App_Data/             # 應用程式資料檔案
│   └── notes.json        # 📝 註記資料儲存檔案（重要備份目標）
├── appsettings.json      # 全域設定檔
├── appsettings.Development.json # 開發環境設定
├── Program.cs            # 進入點與服務設定
└── Demo.csproj           # 專案描述檔
```

---

## 首頁時鐘元件（`Pages/Index.cshtml`）

目前首頁同時顯示兩種時鐘（已移除切換按鈕，兩者並列呈現）：

- 電子時鐘（digital clock）：
  - 24 小時制（HH:MM:SS），使用等寬字型與深色背景。
  - 冒號每秒閃爍（由內嵌 JavaScript 判斷秒數取餘數來控制透明度）。
  - 時間來源以台北時間（UTC+8）為基準，函式 `getTaipeiTime()` 透過系統時間加上時區位移計算得到（使用 UTC 與偏移量運算，避免直接依賴瀏覽器 locale）。

- 指針時鐘（analog clock）：
  - 直徑為 20rem 的圓形面板，採用多層漸層與陰影來呈現立體感。
  - 外框有金色漸層並以 CSS 動畫連續旋轉（20s），外圈光暈以 3s 週期做脈動動畫。
  - 秒針具發光動畫（1s 交替），時/分/秒針以 transform rotate 更新角度。
  - 刻度（60 格）動態由 `renderClockTicks()` 在 DOM 中產生（主要刻度為金色漸層、分鐘刻度為銀色漸層）。

技術重點：

- 所有樣式與動畫皆放在 `Pages/Index.cshtml` 的 `<style>` 區塊（原生 CSS）
- 時鐘邏輯在頁面底部以內嵌 `<script>` 實作：包含 `getTaipeiTime()`、`updateDigitalClock()`、`updateAnalogClock()` 與 `renderClockTicks()`，並在 `DOMContentLoaded` 時初始化並以 1 秒間隔更新。
- 頁面不依賴外部時鐘函式庫；若需延伸或拆分，可將 JavaScript 提取到 `wwwroot/js/` 中的獨立檔案。

若要調整樣式或動畫行為，請編輯 `Pages/Index.cshtml` 中的 CSS 與內嵌 JavaScript。

---

## 多時區電子時鐘頁面（index2.cshtml）

`index2.cshtml`（P2 增強版）為多時區電子時鐘的進階頁面，除了顯示本地時間與多個世界城市時間外，還包含使用者設定面板、語言面板、載入覆蓋與互動式燈箱（modal）等 UX 元件，並與後端 API（Razor Page Handlers）整合以提供可擴充的資料與格式選項。

主要特性：

- 頂部工具列（title / 時區下拉 / 控制按鈕群組），支援鍵盤與無障礙屬性（aria-label、role、tabindex）。
- 設定面板（settings-panel）與語言面板（language-panel）：前端可切換主題（深色/淺色）、時間格式、是否顯示秒數/日期、語言等。
- 載入覆蓋層（loading-overlay）：在向後端抓取時區資料時顯示進度與 spinner。
- 大型可點擊本地時鐘卡（local-clock large-clock clickable-clock）與多時區小卡片排版（small-clock）：響應式設計，行動/桌面皆友好。
- 燈箱（modal）與音效預載（modal-open/close），提升互動感。
- CSS 變數系統與完整的深色模式支援（使用 `data-theme="dark"` 切換），大量樣式皆以 CSS 變數驅動，方便客製化。
- 前端翻譯資源與腳本載入：預載 `i18n-zh-TW.js` / `i18n-zh-CN.js` / `i18n-en-US.js` / `i18n-ja-JP.js`，並載入 `enhanced-clock.js`（增強型時鐘邏輯）與向後相容的 `clock.js`。

後端（`Pages/index2.cshtml.cs`）亮點：

- 提供多個 Razor Page Handlers：`OnGetWorldTimes()`、`OnGetTimeFormats()`、`OnGetLanguages()`，分別回傳世界時區資料、可用時間/日期格式與語言清單（以 JSON 回應，供前端初始化與設定選項）。
- 使用 `EnhancedTimezoneInfo` 類別描述時區：包含 City / Timezone / DisplayName / Country / Region / UtcOffsetMinutes / SupportsDST 等欄位，便於前端顯示並支援日光節約時間標記。
- 提供 `TimeFormatSettings`、`UserPreferences`、向後相容的 `TimezoneInfo` 類別，範例設定包含語言（`zh-TW`）、預設時區（`Asia/Taipei`）、是否顯示秒數等偏好。

延展與使用說明：

- 前端會先呼叫後端的 `OnGetWorldTimes` 初始化時區名單；如需加入/移除城市，可在 `index2.cshtml.cs` 的 `_supportedTimezones` 列表修改或改為讀取資料庫/外部 API。
- 時間格式清單由 `OnGetTimeFormats` 提供，前端可根據回傳的 `example` 字串顯示即時預覽。
- 若要新增語言資源，請在 `wwwroot/js/` 新增對應的 `i18n-<lang>.js` 並在頁面 `@section Scripts` 中引入。

相關檔案（重點）：

- `Pages/index2.cshtml`：頁面結構、樣式與前端初始化點。
- `Pages/index2.cshtml.cs`：頁面 Model 與 Handlers（`OnGetWorldTimes`、`OnGetTimeFormats`、`OnGetLanguages` 等）。
- `wwwroot/js/enhanced-clock.js`：增強型時鐘與多時區邏輯（前端計算、渲染與互動）。
- `wwwroot/js/clock.js`：向後相容的原始時鐘程式。
- `wwwroot/js/i18n-*.js`：多語言資源檔。

補充：目前 `Pages/Index.cshtml.cs` 為簡單的 `PageModel`（只注入 `ILogger<IndexModel>` 並提供空的 `OnGet()`），如需在首頁加入 API 呼叫或後端初始化邏輯，可在此檔案擴充處理程序。

若要快速測試本頁面，啟動網站後瀏覽 `/index2`（或依路由設定），觀察頁面頂部工具列、設定面板、語言切換、以及多時區卡片是否如預期更新。

---

## 互動式月曆系統（index3.cshtml）

`index3.cshtml` 為完整的互動式月曆應用程式，提供月份導航、日期選擇和個人化註記功能。此頁面結合了現代化 UI 設計、健壯的後端邏輯，以及基於 JSON 檔案的資料持久化系統。

### 🎯 核心功能

- **42 格月曆視圖**：標準 6 週 × 7 天格線佈局，週日為起始日
- **智能日期導航**：前後月份切換、年月下拉選擇、一鍵回到今日
- **日期狀態顯示**：今日特殊標記、選中日期高亮、距離今日天數計算
- **📝 個人化註記**：針對任意日期新增、修改、刪除文字註記
- **參數自動修正**：超出範圍的 URL 參數自動調整並提示用戶

### 🎨 UI/UX 特色

- **響應式設計**：Bootstrap 5 框架，支援桌面和行動裝置
- **視覺層次**：漸層背景、陰影效果、懸停動畫
- **無障礙支援**：完整的 ARIA 標籤、鍵盤導航、螢幕閱讀器相容
- **互動反饋**：日期格子懸停放大、選中狀態視覺回饋
- **狀態保持**：操作後維持選中日期狀態，提供連續使用體驗

### 📝 註記功能詳解

**資料儲存**：

- **檔案位置**：`App_Data/notes.json`（🔴 重要備份目標）
- **格式**：`{"2025-08-27": "會議記錄", "2025-08-28": "生日聚會"}`
- **編碼**：UTF-8，支援中文內容和換行符

**操作方式**：

1. **選取日期**：點擊月曆上的任意日期
2. **新增註記**：在文字區域輸入內容，點擊「儲存備註」
3. **修改註記**：輸入框會預填現有內容，修改後重新儲存
4. **刪除註記**：點擊「刪除備註」按鈕並確認，或清空內容後儲存

**安全與可靠性**：

- **執行緒安全**：使用 `SemaphoreSlim` 防止併發檔案操作衝突
- **錯誤恢復**：檔案損壞時自動建立空白檔案
- **輸入驗證**：所有參數經過後端驗證和範圍檢查
- **XSS 防護**：Razor Pages 自動 HTML 編碼用戶輸入

### 🏗️ 技術架構

**後端（`Pages/index3.cshtml.cs`）**：

- **設計模式**：Razor Pages PageModel + 依賴注入
- **核心方法**：`OnGetAsync()` 載入頁面和註記、`OnPostSaveNoteAsync()` 儲存註記、`OnPostDeleteNoteAsync()` 刪除註記
- **業務邏輯**：42 格月曆算法、參數驗證修正、日期狀態計算

**服務層（`Services/NoteService.cs`）**：

- **介面抽象**：`INoteService` 提供 Get/Save/Delete 操作
- **實作類別**：`JsonNoteService` 處理 JSON 檔案讀寫
- **非同步操作**：所有檔案 I/O 使用 async/await 模式

**前端**：

- **技術棧**：ASP.NET Core Razor Pages + Bootstrap 5 + Bootstrap Icons
- **互動性**：CSS 變換動畫、JavaScript 確認對話框
- **表單處理**：多個 POST handler、隱藏欄位保持狀態

### 🔧 使用說明

**URL 格式**：

```text
/index3                           # 顯示當前月份
/index3?year=2025&month=8         # 指定年月
/index3?year=2025&month=8&day=27  # 指定日期（選中狀態）
```

**操作流程**：

1. 啟動應用程式後瀏覽 `/index3`
2. 使用導航控制切換月份或選擇特定年月
3. 點擊日期後在下方註記區域新增備註
4. 註記會自動儲存到 `App_Data/notes.json` 檔案

**⚠️ 重要提醒**：

- **備份資料**：定期備份 `App_Data/notes.json` 檔案
- **檔案權限**：確保應用程式對 `App_Data` 資料夾有讀寫權限
- **檔案大小**：監控註記檔案大小，避免過度增長影響效能

**相關檔案**：

- `Pages/index3.cshtml`：前端頁面和註記表單 UI
- `Pages/index3.cshtml.cs`：後端邏輯和 API handlers  
- `Services/NoteService.cs`：註記資料存取服務
- `App_Data/notes.json`：註記資料持久化檔案

若要測試註記功能，請選擇任意日期，輸入備註內容並儲存，然後切換到其他日期再回來驗證註記是否正確保存。

---

---

## 設定說明

- **appsettings.json**：全域設定檔，適用於所有環境。
- **appsettings.Development.json**：開發環境專用設定，會覆蓋部分全域設定。
- **wwwroot/**：放置網站靜態資源（如 CSS、JS、圖片等）。
- **🔴 App_Data/notes.json**：註記功能的資料儲存檔案，包含所有使用者的日期註記內容。

### 📝 註記功能設定

**檔案位置**: `App_Data/notes.json`
**格式**: JSON 鍵值對，日期為鍵（yyyy-MM-dd），註記內容為值
**備份建議**: 建議定期備份此檔案，避免資料遺失
**權限要求**: 應用程式需要對 App_Data 資料夾有完整讀寫權限

**範例內容**:

```json
{
  "2025-08-27": "重要會議 - 討論專案需求",
  "2025-08-28": "生日聚會\n記得帶禮物",
  "2025-09-01": "專案截止日"
}
```

## 常見問題

> [!NOTE]
> 若遇到執行錯誤，請確認已安裝 .NET 8 SDK，並檢查專案相依套件是否完整。

---

如需更多協助，請參閱原始碼或提出 Issue。
