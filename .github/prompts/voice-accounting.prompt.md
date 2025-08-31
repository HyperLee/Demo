# 語音記帳功能開發規格書

## 專案概述
開發一個智能語音記帳功能，讓使用者可以透過語音輸入快速新增記帳記錄。此功能將大幅提升記帳的便利性，特別適合在移動中或無法使用鍵盤的情況下使用。

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

### 1. 語音記帳主頁面
- **前端**: `#file:voice-accounting.cshtml`
- **後端**: `#file:voice-accounting.cshtml.cs`
- **路由**: `/voice-accounting`

### 1.1 功能描述
- **語音輸入**: 使用 Web Speech API 進行語音識別
- **智能解析**: 自動解析語音內容中的金額、類別、描述
- **即時回饋**: 顯示識別結果並允許用戶確認或修改
- **快速儲存**: 確認後立即儲存到記帳記錄中

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
<div class="container mt-4">
    <div class="row justify-content-center">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h4><i class="fas fa-microphone me-2"></i>語音記帳</h4>
                </div>
                <div class="card-body">
                    <!-- 語音狀態顯示 -->
                    <div class="text-center mb-4">
                        <div id="voiceStatus" class="voice-status-idle">
                            <div class="voice-animation">
                                <div class="voice-wave"></div>
                                <div class="voice-wave"></div>
                                <div class="voice-wave"></div>
                            </div>
                            <p class="mt-3" id="statusText">點擊開始語音輸入</p>
                        </div>
                        <button id="startVoiceBtn" class="btn btn-primary btn-lg rounded-pill">
                            <i class="fas fa-microphone"></i> 開始錄音
                        </button>
                        <button id="stopVoiceBtn" class="btn btn-danger btn-lg rounded-pill d-none">
                            <i class="fas fa-stop"></i> 停止錄音
                        </button>
                    </div>

                    <!-- 識別結果顯示 -->
                    <div id="speechResult" class="alert alert-info d-none">
                        <h6>語音識別結果：</h6>
                        <p id="speechText" class="mb-0"></p>
                    </div>

                    <!-- 解析結果確認 -->
                    <div id="parsedResult" class="card mt-3 d-none">
                        <div class="card-header">
                            <h6 class="mb-0">請確認解析結果</h6>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-4">
                                    <label class="form-label">類型</label>
                                    <select id="recordType" class="form-select">
                                        <option value="支出">支出</option>
                                        <option value="收入">收入</option>
                                    </select>
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label">金額</label>
                                    <input type="number" id="amount" class="form-control" step="0.01">
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label">類別</label>
                                    <select id="category" class="form-select">
                                        <!-- 動態載入類別 -->
                                    </select>
                                </div>
                            </div>
                            <div class="row mt-3">
                                <div class="col-12">
                                    <label class="form-label">描述</label>
                                    <input type="text" id="description" class="form-control">
                                </div>
                            </div>
                            <div class="mt-3">
                                <button id="saveRecord" class="btn btn-success">
                                    <i class="fas fa-save"></i> 儲存記錄
                                </button>
                                <button id="resetForm" class="btn btn-secondary">
                                    <i class="fas fa-redo"></i> 重新開始
                                </button>
                            </div>
                        </div>
                    </div>

                    <!-- 最近記錄 -->
                    <div class="mt-4">
                        <h6>最近新增的記錄</h6>
                        <div id="recentRecords" class="list-group">
                            <!-- 動態載入最近記錄 -->
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- CSS 樣式 -->
<style>
.voice-status-idle, .voice-status-listening, .voice-status-processing {
    padding: 2rem;
    border-radius: 15px;
    transition: all 0.3s ease;
}

.voice-status-idle {
    background: linear-gradient(135deg, #f8f9fa, #e9ecef);
}

.voice-status-listening {
    background: linear-gradient(135deg, #d4edda, #c3e6cb);
    animation: pulse 1.5s infinite;
}

.voice-status-processing {
    background: linear-gradient(135deg, #fff3cd, #ffeaa7);
}

.voice-animation {
    display: flex;
    justify-content: center;
    align-items: end;
    height: 60px;
    gap: 5px;
}

.voice-wave {
    width: 8px;
    background: var(--bs-primary);
    border-radius: 4px;
    animation: wave 1.2s ease-in-out infinite;
}

.voice-wave:nth-child(2) {
    animation-delay: 0.1s;
}

.voice-wave:nth-child(3) {
    animation-delay: 0.2s;
}

@keyframes wave {
    0%, 100% { height: 20px; }
    50% { height: 60px; }
}

@keyframes pulse {
    0% { transform: scale(1); }
    50% { transform: scale(1.05); }
    100% { transform: scale(1); }
}
</style>
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

### 1.5 後端實作 (voice-accounting.cshtml.cs)
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

### 3. 語音回饋系統
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
```

### 4. 離線語音處理
- 實作本地語音模型（可選）
- 網路斷線時的暫存機制
- 自動同步功能

## 測試規範

### 4.1 功能測試
- [ ] 語音識別準確度測試
- [ ] 不同瀏覽器相容性測試
- [ ] 語音命令格式測試
- [ ] 資料儲存完整性測試

### 4.2 使用者體驗測試
- [ ] 響應速度測試
- [ ] 錯誤處理測試
- [ ] 無障礙功能測試
- [ ] 多語言支援測試（可選）

## 安全性考量
- 語音數據不會上傳到伺服器
- 所有處理都在用戶端進行
- 敏感資訊的語音輸入警告機制
- CSRF 保護機制

## 未來擴展計畫
- AI 語音助手整合
- 多語言語音識別支援
- 語音指令自訂功能
- 語音搜尋記帳記錄功能
