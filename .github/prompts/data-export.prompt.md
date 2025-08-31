# 數據匯出開發規格書

## 📋 專案概述
開發全方位的數據匯出系統，提供實用的資料導出工具。支援多種格式匯出（PDF、Excel、CSV、JSON），包含記帳資料、統計報表、習慣追蹤、備忘錄等所有模組的資料匯出功能，滿足用戶的備份、分析和分享需求。

## 🎯 開發目標
- 建立統一的資料匯出框架
- 支援多種匯出格式和樣板
- 提供自訂匯出範圍和篩選條件
- 實現批次匯出和排程匯出功能
- 建立匯出歷史記錄和管理系統

## 🔧 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, jQuery, Chart.js, HTML5, CSS3
- **匯出函式庫**: 
  - iTextSharp/iText 7 (PDF)
  - ClosedXML (Excel)
  - CsvHelper (CSV)
  - Newtonsoft.Json (JSON)
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 📂 檔案結構規劃

### 新增檔案
```
Pages/
├── export.cshtml                # 數據匯出主頁面
├── export.cshtml.cs            # 數據匯出後端邏輯

Services/
├── ExportService.cs            # 統一匯出服務
├── PdfExportService.cs         # PDF 匯出服務
├── ExcelExportService.cs       # Excel 匯出服務  
├── CsvExportService.cs         # CSV 匯出服務
├── ReportTemplateService.cs    # 報表樣板服務

Models/
├── ExportModels.cs             # 匯出相關資料模型

App_Data/
├── export-templates/           # 匯出樣板目錄
│   ├── pdf-templates/          # PDF 樣板
│   ├── excel-templates/        # Excel 樣板
│   └── report-configs.json     # 報表配置
├── export-history.json         # 匯出歷史記錄

wwwroot/
├── exports/                    # 匯出檔案暫存目錄
├── js/
│   └── export.js              # 匯出功能前端邏輯
├── css/
│   └── export.css             # 匯出頁面樣式

Utilities/
├── PdfExportUtility.cs        # PDF 匯出工具 (已存在，需擴充)
```

## 🎨 核心功能模組

### 1. 數據匯出主頁面
- **前端**: `export.cshtml`
- **後端**: `export.cshtml.cs`
- **路由**: `/export`

#### 1.1 頁面佈局設計

