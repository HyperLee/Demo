# index3 - 日曆頁面開發規格書

## 專案概述
本規格書定義 ASP.NET Core Razor Pages 應用程式中「index3」日曆頁面的完整開發標準，包含月曆視圖功能、**個人化備註系統**、**有備註日期的視覺標記**、UI/UX 設計、路由規格、參數驗證、狀態處理、響應式設計、無障礙設計、國際化支援、效能最佳化、測試策略與驗收標準。

## 檔案架構
```
Pages/
├── index3.cshtml          # 前端視圖（Razor 頁面標記、Bootstrap 樣式、互動邏輯、備註表單）
├── index3.cshtml.cs       # 後端 PageModel（參數處理、業務邏輯、資料模型、備註處理）
└── Shared/
    └── _Calendar.cshtml   # 可選：日曆元件部分檢視

Services/
└── NoteService.cs         # 備註服務（JSON 檔案儲存、CRUD 操作、月份查詢）

App_Data/
└── notes.json            # 備註資料儲存（JSON 格式持久化）
```

## 核心目標
### 主要功能
1. **月曆視圖**：呈現完整的月份日曆格線，支援任意年月瀏覽
2. **導航控制**：提供直覺的年月切換機制（按鈕 + 下拉選單）
3. **跳至今日功能**：一鍵快速返回當前日期並選中（新增功能）
4. **日期選擇**：支援點選特定日期並導向詳細資訊頁面
5. **今日強調**：視覺上突出顯示當前日期，包含特殊圖示和文字標籤
6. **智能距離計算**：選取日期後顯示與今日的天數差異（今天/明天/昨天/X天前後）
7. **華麗視覺效果**：漸層背景、陰影效果、懸停動畫等現代化設計
8. **響應式設計**：適配桌面、平板、手機等不同螢幕尺寸
9. **無障礙體驗**：支援鍵盤操作與螢幕閱讀器
10. **即時時間顯示**：標題區顯示當前台灣時區時間
11. **🆕 個人化備註系統**：為任意日期新增、編輯、刪除文字備註
12. **🆕 有備註日期視覺標記**：金色漸層背景、閃爍徽章、書籤圖示等多重視覺提示
13. **🆕 備註統計與引導**：月份統計資訊、智慧鼓勵文案、圖例說明

### 技術特色
- 純 ASP.NET Core 8.0 實作，無需額外 JavaScript 框架
- Bootstrap 5 響應式設計系統，搭配自定義漸層樣式
- C# 13 現代語法：switch expressions、record types、DateOnly 結構
- 支援多國語言與時區
- SEO 友善的 URL 結構
- 高效能日期計算演算法（42格月曆）
- CSS3 高級特效：漸層背景、變換動畫、陰影系統
- 智能參數修正與用戶友善的錯誤處理
- 無障礙設計完全符合 WCAG 2.1 AA 標準
- 先進的視覺設計：懸停效果、放大動畫、狀態區分
- **🆕 JSON 檔案儲存系統**：輕量級個人備註持久化，支援 UTF-8 多語言
- **🆕 執行緒安全檔案操作**：SemaphoreSlim 保護並發存取
- **🆕 高效能備註查詢**：HashSet 優化的 O(1) 日期查詢演算法
- **🆕 跨月備註載入**：智慧處理月曆跨越多月的備註顯示
- **🆕 CSS 動畫系統**：脈衝、呼吸燈等專業動畫效果

## 路由與 URL 設計規格

### 基礎路由
- **主要路由**：`/index3`
- **完整路由**：`/index3{?year,month,day}`

### 查詢參數規格
| 參數名稱 | 類型 | 範圍 | 必填 | 說明 | 範例 |
|---------|------|------|------|------|------|
| `year` | int | 1900-2100 | 否 | 四位數年份 | `2025` |
| `month` | int | 1-12 | 否 | 月份數值 | `8` |
| `day` | int | 1-31 | 否 | 日期（用於高亮特定日期） | `26` |

### URL 範例
```
/index3                           # 顯示當前月份
/index3?year=2025                 # 顯示 2025 年當前月
/index3?year=2025&month=8         # 顯示 2025 年 8 月
/index3?year=2025&month=8&day=26  # 顯示 2025 年 8 月並高亮 26 日
```

### 參數預設行為
- **無參數**：自動使用系統當前日期（年/月）
- **部分參數**：缺失參數以當前日期補齊
- **無效參數**：自動修正至最接近的有效值，並顯示友善提示訊息

### 錯誤處理策略
- **超出範圍的年份**：自動調整至邊界值（1900 或 2100）
- **無效月份**：調整至 1-12 範圍內最接近值
- **無效日期**：調整至該月最後一天
- **非數字參數**：忽略並使用預設值
- **所有修正都會顯示使用者友善的通知訊息**

## 使用者互動流程設計

### 主要使用情境
1. **初始載入流程**
   - 解析 URL 查詢參數
   - 驗證並修正參數值
   - 計算月曆格線資料（42 格 = 6 週 × 7 天）
   - 渲染完整月曆視圖

2. **月份導航流程**
   - **上/下月按鈕**：單擊切換至相鄰月份
   - **年份選擇器**：下拉選單快速跳轉至指定年份
   - **月份選擇器**：下拉選單快速跳轉至指定月份
   - **即時更新**：每次切換都會更新 URL 並重新載入頁面

3. **日期選擇流程**
   - **當月日期**：可點擊，導向詳細頁面或更新選取狀態
   - **非當月日期**：顯示但不可點擊，提供視覺連續性
   - **今日高亮**：自動以特殊樣式標示當前日期

4. **錯誤恢復流程**
   - 自動修正無效參數
   - 顯示友善的修正通知
   - 保持使用者操作的連續性

### 互動設計原則
- **最少點擊原則**：常用操作（上/下月）需在一次點擊內完成
- **視覺回饋原則**：所有可點擊元素都有 hover 和 active 狀態
- **容錯設計原則**：優雅處理所有異常輸入
- **無障礙原則**：支援鍵盤導航和螢幕閱讀器

## 🆕 個人化備註系統規格

### 備註功能核心需求
#### 備註 CRUD 操作
- **新增備註**：選取日期後在備註區域輸入文字內容並儲存
- **編輯備註**：點選已有備註的日期，預載現有內容供編輯
- **刪除備註**：提供專用刪除按鈕，含確認對話框防誤刪
- **檢視備註**：選取日期後在頁面下方顯示完整備註內容

#### 備註資料格式
- **儲存格式**：JSON 鍵值對 `{"yyyy-MM-dd": "備註內容"}`
- **檔案路徑**：`App_Data/notes.json`
- **編碼格式**：UTF-8 with BOM，支援多語言文字
- **內容限制**：純文字格式，支援換行符，無長度限制
- **特殊字元**：完整 Unicode 支援，包含表情符號

#### 備註視覺標記系統
- **標記優先級**：今日 > 選取 > 有備註 > 一般日期
- **金色漸層效果**：
  - 背景：135度金色到橘色漸層
  - 縮放：1.02倍輕微放大突出顯示
  - 動畫：3秒週期呼吸燈光暈效果
- **多重圖示標記**：
  - 便利貼徽章：右上角紅色 `bi-sticky-fill` 脈衝動畫
  - 書籤圖示：日期內部 `bi-journal-bookmark-fill` 標記
  - "備註" 文字：金色背景下的白色文字標籤
- **懸停效果**：保持金色主題，不覆蓋備註狀態

