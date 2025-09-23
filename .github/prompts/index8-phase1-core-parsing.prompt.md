# 記帳系統語音輸入功能 Phase 1: 核心解析功能開發規格書

## 階段概述
**優先級**: 高  
**預估時程**: 2週  
**目標**: 建立完整的語音解析核心功能，使語音輸入能夠解析手動輸入的所有主要欄位

## 開發目標

### 主要目標
1. 擴充 VoiceParseResult 模型以支援所有手動輸入欄位
2. 重構語音解析邏輯，增加專用解析方法
3. 建立穩健的錯誤處理和降級策略
4. 更新前端UI以支援新的解析結果展示

### 成功指標
- [ ] 支援解析 9 個完整欄位（日期、類型、金額、付款方式、大分類、細分類、描述、商家、備註）
- [ ] 基本解析準確度達到 60%+
- [ ] 解析時間控制在 2 秒內
- [ ] 向下相容，不影響現有功能

## 技術實作細節

### 1. 模型擴充 (VoiceModels.cs)

#### 1.1 VoiceParseResult 模型增強
```csharp
/// <summary>
/// 語音解析結果模型 (Phase 1 增強版)
/// </summary>
public class VoiceParseResult
{
    // === 現有欄位 (保持不變) ===
    public string OriginalText { get; set; } = string.Empty;
    public string Type { get; set; } = "Expense";
    public decimal? Amount { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ParsedAt { get; set; } = DateTime.Now;
    public double ParseConfidence { get; set; } = 1.0;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    
    // === Phase 1 新增欄位 ===
    /// <summary>
    /// 解析的日期
    /// </summary>
    public DateTime? Date { get; set; }
    
    /// <summary>
    /// 付款方式
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// 細分類
    /// </summary>
    public string? SubCategory { get; set; }
    
    /// <summary>
    /// 商家名稱
    /// </summary>
    public string? MerchantName { get; set; }
    
    /// <summary>
    /// 備註資訊 (從描述中分離出來的額外資訊)
    /// </summary>
    public string? Note { get; set; }
    
    /// <summary>
    /// 各欄位解析信心度 (0-1)
    /// </summary>
    public Dictionary<string, double> FieldConfidence { get; set; } = new();
    
    /// <summary>
    /// 無法解析的剩餘內容
    /// </summary>
    public string? UnparsedContent { get; set; }
    
    /// <summary>
    /// 解析過程中的中間結果 (供除錯使用)
    /// </summary>
    public Dictionary<string, object> ParsedSteps { get; set; } = new();
}
```

#### 1.2 輔助模型定義
```csharp
/// <summary>
/// 解析配置模型
/// </summary>
public class VoiceParseConfig
{
    /// <summary>
    /// 最低信心度閾值
    /// </summary>
    public double MinConfidenceThreshold { get; set; } = 0.3;
    
    /// <summary>
    /// 是否啟用模糊匹配
    /// </summary>
    public bool EnableFuzzyMatching { get; set; } = true;
    
    /// <summary>
    /// 日期解析模式
    /// </summary>
    public DateParseMode DateParseMode { get; set; } = DateParseMode.Flexible;
}

/// <summary>
/// 日期解析模式
/// </summary>
public enum DateParseMode
{
    Strict,     // 嚴格模式：只解析明確的日期格式
    Flexible,   // 彈性模式：支援相對日期 (今天、昨天等)
    Smart       // 智能模式：根據上下文推測日期
}
```

### 2. 後端解析邏輯重構 (index8.cshtml.cs)