##### A. 匯出功能選擇區
```html
<div class="export-dashboard">
    <div class="row g-4 mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h4><i class="fas fa-download text-primary"></i> 數據匯出中心</h4>
                    <p class="text-muted mb-0">選擇要匯出的資料類型和格式</p>
                </div>
                <div class="card-body">
                    <!-- 資料類型選擇 -->
                    <div class="data-type-selection mb-4">
                        <h5 class="mb-3">選擇匯出資料類型</h5>
                        <div class="row g-3">
                            <div class="col-md-3">
                                <div class="export-option" data-type="accounting">
                                    <div class="export-icon">
                                        <i class="fas fa-calculator"></i>
                                    </div>
                                    <h6>記帳資料</h6>
                                    <p class="text-muted small">收支記錄、分類統計</p>
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="exportAccounting" value="accounting">
                                        <label class="form-check-label" for="exportAccounting">選擇</label>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="export-option" data-type="habits">
                                    <div class="export-icon">
                                        <i class="fas fa-chart-line"></i>
                                    </div>
                                    <h6>習慣追蹤</h6>
                                    <p class="text-muted small">習慣記錄、進度統計</p>
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="exportHabits" value="habits">
                                        <label class="form-check-label" for="exportHabits">選擇</label>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="export-option" data-type="notes">
                                    <div class="export-icon">
                                        <i class="fas fa-sticky-note"></i>
                                    </div>
                                    <h6>備忘錄</h6>
                                    <p class="text-muted small">筆記內容、標籤分類</p>
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="exportNotes" value="notes">
                                        <label class="form-check-label" for="exportNotes">選擇</label>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="export-option" data-type="calendar">
                                    <div class="export-icon">
                                        <i class="fas fa-calendar"></i>
                                    </div>
                                    <h6>日曆事件</h6>
                                    <p class="text-muted small">日程安排、提醒事項</p>
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="exportCalendar" value="calendar">
                                        <label class="form-check-label" for="exportCalendar">選擇</label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- 匯出格式選擇 -->
                    <div class="export-format-selection mb-4">
                        <h5 class="mb-3">選擇匯出格式</h5>
                        <div class="btn-group" role="group" aria-label="匯出格式選擇">
                            <input type="radio" class="btn-check" name="exportFormat" id="formatPdf" value="pdf">
                            <label class="btn btn-outline-primary" for="formatPdf">
                                <i class="fas fa-file-pdf"></i> PDF 報表
                            </label>
                            
                            <input type="radio" class="btn-check" name="exportFormat" id="formatExcel" value="excel">
                            <label class="btn btn-outline-success" for="formatExcel">
                                <i class="fas fa-file-excel"></i> Excel 試算表
                            </label>
                            
                            <input type="radio" class="btn-check" name="exportFormat" id="formatCsv" value="csv">
                            <label class="btn btn-outline-info" for="formatCsv">
                                <i class="fas fa-file-csv"></i> CSV 檔案
                            </label>
                            
                            <input type="radio" class="btn-check" name="exportFormat" id="formatJson" value="json">
                            <label class="btn btn-outline-warning" for="formatJson">
                                <i class="fas fa-file-code"></i> JSON 資料
                            </label>
                        </div>
                    </div>
                    
                    <!-- 匯出範圍設定 -->
                    <div class="export-range-settings">
                        <h5 class="mb-3">匯出範圍設定</h5>
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label for="startDate" class="form-label">開始日期</label>
                                <input type="date" class="form-control" id="startDate" name="startDate">
                            </div>
                            <div class="col-md-6">
                                <label for="endDate" class="form-label">結束日期</label>
                                <input type="date" class="form-control" id="endDate" name="endDate">
                            </div>
                        </div>
                        
                        <!-- 快速日期選擇 -->
                        <div class="quick-date-buttons mt-3">
                            <label class="form-label">快速選擇：</label>
                            <div class="btn-group btn-group-sm" role="group">
                                <button type="button" class="btn btn-outline-secondary" data-range="7">最近7天</button>
                                <button type="button" class="btn btn-outline-secondary" data-range="30">最近30天</button>
                                <button type="button" class="btn btn-outline-secondary" data-range="90">最近3個月</button>
                                <button type="button" class="btn btn-outline-secondary" data-range="365">最近1年</button>
                                <button type="button" class="btn btn-outline-secondary" data-range="all">全部資料</button>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="card-footer">
                    <div class="d-flex justify-content-between align-items-center">
                        <div class="export-preview">
                            <small class="text-muted" id="exportPreview">
                                請選擇要匯出的資料類型和格式
                            </small>
                        </div>
                        <div class="export-actions">
                            <button type="button" class="btn btn-primary" id="startExport" disabled>
                                <i class="fas fa-download"></i> 開始匯出
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

##### B. 匯出歷史記錄
```html
<div class="export-history-section">
    <div class="card">
        <div class="card-header">
            <h5><i class="fas fa-history text-secondary"></i> 匯出歷史記錄</h5>
        </div>
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-hover">
                    <thead>
                        <tr>
                            <th>匯出時間</th>
                            <th>資料類型</th>
                            <th>格式</th>
                            <th>範圍</th>
                            <th>檔案大小</th>
                            <th>狀態</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody id="exportHistoryTable">
                        <!-- 動態載入匯出歷史 -->
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>
```

##### C. 匯出進度模態框
```html
<div class="modal fade" id="exportProgressModal" data-bs-backdrop="static" data-bs-keyboard="false">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">
                    <i class="fas fa-download text-primary"></i> 匯出處理中
                </h5>
            </div>
            <div class="modal-body">
                <div class="export-progress-info mb-3">
                    <div class="d-flex justify-content-between mb-2">
                        <span>匯出進度：</span>
                        <span id="progressPercent">0%</span>
                    </div>
                    <div class="progress mb-3">
                        <div class="progress-bar progress-bar-striped progress-bar-animated" 
                             role="progressbar" id="exportProgressBar" style="width: 0%">
                        </div>
                    </div>
                    <div class="current-step text-muted" id="currentStep">
                        準備匯出...
                    </div>
                </div>
                
                <div class="export-details">
                    <small class="text-muted">
                        <div>資料類型：<span id="exportDataType"></span></div>
                        <div>匯出格式：<span id="exportFormat"></span></div>
                        <div>匯出範圍：<span id="exportRange"></span></div>
                    </small>
                </div>
            </div>
            <div class="modal-footer" style="display: none;" id="exportComplete">
                <button type="button" class="btn btn-success" id="downloadExportFile">
                    <i class="fas fa-download"></i> 下載檔案
                </button>
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
            </div>
        </div>
    </div>