#### 備註表單設計
- **表單位置**：選取日期下方，左側綠色邊框卡片
- **輸入控制項**：
  - 大尺寸 `textarea`（`form-control-lg`）
  - 可調整高度（`resize: vertical`）
  - 最小高度 80px，3行預設顯示
  - 佔位文字：「在此輸入您的備註...」
- **操作按鈕**：
  - **儲存備註**：綠色主要按鈕，`bi-check-circle` 圖示
  - **刪除備註**：紅色外框按鈕，含確認對話框
  - **清空**：灰色次要按鈕，重設輸入框
- **驗證與回饋**：
  - 即時驗證：無需 JavaScript，伺服器端驗證
  - 錯誤顯示：Bootstrap Alert 警告訊息
  - 成功回饋：重新導向保持選取狀態

### 備註統計與引導功能
#### 月份統計顯示
- **統計卡片**：頁面底部漸層背景資訊卡片
- **統計內容**：「本月共有 X 天有備註資訊」
- **智慧文案**：
  - ≥10天：「您非常勤於記錄！繼續保持這個好習慣。」
  - 5-9天：「記錄得不錯！可以考慮記錄更多重要日子。」
  - 1-4天：「開始記錄重要日子，讓您的行程管理更有效率。」
  - 0天：「目前還沒有任何備註，點選任一日期開始新增備註。」

#### 使用者引導設計
- **圖例說明**：四格視覺圖例清楚說明不同日期狀態
  - 今日：藍色 + 星星圖示
  - 已選取：綠色 + 勾選圖示
  - 有備註：金色漸層 + 書籤圖示 + 徽章
  - 一般：白色 + 日曆圖示
- **空狀態指引**：無備註時提供明確的操作指導
- **功能提示**：表單下方顯示使用說明文字

### 備註服務架構設計
#### INoteService 介面規格
```csharp
public interface INoteService
{
    Task<string?> GetNoteAsync(DateOnly date);
    Task SaveNoteAsync(DateOnly date, string note);
    Task DeleteNoteAsync(DateOnly date);
    Task<HashSet<DateOnly>> GetNoteDatesInMonthAsync(int year, int month);
}
```

#### JsonNoteService 實作特色
- **執行緒安全**：`SemaphoreSlim` 檔案鎖保護並發操作
- **例外處理**：完整的檔案 I/O 例外處理與復原機制
- **效能優化**：
  - 單次檔案讀取，記憶體中操作
  - `HashSet<DateOnly>` 提供 O(1) 查詢效能
  - 跨月查詢邏輯處理 42 格月曆需求
- **資料驗證**：
  - 日期格式驗證 `DateOnly.TryParse`
  - 空白內容自動刪除處理
  - Unicode 字元完整支援

#### 資料持久化策略
- **檔案位置**：`App_Data/notes.json`（應用程式相對路徑）
- **檔案權限**：應用程式讀寫權限，用戶無法直接存取
- **備份建議**：定期備份 JSON 檔案防資料遺失
- **遷移路徑**：預留資料庫升級介面，支援未來 SQL 資料庫遷移

## 功能需求詳細規格

### 月曆顯示規格
#### 週起始設定
- **起始日**：星期日（符合台灣使用習慣）
- **週標題順序**：日、一、二、三、四、五、六
- **多語言支援**：週標題可依語系切換（中文/英文/日文）

#### 日期格線設計
- **格線結構**：6 行 × 7 列 = 42 格（涵蓋最長月份顯示需求）
- **當月日期**：正常顯示，可點擊互動
- **前月補齊**：淡化顯示，不可點擊，提供視覺連續性
- **後月補齊**：淡化顯示，不可點擊，補足格線完整性

#### 特殊日期標示
- **今日標示**：
  - 視覺：藍色底色 + 白色文字 + 外圈陰影
  - 屬性：`aria-current="date"` 無障礙標記
  - 條件：僅在當前瀏覽月份中顯示
- **選擇日期**：
  - 視覺：外框高亮 + 背景色變化
  - 行為：點擊後導向詳細頁面或更新 URL
- **🆕 有備註日期標示**：
  - **金色漸層背景**：`linear-gradient(135deg, #ffd700, #ffa500)` 華麗金色效果
  - **閃爍徽章**：右上角紅色便利貼圖示 `bi-sticky-fill`，2秒脈衝動畫
  - **書籤圖示**：日期內部 `bi-journal-bookmark-fill` 標記
  - **呼吸燈效果**：3秒循環的金色光暈動畫 `noteGlow`
  - **文字陰影**：增加可讀性的文字陰影效果
  - **無障礙標籤**：`aria-label` 包含「有備註」提示資訊

### 導航控制規格
#### 月份切換
- **上月/下月按鈕**：
  - 位置：標題區左右兩側
  - 樣式：Bootstrap `btn-outline-light btn-lg` 配漸層背景
  - 圖示：Bootstrap Icons `bi-chevron-left` / `bi-chevron-right`
  - 行為：GET 請求更新 URL 參數

#### 快速跳轉與導航
- **年份選擇器**：
  - 類型：HTML `<select>` 下拉選單
  - 範圍：1900-2100（200年範圍）
  - 預設：當前瀏覽年份
  - 樣式：`form-select-lg` 大尺寸選擇器
- **月份選擇器**：
  - 類型：HTML `<select>` 下拉選單  
  - 選項：1-12月（支援多語言月份名稱）
  - 預設：當前瀏覽月份
  - 樣式：`form-select-lg` 大尺寸選擇器
- **前往按鈕**：
  - 功能：提交年月選擇表單
  - 樣式：`btn-primary btn-lg` 主要操作按鈕
  - 圖示：`bi-arrow-right-circle` 向右箭頭
- **跳至今日按鈕**（新增功能）：
  - 功能：一鍵跳轉至當前日期並選中今日
  - 位置：前往按鈕右側，響應式並排顯示
  - 樣式：`btn-success btn-lg` 成功主題色
  - 圖示：`bi-calendar-check` 日曆勾選圖示
  - 行為：導向 `/index3?year=YYYY&month=MM&day=DD`（當前日期）
  - 工具提示：顯示今日完整日期資訊
  - 無障礙：`aria-label="跳至今日日期"`

#### 快速導航卡片設計
- **容器樣式**：Bootstrap 卡片組件，具陰影效果
- **標題區**：包含圖示的「快速導航」標題
- **響應式佈局**：
  - 桌面版：三列並排（年份/月份/操作按鈕）
  - 手機版：垂直堆疊，按鈕組水平排列
- **今日提示**：在按鈕組下方顯示當前日期
- **視覺增強**：表單控制項使用陰影效果

### 頁面回饋機制
#### 成功狀態
- **正常載入**：無特殊提示
- **成功切換**：平滑過渡動畫（可選）

#### 錯誤狀態處理
- **參數修正通知**：
  - 樣式：Bootstrap Alert (`alert-info`)
  - 內容：「已自動修正日期為 YYYY年M月D日」
  - 行為：可關閉的提示訊息
- **載入失敗**：降級為當前月份顯示
- **網路異常**：顯示友善的錯誤頁面

## 頁面導向與整合規格

### 詳細頁面導向設計
#### 目標路由架構
- **短期實作**：`/index3?year=YYYY&month=MM&day=DD`（同頁更新選取狀態）
- **長期規劃**：`/Calendar/Detail?date=YYYY-MM-DD`（專用詳細頁面）
- **參數格式**：ISO 8601 日期格式（YYYY-MM-DD）確保國際相容性

