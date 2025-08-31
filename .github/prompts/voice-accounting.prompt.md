# 語音記帳功能開發規格書

## 專案概述
將智能語音記帳功能整合到現有的記帳系統中，作為表單輸入的增強功能。讓使用者可以透過語音輸入快速填寫記帳表單，支援個人記帳和家庭共享記帳兩種模式。此功能將大幅提升記帳的便利性，特別適合在移動中或無法使用鍵盤的情況下使用。

## 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, jQuery, Web Speech API
- **語音識別**: Web Speech API (SpeechRecognition)
- **語音合成**: Web Speech Synthesis API (可選)
- **自然語言處理**: 正規表示式 + 關鍵字匹配
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 核心功能模組

### 1. 語音輸入組件整合
- **整合位置**: 
  - `index8.cshtml` - 個人記帳表單語音輸入
  - `family-accounting.cshtml` - 家庭記帳語音輸入
- **共用組件**: `wwwroot/js/voice-input.js`
- **實作方式**: 模組化 JavaScript 組件

### 1.1 功能描述
- **語音輸入**: 使用 Web Speech API 進行語音識別
- **智能解析**: 自動解析語音內容中的金額、類別、描述
- **表單整合**: 解析結果自動填入現有表單欄位
- **即時預覽**: 顯示識別結果並允許用戶確認或修改
- **無縫儲存**: 使用現有的表單提交和儲存機制

### 1.2 語音命令格式設計
```text
支援的語音格式：
1. "我花了 100 元買咖啡"
2. "收入 3000 元薪水"
3. "支出 50 元搭計程車"
4. "午餐花了 120 元"
5. "今天買菜花了 200 元"
```

### 1.3 前端實作 (voice-accounting.cshtml)
```html
### 1.3 個人記帳表單整合 (index8.cshtml)
在現有的記帳表單中加入語音輸入功能：

```html
<!-- 在表單頂部加入語音輸入區塊 -->
<div class="row mb-4">
    <div class="col-12">
        <div class="card border-primary">
            <div class="card-header bg-primary text-white">
                <h5 class="mb-0">
                    <i class="fas fa-microphone me-2"></i>語音快速輸入
                </h5>
            </div>
            <div class="card-body text-center" id="voiceInputArea">
                <div id="voiceStatus" class="voice-status-idle">
                    <div class="voice-animation d-none" id="voiceAnimation">
                        <div class="voice-wave"></div>
                        <div class="voice-wave"></div>
                        <div class="voice-wave"></div>
                        <div class="voice-wave"></div>
                        <div class="voice-wave"></div>
                    </div>
                    <div id="voiceIcon">
                        <i class="fas fa-microphone fa-3x text-primary mb-3"></i>
                    </div>
                    <p id="statusText" class="mb-3">點擊開始語音輸入</p>
                    <button type="button" id="startVoiceBtn" class="btn btn-primary btn-lg">
                        <i class="fas fa-microphone"></i> 開始語音輸入
                    </button>
                    <button type="button" id="stopVoiceBtn" class="btn btn-danger btn-lg d-none">
                        <i class="fas fa-stop"></i> 停止錄音
                    </button>
                </div>
                
                <!-- 語音識別結果顯示 -->
                <div id="speechResult" class="alert alert-info mt-3 d-none">
                    <h6>語音識別結果：</h6>
                    <p id="speechText" class="mb-0"></p>
                </div>
                
                <!-- 解析結果預覽 -->
                <div id="parsedPreview" class="alert alert-success mt-3 d-none">
                    <h6>解析結果預覽：</h6>
                    <div class="row">
                        <div class="col-md-3">
                            <strong>類型:</strong> <span id="previewType"></span>
                        </div>
                        <div class="col-md-3">
                            <strong>金額:</strong> <span id="previewAmount"></span>
                        </div>
                        <div class="col-md-3">
                            <strong>類別:</strong> <span id="previewCategory"></span>
                        </div>
                        <div class="col-md-3">
                            <strong>描述:</strong> <span id="previewDescription"></span>
                        </div>
                    </div>
                    <div class="mt-2">
                        <button type="button" id="applyVoiceInput" class="btn btn-success btn-sm">
                            <i class="fas fa-check"></i> 套用到表單
                        </button>
                        <button type="button" id="resetVoiceInput" class="btn btn-secondary btn-sm">
                            <i class="fas fa-redo"></i> 重新輸入
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- 原有的記帳表單保持不變 -->
```

### 1.4 家庭記帳表單整合 (family-accounting.cshtml)
在快速記帳區域加入語音輸入功能：

```html
<!-- 在快速記帳表單中加入語音按鈕 -->
<div class="card mb-4">
    <div class="card-header bg-success text-white d-flex justify-content-between align-items-center">
        <h5 class="mb-0"><i class="fas fa-plus-circle me-2"></i>快速新增家庭支出</h5>
        <button type="button" id="voiceInputToggle" class="btn btn-light btn-sm">
            <i class="fas fa-microphone"></i> 語音輸入
        </button>
    </div>
    
    <!-- 語音輸入區塊（預設隱藏） -->
    <div id="familyVoiceInputArea" class="card-body border-bottom bg-light d-none">
        <!-- 語音輸入介面 - 類似個人記帳但適配家庭功能 -->
    </div>
    
    <!-- 原有的快速記帳表單 -->
    <div class="card-body">
        <form method="post" asp-page-handler="QuickExpense">
            <!-- 現有表單內容保持不變 -->
        </form>
    </div>