</div>
```

### 2. 資料模型定義 (ExportModels.cs)

```csharp
namespace Demo.Models
{
    public class ExportRequest
    {
        public List<string> DataTypes { get; set; } = new List<string>();
        public string Format { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TemplateName { get; set; } = "default";
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ExportResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public ExportStatus Status { get; set; } = ExportStatus.Processing;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public ExportMetadata Metadata { get; set; } = new ExportMetadata();
    }

    public enum ExportStatus
    {
        Pending,     // 等待處理
        Processing,  // 處理中
        Completed,   // 完成
        Failed,      // 失敗
        Expired      // 已過期
    }

    public class ExportMetadata
    {
        public List<string> DataTypes { get; set; } = new List<string>();
        public string Format { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalRecords { get; set; }
        public Dictionary<string, int> RecordCounts { get; set; } = new Dictionary<string, int>();
        public string TemplateName { get; set; } = string.Empty;
    }

    public class ExportHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public ExportRequest Request { get; set; } = new ExportRequest();
        public ExportResult Result { get; set; } = new ExportResult();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // PDF 專用模型
    public class PdfExportData
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string Period { get; set; } = string.Empty;
        public List<PdfSection> Sections { get; set; } = new List<PdfSection>();
        public Dictionary<string, object> Summary { get; set; } = new Dictionary<string, object>();
    }

    public class PdfSection
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
        public List<ChartData> Charts { get; set; } = new List<ChartData>();
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();
    }

    public class ChartData
    {
        public string Type { get; set; } = string.Empty; // pie, bar, line
        public string Title { get; set; } = string.Empty;
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Values { get; set; } = new List<decimal>();
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }

    // Excel 專用模型
    public class ExcelExportData
    {
        public string WorkbookName { get; set; } = string.Empty;
        public List<ExcelWorksheet> Worksheets { get; set; } = new List<ExcelWorksheet>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ExcelWorksheet
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new List<string>();
        public List<List<object>> Rows { get; set; } = new List<List<object>>();
        public Dictionary<string, ExcelColumnFormat> ColumnFormats { get; set; } = new Dictionary<string, ExcelColumnFormat>();
        public List<ExcelChart> Charts { get; set; } = new List<ExcelChart>();
    }

    public class ExcelColumnFormat
    {
        public string DataType { get; set; } = "General";
        public string NumberFormat { get; set; } = string.Empty;
        public int Width { get; set; } = 15;
        public bool Bold { get; set; } = false;
        public string BackgroundColor { get; set; } = string.Empty;
    }

    public class ExcelChart
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DataRange { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }

    // 匯出設定
    public class ExportTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public List<string> SupportedDataTypes { get; set; } = new List<string>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUsed { get; set; }
        public int UsageCount { get; set; } = 0;
    }
}
```

### 3. 統一匯出服務 (ExportService.cs)

```csharp
namespace Demo.Services
{
    public class ExportService
    {
        private readonly AccountingService _accountingService;
        private readonly NoteService _noteService;
        private readonly PdfExportService _pdfService;
        private readonly ExcelExportService _excelService;
        private readonly CsvExportService _csvService;
        private readonly string _exportPath;
        private readonly string _historyPath;

        public ExportService(
            AccountingService accountingService,
            NoteService noteService,
            PdfExportService pdfService,
            ExcelExportService excelService,
            CsvExportService csvService)
        {
            _accountingService = accountingService;
            _noteService = noteService;
            _pdfService = pdfService;
            _excelService = excelService;
            _csvService = csvService;
            
            _exportPath = Path.Combine("wwwroot", "exports");
            _historyPath = Path.Combine("App_Data", "export-history.json");
            
            EnsureDirectoriesExist();
        }

