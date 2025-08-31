using System.Text.Json.Serialization;

namespace Demo.Models;

/// <summary>
/// 匯出請求模型
/// </summary>
public class ExportRequest
{
    /// <summary>
    /// 要匯出的資料類型清單
    /// </summary>
    public List<string> DataTypes { get; set; } = [];

    /// <summary>
    /// 匯出格式 (pdf, excel, csv, json)
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// 開始日期 (可選)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期 (可選)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 使用的樣板名稱
    /// </summary>
    public string TemplateName { get; set; } = "default";

    /// <summary>
    /// 額外選項
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new();

    /// <summary>
    /// 使用者 ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 匯出結果模型
/// </summary>
public class ExportResult
{
    /// <summary>
    /// 匯出結果唯一識別碼
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 檔案名稱
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 檔案路徑
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 內容類型
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 檔案大小 (位元組)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 匯出狀態
    /// </summary>
    public ExportStatus Status { get; set; } = ExportStatus.Processing;

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 完成時間
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 處理時長
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// 匯出元資料
    /// </summary>
    public ExportMetadata Metadata { get; set; } = new();
}

/// <summary>
/// 匯出狀態列舉
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExportStatus
{
    /// <summary>等待處理</summary>
    Pending,
    /// <summary>處理中</summary>
    Processing,
    /// <summary>完成</summary>
    Completed,
    /// <summary>失敗</summary>
    Failed,
    /// <summary>已過期</summary>
    Expired
}

/// <summary>
/// 匯出元資料
/// </summary>
public class ExportMetadata
{
    /// <summary>
    /// 資料類型清單
    /// </summary>
    public List<string> DataTypes { get; set; } = [];

    /// <summary>
    /// 匯出格式
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 總記錄數
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// 各類型記錄數統計
    /// </summary>
    public Dictionary<string, int> RecordCounts { get; set; } = new();

    /// <summary>
    /// 使用的樣板名稱
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;
}

/// <summary>
/// 匯出歷史記錄
/// </summary>
public class ExportHistory
{
    /// <summary>
    /// 歷史記錄唯一識別碼
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 使用者 ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 匯出請求
    /// </summary>
    public ExportRequest Request { get; set; } = new();

    /// <summary>
    /// 匯出結果
    /// </summary>
    public ExportResult Result { get; set; } = new();

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// PDF 專用匯出資料模型
/// </summary>
public class PdfExportData
{
    /// <summary>
    /// 報表標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 報表副標題
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// 生成時間
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 期間描述
    /// </summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// 報表章節
    /// </summary>
    public List<PdfSection> Sections { get; set; } = [];

    /// <summary>
    /// 摘要資料
    /// </summary>
    public Dictionary<string, object> Summary { get; set; } = new();
}

/// <summary>
/// PDF 報表章節
/// </summary>
public class PdfSection
{
    /// <summary>
    /// 章節標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 章節描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 章節資料
    /// </summary>
    public List<Dictionary<string, object>> Data { get; set; } = [];

    /// <summary>
    /// 圖表資料
    /// </summary>
    public List<ChartData> Charts { get; set; } = [];

    /// <summary>
    /// 統計資料
    /// </summary>
    public Dictionary<string, object> Statistics { get; set; } = new();
}

/// <summary>
/// 圖表資料模型
/// </summary>
public class ChartData
{
    /// <summary>
    /// 圖表類型 (pie, bar, line)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 圖表標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 標籤
    /// </summary>
    public List<string> Labels { get; set; } = [];

    /// <summary>
    /// 數值
    /// </summary>
    public List<decimal> Values { get; set; } = [];

    /// <summary>
    /// 額外選項
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// Excel 專用匯出資料模型
/// </summary>
public class ExcelExportData
{
    /// <summary>
    /// 工作簿名稱
    /// </summary>
    public string WorkbookName { get; set; } = string.Empty;

    /// <summary>
    /// 工作表清單
    /// </summary>
    public List<ExcelWorksheet> Worksheets { get; set; } = [];

    /// <summary>
    /// 元資料
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Excel 工作表
/// </summary>
public class ExcelWorksheet
{
    /// <summary>
    /// 工作表名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 表頭
    /// </summary>
    public List<string> Headers { get; set; } = [];

    /// <summary>
    /// 資料列
    /// </summary>
    public List<List<object>> Rows { get; set; } = [];

    /// <summary>
    /// 欄位格式
    /// </summary>
    public Dictionary<string, ExcelColumnFormat> ColumnFormats { get; set; } = new();

    /// <summary>
    /// 圖表清單
    /// </summary>
    public List<ExcelChart> Charts { get; set; } = [];
}

/// <summary>
/// Excel 欄位格式
/// </summary>
public class ExcelColumnFormat
{
    /// <summary>
    /// 資料類型
    /// </summary>
    public string DataType { get; set; } = "General";

    /// <summary>
    /// 數字格式
    /// </summary>
    public string NumberFormat { get; set; } = string.Empty;

    /// <summary>
    /// 欄寬
    /// </summary>
    public int Width { get; set; } = 15;

    /// <summary>
    /// 是否粗體
    /// </summary>
    public bool Bold { get; set; } = false;

    /// <summary>
    /// 背景色
    /// </summary>
    public string BackgroundColor { get; set; } = string.Empty;
}

/// <summary>
/// Excel 圖表
/// </summary>
public class ExcelChart
{
    /// <summary>
    /// 圖表類型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 圖表標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 資料範圍
    /// </summary>
    public string DataRange { get; set; } = string.Empty;

    /// <summary>
    /// 圖表位置
    /// </summary>
    public string Position { get; set; } = string.Empty;
}

/// <summary>
/// 匯出樣板
/// </summary>
public class ExportTemplate
{
    /// <summary>
    /// 樣板唯一識別碼
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 樣板名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 樣板描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 支援的匯出格式
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// 支援的資料類型
    /// </summary>
    public List<string> SupportedDataTypes { get; set; } = [];

    /// <summary>
    /// 樣板設定
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// 是否為預設樣板
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最後使用時間
    /// </summary>
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// 使用次數
    /// </summary>
    public int UsageCount { get; set; } = 0;
}

/// <summary>
/// 匯出進度模型
/// </summary>
public class ExportProgress
{
    /// <summary>
    /// 匯出 ID
    /// </summary>
    public string ExportId { get; set; } = string.Empty;

    /// <summary>
    /// 進度百分比 (0-100)
    /// </summary>
    public int Percentage { get; set; } = 0;

    /// <summary>
    /// 目前步驟描述
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 狀態
    /// </summary>
    public ExportStatus Status { get; set; } = ExportStatus.Pending;

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 匯出結果 (完成時)
    /// </summary>
    public ExportResult? Result { get; set; }
}
