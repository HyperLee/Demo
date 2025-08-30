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
  - [進階備忘錄管理系統（index4.cshtml）](#進階備忘錄管理系統index4cshtml)
    - [🎯 核心管理功能](#-核心管理功能)
    - [🎨 使用者體驗特色](#-使用者體驗特色)
    - [🔧 技術架構亮點](#-技術架構亮點)
    - [📊 匯出功能詳解](#-匯出功能詳解)
  - [智慧備忘錄編輯器（index5.cshtml）](#智慧備忘錄編輯器index5cshtml)
    - [✏️ 編輯核心功能](#️-編輯核心功能)
    - [🏷️ 標籤管理系統](#️-標籤管理系統)
    - [📁 分類管理系統](#-分類管理系統)
    - [🛡️ 使用者體驗保護](#️-使用者體驗保護)
    - [🎨 現代化介面設計](#-現代化介面設計)
    - [🔗 系統整合](#-系統整合)
  - [台幣外幣匯率計算器（index6.cshtml）](#台幣外幣匯率計算器index6cshtml)
    - [🏦 核心功能特色](#-核心功能特色)
    - [💱 支援貨幣清單](#-支援貨幣清單)
    - [🎯 使用者體驗設計](#-使用者體驗設計)
    - [🔧 技術架構亮點](#-技術架構亮點-1)
    - [📊 匯率計算邏輯](#-匯率計算邏輯)
    - [🛡️ 可靠性設計](#️-可靠性設計)
  - [個人記帳系統（index7.cshtml \& index8.cshtml）](#個人記帳系統index7cshtml--index8cshtml)
    - [💰 系統特色](#-系統特色)
    - [📊 記帳列表頁面（index7）](#-記帳列表頁面index7)
      - [核心功能](#核心功能)
      - [月份導航系統](#月份導航系統)
    - [✏️ 記帳編輯頁面（index8）](#️-記帳編輯頁面index8)
      - [表單設計特色](#表單設計特色)
      - [資料處理流程](#資料處理流程)
    - [📄 多格式匯出功能](#-多格式匯出功能)
      - [CSV 匯出 - 中文編碼最佳化](#csv-匯出---中文編碼最佳化)
      - [Excel 匯出 - 多工作表設計](#excel-匯出---多工作表設計)
      - [PDF 匯出 - 中文字型支援](#pdf-匯出---中文字型支援)
    - [🔧 技術架構](#-技術架構)
      - [後端服務層](#後端服務層)
      - [前端技術棧](#前端技術棧)
      - [資料持久化](#資料持久化)
    - [🤖 AI 智慧分析功能](#-ai-智慧分析功能)
      - [財務健康度評分系統](#財務健康度評分系統)
      - [智慧洞察與分析](#智慧洞察與分析)
      - [異常偵測與警報系統](#異常偵測與警報系統)
      - [支出預測與現金流分析](#支出預測與現金流分析)
      - [個人化建議系統](#個人化建議系統)
      - [技術架構特色](#技術架構特色)
  - [智慧待辦清單系統（todo.cshtml）](#智慧待辦清單系統todocshtml)
    - [🎯 核心功能特色](#-核心功能特色-1)
      - [智慧時間分組](#智慧時間分組)
      - [任務管理功能](#任務管理功能)
      - [進階搜尋與篩選](#進階搜尋與篩選)
    - [🎨 使用者介面設計](#-使用者介面設計)
      - [響應式卡片布局](#響應式卡片布局)
      - [互動式編輯體驗](#互動式編輯體驗)
    - [🏗️ 技術架構亮點](#️-技術架構亮點)
      - [後端設計模式](#後端設計模式)
      - [服務層設計](#服務層設計)
      - [前端技術整合](#前端技術整合)
    - [📊 資料模型設計](#-資料模型設計)
      - [TodoTask 核心模型](#todotask-核心模型)
      - [統計分析模型](#統計分析模型)
    - [🚀 效能最佳化](#-效能最佳化)
      - [快取策略](#快取策略)
      - [使用者體驗優化](#使用者體驗優化)
  - [財務儀表板系統（index9.cshtml）](#財務儀表板系統index9cshtml)
    - [📊 核心分析功能](#-核心分析功能)
      - [多維度財務統計](#多維度財務統計)
      - [視覺化圖表系統](#視覺化圖表系統)
      - [智慧統計卡片](#智慧統計卡片)
    - [🎨 使用者介面設計](#-使用者介面設計-1)
      - [響應式儀表板布局](#響應式儀表板布局)
      - [動態圖表互動](#動態圖表互動)
    - [🏗️ 技術架構設計](#️-技術架構設計)
      - [智慧快取系統](#智慧快取系統)
      - [AJAX 資料更新機制](#ajax-資料更新機制)
    - [📈 分析演算法](#-分析演算法)
      - [趨勢計算邏輯](#趨勢計算邏輯)
      - [分類分析功能](#分類分析功能)
    - [🛡️ 效能與安全性](#️-效能與安全性)
      - [快取最佳化](#快取最佳化)
      - [安全性設計](#安全性設計)
    - [🚀 擴展性設計](#-擴展性設計)
      - [模組化架構](#模組化架構)
      - [升級路徑](#升級路徑)
  - [設定說明](#設定說明)
    - [📝 註記功能設定](#-註記功能設定)
    - [🗂️ 備忘錄系統設定](#️-備忘錄系統設定)
    - [🏦 匯率系統設定](#-匯率系統設定)
    - [✅ 待辦清單系統設定](#-待辦清單系統設定)
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
- **🗂️ 進階備忘錄管理系統**，支援標籤分類、批次操作、搜尋篩選和匯出功能
- **✏️ 智慧備忘錄編輯器**，具備標籤管理、分類整合、字元計數和自動保存提醒
- **🏦 台幣外幣匯率計算器**，整合台灣銀行官方 API，支援即時匯率查詢與雙向精確計算
- **💰 個人記帳系統**，月曆檢視記帳記錄、收支統計分析、多格式報表匯出（CSV/Excel/PDF）
- **✅ 智慧待辦清單系統**，時間智慧分組、拖拽排序、進階篩選、標籤管理和完成率統計
- **📊 財務儀表板系統**，動態圖表分析、多時間範圍統計、收支趨勢預測和分類支出視覺化
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
# 備忘錄列表: http://localhost:5000/index4
# 備忘錄編輯: http://localhost:5000/index5
# 匯率計算器: http://localhost:5000/index6
# 記帳系統列表: http://localhost:5000/index7
# 記帳記錄編輯: http://localhost:5000/index8
# 財務儀表板: http://localhost:5000/index9
# 待辦清單: http://localhost:5000/todo
```

> [!TIP]
> 可透過 `appsettings.Development.json` 與 `appsettings.json` 進行環境參數調整。
>
> **首次使用註記功能時**，`App_Data/notes.json` 檔案會自動建立。


## 專案結構

```text
Demo/
├── Services/             # 業務服務層
│   ├── NoteService.cs    # 註記功能服務（JSON 檔案 I/O）+ 備忘錄管理服務
│   ├── ExchangeRateService.cs # 匯率資料服務（台銀 API 整合）
│   ├── AccountingService.cs # 記帳資料服務（JSON 檔案 I/O）
│   └── TodoService.cs # 待辦事項資料服務（JSON 檔案 I/O）
├── Models/               # 資料模型
│   ├── ExchangeRate.cs   # 匯率資料模型
│   ├── AccountingModels.cs # 記帳資料模型
│   ├── DashboardModels.cs # 財務儀表板資料模型
│   └── TodoModels.cs # 待辦事項資料模型
├── Utilities/            # 工具類別
│   ├── DataFixUtility.cs # 資料修正工具
│   └── PdfExportUtility.cs # PDF 匯出工具
├── Pages/                # Razor Pages 與後端程式
│   ├── Shared/           # 共用頁面元件 (Layout、部分視圖)
│   ├── Index.cshtml      # 首頁：電子時鐘與指針時鐘
│   ├── index2.cshtml     # 多時區電子時鐘頁面
│   ├── index2.cshtml.cs  # index2 頁面 Model（預留 API 擴充）
│   ├── index3.cshtml     # 互動式月曆系統
│   ├── index3.cshtml.cs  # index3 頁面 Model（月曆邏輯與註記處理）
│   ├── index4.cshtml     # 🗂️ 備忘錄列表管理頁面
│   ├── index4.cshtml.cs  # index4 頁面 Model（列表邏輯、搜尋、批次操作）
│   ├── index5.cshtml     # ✏️ 備忘錄編輯頁面
│   ├── index5.cshtml.cs  # index5 頁面 Model（編輯邏輯、標籤分類管理）
│   ├── index6.cshtml     # 🏦 台幣外幣匯率計算器頁面
│   ├── index6.cshtml.cs  # index6 頁面 Model（匯率計算、API 整合）
│   ├── index7.cshtml     # 💰 記帳系統列表頁面（月曆檢視）
│   ├── index7.cshtml.cs  # index7 頁面 Model（記帳列表、統計、匯出）
│   ├── index8.cshtml     # ✏️ 記帳記錄編輯頁面
│   ├── index8.cshtml.cs  # index8 頁面 Model（記錄新增修改、驗證）
│   ├── index9.cshtml     # 📊 財務儀表板頁面（圖表分析）
│   ├── index9.cshtml.cs  # index9 頁面 Model（儀表板統計、圖表資料）
│   ├── todo.cshtml       # ✅ 智慧待辦清單頁面
│   ├── todo.cshtml.cs    # todo 頁面 Model（任務管理、統計分析）
│   └── ...
├── wwwroot/              # 靜態資源 (CSS, JS, 圖片)
├── App_Data/             # 應用程式資料檔案
│   ├── notes.json        # 📝 註記資料儲存檔案（重要備份目標）
│   ├── memo-notes.json   # 🗂️ 備忘錄資料儲存檔案（重要備份目標）
│   ├── exchange_rates.json # 🏦 匯率快取檔案（重要備份目標）
│   ├── tags.json         # 🏷️ 標籤資料儲存檔案
│   ├── categories.json   # 📁 分類資料儲存檔案
│   └── accounting-records.json # 💰 記帳記錄檔案（重要備份目標）
│   ├── accounting-categories.json # 💰 記帳分類檔案（重要備份目標）
│   ├── todo-tasks.json # ✅ 待辦任務資料檔案（重要備份目標）
│   └── todo-categories.json # ✅ 待辦分類資料檔案（重要備份目標）
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

## 進階備忘錄管理系統（index4.cshtml）

`index4.cshtml` 為功能完整的備忘錄列表管理頁面，提供企業級的資料管理功能，包含搜尋、篩選、批次操作和匯出等進階功能。此系統基於增強型服務架構，支援標籤分類體系和多格式資料匯出。

### 🎯 核心管理功能

- **智能搜尋系統**：關鍵字搜尋（標題、內容、標籤）+ 進階篩選（標籤、分類、日期範圍）
- **批次操作中心**：批次刪除、批次標籤、批次分類變更、批次匯出
- **標籤管理系統**：標籤建立、顏色設定、使用統計、標籤刪除
- **分類管理系統**：分類建立、階層結構、分類重新整理、批次分類變更
- **多格式匯出**：PDF 匯出（支援中文字型）、Excel 匯出（.xlsx 格式）
- **響應式分頁**：智慧分頁控制，支援大量資料瀏覽

### 🎨 使用者體驗特色

- **現代化介面**：Bootstrap 5 設計，支援深色模式和行動裝置
- **即時搜尋**：輸入關鍵字後自動觸發搜尋，無需手動提交
- **視覺化標籤**：彩色標籤徽章，支援自訂顏色和懸停效果
- **批次選擇**：全選/部分選擇機制，批次工具列動態顯示
- **確認對話框**：重要操作提供確認機制，防止誤刪
- **載入狀態**：操作過程顯示載入動畫，提升使用者體驗

### 🔧 技術架構亮點

**後端服務（`Pages/index4.cshtml.cs`）**：

- **增強型服務**：`IEnhancedMemoNoteService` 提供完整的 CRUD 和進階功能
- **搜尋引擎**：支援多欄位搜尋、日期範圍篩選、標籤/分類篩選
- **批次處理**：`BatchOperationRequest` 處理大量資料操作
- **匯出功能**：整合 iText7（PDF）和 ClosedXML（Excel）函式庫
- **API 端點**：多個 OnPost Handler 支援 AJAX 操作

**資料持久化**：

- **檔案儲存**：`App_Data/memo-notes.json`、`tags.json`、`categories.json`
- **關聯管理**：備忘錄與標籤、分類的多對一/多對多關係
- **資料完整性**：標籤使用計數、分類階層驗證、外鍵約束模擬

### 📊 匯出功能詳解

**PDF 匯出特色**：
- 支援繁體中文字型渲染
- 自動分頁和頁碼
- 標籤和分類資訊完整保留
- 建立/修改時間戳記

**Excel 匯出特色**：
- .xlsx 格式，相容 Office 365
- 自動欄位寬度調整
- 標題列格式化
- 支援大量資料匯出

---

## 智慧備忘錄編輯器（index5.cshtml）

`index5.cshtml` 為進階的備忘錄編輯頁面，提供完整的內容建立和編輯體驗。整合智能標籤系統、分類管理、字元計數和防護機制，確保資料輸入的準確性和使用者體驗的流暢性。

### ✏️ 編輯核心功能

- **雙模式編輯**：新增模式 vs 編輯模式，介面自動調整
- **智慧標籤輸入**：HTML5 datalist 自動完成 + 即時標籤建立
- **分類整合**：下拉選擇現有分類 + 快速建立新分類
- **字元計數器**：即時顯示標題/內容字數，接近上限時顏色警告
- **自動調整**：文字區域高度自動適應內容長度
- **離開保護**：表單變更時防止意外離開頁面

### 🏷️ 標籤管理系統

**智能輸入**：
- **自動完成**：輸入時顯示現有標籤建議清單
- **即時建立**：輸入新標籤名稱直接建立並套用
- **重複檢查**：防止同一備忘錄重複加入相同標籤
- **視覺化管理**：已選標籤以彩色徽章顯示，支援單獨移除

**AJAX 整合**：
- **OnPostCreateTagAsync**：後端標籤建立 API
- **動態更新**：新建標籤即時加入建議清單
- **錯誤處理**：建立失敗時提供明確錯誤訊息

### 📁 分類管理系統

**選擇機制**：
- **下拉選單**：顯示所有現有分類，支援層級顯示
- **未分類選項**：提供「未分類」選項，對應 CategoryId = 0
- **快速建立**：彈出式對話框快速建立新分類

**即時整合**：
- **OnPostCreateCategoryAsync**：後端分類建立 API
- **自動選取**：新建分類自動設為當前備忘錄分類
- **重複驗證**：防止建立重複名稱的分類

### 🛡️ 使用者體驗保護

**資料保護**：
- **變更偵測**：監控表單所有輸入欄位變更狀態
- **離開警告**：使用 `beforeunload` 事件防止意外離開
- **提交清除**：表單提交時清除警告狀態

**輸入驗證**：
- **長度限制**：標題 200 字元、內容 2000 字元限制
- **即時反饋**：字元計數接近上限時顏色變化提醒
- **必填驗證**：標題為必填欄位，空白時無法儲存

### 🎨 現代化介面設計

**響應式佈局**：
- **行動優先**：Bootstrap 5 響應式網格系統
- **觸控友善**：按鈕大小和間距適合觸控操作
- **彈性排版**：標籤和分類區域支援動態調整

**視覺層次**：
- **卡片設計**：主要內容區域使用卡片容器
- **漸層效果**：按鈕和表單元件具備懸停動畫
- **色彩系統**：一致的色彩主題，支援品牌客製化

### 🔗 系統整合

**與 index4 無縫整合**：
- **共用服務**：使用相同的 `IEnhancedMemoNoteService`
- **資料一致性**：標籤和分類資料即時同步
- **導航流暢**：編輯完成後返回列表頁面，保持選中狀態

**API 相容性**：
- **RESTful 設計**：使用標準的 HTTP POST 方法
- **JSON 回應**：AJAX 操作回傳 JSON 格式結果
- **錯誤處理**：統一的錯誤處理和使用者提示機制

---

---

## 台幣外幣匯率計算器（index6.cshtml）

`index6.cshtml` 為企業級的台幣外幣匯率計算系統，整合台灣銀行官方 CSV API，提供即時匯率查詢、雙向精確計算和使用者友善的匯率管理體驗。

### 🏦 核心功能特色

- **即時匯率資料**：直接整合台灣銀行官方 CSV API (`https://rate.bot.com.tw/xrt/flcsv/0/day`)
- **雙向計算系統**：支援台幣→外幣、外幣→台幣的精確匯率計算
- **智能匯率選擇**：自動選擇最適合的匯率類型（現金買入/賣出、即期買入/賣出）
- **即時匯率顯示**：四種匯率類型完整展示，視覺化匯率資訊卡片
- **資料快取機制**：JSON 檔案本地快取，API 失敗時自動降級使用
- **精確計算保證**：所有計算保留小數點後6位，避免浮點數精度損失

### 💱 支援貨幣清單

- **USD** (美金) 🇺🇸 - 最常用的國際結算貨幣
- **JPY** (日圓) 🇯🇵 - 亞洲主要貨幣，旅遊熱門
- **CNY** (人民幣) 🇨🇳 - 兩岸貿易主要貨幣
- **EUR** (歐元) 🇪🇺 - 歐洲統一貨幣
- **GBP** (英鎊) 🇬🇧 - 傳統國際儲備貨幣
- **HKD** (港幣) 🇭🇰 - 亞洲金融中心貨幣
- **AUD** (澳幣) 🇦🇺 - 太平洋地區重要貨幣

### 🎯 使用者體驗設計

**響應式界面**：
- Bootstrap 5 完全響應式設計，支援桌面和行動裝置
- 視覺化匯率卡片，色彩編碼不同匯率類型
- 即時表單驗證和錯誤提示

**智能操作流程**：
- 計算方式切換時自動清除結果並更新匯率顯示
- 貨幣選擇變更時使用 AJAX 即時更新匯率，避免頁面重新載入
- 匯率資料過期時提供明確警告和更新建議

**安全性保障**：
- 所有輸入參數經過後端驗證
- 匯率資料完整性檢查
- 計算結果精度保證

### 🔧 技術架構亮點

**後端服務層**：
- `ExchangeRateService.cs`：核心匯率服務，負責 API 整合、資料解析、計算邏輯
- `ExchangeRate.cs`：完整的匯率資料模型，支援四種匯率類型
- `index6.cshtml.cs`：PageModel 設計，提供多個 Handler 支援不同操作

**前端增強功能**：
- AJAX 部分更新：匯率顯示區域支援無縫更新
- JavaScript 表單增強：智能表單狀態管理
- 載入狀態提示：操作過程提供視覺回饋

**資料持久化**：
- **檔案位置**：`App_Data/exchange_rates.json` 🔴
- **更新策略**：每次 API 呼叫成功後自動更新本地快取
- **過期檢測**：24小時資料過期提醒機制

### 📊 匯率計算邏輯

**台幣 → 外幣**：
```
結果金額 = 台幣金額 ÷ 現金賣出匯率 (優先)
         或 ÷ 即期賣出匯率 (備用)
```

**外幣 → 台幣**：
```
結果金額 = 外幣金額 × 現金買入匯率 (優先)
         或 × 即期買入匯率 (備用)
```

### 🛡️ 可靠性設計

**錯誤處理機制**：
- API 連線失敗時自動使用本地快取資料
- 匯率資料缺失時停用計算功能並提供明確提示
- 所有異常情況提供使用者友善的錯誤訊息

**資料驗證**：
- 金額輸入範圍檢查 (0.01 ~ double.MaxValue)
- 匯率資料完整性驗證
- 計算結果精度驗證

**⚠️ 重要提醒**：
- **備份資料**：定期備份 `App_Data/exchange_rates.json` 檔案
- **網路依賴**：首次使用需要網路連線獲取匯率資料
- **更新頻率**：建議每日更新匯率資料以確保準確性
- **免責聲明**：匯率資料僅供參考，實際交易請以銀行公告為準

**相關檔案**：
- `Pages/index6.cshtml`：前端頁面和 JavaScript 增強功能
- `Pages/index6.cshtml.cs`：後端邏輯和 API handlers  
- `Services/ExchangeRateService.cs`：匯率資料服務核心
- `Models/ExchangeRate.cs`：匯率資料模型
- `App_Data/exchange_rates.json`：匯率快取檔案 🔴

---

## 個人記帳系統（index7.cshtml & index8.cshtml）

個人記帳系統提供完整的收支管理解決方案，採用月曆檢視設計，支援記錄新增修改、統計分析、以及多格式報表匯出功能。系統設計注重使用者體驗和資料可視化，適合個人財務管理使用。

### 💰 系統特色

- **月曆檢視界面**：直觀顯示每日收支狀況，支援月份導航
- **收支分類管理**：二階分類系統（大分類 > 細分類），支援動態載入
- **即時統計分析**：自動計算月度收入、支出、淨收支和記錄筆數
- **多格式匯出**：CSV、Excel、PDF 三種格式，含中文編碼最佳化
- **金額格式化**：支援大金額（最高 9.99 億）和千分位顯示
- **響應式設計**：Bootstrap 5 框架，完美適配桌面和行動裝置

### 📊 記帳列表頁面（index7）

#### 核心功能

**月曆檢視特色**：
- 標準月曆格局 (7x6)，支援跨月份瀏覽
- 每日收支統計：收入（綠色）、支出（紅色）即時顯示
- 記錄數量徽章：顯示當日記錄筆數
- 今日標記：特殊背景色標識當前日期

**統計卡片系統**：
- 月度統計摘要：總收入、總支出、淨收支、記錄數量
- 視覺化指標：收入用綠色、支出用紅色、淨收支動態顏色
- 即時更新：新增或修改記錄後自動重新計算

**記錄管理操作**：
- 快速編輯：直接從月曆格子點擊編輯記錄
- 刪除確認：安全的刪除操作流程
- 批次操作：支援多筆記錄同時處理

#### 月份導航系統
- 上月/下月快速切換按鈕
- 年月下拉選擇器
- 一鍵回到當月功能
- URL 參數記憶：支援書籤和分享

### ✏️ 記帳編輯頁面（index8）

#### 表單設計特色

**收支類型選擇**：
- 大按鈕設計：收入（綠色）、支出（紅色）
- 視覺化反饋：選中狀態有縮放動畫效果
- 類型切換時自動更新分類選項

**動態分類系統**：
- 大分類下拉選單：根據收支類型動態載入
- 子分類 AJAX 載入：選擇大分類後自動載入對應子分類
- 分類資料持久化：JSON 檔案儲存分類結構

**金額輸入增強**：
- 即時格式化：自動添加千分位逗號
- 範圍驗證：支援 0.01 到 999,999,999
- 伺服器端驗證：雙重檢查確保資料正確性

**智能表單驗證**：
- 客戶端即時驗證：輸入時立即檢查
- 日期限制：不允許未來日期
- 必填欄位檢查：視覺化錯誤提示

#### 資料處理流程
1. 載入現有記錄（編輯模式）或設定預設值（新增模式）
2. 動態載入分類選項和付款方式
3. 表單驗證和資料檢查
4. 儲存到 JSON 檔案
5. 重導向回列表頁面並顯示成功訊息

### 📄 多格式匯出功能

#### CSV 匯出 - 中文編碼最佳化
```csharp
// 使用 UTF-8 with BOM 解決中文亂碼問題
var encoding = new UTF8Encoding(true);
return encoding.GetBytes(csv.ToString());
```

**特色**：
- 支援 Excel 直接開啟無亂碼
- 特殊字元跳脫處理
- 千分位金額格式

#### Excel 匯出 - 多工作表設計
**包含工作表**：
1. **統計摘要**：報表期間、財務統計、匯出時間
2. **詳細記錄**：完整記錄列表，含篩選功能
3. **分類分析**：收入和支出分類統計，含占比分析

**技術特色**：
- ClosedXML 函式庫支援複雜格式
- 自動欄寬調整
- 收入支出顏色區分
- 數字格式化 (#,##0)

#### PDF 匯出 - 中文字型支援
```csharp
// 使用專門的中文字型提供者
var pdfBytes = PdfExportUtility.ConvertHtmlToPdfWithChineseSupport(htmlReport, _logger);
```

**內容包括**：
- 財務摘要表格
- 收入支出分類分析（含占比）
- 詳細記錄列表
- 自動分頁和頁碼

### 🔧 技術架構

#### 後端服務層
**AccountingService.cs**：
- 記錄 CRUD 操作
- 月度統計計算
- 月曆資料產生
- 分類管理功能

**資料模型設計**：
```csharp
public class AccountingRecord
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } // "Income" or "Expense"
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public string PaymentMethod { get; set; }
    public string Note { get; set; }
}
```

#### 前端技術棧
- **Bootstrap 5**：響應式網格系統和元件
- **Font Awesome**：圖標系統
- **JavaScript ES6+**：AJAX 互動和表單處理
- **CSS3**：動畫效果和視覺增強

#### 資料持久化
**檔案位置**：
- `App_Data/accounting-records.json`：記帳記錄資料 🔴
- `App_Data/accounting-categories.json`：分類結構資料 🔴

**資料格式範例**：
```json
{
  "records": [
    {
      "id": 1,
      "date": "2025-08-29",
      "type": "Expense",
      "amount": 150,
      "category": "餐飲",
      "subCategory": "午餐",
      "paymentMethod": "現金",
      "note": "便當"
    }
  ]
}
```

**⚠️ 重要提醒**：
- **定期備份**：`App_Data/accounting-*.json` 檔案是重要資料
- **檔案權限**：確保應用程式對 App_Data 資料夾有讀寫權限
- **大金額處理**：系統支援最高 999,999,999 的金額輸入
- **日期限制**：不允許輸入未來日期的記錄

**相關檔案**：
- `Pages/index7.cshtml`：記帳列表頁面和月曆檢視
- `Pages/index7.cshtml.cs`：列表邏輯、統計計算、匯出功能
- `Pages/index8.cshtml`：記帳編輯頁面和表單驗證
- `Pages/index8.cshtml.cs`：編輯邏輯、分類載入、資料驗證
- `Services/AccountingService.cs`：記帳資料服務核心
- `Models/AccountingModels.cs`：記帳資料模型定義
- `Services/StatisticsService.cs`：統計分析服務
- `Services/FinancialInsightsService.cs`：財務洞察服務
- `Services/AnomalyDetectionService.cs`：異常偵測服務
- `Services/PredictiveAnalysisService.cs`：預測分析服務
- `wwwroot/js/financial-ai.js`：AI 智慧分析前端腳本
- `wwwroot/js/statistics-advanced.js`：進階統計分析腳本
- `App_Data/accounting-records.json`：記帳記錄檔案 🔴
- `App_Data/accounting-categories.json`：分類結構檔案 🔴

### 🤖 AI 智慧分析功能

記帳系統整合了先進的 AI 智慧分析功能，提供深度財務洞察和個人化建議，幫助使用者更好地管理財務狀況。

#### 財務健康度評分系統
- **整體健康度評分**：0-100 分制，綜合評估財務狀況
- **儲蓄能力分析**：評估收支比例和儲蓄潛力
- **收支平衡指標**：分析收入支出的健康比例
- **成長趨勢評估**：追蹤財務狀況的變化趨勢

#### 智慧洞察與分析
```javascript
// 智慧洞察載入示例
class FinancialAI {
    async loadSmartInsights() {
        const insights = await fetch('/index7?handler=SmartInsights');
        // 分析支出模式、節省機會、趨勢變化
    }
}
```

**洞察類型**：
- **支出模式分析**：識別消費習慣和週期性模式
- **節省機會識別**：發現可優化的支出類別
- **趨勢分析洞察**：預測未來財務發展方向
- **比較分析**：與歷史數據和標準基準的對比

#### 異常偵測與警報系統
**Z-Score 分析**：
- 偵測異常高額支出
- 識別消費模式的突然變化
- 提供風險等級評估（低、中、高、嚴重）

**移動平均偏差分析**：
- 追蹤支出模式的長期變化
- 偵測消費習慣的異常波動
- 提供預警機制和建議

**頻率異常分析**：
- 監控消費頻率的變化
- 識別新的消費模式
- 分析分類消費頻率的異常

#### 支出預測與現金流分析
**預測演算法**：
```csharp
// 線性回歸預測支出
private ExpenseForecast? ForecastCategoryExpense(
    string category, List<AccountingRecord> historicalRecords, int monthsAhead)
{
    // 使用歷史數據建立預測模型
    var (slope, intercept) = CalculateLinearRegression(monthlyExpenses);
    // 加入季節性調整因子
    var forecastAmount = baseAmount * seasonalFactor;
}
```

**預測功能**：
- **支出預測**：基於歷史數據預測未來 6 個月支出
- **收入預測**：分析收入趨勢和穩定性
- **現金流預測**：預測未來 12 個月的現金流狀況
- **季節性分析**：識別不同月份的消費模式

#### 個人化建議系統
**建議生成邏輯**：
- **分類建議**：針對各支出分類提供優化建議
- **預算建議**：基於歷史數據建議合理預算
- **節約機會**：識別具體的節約潛力和方法
- **財務目標**：提供達成財務目標的具體步驟

**建議類型**：
```typescript
interface PersonalizedRecommendation {
    category: string;           // 建議類別
    priority: "high" | "medium" | "low";  // 優先級
    impact: number;            // 預期影響 (0-100)
    description: string;       // 建議描述
    actionItems: string[];     // 具體行動項目
    estimatedSavings: number;  // 預估節省金額
}
```

#### 技術架構特色
**微服務化設計**：
- `FinancialInsightsService`：財務洞察核心服務
- `AnomalyDetectionService`：異常偵測專門服務  
- `PredictiveAnalysisService`：預測分析演算法服務
- `StatisticsService`：統計分析統一介面

**前端整合**：
- Chart.js 圖表視覺化
- 即時資料載入和更新
- 響應式儀表板設計
- 互動式分析面板

**資料處理流程**：
1. **資料收集**：從記帳記錄提取分析所需資料
2. **模型訓練**：使用歷史資料建立預測和分析模型  
3. **洞察生成**：運行各種分析演算法產生洞察
4. **建議產生**：基於分析結果生成個人化建議
5. **視覺化呈現**：透過圖表和儀表板展示結果

**⚠️ AI 分析注意事項**：
- **資料需求**：需要至少 3-6 個月的記帳資料才能產生準確分析
- **模型準確性**：預測準確度會隨著資料量增加而提升
- **隱私保護**：所有分析均在本地進行，不涉及外部 API
- **定期更新**：建議定期檢視和更新 AI 分析結果

---

## 智慧待辦清單系統（todo.cshtml）

`todo.cshtml` 是一個功能完整的智慧待辦清單管理系統，提供直覺的任務管理體驗，具備時間智慧分組、進階篩選、標籤系統等現代化功能。

### 🎯 核心功能特色

#### 智慧時間分組
- **自動分類**：系統自動將任務分為今日、明日、本週、未來和無到期日等類別
- **逾期識別**：自動標示逾期任務，提供視覺警示
- **優先級管理**：支援高、中、低優先級設定，並以顏色編碼區分
- **進度追蹤**：實時統計待處理、進行中、已完成任務數量

#### 任務管理功能
- **完整 CRUD**：支援任務的建立、讀取、更新、刪除操作
- **狀態管理**：支援待處理、進行中、已完成三種狀態
- **拖拽排序**：支援拖拽重新排序任務優先順序
- **批次操作**：支援批次標示完成、批次刪除等操作

#### 進階搜尋與篩選
```javascript
// 多維度篩選功能
const filterOptions = {
    status: ['pending', 'in-progress', 'completed'],
    priority: ['high', 'medium', 'low'],
    category: ['work', 'personal', 'shopping'],
    tags: ['urgent', 'important', 'routine'],
    dueDate: ['today', 'tomorrow', 'overdue']
};
```

### 🎨 使用者介面設計

#### 響應式卡片布局
- **統計摘要**：頂部顯示待處理、進行中、已完成、逾期任務統計
- **分組檢視**：按時間緊急程度自動分組顯示
- **任務卡片**：每個任務以卡片形式呈現，包含標題、描述、到期日、標籤等資訊
- **狀態指示**：使用顏色編碼和圖示清楚表示任務狀態

#### 互動式編輯體驗
- **模態框編輯**：點擊任務即可在彈出視窗中編輯
- **即時保存**：支援自動保存和手動保存
- **表單驗證**：完整的前端和後端驗證機制
- **標籤輸入**：支援標籤的新增、編輯、刪除和自動完成

### 🏗️ 技術架構亮點

#### 後端設計模式
```csharp
public class TodoModel : PageModel
{
    private readonly TodoService _todoService;
    private readonly ILogger<TodoModel> _logger;

    // 智慧分組屬性
    public List<TodoTask> TodayTasks { get; set; } = [];
    public List<TodoTask> TomorrowTasks { get; set; } = [];
    public List<TodoTask> ThisWeekTasks { get; set; } = [];
    public List<TodoTask> FutureTasks { get; set; } = [];
    public List<TodoTask> CompletedTasks { get; set; } = [];
    
    // 統計資訊
    public TodoStatistics Statistics { get; set; } = new();
}
```

#### 服務層設計
- **TodoService**：核心業務邏輯服務，處理任務 CRUD 操作
- **依賴注入**：使用 ASP.NET Core 內建 DI 容器
- **資料持久化**：使用 JSON 檔案儲存，支援升級到資料庫
- **錯誤處理**：完整的例外處理和日誌記錄

#### 前端技術整合
- **AJAX 化操作**：所有操作均透過 AJAX 進行，無需頁面重載
- **即時搜尋**：JavaScript 實現本地即時搜尋功能
- **拖拽支援**：整合 Sortable.js 實現拖拽排序
- **Bootstrap 5**：使用最新 Bootstrap 框架確保響應式設計

### 📊 資料模型設計

#### TodoTask 核心模型
```csharp
public class TodoTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public DateTime? DueDate { get; set; }
    public int EstimatedMinutes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsCompleted { get; set; }
    public int Order { get; set; }
}
```

#### 統計分析模型
```csharp
public class TodoStatistics
{
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
    public int TotalCount { get; set; }
    public double CompletionRate { get; set; }
}
```

### 🚀 效能最佳化

#### 快取策略
- **記憶體快取**：分組資料在記憶體中快取 5 分鐘
- **延遲載入**：已完成任務限制顯示最近 30 個
- **本地搜尋**：搜尋功能在前端執行，減少伺服器負載

#### 使用者體驗優化
- **載入提示**：所有非同步操作提供載入動畫
- **錯誤處理**：友善的錯誤訊息和重試機制
- **自動保存**：編輯過程中定期自動保存草稿
- **鍵盤快速鍵**：支援常用操作的鍵盤快速鍵

**相關檔案**：
- `Pages/todo.cshtml`：待辦清單主頁面和 UI 模板
- `Pages/todo.cshtml.cs`：頁面後端邏輯和 API 處理器
- `Services/TodoService.cs`：待辦事項核心服務
- `Models/TodoModels.cs`：待辦事項資料模型定義
- `App_Data/todo-tasks.json`：任務資料檔案 🔴
- `App_Data/todo-categories.json`：分類資料檔案 🔴
- `wwwroot/js/todo.js`：前端互動邏輯腳本

---

## 財務儀表板系統（index9.cshtml）

`index9.cshtml` 是一個進階財務儀表板系統，整合記帳數據提供全方位的財務分析視覺化介面，具備動態圖表、多時間範圍分析、智慧統計等功能。

### 📊 核心分析功能

#### 多維度財務統計
- **收支分析**：實時計算當期收入、支出和淨收支
- **時間範圍切換**：支援本週、本月、本年、上月等多種時間維度
- **比較分析**：自動計算與上期的變化百分比和趨勢方向
- **日均統計**：精確計算日均消費和收入指標

#### 視覺化圖表系統
```javascript
// 收支趨勢線圖
window.trendChart = new Chart(ctx, {
    type: 'line',
    data: {
        labels: months,
        datasets: [
            {
                label: '收入',
                data: incomeData,
                borderColor: '#28a745',
                backgroundColor: 'rgba(40, 167, 69, 0.1)'
            },
            {
                label: '支出',
                data: expenseData,
                borderColor: '#dc3545',
                backgroundColor: 'rgba(220, 53, 69, 0.1)'
            }
        ]
    }
});
```

#### 智慧統計卡片
- **收入統計卡**：顯示當期收入、變化趨勢和成長率
- **支出統計卡**：顯示當期支出、變化趨勢和控制狀況
- **淨收支卡**：顯示收支平衡和財務健康度
- **日均支出卡**：顯示消費習慣和預算控制情況

### 🎨 使用者介面設計

#### 響應式儀表板布局
- **統計卡片區**：4 欄網格布局，展示關鍵財務指標
- **圖表區域**：分為趨勢線圖和分類圓餅圖兩個主要區塊
- **交易記錄區**：顯示最近交易記錄的表格檢視
- **控制面板**：時間範圍選擇和重新整理控制

#### 動態圖表互動
- **Chart.js 整合**：使用最新版本 Chart.js 提供豐富圖表類型
- **即時更新**：切換時間範圍時圖表自動重繪
- **互動提示**：滑鼠懸停顯示詳細數據
- **響應式適配**：自動適應不同螢幕尺寸

### 🏗️ 技術架構設計

#### 智慧快取系統
```csharp
public class Index9Model : PageModel
{
    private static DashboardStats? _cachedStats;
    private static DateTime _lastCacheUpdate = DateTime.MinValue;

    private async Task GetDashboardStatsAsync()
    {
        // 5分鐘快取機制，避免重複計算
        if (_cachedStats != null && 
            DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes < 5)
        {
            return;
        }
        
        // 重新計算統計資料
        var allRecords = await _accountingService.GetRecordsAsync();
        _cachedStats = CalculateStats(filteredRecords, allRecords);
        _lastCacheUpdate = DateTime.Now;
    }
}
```

#### AJAX 資料更新機制
- **非同步載入**：所有資料更新透過 AJAX 進行
- **載入指示器**：提供載入動畫和進度提示
- **錯誤處理**：完善的網路錯誤處理機制
- **資料快取**：前端快取機制減少重複請求

### 📈 分析演算法

#### 趨勢計算邏輯
```csharp
private List<MonthlyTrend> CalculateTrendData(List<AccountingRecord> records)
{
    if (TimeRange == "thisYear")
    {
        // 年度趨勢：月份統計
        for (int month = 1; month <= 12; month++)
        {
            var monthlyIncome = monthRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);
            var monthlyExpense = Math.Abs(monthRecords.Where(r => r.Amount < 0).Sum(r => r.Amount));
            
            trendData.Add(new MonthlyTrend
            {
                MonthName = $"{month}月",
                Income = monthlyIncome,
                Expense = monthlyExpense
            });
        }
    }
    else
    {
        // 月度趨勢：日統計
        var groupedByDay = records.GroupBy(r => r.Date.Date);
        // 處理日度數據...
    }
    
    return trendData;
}
```

#### 分類分析功能
- **支出分類統計**：自動計算各分類支出佔比
- **圓餅圖視覺化**：直觀展示支出分布
- **動態顏色配置**：為不同分類自動分配顏色
- **百分比計算**：精確計算各分類佔比

### 🛡️ 效能與安全性

#### 快取最佳化
- **靜態快取**：使用靜態變數實現記憶體快取
- **過期機制**：5分鐘自動過期，確保資料新鮮度
- **記憶體管理**：合理控制快取大小，避免記憶體洩漏

#### 安全性設計
- **CSRF 保護**：所有 POST 請求使用防偽令牌
- **輸入驗證**：完整的前端和後端資料驗證
- **XSS 防護**：Razor 引擎自動 HTML 編碼輸出

### 🚀 擴展性設計

#### 模組化架構
- **服務解耦**：清晰的服務層邊界
- **介面導向**：使用介面實現依賴倒置
- **配置外部化**：支援外部配置檔案
- **API 準備**：可輕易轉換為 Web API

#### 升級路徑
- **資料庫支援**：可升級到 Entity Framework Core
- **分散式快取**：可整合 Redis 或 Memcached
- **微服務架構**：支援拆分為獨立微服務
- **雲端部署**：支援 Azure、AWS 等雲端平台

**相關檔案**：
- `Pages/index9.cshtml`：財務儀表板主頁面和圖表模板
- `Pages/index9.cshtml.cs`：儀表板後端邏輯和統計計算
- `Models/DashboardModels.cs`：儀表板資料模型定義
- `Services/IAccountingService.cs`：記帳資料服務介面
- `wwwroot/js/dashboard.js`：前端圖表和互動邏輯
- 整合 `App_Data/accounting-records.json`：記帳資料來源 🔴

---

## 設定說明

- **appsettings.json**：全域設定檔，適用於所有環境。
- **appsettings.Development.json**：開發環境專用設定，會覆蓋部分全域設定。
- **wwwroot/**：放置網站靜態資源（如 CSS、JS、圖片等）。
- **🔴 App_Data/notes.json**：註記功能的資料儲存檔案，包含所有使用者的日期註記內容。
- **🔴 App_Data/memo-notes.json**：備忘錄主要資料檔案，包含所有備忘錄內容和關聯資訊。
- **🔴 App_Data/tags.json**：標籤系統資料檔案，包含標籤定義、顏色和使用統計。
- **🔴 App_Data/categories.json**：分類系統資料檔案，包含分類階層和描述資訊。
- **🔴 App_Data/todo-tasks.json**：待辦任務資料檔案，包含所有任務資訊和狀態。
- **🔴 App_Data/todo-categories.json**：待辦分類資料檔案，包含任務分類定義。

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

### 🗂️ 備忘錄系統設定

**主要資料檔案**: `App_Data/memo-notes.json`
**標籤資料檔案**: `App_Data/tags.json`
**分類資料檔案**: `App_Data/categories.json`

**資料格式說明**：

**memo-notes.json**：
```json
[
  {
    "Id": 1,
    "Title": "專案企劃書",
    "Content": "完成 Q4 專案企劃書初稿...",
    "CreatedDate": "2025-08-27T10:30:00",
    "ModifiedDate": "2025-08-28T14:20:00",
    "CategoryId": 1,
    "Tags": [1, 2, 5]
  }
]
```

**tags.json**：
```json
[
  {
    "Id": 1,
    "Name": "工作",
    "Color": "#007bff",
    "Description": "工作相關事項",
    "CreatedDate": "2025-08-27T09:00:00",
    "UsageCount": 15
  }
]
```

**categories.json**：
```json
[
  {
    "Id": 1,
    "Name": "專案管理",
    "Description": "專案相關文件和追蹤",
    "ParentId": null,
    "Icon": "fas fa-project-diagram"
  }
]
```

### 🏦 匯率系統設定

**匯率快取檔案**: `App_Data/exchange_rates.json`
**資料來源**: 台灣銀行官方 CSV API
**更新策略**: 每次 API 呼叫成功後自動更新，24小時過期提醒
**備份建議**: 建議定期備份，避免需要重複下載匯率資料

**資料格式說明**：

**exchange_rates.json**：
```json
{
  "lastUpdated": "2025-08-28T10:30:00",
  "source": "台灣銀行CSV API",
  "rates": [
    {
      "currencyCode": "USD",
      "currencyName": "美金",
      "buyRate": 31.500000,
      "sellRate": 31.600000,
      "cashBuyRate": 31.400000,
      "cashSellRate": 31.700000
    }
  ]
}
```

### ✅ 待辦清單系統設定

**主要資料檔案**: `App_Data/todo-tasks.json`
**分類資料檔案**: `App_Data/todo-categories.json`

**資料格式說明**：

**todo-tasks.json**：
```json
[
  {
    "Id": 1,
    "Title": "完成專案報告",
    "Description": "撰寫 Q4 專案總結報告",
    "Status": "InProgress",
    "Priority": "High", 
    "Category": "工作",
    "Tags": ["重要", "截止日期"],
    "DueDate": "2025-09-01T18:00:00",
    "EstimatedMinutes": 120,
    "CreatedDate": "2025-08-27T10:00:00",
    "CompletedDate": null,
    "IsCompleted": false,
    "Order": 1
  }
]
```

**todo-categories.json**：
```json
[
  {
    "Id": 1,
    "Name": "工作",
    "Description": "工作相關任務",
    "Color": "#007bff",
    "Icon": "fas fa-briefcase"
  }
]
```

**⚠️ 重要備份提醒**：
- 定期備份整個 `App_Data` 資料夾
- 系統升級前務必完整備份
- 建議使用版本控制系統追蹤資料變更

## 常見問題

> [!NOTE]
> 若遇到執行錯誤，請確認已安裝 .NET 8 SDK，並檢查專案相依套件是否完整。

---

如需更多協助，請參閱原始碼或提出 Issue。
