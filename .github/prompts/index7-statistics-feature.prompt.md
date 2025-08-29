# 統計分析功能開發提示

## 功能概述
在 `index7.cshtml` 記帳系統月曆檢視頁面中，有一個「統計分析」按鈕需要實現完整功能。目前僅顯示 "統計分析功能開發中，敬請期待！" 的訊息。

## 按鈕位置
- 檔案：`Demo/Pages/index7.cshtml`
- 位置：統計摘要卡片區域的第四個卡片
- 現有程式碼：
```html
<button type="button" class="btn btn-outline-primary btn-sm w-100 mt-1" onclick="showStatisticsModal()">
    <i class="fas fa-chart-pie"></i> 統計分析
</button>
```

## 建議功能開發內容

### 1. 統計分析對話框
建立一個功能豐富的統計分析 Modal，包含以下頁籤：

#### 📊 收支統計頁籤
- **月度收支趨勢圖**：顯示最近 6-12 個月的收支變化
- **年度收支對比**：當年度 vs 去年同期的收支比較
- **週期性分析**：顯示每週、每月的平均收支狀況

#### 📈 分類分析頁籤  
- **支出分類圓餅圖**：各類別支出佔比
- **收入分類圓餅圖**：各類別收入佔比
- **分類排行榜**：支出/收入前 10 大分類
- **分類趨勢分析**：各分類的月度變化趨勢

#### 📅 時間分析頁籤
- **每日消費模式**：一週七天的平均消費分佈
- **月內消費模式**：月初、月中、月底的消費習慣
- **季節性分析**：春夏秋冬各季的消費特徵

#### 🎯 目標分析頁籤
- **預算執行率**：各分類的預算使用情況（需先實現預算功能）
- **儲蓄目標進度**：每月儲蓄率和目標達成情況
- **異常支出警告**：超出平均值的異常支出提醒

### 2. 技術實現建議

#### 前端技術
- **圖表函式庫**：使用 Chart.js 或 ApexCharts 繪製互動式圖表
- **UI 框架**：繼續使用 Bootstrap 5 保持一致性
- **響應式設計**：支援手機和平板的統計檢視

#### 後端實現
- **統計服務類別**：建立 `StatisticsService.cs`
- **資料分析方法**：
  - `GetIncomeExpenseTrend(int months)` - 收支趨勢
  - `GetCategoryAnalysis(DateTime start, DateTime end)` - 分類分析
  - `GetTimePatternAnalysis()` - 時間模式分析
  - `GetBudgetAnalysis()` - 預算分析

#### 資料結構設計
```csharp
public class StatisticsViewModel
{
    public List<MonthlyTrendData> IncomeExpenseTrend { get; set; }
    public List<CategoryAnalysisData> CategoryBreakdown { get; set; }
    public TimePatternData TimePatterns { get; set; }
    public BudgetAnalysisData BudgetStatus { get; set; }
}

public class MonthlyTrendData
{
    public string Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal NetIncome { get; set; }
}

public class CategoryAnalysisData
{
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int RecordCount { get; set; }
    public string Color { get; set; }
}
```

### 3. 使用者體驗設計

#### 互動功能
- **時間範圍選擇器**：讓使用者自訂分析期間
- **篩選功能**：可按分類、金額範圍篩選資料
- **匯出功能**：統計報表可匯出為 PDF 或 Excel
- **深度分析**：點擊圖表元素可查看詳細資料

#### 視覺化效果
- **Loading 動畫**：統計計算時顯示載入效果
- **動態圖表**：使用動畫展示資料變化
- **配色方案**：使用一致的品牌配色

### 4. 開發優先順序

#### Phase 1 - 基礎統計 (MVP)
1. 建立統計分析 Modal 基本結構
2. 實現月度收支趨勢圖
3. 實現支出分類圓餅圖
4. 基本的時間範圍選擇功能

#### Phase 2 - 進階分析
1. 新增收入分類分析
2. 實現時間模式分析
3. 新增分類排行榜功能
4. 新增資料匯出功能

#### Phase 3 - 智能分析
1. 實現異常支出偵測
2. 新增預算追蹤功能
3. 實現消費習慣分析
4. 新增個人化建議功能

### 5. 相關檔案修改

#### 需要修改的檔案
- `Pages/index7.cshtml` - 新增統計 Modal HTML
- `Pages/index7.cshtml.cs` - 新增統計資料處理方法
- `Services/StatisticsService.cs` - 建立統計分析服務（新檔案）
- `Models/StatisticsModels.cs` - 統計相關的資料模型（新檔案）

#### JavaScript 函式更新
```javascript
// 更新現有的 showStatisticsModal() 函式
function showStatisticsModal() {
    // 載入統計資料
    loadStatisticsData();
    // 顯示 Modal
    var modal = new bootstrap.Modal(document.getElementById('statisticsModal'));
    modal.show();
}
```

### 6. 測試計劃
- **單元測試**：測試統計計算邏輯
- **整合測試**：測試統計服務與資料庫的整合
- **UI 測試**：測試不同裝置上的統計檢視
- **效能測試**：測試大量資料的統計計算效能

## 預期效益
1. **提升使用者體驗**：提供深入的財務分析洞察
2. **增加產品價值**：從單純記帳工具升級為財務管理助手
3. **促進使用者留存**：豐富的分析功能增加黏著度
4. **支援決策制定**：幫助使用者做出更好的財務決策

## 開發注意事項
- 確保統計計算的準確性和效能
- 注意行動裝置上的圖表顯示效果
- 考慮大量資料的分頁和效能最佳化
- 預留擴充性，方便未來新增更多統計功能