#### 導向邏輯
```
點擊當月日期 → 建構完整日期參數 → GET 導向 → 頁面更新/重新載入
點擊非當月日期 → 無動作（cursor: not-allowed）
```

#### 狀態同步
- **URL 同步**：所有日期變更都反映在 URL 中
- **瀏覽器歷史**：支援瀏覽器前進/後退按鈕
- **書籤支援**：URL 可直接書籤儲存和分享

### 與既有系統整合
#### Layout 繼承
- 繼承專案主要 Layout 設計
- 整合導航麵包屑：首頁 → 日曆 → YYYY年MM月
- 保持品牌一致性和使用者體驗連續性

#### 樣式系統整合
- 重用既有 Bootstrap 5 主題設定
- 採用專案既定的色彩規範
- 整合既有的 CSS 變數和 Utilities 類別

## UI/UX 設計系統規格

### 視覺設計語言
#### 色彩系統與漸層設計
- **主要漸層背景**：標題區使用 135度漸層 `linear-gradient(135deg, #667eea 0%, #764ba2 100%)`
- **週標題漸層**：`linear-gradient(45deg, #6c757d, #495057)` 營造立體感
- **今日標示**：
  - 背景：`bg-primary` 藍色主題
  - 特效：放大 1.02倍 + 特殊陰影 `box-shadow: 0 8px 25px rgba(0,123,255,0.3)`
  - 裝飾：星星圖示 `bi-star-fill` + "今日" 文字標籤
- **選取狀態**：
  - 背景：`bg-success` 綠色主題  
  - 特效：放大 1.02倍 + 綠色陰影 `box-shadow: 0 8px 25px rgba(40,167,69,0.3)`
- **🆕 有備註日期標示**：
  - 背景：金色漸層 `linear-gradient(135deg, #ffd700, #ffa500)`
  - 特效：放大 1.02倍 + 動態金色光暈動畫
  - 徽章：右上角紅色脈衝徽章 `bi-sticky-fill`
  - 圖示：`bi-journal-bookmark-fill` 書籤標記
  - 文字：白色 "備註" 標籤配陰影效果
  - 動畫：3秒週期呼吸燈效果 `noteGlow`
- **懸停效果**：
  - 放大：`transform: scale(1.05)`
  - 陰影：`box-shadow: 0 4px 15px rgba(0,0,0,0.15)`
  - 過渡：`transition: all 0.2s ease-in-out`
  - **備註日期例外**：保持金色主題，不覆蓋備註視覺效果
- **非當月日期**：`text-muted opacity-50` 淡化顯示
- **選取日期面板**：漸層背景 `linear-gradient(45deg, #d4edda, #c3e6cb)` + 左側綠色邊框
- **🆕 備註統計卡片**：
  - 有備註：漸層背景 `linear-gradient(45deg, #d1ecf1, #bee5eb)` + 藍色左邊框
  - 無備註：淡灰漸層 `linear-gradient(45deg, #f8f9fa, #e9ecef)` + 灰色左邊框
- **🆕 圖例說明卡片**：白色背景，2px 淡灰邊框，15px 圓角

#### 字型與圖示系統
- **主標題**：`h3 fw-bold` 配 `bi-calendar3` 圖示，使用文字陰影增加層次
- **目前月份標題**：
  - 樣式：`h2 fw-bold text-primary` 
  - 裝飾：黃色日曆圖示 `bi-calendar-event text-warning`
  - 效果：文字陰影 `text-shadow: 2px 2px 4px rgba(0,0,0,0.1)`
  - 底線：4px 高藍色裝飾線，圓角 2px
- **週標題**：`fw-bold text-white` 配小圓點裝飾 `bi-circle-fill`
- **日期數字**：
  - 一般：`fw-bold` 1.25rem 大尺寸
  - 今日：加上星星圖示和"今日"標籤
- **按鈕圖示**：統一使用 Bootstrap Icons，增加視覺識別度

#### 卡片與陰影系統
- **標題卡片**：`shadow-lg border-0` 大陰影無邊框設計
- **快速導航卡片**：`shadow-sm` 輕微陰影
- **表單控制項**：`shadow-sm` 統一陰影效果
- **日期格子**：`shadow-sm` 基礎陰影，特殊狀態增加陰影強度

### 響應式斷點設計
#### 桌面版設計（≥992px）
- **格線尺寸**：每格最小 90px 高度，自適應寬度
- **字體大小**：日期數字 1.25rem (20px)，週標題 1rem
- **間距設定**：格子間距 2px (`g-2`)，容器 padding 24px
- **互動區域**：完整格子區域可點擊，懸停放大 1.05倍
- **快速導航**：三列並排佈局 (`col-md-4`)

#### 平板版設計（768px-991px）
- **格線尺寸**：每格最小 80px 高度，保持 7列佈局
- **字體大小**：日期數字 1.1rem，週標題 0.9rem
- **間距設定**：格子間距 2px，容器 padding 20px
- **導航控制**：按鈕保持大尺寸 (`btn-lg`) 便於觸控
- **快速導航**：響應式堆疊，按鈕組水平排列

#### 手機版設計（<768px）
- **格線尺寸**：每格最小 70px 高度，緊湊佈局
- **字體大小**：日期數字 1rem，週標題 0.8rem
- **間距設定**：格子間距 1px，容器 padding 16px
- **觸控優化**：
  - 按鈕最小點擊區域 44px×44px
  - 日期格增加觸控反饋
  - 快速導航垂直堆疊 (`d-grid gap-2`)
- **特殊適配**：標題區時間資訊在小螢幕上可能隱藏或簡化

### 動畫與過場效果
#### 頁面切換動畫
- **月份切換**：淡入淡出效果（fade in/out 0.3s）
- **載入狀態**：骨架屏或載入指示器
- **錯誤狀態**：輕微震動效果提示修正

#### 🆕 備註相關 CSS 動畫系統
- **脈衝動畫（pulse）**：
  ```css
  @keyframes pulse {
      0% { transform: scale(1); opacity: 1; }
      50% { transform: scale(1.1); opacity: 0.8; }
      100% { transform: scale(1); opacity: 1; }
  }
  ```
  - 用途：備註徽章閃爍效果，吸引使用者注意
  - 週期：2 秒無限循環
  - 應用：`.note-badge` 類別

- **呼吸燈動畫（noteGlow）**：
  ```css
  @keyframes noteGlow {
      0% { box-shadow: 0 6px 20px rgba(255,215,0,0.4); }
      50% { box-shadow: 0 8px 25px rgba(255,215,0,0.6); }
      100% { box-shadow: 0 6px 20px rgba(255,215,0,0.4); }
  }
  ```
  - 用途：有備註日期的柔和發光效果
  - 週期：3 秒無限循環，緩動函數 `ease-in-out`
  - 應用：`.note-date` 類別

- **互動微動畫優化**：
  - 硬體加速：使用 `transform` 和 `opacity` 屬性
  - 效能考量：避免觸發 layout 和 paint
  - 動畫分層：不同元素使用不同週期避免視覺衝突

#### 互動微動畫與視覺回饋
- **日期格子懸停**：
  - 放大效果：`transform: scale(1.05)`
  - 陰影增強：`box-shadow: 0 4px 15px rgba(0,0,0,0.15)`
  - 過渡時間：`transition: all 0.2s ease-in-out`
- **今日與選取狀態**：
  - 預設放大：`transform: scale(1.02)`
  - 特殊陰影：高強度彩色陰影效果
  - 無懸停動畫：保持穩定視覺狀態