        // 主要匯出方法
        public async Task<ExportResult> ExportDataAsync(ExportRequest request)
        {
            var result = new ExportResult
            {
                Status = ExportStatus.Processing
            };

            try
            {
                // 1. 收集資料
                var exportData = await CollectExportDataAsync(request);
                
                // 2. 根據格式匯出
                switch (request.Format.ToLower())
                {
                    case "pdf":
                        result = await _pdfService.ExportToPdfAsync(exportData, request);
                        break;
                    case "excel":
                        result = await _excelService.ExportToExcelAsync(exportData, request);
                        break;
                    case "csv":
                        result = await _csvService.ExportToCsvAsync(exportData, request);
                        break;
                    case "json":
                        result = await ExportToJsonAsync(exportData, request);
                        break;
                    default:
                        throw new ArgumentException($"不支援的匯出格式: {request.Format}");
                }

                result.Status = ExportStatus.Completed;
                result.CompletedAt = DateTime.Now;
                result.Duration = result.CompletedAt.Value - result.CreatedAt;
                
                // 3. 記錄匯出歷史
                await SaveExportHistoryAsync(request, result);

            }
            catch (Exception ex)
            {
                result.Status = ExportStatus.Failed;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        // 收集匯出資料
        private async Task<Dictionary<string, object>> CollectExportDataAsync(ExportRequest request)
        {
            var data = new Dictionary<string, object>();
            
            foreach (var dataType in request.DataTypes)
            {
                switch (dataType.ToLower())
                {
                    case "accounting":
                        data["accounting"] = await CollectAccountingDataAsync(request.StartDate, request.EndDate);
                        break;
                    case "notes":
                        data["notes"] = await CollectNotesDataAsync(request.StartDate, request.EndDate);
                        break;
                    case "habits":
                        data["habits"] = await CollectHabitsDataAsync(request.StartDate, request.EndDate);
                        break;
                    case "calendar":
                        data["calendar"] = await CollectCalendarDataAsync(request.StartDate, request.EndDate);
                        break;
                }
            }
            
            return data;
        }

        private async Task<object> CollectAccountingDataAsync(DateTime? startDate, DateTime? endDate)
        {
            var records = await _accountingService.GetAccountingRecordsAsync();
            
            if (startDate.HasValue)
                records = records.Where(r => r.Date >= startDate.Value).ToList();
            if (endDate.HasValue)
                records = records.Where(r => r.Date <= endDate.Value).ToList();

            var categories = await _accountingService.GetCategoriesAsync();
            
            return new
            {
                Records = records,
                Categories = categories,
                Summary = new
                {
                    TotalRecords = records.Count,
                    TotalIncome = records.Where(r => r.Type == "income").Sum(r => r.Amount),
                    TotalExpense = records.Where(r => r.Type == "expense").Sum(r => r.Amount),
                    Period = new { StartDate = startDate, EndDate = endDate }
                },
                CategorySummary = records
                    .GroupBy(r => r.CategoryId)
                    .Select(g => new
                    {
                        CategoryId = g.Key,
                        CategoryName = categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "未知分類",
                        Count = g.Count(),
                        TotalAmount = g.Sum(r => r.Amount)
                    })
                    .ToList()
            };
        }

        // 清理過期匯出檔案
        public async Task CleanupExpiredExportsAsync()
        {
            var expiredDate = DateTime.Now.AddDays(-7); // 7天後清理
            var exportDir = new DirectoryInfo(_exportPath);
            
            if (exportDir.Exists)
            {
                var expiredFiles = exportDir.GetFiles()
                    .Where(f => f.CreationTime < expiredDate);
                
                foreach (var file in expiredFiles)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        // 記錄清理錯誤，但不中斷程序
                        Console.WriteLine($"清理過期檔案失敗: {file.Name}, 錯誤: {ex.Message}");
                    }
                }
            }
        }

        // 獲取匯出歷史
        public async Task<List<ExportHistory>> GetExportHistoryAsync(string userId = "", int limit = 50)
        {
            try
            {
                var history = await LoadExportHistoryAsync();
                
                var filtered = history.AsEnumerable();
                
                if (!string.IsNullOrEmpty(userId))
                    filtered = filtered.Where(h => h.UserId == userId);
                
                return filtered
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(limit)
                    .ToList();
            }
            catch
            {
                return new List<ExportHistory>();
            }
        }
    }
}
```

### 4. PDF 匯出服務 (PdfExportService.cs)

```csharp
namespace Demo.Services
{
    public class PdfExportService
    {
        private readonly string _templatesPath;
        
        public PdfExportService()
        {
            _templatesPath = Path.Combine("App_Data", "export-templates", "pdf-templates");
        }