#### 2.1 主要解析方法重構
```csharp
/// <summary>
/// 語音文字解析主方法 (Phase 1 重構版)
/// </summary>
private async Task<VoiceParseResult> ParseVoiceTextAsync(string voiceText)
{
    var result = new VoiceParseResult
    {
        OriginalText = voiceText,
        IsSuccess = false
    };

    try
    {
        // 1. 預處理：清理和標準化文字
        var normalizedText = PreprocessVoiceText(voiceText);
        result.ParsedSteps["PreprocessedText"] = normalizedText;

        // 2. 金額解析 (優先級最高)
        var amountResult = ParseAmountFromText(normalizedText);
        if (amountResult.HasValue)
        {
            result.Amount = amountResult.Value;
            result.FieldConfidence["Amount"] = 0.9; // 數字解析通常很準確
        }

        // 3. 日期解析
        var dateResult = ParseDateFromText(normalizedText);
        if (dateResult != null)
        {
            result.Date = dateResult.Value.Date;
            result.FieldConfidence["Date"] = dateResult.Value.Confidence;
        }

        // 4. 收支類型判斷
        var typeResult = ParseTypeFromText(normalizedText);
        result.Type = typeResult.Type;
        result.FieldConfidence["Type"] = typeResult.Confidence;

        // 5. 付款方式識別
        var paymentResult = ParsePaymentMethodFromText(normalizedText);
        if (!string.IsNullOrEmpty(paymentResult.Method))
        {
            result.PaymentMethod = paymentResult.Method;
            result.FieldConfidence["PaymentMethod"] = paymentResult.Confidence;
        }

        // 6. 商家名稱提取
        var merchantResult = ParseMerchantNameFromText(normalizedText);
        if (!string.IsNullOrEmpty(merchantResult.Name))
        {
            result.MerchantName = merchantResult.Name;
            result.FieldConfidence["MerchantName"] = merchantResult.Confidence;
        }

        // 7. 分類解析 (需要考慮商家名稱)
        var categoryResult = await ParseCategoryFromTextAsync(normalizedText, result.Type, result.MerchantName);
        if (!string.IsNullOrEmpty(categoryResult.Category))
        {
            result.Category = categoryResult.Category;
            result.SubCategory = categoryResult.SubCategory;
            result.FieldConfidence["Category"] = categoryResult.Confidence;
            if (!string.IsNullOrEmpty(categoryResult.SubCategory))
            {
                result.FieldConfidence["SubCategory"] = categoryResult.Confidence * 0.8; // 子分類信心度稍低
            }
        }

        // 8. 描述和備註分離
        var descriptionResult = ParseDescriptionAndNoteFromText(normalizedText, result);
        result.Description = descriptionResult.Description;
        result.Note = descriptionResult.Note;
        result.FieldConfidence["Description"] = descriptionResult.DescriptionConfidence;
        if (!string.IsNullOrEmpty(result.Note))
        {
            result.FieldConfidence["Note"] = descriptionResult.NoteConfidence;
        }

        // 9. 計算整體信心度和成功狀態
        result.ParseConfidence = CalculateOverallConfidence(result);
        result.IsSuccess = result.ParseConfidence >= 0.4; // 降低成功閾值

        // 10. 識別未解析內容
        result.UnparsedContent = IdentifyUnparsedContent(normalizedText, result);

        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "語音文字解析時發生錯誤");
        result.ErrorMessage = "語音解析過程中發生錯誤";
        return result;
    }
}
```

#### 2.2 專用解析方法實作