- **🆕 有備註日期特殊處理**：
  - 預設狀態：金色漸層 + 1.02倍放大 + 呼吸燈動畫
  - 懸停限制：不觸發額外縮放，保持備註視覺一致性
  - 點擊回饋：短暫縮放回饋後恢復備註狀態
- **按鈕互動**：
  - 懸停：輕微陰影增強
  - 點擊：短暫縮放回饋
- **表單控制項**：
  - 焦點：邊框高亮 + 陰影效果
  - 選擇：平滑過渡動畫
- **智能距離計算動畫**：
  - 選取日期後資訊面板淡入效果
  - 距離文字根據天數差異使用不同色彩提示

### 無障礙設計規範
#### 鍵盤導航
- **Tab 順序**：導航控制 → 日期格線（由左至右、由上至下）
- **快速鍵**：方向鍵在日期格間移動，Enter/Space 選取日期
- **焦點指示**：清晰可見的焦點外框（`focus:ring-2 ring-primary`）

#### 螢幕閱讀器支援
- **日期格 ARIA 標籤**：`aria-label="2025年8月26日，星期二"`
- **今日標示**：`aria-current="date"`
- **導航控制**：`aria-label="切換至上個月"` 等明確描述
- **頁面結構**：適當的標題階層（h1, h2, h3）

#### 視覺無障礙
- **色彩對比**：所有文字與背景符合 WCAG AA 標準（對比度 ≥4.5:1）
- **色盲友善**：不僅依賴色彩傳達資訊，搭配形狀、圖示或文字
- **字型大小**：支援瀏覽器字體縮放至 200%

## 前端架構設計規格

### HTML 結構設計
```html
<div class="calendar-container">
  <!-- 標題與導航區 -->
  <header class="calendar-header">
    <div class="calendar-title">
      <h2>2025年 8月</h2>
    </div>
    <div class="calendar-controls">
      <button class="btn-prev-month">上月</button>
      <select class="year-selector">...</select>
      <select class="month-selector">...</select>
      <button class="btn-next-month">下月</button>
    </div>
  </header>
  
  <!-- 狀態訊息區 -->
  <div class="calendar-alerts">
    <!-- Bootstrap Alerts for parameter corrections -->
  </div>
  
  <!-- 週標題列 -->
  <div class="calendar-weekdays">
    <div class="weekday">日</div>
    <!-- ... 其他週標題 -->
  </div>
  
  <!-- 日期格線 -->
  <div class="calendar-grid">
    <!-- 42個日期格子 -->
    <button class="calendar-cell" data-date="2025-08-01">1</button>
    <!-- ... 其他日期格子 -->
  </div>
</div>
```

### CSS 類別命名規範
#### 主要容器
- `.calendar-container`：最外層容器
- `.calendar-header`：標題與導航控制區
- `.calendar-alerts`：狀態訊息顯示區
- `.calendar-weekdays`：週標題列容器
- `.calendar-grid`：日期格線容器

#### 互動元件
- `.calendar-cell`：單個日期格子
- `.calendar-cell--today`：今日特殊樣式
- `.calendar-cell--selected`：選取狀態樣式
- `.calendar-cell--muted`：非當月日期淡化樣式
- `.calendar-cell--disabled`：不可點擊狀態

#### 控制元件
- `.btn-prev-month`、`.btn-next-month`：月份導航按鈕
- `.year-selector`、`.month-selector`：年月選擇下拉選單

### JavaScript 增強功能（可選）
#### 基礎功能
- 表單自動提交：年月選擇器變更時自動提交表單
- 鍵盤導航：方向鍵在日期格間移動
- 快速回到今日：雙擊標題或快速鍵

#### 進階功能（未來擴充）
- 日期範圍選擇
- 事件標記功能  
- 匯入/匯出日曆功能

## 後端 PageModel 架構規格

### 類別定義（`Pages/index3.cshtml.cs`）
```csharp
public sealed class Index3Model : PageModel
{
    private readonly ILogger<Index3Model> logger;
    private readonly INoteService noteService;
    
    // QueryString 輸入參數
    [BindProperty(SupportsGet = true)] public int? Year { get; set; }
    [BindProperty(SupportsGet = true)] public int? Month { get; set; }
    [BindProperty(SupportsGet = true)] public int? Day { get; set; }
    
    // 🆕 備註相關 POST 參數
    [BindProperty] public string? NoteText { get; set; }
    
    // 核心顯示屬性
    public int DisplayYear { get; private set; }
    public int DisplayMonth { get; private set; }
    public DateOnly Today { get; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? SelectedDate { get; private set; }
    
    // 🆕 備註系統屬性
    public string? SelectedDateNote { get; private set; }
    public HashSet<DateOnly> NoteDatesInMonth { get; private set; } = new();
    
    // 月曆資料
    public IReadOnlyList<CalendarCellView> CalendarCells { get; private set; } = Array.Empty<CalendarCellView>();
    
    // 使用者體驗屬性
    public string? CorrectionMessage { get; private set; }
    public bool HasCorrection => !string.IsNullOrEmpty(CorrectionMessage);
    
    // 導航輔助屬性
    public IReadOnlyList<int> YearOptions { get; } = Enumerable.Range(1900, 201).ToArray();
    public IReadOnlyList<int> MonthOptions { get; } = Enumerable.Range(1, 12).ToArray();
    public IReadOnlyList<string> WeekdayNames { get; } = ["日", "一", "二", "三", "四", "五", "六"];
    
    // 導航計算屬性
    public int PrevYear => DisplayMonth == 1 ? DisplayYear - 1 : DisplayYear;
    public int PrevMonth => DisplayMonth == 1 ? 12 : DisplayMonth - 1;
    public int NextYear => DisplayMonth == 12 ? DisplayYear + 1 : DisplayYear;
    public int NextMonth => DisplayMonth == 12 ? 1 : DisplayMonth + 1;
}
```

### 🆕 備註服務架構（`Services/NoteService.cs`）
```csharp
public interface INoteService
{
    Task<string?> GetNoteAsync(DateOnly date);
    Task SaveNoteAsync(DateOnly date, string note);  
    Task DeleteNoteAsync(DateOnly date);
    Task<HashSet<DateOnly>> GetNoteDatesInMonthAsync(int year, int month);
}

public sealed class JsonNoteService : INoteService
{
    private readonly string notesFilePath;
    private readonly ILogger<JsonNoteService> logger;
    private readonly SemaphoreSlim fileLock = new(1, 1);
    
    // 完整的執行緒安全檔案 I/O 實作
    // JSON 序列化與反序列化
    // 例外處理與容錯機制
}
```

### 檢視模型設計
```csharp
public sealed record CalendarCellView(
    DateOnly Date,
    bool InCurrentMonth,
    bool IsToday,
    bool IsSelected,
    bool HasNote,              // 🆕 備註狀態標記
    int DayOfWeekIndex
);
```

### 核心業務邏輯方法
#### 主要處理方法
```csharp
// GET 進入點：頁面載入和月曆生成
public async Task<IActionResult> OnGetAsync()
{
    // 1. 參數驗證與修正
    // 2. 載入備註資料
    // 3. 生成月曆格線
    // 4. 設定選取狀態
}

// 🆕 POST 備註儲存
public async Task<IActionResult> OnPostSaveNoteAsync()
{
    // 1. 日期參數驗證
    // 2. 備註內容處理（空白自動刪除）
    // 3. 呼叫服務儲存
    // 4. 重新導向保持狀態
}

// 🆕 POST 備註刪除
public async Task<IActionResult> OnPostDeleteNoteAsync()
{
    // 1. 日期參數驗證
    // 2. 呼叫服務刪除
    // 3. 重新導向保持狀態
}
```