        public async Task<ExportResult> ExportToPdfAsync(
            Dictionary<string, object> data, 
            ExportRequest request)
        {
            var result = new ExportResult();
            var fileName = GenerateFileName("report", "pdf");
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            
            try
            {
                using (var document = new Document(PageSize.A4, 50, 50, 25, 25))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        var writer = PdfWriter.GetInstance(document, stream);
                        document.Open();
                        
                        // 添加標題頁
                        await AddTitlePageAsync(document, request);
                        
                        // 根據資料類型添加內容
                        foreach (var dataType in request.DataTypes)
                        {
                            if (data.ContainsKey(dataType))
                            {
                                await AddDataSectionAsync(document, dataType, data[dataType]);
                            }
                        }
                        
                        // 添加摘要頁
                        await AddSummaryPageAsync(document, data);
                        
                        document.Close();
                    }
                }
                
                var fileInfo = new FileInfo(filePath);
                result.FileName = fileName;
                result.FilePath = filePath;
                result.FileSize = fileInfo.Length;
                result.ContentType = "application/pdf";
                result.Status = ExportStatus.Completed;
                
                return result;
            }
            catch (Exception ex)
            {
                result.Status = ExportStatus.Failed;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        private async Task AddTitlePageAsync(Document document, ExportRequest request)
        {
            // 添加標題
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.DARK_GRAY);
            var title = new Paragraph("個人管理系統 - 資料報表", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);
            
            // 添加生成資訊
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            var info = new List<string>
            {
                $"生成時間：{DateTime.Now:yyyy年MM月dd日 HH:mm:ss}",
                $"資料範圍：{request.StartDate?.ToString("yyyy-MM-dd")} 至 {request.EndDate?.ToString("yyyy-MM-dd")}",
                $"資料類型：{string.Join("、", request.DataTypes)}",
                $"報表格式：PDF"
            };
            
            foreach (var line in info)
            {
                document.Add(new Paragraph(line, infoFont) { SpacingAfter = 10 });
            }
            
            document.NewPage();
        }

        private async Task AddDataSectionAsync(Document document, string dataType, object data)
        {
            switch (dataType.ToLower())
            {
                case "accounting":
                    await AddAccountingSectionAsync(document, data);
                    break;
                case "notes":
                    await AddNotesSectionAsync(document, data);
                    break;
                case "habits":
                    await AddHabitsSectionAsync(document, data);
                    break;
            }
        }

        private async Task AddAccountingSectionAsync(Document document, object data)
        {
            // 解析記帳資料
            var accountingData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
            
            // 添加標題
            var sectionTitle = new Paragraph("記帳資料分析", 
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16))
            {
                SpacingAfter = 15
            };
            document.Add(sectionTitle);
            
            // 添加摘要統計
            var summary = accountingData.Summary;
            var summaryTable = new PdfPTable(2) { WidthPercentage = 100 };
            summaryTable.AddCell("統計項目");
            summaryTable.AddCell("金額");
            summaryTable.AddCell($"總收入：${summary.TotalIncome}");
            summaryTable.AddCell($"總支出：${summary.TotalExpense}");
            summaryTable.AddCell($"淨收入：${summary.TotalIncome - summary.TotalExpense}");
            summaryTable.AddCell($"記錄筆數：{summary.TotalRecords}");
            document.Add(summaryTable);
            
            // 添加詳細記錄
            document.Add(new Paragraph("詳細記錄", 
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                SpacingBefore = 20,
                SpacingAfter = 10
            });
            
            var recordsTable = new PdfPTable(5) { WidthPercentage = 100 };
            recordsTable.AddCell("日期");
            recordsTable.AddCell("描述");
            recordsTable.AddCell("分類");
            recordsTable.AddCell("類型");
            recordsTable.AddCell("金額");
            
            foreach (var record in accountingData.Records)
            {
                recordsTable.AddCell(record.Date.ToString("yyyy-MM-dd"));
                recordsTable.AddCell(record.Description?.ToString() ?? "");
                recordsTable.AddCell(record.CategoryName?.ToString() ?? "");
                recordsTable.AddCell(record.Type?.ToString() == "income" ? "收入" : "支出");
                recordsTable.AddCell($"${record.Amount}");
            }
            