</div>
```
```

### 1.4 JavaScript 實作
```javascript
class VoiceAccounting {
    constructor() {
        this.recognition = null;
        this.isListening = false;
        this.categories = [];
        this.init();
    }

    init() {
        this.setupSpeechRecognition();
        this.loadCategories();
        this.bindEvents();
        this.loadRecentRecords();
    }

    setupSpeechRecognition() {
        if ('SpeechRecognition' in window || 'webkitSpeechRecognition' in window) {
            const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            this.recognition = new SpeechRecognition();
            
            this.recognition.lang = 'zh-TW';
            this.recognition.continuous = false;
            this.recognition.interimResults = true;
            this.recognition.maxAlternatives = 1;

            this.recognition.onstart = () => {
                this.updateStatus('listening', '正在聆聽...');
            };

            this.recognition.onresult = (event) => {
                const result = event.results[event.results.length - 1];
                if (result.isFinal) {
                    this.processSpeechResult(result[0].transcript);
                }
            };

            this.recognition.onerror = (event) => {
                console.error('語音識別錯誤:', event.error);
                this.updateStatus('idle', '語音識別發生錯誤，請重試');
                this.resetUI();
            };

            this.recognition.onend = () => {
                this.isListening = false;
                this.resetUI();
            };
        } else {
            alert('您的瀏覽器不支援語音識別功能');
        }
    }

    bindEvents() {
        $('#startVoiceBtn').on('click', () => this.startListening());
        $('#stopVoiceBtn').on('click', () => this.stopListening());
        $('#saveRecord').on('click', () => this.saveRecord());
        $('#resetForm').on('click', () => this.resetForm());
    }

    startListening() {
        if (this.recognition && !this.isListening) {
            this.isListening = true;
            this.recognition.start();
            $('#startVoiceBtn').addClass('d-none');
            $('#stopVoiceBtn').removeClass('d-none');
        }
    }

    stopListening() {
        if (this.recognition && this.isListening) {
            this.recognition.stop();
            this.isListening = false;
        }
    }

    processSpeechResult(transcript) {
        $('#speechText').text(transcript);
        $('#speechResult').removeClass('d-none');
        
        this.updateStatus('processing', '正在解析語音內容...');
        
        // 解析語音內容
        const parsed = this.parseVoiceInput(transcript);
        this.displayParsedResult(parsed);
        
        setTimeout(() => {
            this.updateStatus('idle', '解析完成，請確認資訊');
        }, 1000);
    }

    parseVoiceInput(text) {
        const result = {
            type: '支出',
            amount: null,
            category: null,
            description: text
        };

        // 金額解析
        const amountPattern = /(\d+(?:\.\d{1,2})?)\s*[元塊]/;
        const amountMatch = text.match(amountPattern);
        if (amountMatch) {
            result.amount = parseFloat(amountMatch[1]);
        }

        // 類型判斷
        if (text.includes('收入') || text.includes('賺') || text.includes('薪水') || text.includes('獎金')) {
            result.type = '收入';
        }

        // 類別推測
        const categoryKeywords = {
            '餐飲': ['吃', '喝', '咖啡', '午餐', '晚餐', '早餐', '餐廳', '飲料'],
            '交通': ['車', '計程車', '公車', '捷運', '油錢', '停車'],
            '購物': ['買', '購買', 'shopping'],
            '娛樂': ['電影', '遊戲', '唱歌', 'KTV'],
            '醫療': ['看病', '藥', '醫院', '診所'],
            '學習': ['書', '課程', '學費'],
            '其他': []
        };

        for (const [category, keywords] of Object.entries(categoryKeywords)) {
            if (keywords.some(keyword => text.includes(keyword))) {
                result.category = category;
                break;
            }
        }

        // 描述清理
        result.description = text
            .replace(amountPattern, '')
            .replace(/[花了花費支出收入]/g, '')
            .trim();

        return result;
    }

    displayParsedResult(parsed) {
        $('#recordType').val(parsed.type);
        $('#amount').val(parsed.amount || '');
        $('#description').val(parsed.description);
        
        if (parsed.category) {
            $('#category').val(parsed.category);
        }
        
        $('#parsedResult').removeClass('d-none');
    }

    async saveRecord() {
        const recordData = {
            date: new Date().toISOString().split('T')[0],
            type: $('#recordType').val(),
            amount: parseFloat($('#amount').val()),
            category: $('#category').val(),
            description: $('#description').val(),
            createdAt: new Date().toISOString()
        };

        try {
            const response = await fetch('/VoiceAccounting/SaveRecord', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify(recordData)
            });

            if (response.ok) {
                alert('記錄已成功儲存！');
                this.resetForm();
                this.loadRecentRecords();
            } else {
                alert('儲存失敗，請重試');
            }
        } catch (error) {
            console.error('儲存記錄時發生錯誤:', error);
            alert('儲存失敗，請檢查網路連線');
        }
    }

    updateStatus(status, message) {
        const statusEl = $('#voiceStatus');
        const textEl = $('#statusText');
        
        statusEl.removeClass('voice-status-idle voice-status-listening voice-status-processing');
        statusEl.addClass(`voice-status-${status}`);
        textEl.text(message);
    }

    resetForm() {
        $('#speechResult').addClass('d-none');
        $('#parsedResult').addClass('d-none');
        $('#recordType').val('支出');
        $('#amount').val('');
        $('#category').val('');
        $('#description').val('');
        this.updateStatus('idle', '點擊開始語音輸入');
    }

    resetUI() {
        $('#startVoiceBtn').removeClass('d-none');
        $('#stopVoiceBtn').addClass('d-none');
    }

    async loadCategories() {
        // 載入類別選項
        try {
            const response = await fetch('/VoiceAccounting/GetCategories');
            this.categories = await response.json();
            
            const categorySelect = $('#category');
            categorySelect.empty();
            this.categories.forEach(cat => {
                categorySelect.append(`<option value="${cat}">${cat}</option>`);
            });
        } catch (error) {
            console.error('載入類別失敗:', error);
        }
    }

    async loadRecentRecords() {
        // 載入最近記錄
        try {
            const response = await fetch('/VoiceAccounting/GetRecentRecords');
            const records = await response.json();
            
            const container = $('#recentRecords');
            container.empty();
            
            records.slice(0, 5).forEach(record => {
                const item = `
                    <div class="list-group-item">
                        <div class="d-flex justify-content-between">
                            <div>
                                <strong>${record.type}</strong> - ${record.description}
                                <br><small class="text-muted">${record.category} | ${record.date}</small>
                            </div>
                            <div class="text-end">
                                <strong class="${record.type === '支出' ? 'text-danger' : 'text-success'}">
                                    ${record.type === '支出' ? '-' : '+'}$${record.amount}
                                </strong>
                            </div>
                        </div>
                    </div>
                `;
                container.append(item);
            });
        } catch (error) {
            console.error('載入最近記錄失敗:', error);
        }
    }
}

// 初始化語音記帳功能
$(document).ready(function() {
    new VoiceAccounting();
});
```