#### 🆕 高效能月曆生成演算法
```csharp
private async Task<IReadOnlyList<CalendarCellView>> GenerateCalendarGridAsync(
    int year, int month, DateOnly? selectedDate)
{
    var firstOfMonth = new DateOnly(year, month, 1);
    var startOffset = (int)firstOfMonth.DayOfWeek;
    var gridStartDate = firstOfMonth.AddDays(-startOffset);

    // 🆕 跨月備註查詢最佳化
    var gridEndDate = gridStartDate.AddDays(41);
    var allNoteDates = new HashSet<DateOnly>();
    
    // 智慧載入月曆範圍內的所有備註資料
    for (var date = gridStartDate; date <= gridEndDate; date = date.AddMonths(1))
    {
        var monthNoteDates = await noteService.GetNoteDatesInMonthAsync(date.Year, date.Month);
        foreach (var noteDate in monthNoteDates)
        {
            if (noteDate >= gridStartDate && noteDate <= gridEndDate)
            {
                allNoteDates.Add(noteDate);
            }
        }
        
        // 移動到下個月邏輯
        var nextMonth = date.Month == 12 ? 1 : date.Month + 1;
        var nextYear = date.Month == 12 ? date.Year + 1 : date.Year;
        date = new DateOnly(nextYear, nextMonth, 1).AddDays(-1);
    }

    // 生成 42 格月曆，包含備註狀態
    var cells = new List<CalendarCellView>(42);
    for (var i = 0; i < 42; i++)
    {
        var currentDate = gridStartDate.AddDays(i);
        var hasNote = allNoteDates.Contains(currentDate);  // O(1) 查詢
        
        cells.Add(new CalendarCellView(
            Date: currentDate,
            InCurrentMonth: isInCurrentMonth,
            IsToday: isToday,
            IsSelected: isSelected,
            HasNote: hasNote,           // 🆕 備註狀態
            DayOfWeekIndex: dayOfWeekIndex
        ));
    }
    
    return cells;
}
```

### 🆕 備註系統錯誤處理
#### POST 操作例外處理
```csharp
try
{
    var date = new DateOnly(Year.Value, Month.Value, Day.Value);
    
    if (string.IsNullOrWhiteSpace(NoteText))
    {
        await noteService.DeleteNoteAsync(date);
    }
    else
    {
        await noteService.SaveNoteAsync(date, NoteText);
    }

    // 成功：重新導向保持選取狀態
    return RedirectToPage("/index3", new { Year, Month, Day });
}
catch (Exception ex)
{
    // 錯誤：記錄日誌並顯示友善訊息
    logger.LogError(ex, "備註操作失敗");
    ModelState.AddModelError(string.Empty, "操作失敗，請稍後再試。");
    
    // 重新載入頁面資料
    await OnGetAsync();
    return Page();
}
```

### 效能最佳化策略
#### 🆕 備註資料快取
- **月份級快取**：`NoteDatesInMonth` 屬性快取當月所有備註日期
- **HashSet 查詢**：O(1) 時間複雜度的日期查詢
- **跨月最佳化**：只載入月曆顯示範圍內的備註資料
- **記憶體控制**：使用 `IReadOnlyList` 和 `HashSet` 優化記憶體使用

#### 計算最佳化
- **單次檔案讀取**：每次頁面載入只讀取一次備註檔案
- **非同步操作**：所有備註服務操作使用 `async/await`
- **延遲載入**：只在選取日期時載入具體備註內容
- **預計算**：導航相關屬性使用預計算避免重複運算

### 核心業務邏輯
#### 參數驗證與修正
1. **輸入驗證流程**：
   ```
   接收參數 → 範圍檢查 → 邊界處理 → 有效性驗證 → 修正記錄
   ```

2. **修正策略**：
   - Year 超界：裁切至 1900-2100 範圍
   - Month 無效：調整至 1-12 範圍  
   - Day 無效：調整至該月有效日期
   - 修正時記錄訊息供 UI 顯示

#### 月曆計算演算法
1. **基礎計算**：
   - 目標月份第一天是週幾
   - 該月總天數
   - 需要補齊的前月天數
   - 需要補齊的後月天數

2. **格線生成**：
   - 固定產生 42 格（6週 × 7天）
   - 標記每格的月份歸屬、今日狀態、選取狀態
   - 生成對應的 CSS 類別和 ARIA 標籤

#### 錯誤處理機制
- **參數異常**：友善修正而非拋出錯誤
- **日期計算溢出**：邊界保護機制
- **效能監控**：大量日期計算的效能考量

### 多語言與國際化支援
#### 月份名稱國際化
```csharp
private static readonly Dictionary<string, string[]> MonthNames = new()
{
    ["zh-TW"] = ["一月", "二月", "三月", ...],
    ["en-US"] = ["January", "February", "March", ...],
    ["ja-JP"] = ["1月", "2月", "3月", ...]
};
```

#### 週標題國際化  
```csharp
private static readonly Dictionary<string, string[]> WeekdayNames = new()
{
    ["zh-TW"] = ["日", "一", "二", "三", "四", "五", "六"],
    ["en-US"] = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]
};
```

### 效能最佳化策略
#### 快取機制
- **月曆資料快取**：相同年月的計算結果可快取
- **本地化資源快取**：月份、週名稱等靜態資源快取

#### 計算最佳化
- **預計算**：常用年月組合可預計算
- **延遲載入**：視需要才載入詳細資料  
- **記憶體控制**：避免大量物件建立

## 資料驗證與錯誤處理規格

### 輸入驗證規則
#### 年份驗證
- **有效範圍**：1900-2100（200年跨度）
- **邊界處理**：
  - < 1900 → 自動設為 1900
  - > 2100 → 自動設為 2100
- **無效輸入**：非數字或 null → 使用當前年份

#### 月份驗證  
- **有效範圍**：1-12
- **邊界處理**：
  - < 1 → 自動設為 1
  - > 12 → 自動設為 12
- **無效輸入**：非數字或 null → 使用當前月份

#### 日期驗證
- **動態範圍**：1 至該月最後一天
- **邊界處理**：
  - < 1 → 自動設為 1
  - > 月天數 → 自動設為該月最後一天
- **特殊情況**：
  - 閏年二月：2024/2/29 有效，2025/2/29 → 2025/2/28
  - 大小月處理：4月31日 → 4月30日

### 錯誤訊息設計
#### 自動修正通知
```html
<div class="alert alert-info alert-dismissible fade show">
  <i class="bi bi-info-circle"></i>
  已自動修正日期為 <strong>2025年8月28日</strong>
  <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>
```

#### 訊息分類
- **資訊類型（Info）**：參數自動修正
- **警告類型（Warning）**：日期不存在但已修正
- **成功類型（Success）**：操作完成確認

### 例外處理策略
#### 系統層級錯誤
- **日期計算溢出**：使用 DateOnly 的內建保護機制
- **記憶體不足**：降級為基本功能
- **網路中斷**：提供離線緩存資料

#### 使用者層級錯誤
- **JavaScript 關閉**：純 HTML 表單降級方案
- **CSS 載入失敗**：保持基本可讀性的 fallback 樣式
- **字體載入失敗**：系統預設字體 fallback

## 效能與相容性規格

### 效能最佳化目標
#### 載入效能指標
- **首次內容渲染（FCP）**：< 1.2 秒
- **最大內容渲染（LCP）**：< 2.5 秒  
- **互動準備時間（TTI）**：< 3.5 秒
- **累積版面偏移（CLS）**：< 0.1