            document.Add(recordsTable);
            document.NewPage();
        }
    }
}
```

### 5. Excel 匯出服務 (ExcelExportService.cs)

```csharp
namespace Demo.Services
{
    public class ExcelExportService
    {
        public async Task<ExportResult> ExportToExcelAsync(
            Dictionary<string, object> data, 
            ExportRequest request)
        {
            var result = new ExportResult();
            var fileName = GenerateFileName("export", "xlsx");
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    // 為每種資料類型建立工作表
                    foreach (var dataType in request.DataTypes)
                    {
                        if (data.ContainsKey(dataType))
                        {
                            await CreateWorksheetAsync(workbook, dataType, data[dataType]);
                        }
                    }
                    
                    // 添加摘要工作表
                    await CreateSummaryWorksheetAsync(workbook, data, request);
                    
                    workbook.SaveAs(filePath);
                }
                
                var fileInfo = new FileInfo(filePath);
                result.FileName = fileName;
                result.FilePath = filePath;
                result.FileSize = fileInfo.Length;
                result.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                result.Status = ExportStatus.Completed;
                
                return result;
            }
            catch (Exception ex)
            {
                result.Status = ExportStatus.Failed;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        private async Task CreateWorksheetAsync(XLWorkbook workbook, string dataType, object data)
        {
            switch (dataType.ToLower())
            {
                case "accounting":
                    await CreateAccountingWorksheetAsync(workbook, data);
                    break;
                case "notes":
                    await CreateNotesWorksheetAsync(workbook, data);
                    break;
                case "habits":
                    await CreateHabitsWorksheetAsync(workbook, data);
                    break;
            }
        }

        private async Task CreateAccountingWorksheetAsync(XLWorkbook workbook, object data)
        {
            var accountingData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
            var worksheet = workbook.Worksheets.Add("記帳記錄");
            
            // 設定標題
            worksheet.Cell(1, 1).Value = "日期";
            worksheet.Cell(1, 2).Value = "描述";
            worksheet.Cell(1, 3).Value = "分類";
            worksheet.Cell(1, 4).Value = "類型";
            worksheet.Cell(1, 5).Value = "金額";
            worksheet.Cell(1, 6).Value = "備註";
            
            // 設定標題樣式
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            
            // 填入資料
            int row = 2;
            foreach (var record in accountingData.Records)
            {
                worksheet.Cell(row, 1).Value = DateTime.Parse(record.Date.ToString()).ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = record.Description?.ToString() ?? "";
                worksheet.Cell(row, 3).Value = record.CategoryName?.ToString() ?? "";
                worksheet.Cell(row, 4).Value = record.Type?.ToString() == "income" ? "收入" : "支出";
                worksheet.Cell(row, 5).Value = (decimal)record.Amount;
                worksheet.Cell(row, 6).Value = record.Note?.ToString() ?? "";
                row++;
            }
            
            // 自動調整列寬
            worksheet.Columns().AdjustToContents();
            
            // 添加統計
            var summaryStartRow = row + 2;
            worksheet.Cell(summaryStartRow, 4).Value = "總收入：";
            worksheet.Cell(summaryStartRow, 5).Value = (decimal)accountingData.Summary.TotalIncome;
            worksheet.Cell(summaryStartRow + 1, 4).Value = "總支出：";
            worksheet.Cell(summaryStartRow + 1, 5).Value = (decimal)accountingData.Summary.TotalExpense;
            worksheet.Cell(summaryStartRow + 2, 4).Value = "淨收入：";
            worksheet.Cell(summaryStartRow + 2, 5).Value = 
                (decimal)accountingData.Summary.TotalIncome - (decimal)accountingData.Summary.TotalExpense;
                
            // 統計區域樣式
            var summaryRange = worksheet.Range(summaryStartRow, 4, summaryStartRow + 2, 5);
            summaryRange.Style.Font.Bold = true;
            summaryRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
        }
    }
}
```

### 6. 前端 JavaScript 功能 (export.js)

```javascript
class ExportManager {
    constructor() {
        this.selectedDataTypes = [];
        this.selectedFormat = '';
        this.exportInProgress = false;
        this.initializeEventListeners();
        this.loadExportHistory();
    }

    initializeEventListeners() {
        // 資料類型選擇
        $('.export-option input[type="checkbox"]').on('change', this.updateSelection.bind(this));
        
        // 格式選擇
        $('input[name="exportFormat"]').on('change', this.updateSelection.bind(this));
        
        // 快速日期選擇
        $('.quick-date-buttons button').on('click', this.setQuickDateRange.bind(this));
        
        // 日期範圍變更
        $('#startDate, #endDate').on('change', this.updatePreview.bind(this));
        
        // 開始匯出按鈕
        $('#startExport').on('click', this.startExport.bind(this));
        
        // 下載檔案按鈕
        $('#downloadExportFile').on('click', this.downloadExportFile.bind(this));
    }