##### 日期解析方法
```csharp
/// <summary>
/// 日期解析結果
/// </summary>
private class DateParseResult
{
    public DateTime Date { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// 從文字中解析日期
/// </summary>
private DateParseResult? ParseDateFromText(string text)
{
    try
    {
        // 1. 相對日期匹配
        var relativeDatePatterns = new Dictionary<string, int>
        {
            { "今天", 0 }, { "今日", 0 },
            { "昨天", -1 }, { "昨日", -1 },
            { "前天", -2 }, { "前日", -2 },
            { "大前天", -3 },
            { "明天", 1 }, { "明日", 1 },
            { "後天", 2 }
        };

        foreach (var pattern in relativeDatePatterns)
        {
            if (text.Contains(pattern.Key))
            {
                return new DateParseResult
                {
                    Date = DateTime.Today.AddDays(pattern.Value),
                    Confidence = 0.95
                };
            }
        }

        // 2. 絕對日期匹配 - 完整格式
        var fullDatePattern = @"(\d{4})\s*年\s*(\d{1,2})\s*月\s*(\d{1,2})\s*[日號]";
        var fullMatch = Regex.Match(text, fullDatePattern);
        if (fullMatch.Success)
        {
            if (DateTime.TryParse($"{fullMatch.Groups[1].Value}-{fullMatch.Groups[2].Value}-{fullMatch.Groups[3].Value}", out var fullDate))
            {
                return new DateParseResult
                {
                    Date = fullDate,
                    Confidence = 0.9
                };
            }
        }

        // 3. 月日格式
        var monthDayPattern = @"(\d{1,2})\s*月\s*(\d{1,2})\s*[日號]";
        var monthDayMatch = Regex.Match(text, monthDayPattern);
        if (monthDayMatch.Success)
        {
            var month = int.Parse(monthDayMatch.Groups[1].Value);
            var day = int.Parse(monthDayMatch.Groups[2].Value);
            var year = DateTime.Today.Year;
            
            // 如果日期已過，假設是明年
            var candidateDate = new DateTime(year, month, day);
            if (candidateDate < DateTime.Today)
            {
                candidateDate = candidateDate.AddYears(1);
            }

            return new DateParseResult
            {
                Date = candidateDate,
                Confidence = 0.7
            };
        }

        // 4. 中文數字日期解析
        var chineseDatePattern = @"(十二|十一|十|一|二|三|四|五|六|七|八|九)\s*月\s*(三十一|三十|二十九|二十八|二十七|二十六|二十五|二十四|二十三|二十二|二十一|二十|十九|十八|十七|十六|十五|十四|十三|十二|十一|十|九|八|七|六|五|四|三|二|一)\s*[日號]";
        var chineseMatch = Regex.Match(text, chineseDatePattern);
        if (chineseMatch.Success)
        {
            var month = ConvertChineseNumberToInt(chineseMatch.Groups[1].Value);
            var day = ConvertChineseNumberToInt(chineseMatch.Groups[2].Value);
            
            if (month >= 1 && month <= 12 && day >= 1 && day <= 31)
            {
                var year = DateTime.Today.Year;
                var candidateDate = new DateTime(year, month, day);
                if (candidateDate < DateTime.Today)
                {
                    candidateDate = candidateDate.AddYears(1);
                }

                return new DateParseResult
                {
                    Date = candidateDate,
                    Confidence = 0.6
                };
            }
        }

        return null;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "日期解析時發生錯誤");
        return null;
    }
}

/// <summary>
/// 中文數字轉換為阿拉伯數字
/// </summary>
private int ConvertChineseNumberToInt(string chineseNumber)
{
    var chineseToNumber = new Dictionary<string, int>
    {
        {"一", 1}, {"二", 2}, {"三", 3}, {"四", 4}, {"五", 5},
        {"六", 6}, {"七", 7}, {"八", 8}, {"九", 9}, {"十", 10},
        {"十一", 11}, {"十二", 12}, {"十三", 13}, {"十四", 14}, {"十五", 15},
        {"十六", 16}, {"十七", 17}, {"十八", 18}, {"十九", 19}, {"二十", 20},
        {"二十一", 21}, {"二十二", 22}, {"二十三", 23}, {"二十四", 24}, {"二十五", 25},
        {"二十六", 26}, {"二十七", 27}, {"二十八", 28}, {"二十九", 29}, {"三十", 30},
        {"三十一", 31}
    };

    return chineseToNumber.ContainsKey(chineseNumber) ? chineseToNumber[chineseNumber] : 0;
}
```