### 1.5 後端 API 整合
不需要建立新的後端頁面，而是在現有的頁面中加入語音相關的處理方法：

#### A. 個人記帳後端整合 (index8.cshtml.cs)
```csharp
// 加入語音相關的 Handler 方法
[HttpPost]
public async Task<IActionResult> OnPostParseVoiceInput([FromBody] VoiceParseRequest request)
{
    try
    {
        var parseResult = await ParseVoiceTextAsync(request.VoiceText);
        return new JsonResult(parseResult);
    }
    catch (Exception ex)
    {
        return BadRequest($"語音解析失敗: {ex.Message}");
    }
}

[HttpGet] 
public async Task<IActionResult> OnGetCategories()
{
    try
    {
        var categories = await LoadCategoriesAsync();
        var categoryNames = categories.Select(c => c.Name).ToList();
        return new JsonResult(categoryNames);
    }
    catch (Exception ex)
    {
        return BadRequest($"載入類別失敗: {ex.Message}");
    }
}

private async Task<VoiceParseResult> ParseVoiceTextAsync(string voiceText)
{
    // 伺服器端語音解析邏輯（可選）
    // 主要解析仍在前端進行，這裡可以做進階處理
    return new VoiceParseResult
    {
        OriginalText = voiceText,
        ParsedAt = DateTime.Now
    };
}
```