#### 運行效能指標
- **日曆計算時間**：< 50ms（本地測試環境）
- **頁面切換響應**：< 200ms 視覺回饋
- **記憶體使用**：< 10MB 整體頁面記憶體佔用

### 瀏覽器相容性矩陣
#### 完全支援瀏覽器
| 瀏覽器 | 版本要求 | 特殊注意事項 |
|--------|----------|--------------|
| Chrome | ≥ 90 | 完全支援所有功能 |
| Firefox | ≥ 88 | 完全支援所有功能 |
| Safari | ≥ 14 | 完全支援所有功能 |
| Edge | ≥ 90 | 完全支援所有功能 |

#### 降級支援策略
- **舊版瀏覽器**：提供基本功能，移除進階動畫
- **JavaScript 停用**：純 HTML 表單提交功能
- **CSS 不支援**：保持基本版面結構

### 行動裝置最佳化
#### 觸控互動最佳化
- **最小觸控區域**：44px × 44px（符合 iOS/Android 建議）
- **防誤觸設計**：按鈕間適當間距
- **滑動手勢**：支援左右滑動切換月份（可選功能）

#### 行動網路最佳化
- **資源壓縮**：CSS/JS 檔案壓縮
- **圖片最佳化**：使用 WebP 格式（含 fallback）
- **快取策略**：靜態資源長期快取

### 無障礙相容性
#### 無障礙標準符合
- **WCAG 2.1 AA 級**：完全符合 AA 級無障礙標準
- **螢幕閱讀器測試**：支援 NVDA、JAWS、VoiceOver
- **鍵盤導航測試**：所有功能可純鍵盤操作

#### 輔助技術支援
- **高對比模式**：Windows 高對比模式下正常顯示
- **字體縮放**：支援 200% 字體縮放
- **色盲支援**：不僅依賴色彩區分資訊

## 國際化與在地化規格

### 多語言支援架構
#### 支援語系清單
- **zh-TW**：繁體中文（台灣）- 預設語系
- **en-US**：英文（美國）
- **ja-JP**：日文（日本）
- **zh-CN**：簡體中文（中國大陸）- 可選擴充

#### 在地化資源檔案
```javascript
// wwwroot/js/i18n-zh-TW.js
window.CalendarI18n = {
  monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", 
               "七月", "八月", "九月", "十月", "十一月", "十二月"],
  weekdayNames: ["日", "一", "二", "三", "四", "五", "六"],
  todayLabel: "今日",
  previousMonth: "上個月", 
  nextMonth: "下個月",
  dateFormat: "YYYY年M月D日"
};
```

### 日期格式在地化
#### 顯示格式
- **zh-TW**：2025年8月26日（星期二）
- **en-US**：August 26, 2025 (Tuesday)  
- **ja-JP**：2025年8月26日（火曜日）

#### URL 參數格式
- **統一格式**：YYYY-MM-DD（ISO 8601 標準）
- **避免歧義**：不使用地區特定的日期格式

### 文字方向支援
#### LTR 語言支援
- 預設版面：適用於中文、英文、日文
- 閱讀順序：由左至右、由上至下

#### 未來 RTL 擴充
- 預留 RTL（右至左）語言支援架構
- CSS 邏輯屬性：使用 `margin-inline-start` 而非 `margin-left`

### 時區處理策略  
#### 伺服器端時區
- **統一標準**：使用伺服器時區作為基準
- **今日判定**：以伺服器時間判定「今日」日期
- **一致性保證**：確保所有使用者看到一致的「今日」標示

#### 客戶端時區（未來擴充）
- **JavaScript 偵測**：可選的客戶端時區偵測
- **使用者偏好**：儲存使用者時區偏好設定

## 測試策略與品質保證

### 單元測試規格
#### 日期計算邏輯測試
```csharp
[TestClass]
public class CalendarCalculationTests
{
    [TestMethod]
    public void CalculateCalendarCells_August2025_Returns42Cells()
    {
        // Arrange: 2025年8月測試案例
        
        // Act: 計算月曆格線
        
        // Assert: 驗證42格正確性、第一天位置、今日標示等
    }
    
    [TestMethod]
    [DataRow(2024, 2, 29)] // 閏年測試
    [DataRow(2025, 2, 28)] // 平年測試
    public void ValidateLeapYearHandling(int year, int month, int expectedDays)
    {
        // 測試閏年/平年二月處理邏輯
    }
}
```

#### 參數驗證測試
- **邊界值測試**：年份 1899/1900/2100/2101
- **無效輸入測試**：null、負數、非數字字串
- **日期組合測試**：2月30日、4月31日等無效組合

### 整合測試規格
#### 頁面載入測試
```csharp
[TestMethod]
public async Task Index3Page_WithValidParameters_ReturnsCorrectCalendar()
{
    // 測試有效參數的頁面載入
}

[TestMethod]  
public async Task Index3Page_WithInvalidParameters_ShowsCorrectionMessage()
{
    // 測試參數自動修正功能
}
```

#### 導航功能測試
- **月份切換測試**：上月/下月按鈕功能
- **年月選擇測試**：下拉選單功能
- **URL 同步測試**：參數更新後 URL 正確性

### UI 自動化測試
#### Selenium 測試案例
```csharp
[TestClass]
public class CalendarUITests
{
    [TestMethod]
    public void ClickDateCell_NavigatesToDetailPage()
    {
        // 測試日期點擊導向功能
    }
    
    [TestMethod]
    public void ResponsiveDesign_MobileView_CorrectLayout()
    {
        // 測試響應式設計
    }
}
```

### 無障礙測試規格
#### 自動化無障礙檢測
- **axe-core 整合**：使用 axe-core 進行自動化 A11y 檢測
- **色彩對比檢測**：確保所有文字符合 WCAG AA 標準
- **ARIA 標籤檢測**：驗證所有無障礙標籤正確性

#### 手動無障礙測試
- **鍵盤導航測試**：Tab、方向鍵、Enter/Space 功能
- **螢幕閱讀器測試**：使用 NVDA 進行實際測試
- **高對比模式測試**：Windows 高對比模式相容性

### 效能測試規格
#### 載入效能測試
```javascript
// Lighthouse 自動化測試
const lighthouse = require('lighthouse');
const chromeLauncher = require('chrome-launcher');

async function runLighthouseTest() {
  // 自動化 Lighthouse 效能測試
  // 目標：Performance Score > 90
}
```

#### 壓力測試
- **並發使用者測試**：模擬 100 並發使用者
- **大量日期計算測試**：連續月份切換壓力測試
- **記憶體洩漏測試**：長時間運行記憶體監控

### 跨瀏覽器測試
#### 自動化跨瀏覽器測試
```yaml
# GitHub Actions 設定
browsers:
  - Chrome (latest)
  - Firefox (latest)  
  - Safari (latest)
  - Edge (latest)
  
mobile_devices:
  - iPhone 12
  - Samsung Galaxy S21
  - iPad Pro
```

### 迴歸測試策略
#### 自動化 CI/CD 整合
- **Pull Request 觸發**：每次程式碼提交自動執行測試
- **定期測試**：每日自動執行完整測試套件
- **部署前驗證**：生產部署前強制執行所有測試

## 驗收標準與交付檢查清單