##### 付款方式解析方法
```csharp
/// <summary>
/// 付款方式解析結果
/// </summary>
private class PaymentMethodParseResult
{
    public string Method { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// 從文字中解析付款方式
/// </summary>
private PaymentMethodParseResult ParsePaymentMethodFromText(string text)
{
    try
    {
        // 付款方式關鍵字字典 (關鍵字 -> 標準名稱)
        var paymentKeywords = new Dictionary<string, string>
        {
            // 現金類
            {"現金", "現金"}, {"cash", "現金"}, {"付現", "現金"},
            
            // 信用卡類
            {"信用卡", "信用卡"}, {"刷卡", "信用卡"}, {"credit", "信用卡"},
            {"visa", "信用卡"}, {"master", "信用卡"}, {"jcb", "信用卡"},
            
            // 悠遊卡類
            {"悠遊卡", "悠遊卡"}, {"easycard", "悠遊卡"},
            
            // 一卡通
            {"一卡通", "一卡通"}, {"ipass", "一卡通"},
            
            // 電子支付
            {"line pay", "LINE Pay"}, {"linepay", "LINE Pay"},
            {"apple pay", "Apple Pay"}, {"applepay", "Apple Pay"},
            {"google pay", "Google Pay"}, {"googlepay", "Google Pay"},
            {"samsung pay", "Samsung Pay"}, {"samsungpay", "Samsung Pay"},
            
            // 行動支付
            {"街口", "街口支付"}, {"jkopay", "街口支付"},
            {"pi錢包", "Pi錢包"}, {"pi wallet", "Pi錢包"},
            {"歐付寶", "歐付寶"}, {"opay", "歐付寶"},
            
            // 銀行轉帳
            {"轉帳", "銀行轉帳"}, {"匯款", "銀行轉帳"}, {"atm", "銀行轉帳"},
            
            // 其他
            {"支票", "支票"}, {"禮券", "禮券"}, {"點數", "點數"}
        };

        // 先進行精確匹配
        foreach (var keyword in paymentKeywords)
        {
            if (text.Contains(keyword.Key, StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentMethodParseResult
                {
                    Method = keyword.Value,
                    Confidence = 0.9
                };
            }
        }

        // 模糊匹配 (編輯距離)
        if (_parseConfig.EnableFuzzyMatching)
        {
            foreach (var keyword in paymentKeywords)
            {
                if (CalculateEditDistance(text.ToLower(), keyword.Key.ToLower()) <= 2)
                {
                    return new PaymentMethodParseResult
                    {
                        Method = keyword.Value,
                        Confidence = 0.6
                    };
                }
            }
        }

        return new PaymentMethodParseResult
        {
            Method = string.Empty,
            Confidence = 0.0
        };
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "付款方式解析時發生錯誤");
        return new PaymentMethodParseResult { Method = string.Empty, Confidence = 0.0 };
    }
}
```

