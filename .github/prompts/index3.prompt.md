# index3 - 日曆頁面開發規格書

## 專案概述
本規格書定義 ASP.NET Core Razor Pages 應用程式中「index3」日曆頁面的完整開發標準，包含月曆視圖功能、UI/UX 設計、路由規格、參數驗證、狀態處理、響應式設計、無障礙設計、國際化支援、效能最佳化、測試策略與驗收標準。

## 檔案架構
```
Pages/
├── index3.cshtml          # 前端視圖（Razor 頁面標記、Bootstrap 樣式、互動邏輯）
├── index3.cshtml.cs       # 後端 PageModel（參數處理、業務邏輯、資料模型）
└── Shared/
    └── _Calendar.cshtml   # 可選：日曆元件部分檢視
```

## 核心目標
### 主要功能
1. **月曆視圖**：呈現完整的月份日曆格線，支援任意年月瀏覽
2. **導航控制**：提供直覺的年月切換機制（按鈕 + 下拉選單）
3. **日期選擇**：支援點選特定日期並導向詳細資訊頁面
4. **今日強調**：視覺上突出顯示當前日期
5. **響應式設計**：適配桌面、平板、手機等不同螢幕尺寸
6. **無障礙體驗**：支援鍵盤操作與螢幕閱讀器

### 技術特色
- 純 ASP.NET Core 8.0 實作，無需額外 JavaScript 框架
- Bootstrap 5 響應式設計系統
- 支援多國語言與時區
- SEO 友善的 URL 結構
- 高效能日期計算演算法

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

### 導航控制規格
#### 月份切換
- **上月/下月按鈕**：
  - 位置：標題區左右兩側
  - 樣式：Bootstrap `btn-outline-primary`
  - 圖示：使用 Bootstrap Icons 或 Font Awesome
  - 行為：GET 請求更新 URL 參數

#### 快速跳轉
- **年份選擇器**：
  - 類型：HTML `<select>` 下拉選單
  - 範圍：1900-2100（200年範圍）
  - 預設：當前瀏覽年份
- **月份選擇器**：
  - 類型：HTML `<select>` 下拉選單  
  - 選項：1-12月（支援多語言月份名稱）
  - 預設：當前瀏覽月份

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
#### 色彩系統
- **主色調**：繼承專案 Bootstrap 主題的 Primary 色彩
- **今日標示**：`bg-primary text-white` + `shadow-sm` 陰影效果
- **選取狀態**：`border-primary border-2` 外框高亮
- **非當月日期**：`text-muted opacity-50` 淡化顯示
- **互動回饋**：`hover:bg-light active:bg-primary` 階層回饋

#### 字型系統
- **主標題**：`h2` 標籤，`fw-bold text-primary`
- **週標題**：`small` 標籤，`text-uppercase fw-semibold text-secondary`
- **日期數字**：`fw-normal` 正常粗細，清晰易讀

### 響應式斷點設計
#### 桌面版設計（≥992px）
- **格線尺寸**：每格最小 96px × 96px
- **字體大小**：日期數字 16px，週標題 12px
- **間距設定**：格子間距 2px，整體 padding 24px
- **互動區域**：完整格子區域可點擊

#### 平板版設計（768px-991px）
- **格線尺寸**：每格最小 80px × 80px  
- **字體大小**：日期數字 14px，週標題 11px
- **間距設定**：格子間距 1px，整體 padding 16px
- **導航控制**：按鈕稍小但保持易點擊

#### 手機版設計（<768px）
- **格線尺寸**：每格最小 64px × 64px
- **字體大小**：日期數字 12px，週標題 10px
- **間距設定**：最小間距，整體 padding 12px
- **觸控優化**：增加按鈕點擊區域，考慮拇指操作

### 動畫與過場效果
#### 頁面切換動畫
- **月份切換**：淡入淡出效果（fade in/out 0.3s）
- **載入狀態**：骨架屏或載入指示器
- **錯誤狀態**：輕微震動效果提示修正

#### 互動微動畫
- **按鈕 hover**：輕微放大效果（scale 1.05）
- **日期 hover**：背景色平滑過渡（transition 0.2s）
- **選取回饋**：輕微彈性效果（spring animation）

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
public class Index3Model : PageModel
{
    // 輸入參數
    [FromQuery] public int? Year { get; set; }
    [FromQuery] public int? Month { get; set; }
    [FromQuery] public int? Day { get; set; }
    
    // 輸出屬性
    public int DisplayYear { get; set; }
    public int DisplayMonth { get; set; }
    public DateOnly Today { get; set; }
    public DateOnly? SelectedDate { get; set; }
    public IEnumerable<CalendarCellViewModel> CalendarCells { get; set; }
    public string? CorrectionMessage { get; set; }
    
    // 輔助屬性
    public string DisplayMonthName => GetMonthName(DisplayMonth);
    public string PreviousMonthUrl => BuildMonthUrl(DisplayYear, DisplayMonth, -1);
    public string NextMonthUrl => BuildMonthUrl(DisplayYear, DisplayMonth, 1);
}
```

### 檢視模型設計
```csharp
public class CalendarCellViewModel
{
    public DateOnly Date { get; set; }
    public int DayNumber => Date.Day;
    public bool InCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool IsSelected { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string CssClasses => BuildCssClasses();
    public string AriaLabel => BuildAriaLabel();
    public string? DetailUrl => InCurrentMonth ? BuildDetailUrl() : null;
}
```

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

#### 互動功能驗收 ✅
- [ ] **日期點擊**：點擊當月日期導向包含完整日期的 URL
- [ ] **非當月無效**：點擊非當月日期無任何動作
- [ ] **視覺回饋**：所有可點擊元素有 hover 效果
- [ ] **選取狀態**：選中的日期有明顯標示

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

### 錯誤處理驗收標準
#### 參數修正驗收 ✅
- [ ] **年份超界**：1850 → 1900，2150 → 2100，顯示修正訊息
- [ ] **月份無效**：0 → 1，13 → 12，顯示修正訊息
- [ ] **日期無效**：2/30 → 2/28，4/31 → 4/30，顯示修正訊息
- [ ] **修正通知**：所有修正都顯示 Bootstrap Alert 訊息
- [ ] **訊息可關閉**：修正通知可以點擊 × 關閉

#### 容錯處理驗收 ✅
- [ ] **JavaScript 停用**：基本功能仍可運作（表單提交）
- [ ] **CSS 載入失敗**：保持基本可讀的版面結構
- [ ] **網路中斷**：顯示友善的錯誤訊息

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

#### 整合測試重點
- 完整的頁面載入流程
- 導航功能的 URL 同步
- 錯誤處理的使用者體驗

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

#### 風險緩解策略
- **漸進增強**：確保基本功能在所有瀏覽器可用
- **效能監控**：持續監控關鍵效能指標
- **專家諮詢**：無障礙設計專家審查

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

本規格書提供了 index3 日曆頁面開發的完整指導，涵蓋從需求分析到部署交付的各個層面。遵循此規格書可確保交付高品質、使用者友善、無障礙且效能優良的日曆功能。

**關鍵成功因素：**
1. 嚴格遵循響應式設計和無障礙標準
2. 重視使用者體驗和互動設計細節  
3. 實作完整的錯誤處理和容錯機制
4. 進行充分的測試和品質保證
5. 保持程式碼的可維護性和擴展性

透過階段性開發和持續品質檢核，可確保專案按時交付且符合所有技術和業務需求。