### 功能驗收標準
#### 基本功能驗收 ✅
- [ ] **預設載入**：開啟 `/index3` 顯示當前月份完整月曆
- [ ] **週標題正確**：顯示「日一二三四五六」順序
- [ ] **今日標示**：當前日期有明顯視覺標示（藍色背景 + 白色文字）
- [ ] **格線完整**：顯示 42 個日期格（6週 × 7天）
- [ ] **非當月淡化**：前後月補齊日期以淡色顯示且不可點擊

#### 導航功能驗收 ✅  
- [ ] **上月按鈕**：點擊切換至上個月，URL 參數正確更新
- [ ] **下月按鈕**：點擊切換至下個月，URL 參數正確更新
- [ ] **年份選擇**：下拉選單可選擇 1900-2100 年份
- [ ] **月份選擇**：下拉選單可選擇 1-12 月份
- [ ] **選擇即更新**：年月選擇器變更後立即更新頁面
- [ ] **🆕 跳至今日**：點擊「今日」按鈕直接跳轉至當前日期並選中

#### 互動功能驗收 ✅
- [ ] **日期點擊**：點擊當月日期導向包含完整日期的 URL
- [ ] **非當月無效**：點擊非當月日期無任何動作
- [ ] **視覺回饋**：所有可點擊元素有 hover 效果
- [ ] **選取狀態**：選中的日期有明顯標示

#### 🆕 備註系統功能驗收 ✅
- [ ] **備註新增**：選取日期後可在表單中輸入備註並儲存
- [ ] **備註編輯**：點擊有備註的日期可編輯現有內容
- [ ] **備註刪除**：可使用刪除按鈕移除備註，含確認對話框
- [ ] **備註顯示**：選取有備註的日期在下方完整顯示備註內容
- [ ] **多語言支援**：備註支援中文、英文、日文等 Unicode 字元
- [ ] **換行保持**：備註內容的換行符正確保存和顯示

#### 🆕 視覺標記系統驗收 ✅
- [ ] **金色漸層背景**：有備註的日期顯示金色漸層背景
- [ ] **閃爍徽章**：有備註日期右上角顯示紅色便利貼圖示，2秒脈衝動畫
- [ ] **書籤圖示**：有備註日期內部顯示書籤圖示標記
- [ ] **呼吸燈效果**：有備註日期有3秒週期的金色光暈動畫
- [ ] **"備註"文字標籤**：有備註日期顯示白色"備註"文字標籤
- [ ] **標記優先級**：今日 > 選取 > 有備註 > 一般的視覺層次正確

#### 🆕 圖例與統計驗收 ✅
- [ ] **圖例說明完整**：四格圖例正確說明各種日期狀態
- [ ] **統計資訊準確**：頁面底部正確顯示本月備註數量
- [ ] **鼓勵文案智慧**：根據備註數量顯示對應的鼓勵文字
- [ ] **空狀態指引**：無備註時提供清楚的使用指導

#### 🆕 備註表單驗收 ✅
- [ ] **表單樣式**：左側綠色邊框，大尺寸輸入框
- [ ] **按鈕功能**：儲存、刪除、清空按鈕功能正常
- [ ] **驗證錯誤**：伺服器端驗證錯誤正確顯示
- [ ] **操作回饋**：成功操作後重新導向並保持選取狀態

### 品質驗收標準  
#### 響應式設計驗收 ✅
- [ ] **桌面版**（≥992px）：日期格最小 96×96px，字體清晰
- [ ] **平板版**（768-991px）：版面緊湊但保持可讀性
- [ ] **手機版**（<768px）：日期格最小 64×64px，觸控友善
- [ ] **橫豎螢幕**：支援行動裝置橫豎螢幕切換

#### 無障礙驗收 ✅
- [ ] **鍵盤導航**：Tab 鍵可依序聚焦所有互動元素
- [ ] **方向鍵移動**：在日期格間使用方向鍵移動聚焦
- [ ] **ARIA 標籤**：每個日期格有正確的 `aria-label`
- [ ] **今日標示**：今日格有 `aria-current="date"` 屬性
- [ ] **螢幕閱讀器**：使用 NVDA 能正確讀取所有資訊

#### 效能驗收 ✅
- [ ] **首次載入**：< 2 秒完成頁面渲染
- [ ] **月份切換**：< 500ms 完成切換動作  
- [ ] **記憶體使用**：< 20MB 頁面記憶體佔用
- [ ] **Lighthouse 評分**：Performance > 85, Accessibility > 95
- [ ] **🆕 備註查詢**：< 100ms 完成月份備註資料查詢
- [ ] **🆕 備註儲存**：< 200ms 完成備註儲存操作
- [ ] **🆕 動畫流暢**：CSS 動畫在 60fps 下流暢運行

#### 🆕 資料完整性驗收 ✅
- [ ] **檔案持久化**：備註正確儲存至 `App_Data/notes.json`
- [ ] **UTF-8 編碼**：多語言字元正確儲存和讀取
- [ ] **並發安全**：多個操作同時進行時資料一致性
- [ ] **錯誤復原**：檔案損壞時能顯示友善錯誤並復原基本功能

### 錯誤處理驗收標準
#### 參數修正驗收 ✅
- [ ] **年份超界**：1850 → 1900，2150 → 2100，顯示修正訊息
- [ ] **月份無效**：0 → 1，13 → 12，顯示修正訊息
- [ ] **日期無效**：2/30 → 2/28，4/31 → 4/30，顯示修正訊息
- [ ] **修正通知**：所有修正都顯示 Bootstrap Alert 訊息
- [ ] **訊息可關閉**：修正通知可以點擊 × 關閉

#### 🆕 備註系統錯誤處理驗收 ✅
- [ ] **檔案不存在**：首次執行時自動建立 `App_Data/notes.json`
- [ ] **檔案權限錯誤**：顯示友善錯誤訊息並指導解決方法
- [ ] **JSON 格式錯誤**：自動復原或重建檔案，保護既有資料
- [ ] **儲存失敗**：顯示錯誤訊息，不影響頁面其他功能
- [ ] **空白備註處理**：自動刪除空白或僅空格的備註
- [ ] **大量資料**：備註檔案過大時提供效能警告

#### 容錯處理驗收 ✅
- [ ] **JavaScript 停用**：基本功能仍可運作（表單提交）
- [ ] **CSS 載入失敗**：保持基本可讀的版面結構
- [ ] **網路中斷**：顯示友善的錯誤訊息
- [ ] **🆕 備註服務失敗**：月曆功能不受影響，僅備註功能降級

### 相容性驗收標準
#### 瀏覽器相容性 ✅
- [ ] **Chrome**（≥90）：所有功能完全正常
- [ ] **Firefox**（≥88）：所有功能完全正常  
- [ ] **Safari**（≥14）：所有功能完全正常
- [ ] **Edge**（≥90）：所有功能完全正常

#### 裝置相容性 ✅
- [ ] **iPhone**：觸控操作順暢，版面適當
- [ ] **Android**：觸控操作順暢，版面適當
- [ ] **iPad**：平板模式顯示正常
- [ ] **桌面**：滑鼠操作流暢

### 程式碼品質標準
#### 程式碼審查 ✅
- [ ] **命名規範**：類別、方法、變數名稱符合 C# 慣例
- [ ] **程式碼註解**：複雜邏輯有適當註解說明
- [ ] **單元測試**：核心邏輯有完整單元測試覆蓋
- [ ] **效能考量**：無明顯效能瓶頸或資源洩漏

#### 安全性檢查 ✅
- [ ] **輸入驗證**：所有使用者輸入都經過適當驗證
- [ ] **XSS 防護**：輸出內容適當編碼
- [ ] **CSRF 保護**：表單有適當的防護機制