##### 商家名稱解析方法
```csharp
/// <summary>
/// 商家名稱解析結果
/// </summary>
private class MerchantNameParseResult
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// 從文字中解析商家名稱
/// </summary>
private MerchantNameParseResult ParseMerchantNameFromText(string text)
{
    try
    {
        // 常見商家名稱字典
        var merchantKeywords = new Dictionary<string, string>
        {
            // 便利商店
            {"7-11", "7-11"}, {"711", "7-11"}, {"seven", "7-11"},
            {"全家", "全家便利商店"}, {"family", "全家便利商店"},
            {"萊爾富", "萊爾富"}, {"hi-life", "萊爾富"},
            {"ok便利店", "OK便利店"}, {"ok", "OK便利店"},
            
            // 速食餐廳
            {"麥當勞", "麥當勞"}, {"mcdonald", "麥當勞"}, {"mc", "麥當勞"},
            {"肯德基", "肯德基"}, {"kfc", "肯德基"},
            {"摩斯", "摩斯漢堡"}, {"mos", "摩斯漢堡"},
            {"漢堡王", "漢堡王"}, {"burger king", "漢堡王"},
            
            // 咖啡店
            {"星巴克", "星巴克"}, {"starbucks", "星巴克"},
            {"85度c", "85度C"}, {"85c", "85度C"},
            {"路易莎", "路易莎咖啡"}, {"louisa", "路易莎咖啡"},
            {"cama", "CAMA咖啡"}, {"cama咖啡", "CAMA咖啡"},
            
            // 超市
            {"家樂福", "家樂福"}, {"carrefour", "家樂福"},
            {"大潤發", "大潤發"}, {"rt-mart", "大潤發"},
            {"好市多", "好市多"}, {"costco", "好市多"},
            {"全聯", "全聯福利中心"}, {"pxmart", "全聯福利中心"},
            
            // 百貨公司
            {"新光三越", "新光三越"}, {"遠百", "遠東百貨"}, {"sogo", "SOGO百貨"},
            {"統一時代", "統一時代百貨"}, {"京站", "京站時尚廣場"},
            
            // 加油站
            {"中油", "中國石油"}, {"台塑", "台塑石化"},
            {"殼牌", "Shell"}, {"shell", "Shell"}
        };

        // 位置介詞處理
        var locationPrepositions = new[] { "在", "去", "到", "從", "於" };
        var cleanText = text;
        
        foreach (var prep in locationPrepositions)
        {
            cleanText = cleanText.Replace(prep, " ");
        }

        // 精確匹配
        foreach (var merchant in merchantKeywords)
        {
            if (cleanText.Contains(merchant.Key, StringComparison.OrdinalIgnoreCase))
            {
                return new MerchantNameParseResult
                {
                    Name = merchant.Value,
                    Confidence = 0.9
                };
            }
        }

        // 模糊匹配
        if (_parseConfig.EnableFuzzyMatching)
        {
            foreach (var merchant in merchantKeywords)
            {
                if (CalculateEditDistance(cleanText.ToLower(), merchant.Key.ToLower()) <= 1)
                {
                    return new MerchantNameParseResult
                    {
                        Name = merchant.Value,
                        Confidence = 0.7
                    };
                }
            }
        }

        return new MerchantNameParseResult
        {
            Name = string.Empty,
            Confidence = 0.0
        };
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "商家名稱解析時發生錯誤");
        return new MerchantNameParseResult { Name = string.Empty, Confidence = 0.0 };
    }
}
```

### 3. 前端UI更新 (index8.cshtml)