    updateSelection() {
        // 更新選中的資料類型
        this.selectedDataTypes = $('.export-option input[type="checkbox"]:checked')
            .map((i, el) => $(el).val()).get();
            
        // 更新選中的格式
        this.selectedFormat = $('input[name="exportFormat"]:checked').val() || '';
        
        // 更新預覽和按鈕狀態
        this.updatePreview();
        this.updateExportButton();
    }

    updatePreview() {
        const startDate = $('#startDate').val();
        const endDate = $('#endDate').val();
        const dateRange = this.getDateRangeText(startDate, endDate);
        
        let preview = '';
        
        if (this.selectedDataTypes.length > 0 && this.selectedFormat) {
            const dataTypesText = this.selectedDataTypes.map(type => this.getDataTypeDisplayName(type)).join('、');
            const formatText = this.getFormatDisplayName(this.selectedFormat);
            
            preview = `將匯出 ${dataTypesText} 為 ${formatText} 格式`;
            if (dateRange) {
                preview += ` (${dateRange})`;
            }
        } else {
            preview = '請選擇要匯出的資料類型和格式';
        }
        
        $('#exportPreview').text(preview);
    }

    updateExportButton() {
        const canExport = this.selectedDataTypes.length > 0 && 
                         this.selectedFormat && 
                         !this.exportInProgress;
        $('#startExport').prop('disabled', !canExport);
    }

    setQuickDateRange(event) {
        const days = parseInt($(event.target).data('range'));
        const today = new Date();
        
        if (days === 'all') {
            $('#startDate').val('');
            $('#endDate').val('');
        } else {
            const startDate = new Date();
            startDate.setDate(today.getDate() - days);
            
            $('#startDate').val(startDate.toISOString().split('T')[0]);
            $('#endDate').val(today.toISOString().split('T')[0]);
        }
        
        this.updatePreview();
    }

