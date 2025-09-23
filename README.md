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
      - [🎙️ Phase 1 語音輸入核心功能](#️-phase-1-語音輸入核心功能)
      - [🎨 Phase 2 使用者體驗優化](#-phase-2-使用者體驗優化)
      - [🧠 Phase 3 智能化增強功能](#-phase-3-智能化增強功能)
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
  - [習慣追蹤系統（habits.cshtml）](#習慣追蹤系統habitscshtml)
    - [📋 核心功能特色](#-核心功能特色-2)
      - [習慣管理功能](#習慣管理功能)
      - [打卡追蹤系統](#打卡追蹤系統)
      - [進度視覺化](#進度視覺化)
    - [🎨 使用者介面設計](#-使用者介面設計-2)
      - [儀表板布局](#儀表板布局)
      - [互動設計](#互動設計)
    - [🏗️ 技術架構設計](#️-技術架構設計-1)
      - [後端服務設計](#後端服務設計)
      - [前端技術整合](#前端技術整合-1)
    - [📊 統計分析功能](#-統計分析功能)
      - [數據分析](#數據分析)
      - [成就系統](#成就系統)
    - [🚀 資料管理](#-資料管理)
      - [儲存結構](#儲存結構)
      - [備份與還原](#備份與還原)
  - [全方位資料匯出系統（export.cshtml）](#全方位資料匯出系統exportcshtml)
    - [📤 核心匯出功能](#-核心匯出功能)
      - [資料來源整合](#資料來源整合)
      - [多格式支援](#多格式支援)
    - [🎯 匯出配置系統](#-匯出配置系統)
      - [靈活的匯出選項](#靈活的匯出選項)
      - [匯出樣板系統](#匯出樣板系統)
    - [🔧 技術架構特色](#-技術架構特色)
      - [統一匯出服務](#統一匯出服務)
      - [專業匯出服務](#專業匯出服務)
    - [🎨 使用者體驗設計](#-使用者體驗設計-1)
      - [直觀的匯出流程](#直觀的匯出流程)
      - [進度追蹤系統](#進度追蹤系統)
    - [📊 匯出歷史管理](#-匯出歷史管理)
      - [歷史記錄功能](#歷史記錄功能)
      - [統計分析](#統計分析)
    - [🛡️ 安全與效能](#️-安全與效能)
      - [安全控制](#安全控制)
      - [效能優化](#效能優化)
  - [投資追蹤器系統（investment-*.cshtml）](#投資追蹤器系統investment-cshtml)
    - [📈 核心投資功能](#-核心投資功能)
      - [投資組合管理](#投資組合管理)
      - [持倉追蹤系統](#持倉追蹤系統)
      - [交易記錄管理](#交易記錄管理)
    - [📊 股價監控與分析](#-股價監控與分析)
      - [即時股價更新](#即時股價更新)
      - [損益自動計算](#損益自動計算)
      - [投資分析視覺化](#投資分析視覺化)
    - [🎯 使用者介面設計](#-使用者介面設計-2)
      - [響應式投資儀表板](#響應式投資儀表板)
      - [互動式圖表分析](#互動式圖表分析)
    - [🏗️ 技術架構設計](#️-技術架構設計-2)
      - [服務層設計](#服務層設計-1)
      - [API 整合架構](#api-整合架構)
    - [📈 投資分析算法](#-投資分析算法)
      - [投資組合統計](#投資組合統計)
      - [風險評估指標](#風險評估指標)
    - [🚀 資料管理與擴展](#-資料管理與擴展)
      - [JSON 資料持久化](#json-資料持久化)
      - [股價 API 整合](#股價-api-整合)
  - [設定說明](#設定說明)
    - [📝 註記功能設定](#-註記功能設定)
    - [🗂️ 備忘錄系統設定](#️-備忘錄系統設定)
    - [🏦 匯率系統設定](#-匯率系統設定)
    - [✅ 待辦清單系統設定](#-待辦清單系統設定)
    - [📋 習慣追蹤系統設定](#-習慣追蹤系統設定)
    - [📤 資料匯出系統設定](#-資料匯出系統設定)
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
- **📋 習慣追蹤系統**，每日習慣打卡、進度視覺化、連續性統計、成就系統和分析報表
- **📤 全方位資料匯出**，支援 PDF/Excel/CSV/JSON 多格式匯出，整合所有模組資料的統一匯出平台
- **📈 投資追蹤器系統**，投資組合管理、持倉追蹤、交易記錄、股價監控和投資分析視覺化
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
# 習慣追蹤: http://localhost:5000/habits
# 資料匯出: http://localhost:5000/export
# 投資組合: http://localhost:5000/investment-portfolio
# 持倉管理: http://localhost:5000/investment-holdings
# 交易記錄: http://localhost:5000/investment-transactions
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
│   ├── TodoService.cs # 待辦事項資料服務（JSON 檔案 I/O）
│   ├── HabitService.cs # 習慣追蹤資料服務（JSON 檔案 I/O）
│   ├── InvestmentService.cs # 投資追蹤資料服務（JSON 檔案 I/O）
│   ├── StockPriceService.cs # 股價資料服務（API 整合）
│   ├── ExportService.cs # 統一資料匯出服務
│   ├── PdfExportService.cs # PDF 匯出專業服務
│   ├── ExcelExportService.cs # Excel 匯出專業服務
│   ├── CsvExportService.cs # CSV 匯出專業服務
│   ├── StatisticsService.cs # 統計分析服務
│   ├── FinancialInsightsService.cs # 財務洞察分析服務
│   ├── SmartCategoryService.cs # 智慧分類服務
│   ├── AnomalyDetectionService.cs # 異常偵測服務
│   ├── PredictiveAnalysisService.cs # 預測分析服務
│   └── BudgetManagementService.cs # 預算管理服務
├── Models/               # 資料模型
│   ├── ExchangeRate.cs   # 匯率資料模型
│   ├── AccountingModels.cs # 記帳資料模型
│   ├── DashboardModels.cs # 財務儀表板資料模型
│   ├── TodoModels.cs # 待辦事項資料模型
│   ├── HabitModels.cs # 習慣追蹤資料模型
│   ├── InvestmentModels.cs # 投資追蹤資料模型
│   ├── ExportModels.cs # 資料匯出模型
│   ├── StatisticsModels.cs # 統計分析模型
│   ├── InsightsModels.cs # 洞察分析模型
│   ├── SmartCategoryModels.cs # 智慧分類模型
│   ├── AnomalyModels.cs # 異常偵測模型
│   └── BudgetModels.cs # 預算管理模型
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
│   ├── habits.cshtml     # 📋 習慣追蹤系統頁面
│   ├── habits.cshtml.cs  # habits 頁面 Model（習慣管理、進度追蹤）
│   ├── investment-portfolio.cshtml     # 📈 投資組合頁面
│   ├── investment-portfolio.cshtml.cs  # investment-portfolio 頁面 Model（組合管理、統計分析）
│   ├── investment-holdings.cshtml      # 📊 持倉管理頁面
│   ├── investment-holdings.cshtml.cs   # investment-holdings 頁面 Model（持倉追蹤、損益計算）
│   ├── investment-transactions.cshtml  # 💼 交易記錄頁面
│   ├── investment-transactions.cshtml.cs # investment-transactions 頁面 Model（交易管理、統計資料）
│   ├── export.cshtml     # 📤 資料匯出系統頁面
│   ├── export.cshtml.cs  # export 頁面 Model（匯出設定、進度管理）
│   └── ...
├── wwwroot/              # 靜態資源 (CSS, JS, 圖片)
├── App_Data/             # 應用程式資料檔案
│   ├── notes.json        # 📝 註記資料儲存檔案（重要備份目標）
│   ├── memo-notes.json   # 🗂️ 備忘錄資料儲存檔案（重要備份目標）
│   ├── exchange_rates.json # 🏦 匯率快取檔案（重要備份目標）
│   ├── tags.json         # 🏷️ 標籤資料儲存檔案
│   ├── categories.json   # 📁 分類資料儲存檔案
│   ├── accounting-records.json # 💰 記帳記錄檔案（重要備份目標）
│   ├── accounting-categories.json # 💰 記帳分類檔案（重要備份目標）
│   ├── todo-tasks.json # ✅ 待辦任務資料檔案（重要備份目標）
│   ├── todo-categories.json # ✅ 待辦分類資料檔案（重要備份目標）
│   ├── habit-records.json # 📋 習慣記錄資料檔案（重要備份目標）
│   ├── habit-categories.json # 📋 習慣分類資料檔案（重要備份目標）
│   ├── habits.json # 📋 習慣定義資料檔案（重要備份目標）
│   ├── portfolios.json # 📈 投資組合資料檔案（重要備份目標）
│   ├── holdings.json # 📊 投資持倉資料檔案（重要備份目標）
│   ├── transactions.json # 💼 投資交易記錄檔案（重要備份目標）
│   ├── export-history.json # 📤 匯出歷史記錄檔案（重要備份目標）
│   ├── budget-settings.json # 💰 預算設定資料檔案（重要備份目標）
│   ├── spending-patterns.json # 💰 消費模式資料檔案（重要備份目標）
│   ├── category-rules.json # 💰 分類規則資料檔案（重要備份目標）
│   ├── category-training.json # 💰 分類訓練資料檔案（重要備份目標）
│   ├── merchant-mapping.json # 💰 商家對應資料檔案（重要備份目標）
│   └── keyword-dictionary.json # 💰 關鍵字字典檔案（重要備份目標）
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

#### 🎙️ Phase 1 語音輸入核心功能

**智能語音解析系統**：
- **9欄位完整解析**：日期、收支類型、金額、付款方式、大分類、細分類、描述、商家名稱、備註
- **信心度機制**：每個欄位獨立信心度評估，彩色指示器直觀展示（綠色≥70%、黃色40-69%、紅色<40%）
- **整體信心度**：加權計算整體解析可靠性，進度條視覺化展示

**支援語音格式**：
- 完整語句：「我昨天在星巴克用信用卡花了150元買咖啡」
- 相對日期：今天、昨天、前天、明天、後天
- 絕對日期：10月1日、2023年10月1日、十月一日
- 多種金額表達：150元、500塊、花了1000
- 15+種付款方式：信用卡、現金、LINE Pay、悠遊卡等
- 30+個常見商家：星巴克、7-11、麥當勞、全家等

**智能推斷機制**：
- 商家分類對應：星巴克 → 餐飲美食/咖啡茶飲
- 關鍵字智能匹配：咖啡、早餐、加油、購物等
- 上下文分析：結合商家名稱和描述進行精準分類

**語音解析結果預覽**：
- 詳細欄位展示：所有解析結果一目了然
- 信心度視覺化：每個欄位的可靠性清楚標示
- 未解析內容提醒：顯示無法識別的語音內容
- 一鍵套用功能：解析結果直接填入表單欄位

**錯誤處理與降級**：
- 部分解析失敗不影響整體功能
- 最低信心度閾值保證基本可用性
- 用戶友善的錯誤提示和操作指引

#### 🎨 Phase 2 使用者體驗優化

**響應式語音介面設計**：
- **對話式操作面板**：將傳統按鈕式介面升級為自然對話流程
- **智能提示系統**：提供上下文相關的操作建議和引導文字
- **視覺化回饋增強**：美觀的動畫效果和即時狀態顯示
- **錯誤恢復機制**：智能錯誤偵測和恢復建議，降低使用門檻

**多平台適配優化**：
- **行動裝置優化**：針對手機和平板的觸控體驗最佳化
- **語音按鈕增強**：更大的觸控區域和清晰的視覺指示
- **載入效能提升**：優化語音識別服務載入時間和記憶體使用
- **離線降級支援**：網路中斷時的功能降級和本地快取

**互動流程改善**：
- **語音輸入引導**：新手使用教學和範例語句展示
- **結果確認優化**：更直觀的編輯和確認流程
- **快速修正功能**：針對常見錯誤的一鍵修正選項
- **使用習慣記憶**：記住用戶偏好的操作模式和設定

#### 🧠 Phase 3 智能化增強功能

**個人化學習引擎**：
- **用戶偏好學習**：自動學習個人化關鍵字、分類偏好和商家表達方式
- **個人字典建立**：為每位用戶建立專屬的語音識別字典和同義詞庫
- **使用習慣分析**：追蹤語音輸入模式、時間偏好和消費行為特徵
- **準確度自適應**：根據修正記錄動態調整解析算法和信心度權重

**智能上下文理解**：
- **多輪對話支援**：維持對話狀態，支援分次輸入和逐步完善記錄
- **意圖識別系統**：自動識別輸入意圖（新增記錄/修正資料/查詢資訊）
- **上下文感知建議**：基於當前對話內容提供相關建議和後續動作
- **智能欄位補全**：根據已輸入資訊預測和建議缺失欄位內容

**對話式語音助手**：
- **自然對話介面**：模擬真人助手的對話體驗，提供引導性問題和回應
- **智能問答系統**：針對不清楚或缺失的資訊主動詢問，引導用戶完善記錄
- **個人化回應**：基於用戶特徵和歷史行為客製化對話風格和建議內容
- **多輪修正支援**：支援連續多次修正和調整，直到用戶滿意為止

**智能建議與學習**：
- **個人化建議系統**：基於個人消費模式提供智能分類、商家和付款方式建議
- **學習回饋機制**：從用戶修正行為中持續學習，不斷提升個人化準確度
- **模式識別分析**：識別個人消費模式、時間偏好和地點習慣
- **準確度追蹤**：統計個人化學習效果，展示準確度改善趨勢

**智能資料持久化**：
- **個人偏好儲存**：JSON 格式儲存個人化關鍵字、分類映射和學習統計
- **學習歷史記錄**：保存學習事件和修正記錄，支援學習效果分析
- **跨會話記憶**：維持跨瀏覽器會話的個人化設定和學習成果
- **隱私保護設計**：本地儲存確保個人資料隱私，支援資料清除和重置

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

## 習慣追蹤系統（habits.cshtml）

`habits.cshtml` 為完整的習慣追蹤與管理系統，提供習慣建立、每日打卡、進度追蹤和統計分析功能。此系統結合了現代化介面設計、智慧統計算法，以及完整的成就系統。

### 📋 核心功能特色

#### 習慣管理功能
- **習慣建立**：支援自訂習慣名稱、描述、目標和顏色主題
- **習慣分類**：多層級分類系統，便於習慣組織管理
- **目標設定**：支援每日、每週、每月不同週期目標
- **優先級管理**：高、中、低優先級設定，影響顯示順序

#### 打卡追蹤系統
- **一鍵打卡**：簡單快速的打卡操作
- **歷史記錄**：完整的打卡歷史追蹤和查看
- **連續天數統計**：自動計算習慣連續完成天數
- **完成率計算**：動態計算週期內完成率

#### 進度視覺化
- **進度條顯示**：直觀的進度條展示完成情況
- **日曆熱力圖**：以日曆形式顯示打卡密度
- **統計圖表**：趨勢線圖、圓餅圖展示習慣統計
- **成就徽章**：達成里程碑自動獲得成就徽章

### 🎨 使用者介面設計

#### 儀表板布局
- **統計卡片**：今日完成、本週目標、總習慣數等關鍵指標
- **習慣卡片網格**：響應式卡片布局，支援不同裝置
- **快速操作**：卡片上直接支援打卡、編輯、刪除操作
- **搜尋篩選**：支援按名稱、分類、狀態篩選習慣

#### 互動設計
- **拖拽排序**：支援習慣卡片拖拽重新排序
- **模態框編輯**：流暢的模態框編輯體驗
- **即時反饋**：操作後即時更新界面和統計
- **響應式設計**：完美適配桌面和行動裝置

### 🏗️ 技術架構設計

#### 後端服務設計
- **HabitService.cs**：核心習慣管理服務
- **資料模型**：`HabitModels.cs` 定義完整資料結構
- **JSON 儲存**：輕量級檔案儲存，易於備份和遷移
- **依賴注入**：標準 ASP.NET Core DI 容器管理

#### 前端技術整合
- **Bootstrap 5**：響應式界面框架
- **Font Awesome**：豐富的圖示支援
- **Chart.js**：專業圖表繪製函式庫
- **jQuery**：DOM 操作和 AJAX 通信

### 📊 統計分析功能

#### 數據分析
- **完成率統計**：按日、週、月計算完成率
- **趨勢分析**：長期趨勢變化分析
- **習慣排名**：根據完成率和連續天數排名
- **時間分析**：分析習慣完成的時間模式

#### 成就系統
- **里程碑追蹤**：7天、30天、100天等里程碑
- **完成率徽章**：不同完成率等級的成就徽章
- **連續記錄**：連續完成天數的成就認定
- **分類精通**：在特定分類達成專家級別

### 🚀 資料管理

#### 儲存結構
```json
{
  "Id": 1,
  "Name": "晨間運動",
  "Description": "每天早上進行30分鐘運動",
  "Category": "健康",
  "Priority": "High",
  "Color": "#007bff",
  "TargetFrequency": "Daily",
  "CreatedDate": "2025-08-01T00:00:00",
  "Records": [
    {
      "Date": "2025-08-31",
      "Completed": true,
      "Notes": "完成慢跑30分鐘"
    }
  ]
}
```

#### 備份與還原
- **自動備份**：定期自動備份習慣資料
- **匯出功能**：支援匯出為 CSV、PDF 格式
- **資料還原**：支援從備份檔案還原資料
- **跨裝置同步**：（預留擴展功能）

---

## 全方位資料匯出系統（export.cshtml）

`export.cshtml` 為統一的資料匯出平台，整合系統內所有模組的資料，提供多格式匯出功能。支援 PDF、Excel、CSV、JSON 四種主流格式，滿足不同場景的匯出需求。

### 📤 核心匯出功能

#### 資料來源整合
- **記帳系統資料**：收支記錄、分類統計、時間範圍分析
- **習慣追蹤資料**：習慣定義、打卡記錄、統計報表
- **待辦事項資料**：任務列表、完成狀態、分類資訊
- **備忘錄資料**：筆記內容、標籤分類、建立時間
- **註記資料**：日期註記、月曆標記資訊
- **匯率資料**：匯率歷史、計算記錄

#### 多格式支援
- **PDF 報表**：專業級報表，支援圖表、統計表格
- **Excel 工作簿**：多工作表、公式計算、圖表整合
- **CSV 檔案**：標準格式，便於資料分析和匯入
- **JSON 資料**：完整資料結構，便於程式處理

### 🎯 匯出配置系統

#### 靈活的匯出選項
- **資料類型選擇**：多選框選擇要匯出的資料模組
- **時間範圍篩選**：支援自訂開始和結束日期
- **快速時間選擇**：今日、本週、本月、本年等快速選項
- **格式個別設定**：不同格式支援不同的詳細設定

#### 匯出樣板系統
- **預設樣板**：內建多種常用匯出樣板
- **自訂樣板**：支援使用者建立個人化匯出格式
- **樣板管理**：樣板的儲存、修改、刪除功能
- **樣板分享**：（預留功能）樣板匯入匯出

### 🔧 技術架構特色

#### 統一匯出服務
```csharp
public class ExportService
{
    // 統一匯出入口
    public async Task<ExportResult> ExportDataAsync(ExportRequest request)
    {
        var data = await CollectDataAsync(request);
        
        return request.Format switch
        {
            "pdf" => await _pdfService.ExportAsync(data, request),
            "excel" => await _excelService.ExportAsync(data, request),
            "csv" => await _csvService.ExportAsync(data, request),
            "json" => await _jsonService.ExportAsync(data, request),
            _ => throw new NotSupportedException($"不支援的格式: {request.Format}")
        };
    }
}
```

#### 專業匯出服務
- **PdfExportService**：使用 iText 7 製作專業 PDF 報表
- **ExcelExportService**：使用 ClosedXML 建立 Excel 工作簿
- **CsvExportService**：使用 CsvHelper 產生標準 CSV 檔案
- **JSON 匯出**：原生 .NET JSON 序列化

### 🎨 使用者體驗設計

#### 直觀的匯出流程
1. **選擇資料**：勾選要匯出的資料類型
2. **設定範圍**：選擇時間範圍和篩選條件
3. **選擇格式**：選擇匯出格式和樣板
4. **開始匯出**：一鍵開始匯出流程
5. **下載檔案**：匯出完成後立即下載

#### 進度追蹤系統
- **即時進度**：顯示匯出進度百分比
- **狀態更新**：顯示當前處理的資料類型
- **預估時間**：根據資料量預估完成時間
- **錯誤處理**：友善的錯誤訊息和重試機制

### 📊 匯出歷史管理

#### 歷史記錄功能
- **匯出記錄**：保存每次匯出的詳細記錄
- **檔案管理**：已匯出檔案的儲存和管理
- **重新下載**：支援重新下載歷史匯出檔案
- **自動清理**：過期檔案自動清理機制

#### 統計分析
- **匯出統計**：匯出頻率、格式偏好統計
- **使用分析**：最常匯出的資料類型分析
- **效能監控**：匯出時間、檔案大小等效能指標
- **錯誤追蹤**：匯出失敗原因統計和分析

### 🛡️ 安全與效能

#### 安全控制
- **檔案權限**：匯出檔案的存取權限控制
- **資料敏感性**：敏感資料的特殊處理
- **檔案加密**：（預留功能）重要資料加密匯出
- **存取日誌**：完整的匯出存取日誌

#### 效能優化
- **非同步處理**：大量資料非同步匯出處理
- **分批處理**：超大資料集分批匯出
- **記憶體管理**：合理控制記憶體使用
- **快取策略**：常用資料的智慧快取

**相關檔案**：
- `Pages/export.cshtml`：資料匯出主頁面
- `Pages/export.cshtml.cs`：匯出邏輯和設定處理
- `Services/ExportService.cs`：統一匯出服務
- `Services/PdfExportService.cs`：PDF 專業匯出服務
- `Services/ExcelExportService.cs`：Excel 匯出服務
- `Services/CsvExportService.cs`：CSV 匯出服務
- `Models/ExportModels.cs`：匯出相關資料模型
- 整合所有 `App_Data/*.json` 資料檔案 🔴

---

## 投資追蹤器系統（investment-*.cshtml）

### 📈 核心投資功能

#### 投資組合管理

**📊 投資組合總覽**：
- **多組合管理**：支援建立多個投資組合，分別追蹤不同投資策略
- **即時統計**：自動計算總資產、總成本、損益金額和報酬率
- **視覺化分析**：使用 Chart.js 提供圓餅圖資產配置和折線圖趨勢分析
- **組合比較**：並列顯示多個投資組合的績效比較

**🎯 關鍵指標監控**：
```csharp
// 投資組合統計指標
public class Portfolio 
{
    public decimal TotalValue { get; set; }          // 總市值
    public decimal TotalCost { get; set; }           // 總成本
    public decimal TotalGainLoss { get; set; }       // 總損益
    public decimal TotalGainLossPercentage { get; set; } // 總報酬率
}
```

#### 持倉追蹤系統

**💼 持倉明細管理**：
- **全方位持股資訊**：股票代號、名稱、數量、平均成本、現價、市值
- **多維度篩選**：支援依投資組合、投資類型、股票代號快速篩選
- **即時損益計算**：自動計算每檔股票的損益金額和報酬率
- **股價監控**：整合股價更新功能，支援批次或個別股票價格更新

**📊 持倉分析特色**：
```javascript
// 持倉損益視覺化
function updateHoldingColor(gainLoss) {
    return gainLoss >= 0 ? 'text-success' : 'text-danger';
}
```

#### 交易記錄管理

**📝 交易紀錄系統**：
- **完整交易資訊**：買賣類型、數量、價格、手續費、交易日期
- **自動持倉更新**：交易記錄自動同步更新相關持倉資訊和平均成本
- **交易統計分析**：提供交易次數、總成交金額、手續費統計
- **快速交易輸入**：支援批次交易記錄和重複交易快速建立

**💰 交易處理邏輯**：
```csharp
// 交易記錄自動更新持倉
private async Task UpdateHoldingFromTransactionAsync(Transaction transaction)
{
    if (transaction.Type == "買入") {
        // 加權平均成本計算
        var totalCost = (holding.Quantity * holding.AverageCost) + 
                       (transaction.Quantity * transaction.Price);
        holding.Quantity += transaction.Quantity;
        holding.AverageCost = totalCost / holding.Quantity;
    }
}
```

### 📊 股價監控與分析

#### 即時股價更新

**🔄 股價資料服務**：
- **多市場支援**：支援台股（XXXX.TW）和美股（AAPL）格式
- **模擬 API 整合**：現階段提供模擬股價，架構支援真實 API 整合
- **智慧快取機制**：5分鐘快取避免頻繁 API 呼叫
- **股票搜尋功能**：支援股票代號模糊搜尋和自動完成

**📈 股價更新機制**：
```csharp
public class StockPriceService 
{
    // 支援台股與美股不同格式
    private async Task<StockPrice?> GetTaiwanStockPriceAsync(string symbol)
    private async Task<StockPrice?> GetUSStockPriceAsync(string symbol) 
    
    // 智慧快取減少 API 呼叫
    private readonly Dictionary<string, StockPrice> _priceCache;
}
```

#### 損益自動計算

**🧮 損益計算引擎**：
- **即時損益更新**：股價異動時自動重新計算所有相關損益
- **多層級統計**：個股損益、組合損益、總體損益三個層級
- **百分比報酬率**：提供金額和百分比兩種損益顯示方式
- **成本基礎追蹤**：精確追蹤每檔股票的平均成本基礎

#### 投資分析視覺化

**📊 Chart.js 圖表整合**：
- **資產配置圓餅圖**：顯示投資組合中各股票的權重分佈
- **投資趨勢折線圖**：追蹤投資組合價值的時間變化趨勢
- **損益比較柱狀圖**：比較各檔股票的績效表現
- **響應式圖表**：支援桌面和行動裝置的互動體驗

### 🎯 使用者介面設計

#### 響應式投資儀表板

**💻 現代化設計**：
- **Bootstrap 5 響應式布局**：完全適配桌面、平板、手機螢幕
- **統計卡片設計**：清晰展示關鍵投資指標
- **色彩語言**：綠色獲利、紅色虧損的直觀色彩區分
- **載入狀態指示**：提供清晰的操作回饋和載入狀態

#### 互動式圖表分析

**🎨 Chart.js 視覺化**：
```javascript
// 資產配置圓餅圖
this.allocationChart = new Chart(ctx, {
    type: 'doughnut',
    data: {
        labels: allocation.labels,
        datasets: [{
            data: allocation.values,
            backgroundColor: ['#ff6384', '#36a2eb', '#ffce56']
        }]
    }
});
```

### 🏗️ 技術架構設計

#### 服務層設計

**🔧 核心服務架構**：
- **InvestmentService**：投資組合、持倉、交易記錄的 CRUD 操作
- **StockPriceService**：股價資料擷取、快取管理、搜尋功能
- **依賴注入整合**：完整的 ASP.NET Core 依賴注入支援
- **非同步設計**：所有 I/O 操作採用 async/await 模式

#### API 整合架構

**🌐 RESTful API 設計**：
- **統一錯誤處理**：標準化的 API 錯誤回應格式
- **資料驗證機制**：前後端雙重資料驗證
- **HTTP 狀態碼**：正確使用 REST API 狀態碼規範
- **JSON 序列化**：優化的 JSON 資料傳輸格式

```csharp
// API 控制器標準化錯誤處理
try {
    var portfolios = await _investmentService.GetPortfoliosAsync();
    return Ok(portfolios);
} catch (Exception ex) {
    return StatusCode(500, new { 
        message = "取得投資組合失敗", 
        error = ex.Message 
    });
}
```

### 📈 投資分析算法

#### 投資組合統計

**📊 統計計算引擎**：
- **加權平均成本**：精確計算多次買入的平均成本
- **總投資報酬率**：(市值 - 成本) / 成本 × 100%
- **資產權重分析**：計算各檔股票在組合中的權重占比
- **風險分散指標**：分析投資組合的集中度風險

#### 風險評估指標

**⚡ 風險管理機制**：
- **集中度風險**：監控單一股票占比過高的風險
- **波動性追蹤**：追蹤投資組合的價格波動狀況
- **成本基礎保護**：防止成本基礎計算錯誤的資料驗證
- **異常交易偵測**：識別可能的異常交易記錄

### 🚀 資料管理與擴展

#### JSON 資料持久化

**💾 檔案儲存架構**：
- **portfolios.json**：投資組合基本資訊和統計資料
- **holdings.json**：持股明細和計算結果資料  
- **transactions.json**：完整的交易歷史記錄
- **自動備份建議**：建議定期備份投資資料檔案

#### 股價 API 整合

**🔌 擴展性設計**：
- **Alpha Vantage 整合準備**：架構支援 Alpha Vantage API
- **配置管理系統**：透過 appsettings.json 管理 API 金鑰
- **多 API 支援**：設計支援多個股價資料來源
- **錯誤處理機制**：API 失敗時的優雅降級機制

```csharp
// 股價 API 配置範例
"StockApi": {
    "ApiKey": "YOUR_ALPHA_VANTAGE_KEY",
    "BaseUrl": "https://www.alphavantage.co/query",
    "CacheExpiryMinutes": 5
}
```

**🎯 未來擴展規劃**：
- 整合真實股價 API（Alpha Vantage、Yahoo Finance）
- 支援更多投資工具（基金、債券、加密貨幣）  
- 新增技術分析指標和圖表
- 實作投資警報和通知系統
- 提供投資建議和智慧分析

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

### 📋 習慣追蹤系統設定

**主要資料檔案**: `App_Data/habits.json`  
**記錄資料檔案**: `App_Data/habit-records.json`  
**分類資料檔案**: `App_Data/habit-categories.json`

**資料格式說明**：

**habits.json**：
```json
[
  {
    "Id": 1,
    "Name": "晨間運動",
    "Description": "每天早上進行30分鐘運動",
    "Category": "健康",
    "Priority": "High",
    "Color": "#007bff",
    "Icon": "fas fa-running",
    "TargetFrequency": "Daily",
    "TargetValue": 1,
    "Unit": "次",
    "IsActive": true,
    "CreatedDate": "2025-08-01T00:00:00",
    "Order": 1
  }
]
```

**habit-records.json**：
```json
[
  {
    "Id": 1,
    "HabitId": 1,
    "Date": "2025-08-31T00:00:00",
    "Completed": true,
    "Value": 1,
    "Notes": "完成慢跑30分鐘",
    "CreatedDate": "2025-08-31T07:30:00"
  }
]
```

### � 投資追蹤器系統設定

**主要資料檔案**: `App_Data/portfolios.json`
**持倉資料檔案**: `App_Data/holdings.json`  
**交易資料檔案**: `App_Data/transactions.json`

**資料格式說明**：

**portfolios.json**：
```json
[
  {
    "Id": 1,
    "Name": "核心投資組合",
    "Description": "長期價值投資策略", 
    "CreatedAt": "2025-08-01T00:00:00",
    "TotalValue": 150000.00,
    "TotalCost": 120000.00,
    "TotalGainLoss": 30000.00,
    "TotalGainLossPercentage": 25.00
  }
]
```

**holdings.json**：
```json
[
  {
    "Id": 1,
    "PortfolioId": 1,
    "Symbol": "2330.TW",
    "Name": "台積電",
    "Type": "股票",
    "Quantity": 100,
    "AverageCost": 500.00,
    "CurrentPrice": 600.00,
    "MarketValue": 60000.00,
    "GainLoss": 10000.00,
    "GainLossPercentage": 20.00,
    "LastUpdated": "2025-08-31T14:30:00"
  }
]
```

**transactions.json**：
```json
[
  {
    "Id": 1,
    "PortfolioId": 1,
    "Symbol": "2330.TW",
    "Type": "買入",
    "Quantity": 100,
    "Price": 500.00,
    "TotalAmount": 50000.00,
    "Fee": 71.00,
    "Date": "2025-08-01T09:30:00",
    "Note": "分批建倉",
    "CreatedAt": "2025-08-01T09:35:00"
  }
]
```

**股價 API 設定**：
```json
{
  "StockApi": {
    "ApiKey": "demo",
    "BaseUrl": "https://www.alphavantage.co/query",
    "CacheExpiryMinutes": 5,
    "RequestDelayMs": 1000
  }
}
```

### �📤 資料匯出系統設定

**匯出歷史檔案**: `App_Data/export-history.json`  
**匯出暫存目錄**: `wwwroot/exports/`  
**匯出樣板目錄**: `App_Data/export-templates/`

**匯出歷史格式**：
```json
[
  {
    "Id": "guid-string",
    "UserId": "system",
    "DataTypes": ["accounting", "habits", "todo"],
    "Format": "pdf",
    "StartDate": "2025-08-01T00:00:00",
    "EndDate": "2025-08-31T23:59:59",
    "FileName": "export_20250831_143022.pdf",
    "FilePath": "wwwroot/exports/export_20250831_143022.pdf",
    "FileSize": 2048576,
    "Status": "Completed",
    "CreatedDate": "2025-08-31T14:30:22",
    "CompletedDate": "2025-08-31T14:30:45",
    "Duration": "00:00:23"
  }
]
```

**匯出設定說明**：
- **檔案清理週期**：預設7天自動清理過期匯出檔案
- **單次匯出限制**：建議單次匯出資料不超過100MB
- **支援格式**：PDF、Excel (.xlsx)、CSV、JSON
- **並發限制**：同時只能進行一個匯出作業

**⚠️ 重要備份提醒**：
- 定期備份整個 `App_Data` 資料夾
- 系統升級前務必完整備份  
- 建議使用版本控制系統追蹤資料變更
- 匯出的檔案請自行保存，系統會定期清理

## 常見問題

> [!NOTE]
> 若遇到執行錯誤，請確認已安裝 .NET 8 SDK，並檢查專案相依套件是否完整。

---

如需更多協助，請參閱原始碼或提出 Issue。