#### 3.1 語音解析結果預覽區域增強
```html
<!-- 語音解析結果預覽 (Phase 1 增強版) -->
<div id="parsedPreview" class="mt-3 d-none">
    <div class="card border-primary">
        <div class="card-header bg-light">
            <h6 class="mb-0">
                <i class="fas fa-eye text-primary"></i> 語音解析結果預覽
                <small class="text-muted">- 請確認資訊是否正確</small>
            </h6>
        </div>
        <div class="card-body">
            <!-- 整體信心度指示器 -->
            <div class="mb-3">
                <label class="form-label">整體解析信心度</label>
                <div class="progress" style="height: 8px;">
                    <div id="overallConfidenceBar" class="progress-bar" role="progressbar" 
                         style="width: 0%" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100">
                    </div>
                </div>
                <small id="overallConfidenceText" class="text-muted">0%</small>
            </div>

            <!-- 解析結果表格 -->
            <div class="table-responsive">
                <table class="table table-sm">
                    <thead class="table-light">
                        <tr>
                            <th>欄位</th>
                            <th>解析結果</th>
                            <th>信心度</th>
                            <th>狀態</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr id="previewDate">
                            <td><i class="fas fa-calendar"></i> 日期</td>
                            <td id="previewDateValue">-</td>
                            <td><span id="previewDateConfidence" class="badge">-</span></td>
                            <td><span id="previewDateStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewType">
                            <td><i class="fas fa-exchange-alt"></i> 收支類型</td>
                            <td id="previewTypeValue">-</td>
                            <td><span id="previewTypeConfidence" class="badge">-</span></td>
                            <td><span id="previewTypeStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewAmount">
                            <td><i class="fas fa-coins"></i> 金額</td>
                            <td id="previewAmountValue">-</td>
                            <td><span id="previewAmountConfidence" class="badge">-</span></td>
                            <td><span id="previewAmountStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewPaymentMethod">
                            <td><i class="fas fa-credit-card"></i> 付款方式</td>
                            <td id="previewPaymentMethodValue">-</td>
                            <td><span id="previewPaymentMethodConfidence" class="badge">-</span></td>
                            <td><span id="previewPaymentMethodStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewCategory">
                            <td><i class="fas fa-folder"></i> 大分類</td>
                            <td id="previewCategoryValue">-</td>
                            <td><span id="previewCategoryConfidence" class="badge">-</span></td>
                            <td><span id="previewCategoryStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewSubCategory">
                            <td><i class="fas fa-tag"></i> 細分類</td>
                            <td id="previewSubCategoryValue">-</td>
                            <td><span id="previewSubCategoryConfidence" class="badge">-</span></td>
                            <td><span id="previewSubCategoryStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewDescription">
                            <td><i class="fas fa-edit"></i> 交易描述</td>
                            <td id="previewDescriptionValue">-</td>
                            <td><span id="previewDescriptionConfidence" class="badge">-</span></td>
                            <td><span id="previewDescriptionStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewMerchant">
                            <td><i class="fas fa-store"></i> 商家名稱</td>
                            <td id="previewMerchantValue">-</td>
                            <td><span id="previewMerchantConfidence" class="badge">-</span></td>
                            <td><span id="previewMerchantStatus" class="badge">-</span></td>
                        </tr>
                        <tr id="previewNote">
                            <td><i class="fas fa-sticky-note"></i> 備註</td>
                            <td id="previewNoteValue">-</td>
                            <td><span id="previewNoteConfidence" class="badge">-</span></td>
                            <td><span id="previewNoteStatus" class="badge">-</span></td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- 未解析內容提示 -->
            <div id="unparsedContentAlert" class="alert alert-warning d-none">
                <i class="fas fa-exclamation-triangle"></i>
                <strong>未能解析的內容:</strong>
                <span id="unparsedContentText"></span>
                <br><small>這些內容可能需要手動填入或調整語音表達方式</small>
            </div>

            <!-- 操作按鈕 -->
            <div class="d-flex gap-2 mt-3">
                <button type="button" class="btn btn-success" id="applyParseResultBtn">
                    <i class="fas fa-check"></i> 套用到表單
                </button>
                <button type="button" class="btn btn-warning" id="editParseResultBtn">
                    <i class="fas fa-edit"></i> 手動調整
                </button>
                <button type="button" class="btn btn-secondary" id="retryParseBtn">
                    <i class="fas fa-redo"></i> 重新解析
                </button>
            </div>
        </div>
    </div>
</div>
```

#### 3.2 JavaScript 更新 (voice-input.js 擴充)