#### B. 家庭記帳後端整合 (family-accounting.cshtml.cs)
```csharp
// 在現有的 FamilyAccountingModel 中加入語音支援
[HttpPost]
public async Task<IActionResult> OnPostQuickExpenseVoice([FromBody] VoiceFamilyExpenseRequest request)
{
    try
    {
        // 使用現有的 QuickExpense 邏輯，但資料來源是語音解析結果
        var expenseRequest = new QuickExpenseRequest
        {
            Type = request.Type,
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            Date = request.Date ?? DateTime.Today.ToString("yyyy-MM-dd"),
            SplitType = request.SplitType ?? "我支付"
        };
        
        return await OnPostQuickExpense(expenseRequest);
    }
    catch (Exception ex)
    {
        return BadRequest($"語音記帳失敗: {ex.Message}");
    }
}
```
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Demo.Models;

namespace Demo.Pages
{
    public class VoiceAccountingModel : PageModel
    {
        private readonly string _accountingRecordsPath;
        private readonly string _categoriesPath;

        public VoiceAccountingModel()
        {
            _accountingRecordsPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "accounting-records.json");
            _categoriesPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "categories.json");
        }

        public void OnGet()
        {
        }

        [HttpPost]
        public async Task<IActionResult> OnPostSaveRecord([FromBody] AccountingRecord record)
        {
            try
            {
                var records = await LoadRecordsAsync();
                
                // 生成新的 ID
                record.Id = records.Any() ? records.Max(r => r.Id) + 1 : 1;
                record.CreatedAt = DateTime.Now;
                
                records.Add(record);
                
                await SaveRecordsAsync(records);
                
                return new JsonResult(new { success = true, message = "記錄已成功儲存" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetGetCategories()
        {
            try
            {
                var categories = await LoadCategoriesAsync();
                return new JsonResult(categories.Select(c => c.Name).ToList());
            }
            catch (Exception ex)
            {
                return new JsonResult(new List<string> { "其他" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OnGetGetRecentRecords()
        {
            try
            {
                var records = await LoadRecordsAsync();
                var recentRecords = records
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .ToList();
                
                return new JsonResult(recentRecords);
            }
            catch (Exception ex)
            {
                return new JsonResult(new List<AccountingRecord>());
            }
        }

        private async Task<List<AccountingRecord>> LoadRecordsAsync()
        {
            if (!System.IO.File.Exists(_accountingRecordsPath))
                return new List<AccountingRecord>();
                
            var json = await System.IO.File.ReadAllTextAsync(_accountingRecordsPath);
            return JsonConvert.DeserializeObject<List<AccountingRecord>>(json) ?? new List<AccountingRecord>();
        }

        private async Task SaveRecordsAsync(List<AccountingRecord> records)
        {
            var json = JsonConvert.SerializeObject(records, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(_accountingRecordsPath, json);
        }

        private async Task<List<Category>> LoadCategoriesAsync()
        {
            if (!System.IO.File.Exists(_categoriesPath))
                return GetDefaultCategories();
                
            var json = await System.IO.File.ReadAllTextAsync(_categoriesPath);
            return JsonConvert.DeserializeObject<List<Category>>(json) ?? GetDefaultCategories();
        }

        private List<Category> GetDefaultCategories()
        {
            return new List<Category>
            {
                new Category { Name = "餐飲" },
                new Category { Name = "交通" },
                new Category { Name = "購物" },
                new Category { Name = "娛樂" },
                new Category { Name = "醫療" },
                new Category { Name = "學習" },
                new Category { Name = "其他" }
            };
        }
    }
}
```

## 進階功能

### 2. 智能語音命令擴展
- **批量記帳**: "今天買菜花了 200 元，搭車花了 50 元"
- **日期指定**: "昨天午餐花了 120 元"
- **分類學習**: 系統學習用戶的語音習慣，提升識別準確度
- **家庭分攤**: "大家一起吃飯花了 600 元平均分攤"

### 3. 整合現有功能
- **類別系統**: 使用現有的類別管理和智能分類功能
- **資料同步**: 語音記帳與手動記帳共用相同資料結構
- **統計分析**: 語音記帳資料自動納入現有的統計和分析功能
- **匯出功能**: 支援匯出包含語音記帳的完整記錄

### 4. 家庭記帳整合特色
- **即時通知**: 透過 SignalR 即時通知家庭成員語音記帳結果
- **語音分攤**: 支援語音指定分攤方式和金額
- **成員識別**: 可擴展支援語音識別不同家庭成員（進階功能）

### 5. 語音回饋系統
```javascript
// 語音合成回饋
function speakFeedback(text) {
    if ('speechSynthesis' in window) {
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'zh-TW';
        utterance.rate = 0.9;
        speechSynthesis.speak(utterance);
    }
}

// 在記帳完成後提供語音回饋
function onVoiceRecordSaved(result) {
    speakFeedback(`已記錄 ${result.type} ${result.amount} 元，類別：${result.category}`);
}
```

## 實作計畫

### Phase 1: 基礎整合 
1. **建立共用語音組件** (`wwwroot/js/voice-input.js`)
2. **個人記帳整合** - 在 `index8.cshtml` 加入語音輸入區塊
3. **基礎語音解析** - 支援金額、類別、描述的基本識別

### Phase 2: 家庭記帳整合
1. **家庭記帳語音功能** - 在 `family-accounting.cshtml` 整合語音輸入
2. **分攤模式支援** - 語音識別分攤方式
3. **SignalR 整合** - 語音記帳的即時通知

### Phase 3: 進階功能
1. **智能學習** - 個人化語音習慣學習
2. **批量記帳** - 支援一次語音輸入多筆記錄
3. **語音回饋** - 記帳完成的語音確認

### Phase 4: 優化與擴展
1. **效能優化** - 語音識別準確度提升
2. **多語言支援** - 支援英文等其他語言
3. **離線功能** - 網路斷線時的暫存機制

## 檔案結構調整

```
Demo/
├── Pages/
│   ├── index8.cshtml          # 個人記帳表單 (加入語音功能)
│   ├── index8.cshtml.cs       # 後端邏輯 (加入語音 API)
│   ├── family-accounting.cshtml     # 家庭記帳 (加入語音功能)
│   └── family-accounting.cshtml.cs  # 後端邏輯 (加入語音 API)
├── wwwroot/
│   ├── js/
│   │   ├── voice-input.js     # 語音輸入組件 (新增)
│   │   └── voice-styles.css   # 語音介面樣式 (新增)
│   └── ...
├── Models/
│   └── VoiceModels.cs         # 語音相關模型 (新增)
└── ...
```

## 資料模型擴展

```csharp
// Models/VoiceModels.cs
public class VoiceParseRequest
{
    public string VoiceText { get; set; }
    public string Context { get; set; } // "personal" 或 "family"
}

public class VoiceParseResult
{
    public string OriginalText { get; set; }
    public string Type { get; set; }
    public decimal? Amount { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string SplitType { get; set; } // 家庭模式專用
    public DateTime ParsedAt { get; set; }
    public double Confidence { get; set; } // 識別信心度
}

public class VoiceFamilyExpenseRequest : QuickExpenseRequest
{
    public string VoiceText { get; set; }
    public double ParseConfidence { get; set; }
}
```