### 文件交付標準
#### 技術文件 ✅
- [ ] **API 文件**：PageModel 公開屬性和方法有文件
- [ ] **部署指南**：包含部署和設定說明
- [ ] **故障排除**：常見問題和解決方案

#### 使用者文件 ✅  
- [ ] **功能說明**：完整的功能操作說明
- [ ] **無障礙指南**：無障礙功能使用說明
- [ ] **瀏覽器支援**：支援的瀏覽器版本清單

## 實作建議與技術指導

### 開發順序建議
#### 第一階段：核心功能（週數：1-2）
1. **建立基礎架構**
   - 建立 `index3.cshtml` 和 `index3.cshtml.cs` 檔案
   - 設定基本路由和參數接收
   - 實作日期計算核心邏輯

2. **基本日曆顯示**
   - 實作月曆格線計算演算法
   - 建立 `CalendarCellViewModel` 資料結構
   - 完成基本的 HTML 標記和 Bootstrap 樣式

#### 第二階段：互動功能（週數：1）
3. **導航控制實作**
   - 上月/下月按鈕功能
   - 年份/月份選擇器
   - URL 參數同步機制

4. **狀態處理**
   - 今日標示邏輯
   - 參數驗證和錯誤修正
   - 使用者友善的錯誤訊息

#### 第三階段：品質提升（週數：1）
5. **響應式設計**
   - 行動裝置版面最佳化
   - 觸控互動改善
   - 跨瀏覽器相容性測試

6. **無障礙和 i18n**
   - ARIA 標籤和鍵盤導航
   - 多語言資源整合
   - 無障礙功能測試

### 關鍵技術實作要點
#### 日期計算最佳化
```csharp
// 建議使用 .NET 8 的 DateOnly 結構
private static List<CalendarCellViewModel> CalculateCalendarCells(int year, int month)
{
    var firstDay = new DateOnly(year, month, 1);
    var startDate = firstDay.AddDays(-(int)firstDay.DayOfWeek);
    
    return Enumerable.Range(0, 42)
        .Select(i => CreateCalendarCell(startDate.AddDays(i), year, month))
        .ToList();
}
```

#### 效能最佳化策略
- **避免重複計算**：將月曆資料計算結果快取
- **最小化物件建立**：重用 ViewModel 實例
- **延遲載入**：只在需要時載入額外資料

#### 錯誤處理最佳實務
```csharp
private (int year, int month, string? message) ValidateAndCorrectParameters(int? year, int? month)
{
    var correctedYear = Math.Clamp(year ?? DateTime.Now.Year, 1900, 2100);
    var correctedMonth = Math.Clamp(month ?? DateTime.Now.Month, 1, 12);
    
    string? message = null;
    if (year != correctedYear || month != correctedMonth)
    {
        message = $"已自動修正日期為 {correctedYear}年{correctedMonth}月";
    }
    
    return (correctedYear, correctedMonth, message);
}
```

### 常見陷阱與解決方案
#### 日期計算陷阱
- **時區問題**：統一使用 `DateOnly` 避免時區混亂
- **閏年處理**：使用 `DateTime.DaysInMonth()` 確保正確
- **週起始**：確保星期日為 0 的計算基準

#### 效能陷阱
- **過度查詢**：避免在 View 中進行複雜計算
- **記憶體洩漏**：注意大型集合的生命週期管理
- **不必要的重新渲染**：合理使用快取機制

#### UI/UX 陷阱  
- **觸控區域太小**：確保手機上最小 44px 點擊區域
- **色彩對比不足**：定期檢查 WCAG 合規性
- **載入狀態缺失**：提供適當的載入指示

### 測試建議
#### 單元測試重點
- 日期計算邏輯的邊界條件
- 參數驗證和修正功能
- 月曆格線生成的正確性
- **🆕 備註服務 CRUD 操作測試**
- **🆕 JSON 序列化與反序列化測試**
- **🆕 執行緒安全機制測試**
- **🆕 月份備註查詢效能測試**

#### 整合測試重點
- 完整的頁面載入流程
- 導航功能的 URL 同步
- 錯誤處理的使用者體驗
- **🆕 備註表單提交與驗證流程**
- **🆕 備註資料持久化完整性**
- **🆕 跨月備註顯示正確性**

### 部署前檢查清單
#### 程式碼品質檢查
- [ ] 所有單元測試通過
- [ ] 程式碼覆蓋率 > 80%
- [ ] 靜態程式碼分析無重大問題
- [ ] 效能測試達到目標指標

#### 功能完整性檢查
- [ ] 所有驗收標準項目完成
- [ ] 跨瀏覽器測試通過
- [ ] 無障礙測試達標
- [ ] 行動裝置測試正常

## 專案風險管理

### 技術風險
#### 高風險項目
- **瀏覽器相容性問題**：CSS Grid 在舊版瀏覽器的支援
- **效能瓶頸**：大量 DOM 元素可能影響行動裝置效能
- **無障礙合規**：複雜的鍵盤導航實作挑戰
- **🆕 資料遺失風險**：JSON 檔案可能因系統故障損壞或遺失
- **🆕 並發衝突**：多使用者或多分頁同時操作備註檔案
- **🆕 大資料量效能**：備註資料增長時的查詢效能衰減

#### 風險緩解策略
- **漸進增強**：確保基本功能在所有瀏覽器可用
- **效能監控**：持續監控關鍵效能指標
- **專家諮詢**：無障礙設計專家審查
- **🆕 定期備份**：建立備註資料的自動備份機制
- **🆕 檔案鎖機制**：使用 SemaphoreSlim 確保檔案操作安全
- **🆕 資料庫升級路徑**：預留升級至 SQL 資料庫的架構彈性

### 專案風險
#### 時程風險
- **功能範圍擴張**：嚴格控制功能需求範圍
- **整合複雜度**：與既有系統整合的挑戰
- **測試時間不足**：預留充足的測試和修正時間

#### 品質風險
- **使用者體驗不佳**：定期進行使用者測試
- **效能問題**：在開發早期就進行效能測試
- **安全漏洞**：定期進行安全審查

---

## 總結

本規格書提供了 index3 日曆頁面開發的完整指導，涵蓋從需求分析到部署交付的各個層面，包含基礎月曆功能與**進階個人化備註系統**。遵循此規格書可確保交付高品質、使用者友善、無障礙且效能優良的日曆應用程式。

**關鍵成功因素：**
1. 嚴格遵循響應式設計和無障礙標準
2. 重視使用者體驗和互動設計細節  
3. 實作完整的錯誤處理和容錯機制
4. 進行充分的測試和品質保證
5. 保持程式碼的可維護性和擴展性
6. **🆕 確保備註系統的資料完整性與執行緒安全**
7. **🆕 實作直觀的視覺標記系統提升用戶識別效率**
8. **🆕 建立完整的 CSS 動畫系統增加視覺吸引力**
9. **🆕 設計優雅的降級機制確保核心功能穩定性**

**🆕 備註功能特殊考量：**
- **資料持久化策略**：JSON 檔案適合個人使用，未來可平滑遷移至資料庫
- **效能優化重點**：HashSet 查詢演算法確保大量備註時的查詢效率
- **視覺設計平衡**：備註標記增強識別度但不干擾基礎日曆功能
- **使用者引導完整**：圖例說明、統計資訊、鼓勵文案形成完整用戶體驗

透過階段性開發和持續品質檢核，可確保專案按時交付且符合所有技術和業務需求，為使用者提供功能豐富且視覺優美的個人化日曆系統。