```javascript
/**
 * 顯示 Phase 1 增強版解析結果
 */
displayParseResult(parseResult) {
    const previewDiv = document.getElementById('parsedPreview');
    
    if (!parseResult || !parseResult.IsSuccess) {
        previewDiv.classList.add('d-none');
        return;
    }

    // 更新整體信心度
    this.updateOverallConfidence(parseResult.ParseConfidence);

    // 更新各欄位解析結果
    this.updateFieldResult('Date', parseResult.Date, parseResult.FieldConfidence?.Date || 0);
    this.updateFieldResult('Type', parseResult.Type, parseResult.FieldConfidence?.Type || 0);
    this.updateFieldResult('Amount', parseResult.Amount ? `NT$ ${parseResult.Amount}` : null, parseResult.FieldConfidence?.Amount || 0);
    this.updateFieldResult('PaymentMethod', parseResult.PaymentMethod, parseResult.FieldConfidence?.PaymentMethod || 0);
    this.updateFieldResult('Category', parseResult.Category, parseResult.FieldConfidence?.Category || 0);
    this.updateFieldResult('SubCategory', parseResult.SubCategory, parseResult.FieldConfidence?.SubCategory || 0);
    this.updateFieldResult('Description', parseResult.Description, parseResult.FieldConfidence?.Description || 0);
    this.updateFieldResult('Merchant', parseResult.MerchantName, parseResult.FieldConfidence?.MerchantName || 0);
    this.updateFieldResult('Note', parseResult.Note, parseResult.FieldConfidence?.Note || 0);

    // 處理未解析內容
    this.showUnparsedContent(parseResult.UnparsedContent);

    // 顯示預覽區域
    previewDiv.classList.remove('d-none');

    // 綁定按鈕事件
    this.bindPreviewButtons(parseResult);
}

/**
 * 更新整體信心度顯示
 */
updateOverallConfidence(confidence) {
    const percentage = Math.round(confidence * 100);
    const progressBar = document.getElementById('overallConfidenceBar');
    const progressText = document.getElementById('overallConfidenceText');
    
    progressBar.style.width = `${percentage}%`;
    progressBar.setAttribute('aria-valuenow', percentage);
    progressText.textContent = `${percentage}%`;
    
    // 根據信心度設定顏色
    progressBar.className = 'progress-bar';
    if (percentage >= 80) {
        progressBar.classList.add('bg-success');
    } else if (percentage >= 60) {
        progressBar.classList.add('bg-warning');
    } else {
        progressBar.classList.add('bg-danger');
    }
}

/**
 * 更新單一欄位結果顯示
 */
updateFieldResult(fieldName, value, confidence) {
    const valueElement = document.getElementById(`preview${fieldName}Value`);
    const confidenceElement = document.getElementById(`preview${fieldName}Confidence`);
    const statusElement = document.getElementById(`preview${fieldName}Status`);
    
    if (value) {
        valueElement.textContent = value;
        
        const percentage = Math.round(confidence * 100);
        confidenceElement.textContent = `${percentage}%`;
        confidenceElement.className = 'badge';
        
        if (percentage >= 70) {
            confidenceElement.classList.add('bg-success');
            statusElement.textContent = '已解析';
            statusElement.className = 'badge bg-success';
        } else if (percentage >= 40) {
            confidenceElement.classList.add('bg-warning');
            statusElement.textContent = '低信心度';
            statusElement.className = 'badge bg-warning';
        } else {
            confidenceElement.classList.add('bg-danger');
            statusElement.textContent = '需確認';
            statusElement.className = 'badge bg-danger';
        }
    } else {
        valueElement.textContent = '-';
        confidenceElement.textContent = '-';
        confidenceElement.className = 'badge bg-secondary';
        statusElement.textContent = '未解析';
        statusElement.className = 'badge bg-secondary';
    }
}

/**
 * 套用解析結果到表單
 */
applyParseResultToForm(parseResult) {
    try {
        // 日期
        if (parseResult.Date) {
            const dateInput = document.querySelector('input[name="Record.Date"]');
            if (dateInput) {
                dateInput.value = parseResult.Date.split('T')[0]; // 只取日期部分
            }
        }

        // 收支類型
        if (parseResult.Type) {
            const typeRadio = document.querySelector(`input[name="Record.Type"][value="${parseResult.Type}"]`);
            if (typeRadio) {
                typeRadio.checked = true;
                typeRadio.dispatchEvent(new Event('change')); // 觸發變更事件
            }
        }

        // 金額
        if (parseResult.Amount) {
            const amountInput = document.querySelector('.money-input');
            if (amountInput) {
                amountInput.value = parseResult.Amount;
                updateAmountDisplay(); // 更新金額顯示
            }
        }

        // 付款方式
        if (parseResult.PaymentMethod) {
            const paymentSelect = document.querySelector('select[name="Record.PaymentMethod"]');
            if (paymentSelect) {
                // 尋找匹配的選項
                const options = Array.from(paymentSelect.options);
                const matchingOption = options.find(opt => 
                    opt.text.includes(parseResult.PaymentMethod) || 
                    opt.value === parseResult.PaymentMethod
                );
                if (matchingOption) {
                    paymentSelect.value = matchingOption.value;
                }
            }
        }

        // 大分類
        if (parseResult.Category) {
            const categorySelect = document.getElementById('categorySelect');
            if (categorySelect) {
                const options = Array.from(categorySelect.options);
                const matchingOption = options.find(opt => 
                    opt.text.includes(parseResult.Category) || 
                    opt.value === parseResult.Category
                );
                if (matchingOption) {
                    categorySelect.value = matchingOption.value;
                    categorySelect.dispatchEvent(new Event('change')); // 觸發變更以載入子分類
                }
            }
        }

        // 細分類 (需要等大分類載入完成)
        if (parseResult.SubCategory) {
            setTimeout(() => {
                const subCategorySelect = document.getElementById('subCategorySelect');
                if (subCategorySelect) {
                    const options = Array.from(subCategorySelect.options);
                    const matchingOption = options.find(opt => 
                        opt.text.includes(parseResult.SubCategory) || 
                        opt.value === parseResult.SubCategory
                    );
                    if (matchingOption) {
                        subCategorySelect.value = matchingOption.value;
                    }
                }
            }, 500); // 延遲以等待子分類載入
        }

        // 交易描述
        if (parseResult.Description) {
            const descriptionInput = document.getElementById('description');
            if (descriptionInput) {
                descriptionInput.value = parseResult.Description;
            }
        }

        // 商家名稱
        if (parseResult.MerchantName) {
            const merchantInput = document.getElementById('merchant');
            if (merchantInput) {
                merchantInput.value = parseResult.MerchantName;
            }
        }

        // 備註
        if (parseResult.Note) {
            const noteTextarea = document.querySelector('textarea[name="Record.Note"]');
            if (noteTextarea) {
                noteTextarea.value = parseResult.Note;
                updateNoteCount(); // 更新字數統計
            }
        }

        // 隱藏預覽區域
        document.getElementById('parsedPreview').classList.add('d-none');
        
        // 顯示成功訊息
        this.showSuccess('語音解析結果已套用到表單！請檢查並確認資訊是否正確。');

    } catch (error) {
        console.error('套用解析結果時發生錯誤:', error);
        this.showError('套用解析結果時發生錯誤，請手動填入資訊');
    }
}
```