    async startExport() {
        if (this.exportInProgress) return;
        
        this.exportInProgress = true;
        this.updateExportButton();
        
        const exportRequest = {
            dataTypes: this.selectedDataTypes,
            format: this.selectedFormat,
            startDate: $('#startDate').val() || null,
            endDate: $('#endDate').val() || null,
            templateName: 'default'
        };
        
        // 顯示進度模態框
        this.showProgressModal(exportRequest);
        
        try {
            const response = await fetch('/api/export/start', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(exportRequest)
            });
            
            if (response.ok) {
                const result = await response.json();
                await this.trackExportProgress(result.exportId);
            } else {
                throw new Error('匯出請求失敗');
            }
        } catch (error) {
            this.showError('匯出失敗：' + error.message);
            this.hideProgressModal();
        } finally {
            this.exportInProgress = false;
            this.updateExportButton();
        }
    }

    showProgressModal(request) {
        $('#exportDataType').text(request.dataTypes.map(t => this.getDataTypeDisplayName(t)).join('、'));
        $('#exportFormat').text(this.getFormatDisplayName(request.format));
        $('#exportRange').text(this.getDateRangeText(request.startDate, request.endDate) || '全部資料');
        
        $('#exportProgressModal').modal('show');
        $('#exportComplete').hide();
        
        this.updateProgress(0, '準備匯出...');
    }

    async trackExportProgress(exportId) {
        const checkProgress = async () => {
            try {
                const response = await fetch(`/api/export/progress/${exportId}`);
                const progress = await response.json();
                
                this.updateProgress(progress.percentage, progress.message);
                
                if (progress.status === 'completed') {
                    this.currentExportResult = progress.result;
                    $('#exportComplete').show();
                    this.loadExportHistory(); // 重新載入歷史記錄
                } else if (progress.status === 'failed') {
                    throw new Error(progress.errorMessage || '匯出處理失敗');
                } else {
                    setTimeout(checkProgress, 1000); // 1秒後再次檢查
                }
            } catch (error) {
                throw error;
            }
        };
        
        setTimeout(checkProgress, 500);
    }

    updateProgress(percentage, message) {
        $('#progressPercent').text(Math.round(percentage) + '%');
        $('#exportProgressBar').css('width', percentage + '%');
        $('#currentStep').text(message);
    }

    async downloadExportFile() {
        if (this.currentExportResult) {
            const downloadUrl = `/api/export/download/${this.currentExportResult.id}`;
            window.open(downloadUrl, '_blank');
            $('#exportProgressModal').modal('hide');
        }
    }

    async loadExportHistory() {
        try {
            const response = await fetch('/api/export/history');
            const history = await response.json();
            this.renderExportHistory(history);
        } catch (error) {
            console.error('載入匯出歷史失敗:', error);
        }
    }

    renderExportHistory(history) {
        const tbody = $('#exportHistoryTable');
        tbody.empty();
        
        if (history.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="7" class="text-center text-muted">尚無匯出記錄</td>
                </tr>
            `);
            return;
        }
        
        history.forEach(item => {
            const row = $(`
                <tr>
                    <td>${new Date(item.createdAt).toLocaleString()}</td>
                    <td>${item.request.dataTypes.map(t => this.getDataTypeDisplayName(t)).join('、')}</td>
                    <td><span class="badge bg-${this.getFormatBadgeColor(item.request.format)}">${this.getFormatDisplayName(item.request.format)}</span></td>
                    <td>${this.getDateRangeText(item.request.startDate, item.request.endDate) || '全部資料'}</td>
                    <td>${this.formatFileSize(item.result.fileSize)}</td>
                    <td><span class="badge bg-${this.getStatusBadgeColor(item.result.status)}">${this.getStatusDisplayName(item.result.status)}</span></td>
                    <td>
                        ${item.result.status === 'Completed' ? 
                            `<button class="btn btn-sm btn-primary" onclick="downloadExport('${item.result.id}')">
                                <i class="fas fa-download"></i> 下載
                            </button>` : 
                            '<span class="text-muted">無法下載</span>'
                        }
                    </td>
                </tr>
            `);
            tbody.append(row);
        });
    }

    // 工具方法
    getDataTypeDisplayName(type) {
        const names = {
            'accounting': '記帳資料',
            'habits': '習慣追蹤',
            'notes': '備忘錄',
            'calendar': '日曆事件'
        };
        return names[type] || type;
    }

    getFormatDisplayName(format) {
        const names = {
            'pdf': 'PDF 報表',
            'excel': 'Excel 試算表',
            'csv': 'CSV 檔案',
            'json': 'JSON 資料'
        };
        return names[format] || format;
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }
}

// 全域下載函式
window.downloadExport = function(exportId) {
    window.open(`/api/export/download/${exportId}`, '_blank');
};

// 初始化
$(document).ready(function() {
    window.exportManager = new ExportManager();
});
```

## 🎯 開發階段規劃

### Phase 1: 基礎匯出功能 (2-3 週)
- [x] 建立匯出服務架構和資料模型
- [x] 實作基本的 PDF 和 Excel 匯出
- [x] 建立匯出主頁面和基本 UI
- [x] 實作匯出歷史記錄功能

### Phase 2: 進階功能 (2-3 週)
- [ ] 實作 CSV 和 JSON 匯出
- [ ] 加入自訂樣板系統
- [ ] 實作批次匯出和排程功能
- [ ] 建立匯出進度追蹤

### Phase 3: 優化增強 (1-2 週)
- [ ] 效能優化和大量資料處理
- [ ] 匯出檔案壓縮和分割
- [ ] 雲端備份整合
- [ ] 匯出統計和分析報告

## 📝 測試規格

### 功能測試
1. 各種格式匯出正確性
2. 大量資料匯出效能
3. 匯出檔案完整性驗證
4. 日期範圍篩選準確性
5. 匯出歷史記錄管理

### 使用者體驗測試
1. 匯出流程直觀性
2. 進度顯示準確性
3. 錯誤處理友善性
4. 檔案下載便利性

## 🚀 部署注意事項

1. 確保 wwwroot/exports 目錄有寫入權限
2. 安裝必要的匯出函式庫 NuGet 套件
3. 設定檔案清理排程任務
4. 配置檔案大小和匯出頻率限制
5. 建立匯出錯誤日誌和監控

## 📚 相關文件
- [iText 7 PDF 文件](https://itextpdf.com/en/resources/documentation)
- [ClosedXML Excel 操作指南](https://closedxml.readthedocs.io/)
- [CsvHelper 官方文件](https://joshclose.github.io/CsvHelper/)
- [Bootstrap 5 元件文件](https://getbootstrap.com/docs/5.0/components/)
