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
  - [設定說明](#設定說明)
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
- 首頁內建「電子時鐘」與「指針時鐘」元件，支援即時台北時間顯示，並具備現代化美觀設計。

## 快速開始

```bash
# 1. 取得原始碼
$ git clone <your-repo-url>
$ cd Demo

# 2. 建置專案
$ dotnet build Demo/Demo.csproj

# 3. 執行專案
$ dotnet run --project Demo/Demo.csproj
```

> [!TIP]
> 可透過 `appsettings.Development.json` 與 `appsettings.json` 進行環境參數調整。


## 專案結構

```
Demo/
├── Pages/                # Razor Pages 與後端程式
│   ├── Shared/           # 共用頁面元件 (Layout、部分視圖)
│   ├── Index.cshtml      # 首頁：電子時鐘與指針時鐘
│   ├── index2.cshtml     # 多時區電子時鐘頁面
│   ├── index2.cshtml.cs  # index2 頁面 Model（預留 API 擴充）
│   └── ...
├── wwwroot/              # 靜態資源 (CSS, JS, 圖片)
├── appsettings.json      # 全域設定檔
├── appsettings.Development.json # 開發環境設定
├── Program.cs            # 進入點
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

---

## 設定說明

- **appsettings.json**：全域設定檔，適用於所有環境。
- **appsettings.Development.json**：開發環境專用設定，會覆蓋部分全域設定。
- **wwwroot/**：放置網站靜態資源（如 CSS、JS、圖片等）。

## 常見問題

> [!NOTE]
> 若遇到執行錯誤，請確認已安裝 .NET 8 SDK，並檢查專案相依套件是否完整。

---

如需更多協助，請參閱原始碼或提出 Issue。