## 測試規劃

### 單元測試案例
1. **日期解析測試**
   - 相對日期：今天、昨天、前天
   - 絕對日期：2023年10月1日、10月1日
   - 中文數字：十月一日
   - 邊界案例：無效日期、未來日期

2. **付款方式測試**
   - 精確匹配：現金、信用卡、LINE Pay
   - 模糊匹配：linepay、刷卡
   - 大小寫不敏感：CASH、Credit Card

3. **商家名稱測試**
   - 常見商家：星巴克、7-11、麥當勞
   - 介詞處理：在星巴克、去7-11
   - 模糊匹配：starbucks、mc

### 整合測試案例
```
測試案例 1: "我昨天在星巴克用信用卡花了150元買咖啡"
預期結果:
- Date: 昨天日期
- Type: Expense  
- Amount: 150
- PaymentMethod: 信用卡
- MerchantName: 星巴克
- Category: 餐飲美食 (自動推斷)
- Description: 咖啡

測試案例 2: "10月1日收入3000元薪水"
預期結果:
- Date: 今年10月1日
- Type: Income
- Amount: 3000
- Category: 薪資收入 (自動推斷)
- Description: 薪水
```

## 交付標準

### 程式碼品質
- [ ] 所有新增方法都有完整的XML文件註解
- [ ] 異常處理覆蓋所有解析方法
- [ ] 單元測試覆蓋率 > 80%
- [ ] 效能測試：解析時間 < 2秒

### 功能完整性
- [ ] VoiceParseResult 模型完整實作
- [ ] 9個欄位解析方法完成
- [ ] 前端UI完整更新
- [ ] 向下相容性確保

### 使用者體驗
- [ ] 解析結果清晰展示
- [ ] 信心度視覺化呈現
- [ ] 錯誤訊息友善提示
- [ ] 操作流程直覺簡單

## 後續階段準備
Phase 1 完成後，將為 Phase 2 (使用者體驗優化) 奠定基礎，包括：
- 分欄位信心度機制
- 漸進式解析流程
- 用戶確認互動設計

---

**Phase 1 開發時程**: 2週  
**負責範圍**: 核心解析功能建立  
**成功標準**: 基本解析準確度 60%+，支援完整欄位解析
