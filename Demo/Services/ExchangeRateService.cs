using System.Text.Json;
using Demo.Models;

namespace Demo.Services
{
    /// <summary>
    /// 匯率資料服務
    /// </summary>
    public class ExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly string _dataFilePath;
        private const string TaiwanBankCsvUrl = "https://rate.bot.com.tw/xrt/flcsv/0/day";

        public ExchangeRateService(HttpClient httpClient, ILogger<ExchangeRateService> logger, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dataFilePath = Path.Combine(env.ContentRootPath, "App_Data", "exchange_rates.json");
            
            // 設定 HTTP 用戶端
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Exchange Rate Calculator/1.0");
        }

        /// <summary>
        /// 從台銀 CSV API 獲取最新匯率資料
        /// </summary>
        public async Task<ExchangeRateData?> FetchExchangeRatesAsync()
        {
            try
            {
                _logger.LogInformation("開始從台銀 CSV API 獲取匯率資料");
                
                var response = await _httpClient.GetAsync(TaiwanBankCsvUrl);
                response.EnsureSuccessStatusCode();
                
                var csvContent = await response.Content.ReadAsStringAsync();
                var exchangeRateData = ParseCsvData(csvContent);
                
                // 儲存到本地檔案
                await SaveExchangeRatesAsync(exchangeRateData);
                
                _logger.LogInformation("成功獲取並儲存匯率資料，共 {Count} 筆", exchangeRateData.Rates.Count);
                return exchangeRateData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "無法從台銀 API 獲取匯率資料");
                
                // 嘗試讀取本地快取資料
                return await LoadExchangeRatesAsync();
            }
        }

        /// <summary>
        /// 解析 CSV 格式的匯率資料
        /// </summary>
        private ExchangeRateData ParseCsvData(string csvContent)
        {
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var rates = new List<ExchangeRate>();

            // 跳過標題行，從第二行開始解析
            foreach (var line in lines.Skip(1))
            {
                try
                {
                    var fields = line.Split(',');
                    if (fields.Length >= 5)
                    {
                        var rate = new ExchangeRate
                        {
                            CurrencyCode = fields[0].Trim(),
                            CurrencyName = GetCurrencyName(fields[0].Trim()),
                            CashBuyRate = ParseDecimal(fields[1]),
                            CashSellRate = ParseDecimal(fields[2]),
                            BuyRate = ParseDecimal(fields[3]),
                            SellRate = ParseDecimal(fields[4])
                        };

                        // 只加入我們支援的貨幣
                        if (IsSupportedCurrency(rate.CurrencyCode))
                        {
                            rates.Add(rate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析 CSV 行時發生錯誤: {Line}", line);
                }
            }

            return new ExchangeRateData
            {
                Rates = rates,
                LastUpdated = DateTime.Now,
                Source = "台灣銀行CSV API"
            };
        }

        /// <summary>
        /// 安全解析 decimal 值
        /// </summary>
        private decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value?.Trim(), out var result))
            {
                return Math.Round(result, 6); // 保留小數點後6位
            }
            return 0m;
        }

        /// <summary>
        /// 檢查是否為支援的貨幣
        /// </summary>
        private bool IsSupportedCurrency(string currencyCode)
        {
            var supportedCurrencies = new[] { "USD", "JPY", "CNY", "EUR", "GBP", "HKD", "AUD" };
            return supportedCurrencies.Contains(currencyCode?.ToUpper());
        }

        /// <summary>
        /// 取得貨幣中文名稱
        /// </summary>
        private string GetCurrencyName(string currencyCode)
        {
            return currencyCode?.ToUpper() switch
            {
                "USD" => "美金",
                "JPY" => "日圓",
                "CNY" => "人民幣",
                "EUR" => "歐元",
                "GBP" => "英鎊",
                "HKD" => "港幣",
                "AUD" => "澳幣",
                _ => currencyCode ?? ""
            };
        }

        /// <summary>
        /// 儲存匯率資料到本地檔案
        /// </summary>
        public async Task SaveExchangeRatesAsync(ExchangeRateData data)
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                await File.WriteAllTextAsync(_dataFilePath, json);
                _logger.LogInformation("匯率資料已儲存到: {FilePath}", _dataFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "儲存匯率資料時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 從本地檔案載入匯率資料
        /// </summary>
        public async Task<ExchangeRateData?> LoadExchangeRatesAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    _logger.LogWarning("匯率資料檔案不存在: {FilePath}", _dataFilePath);
                    return null;
                }

                var json = await File.ReadAllTextAsync(_dataFilePath);
                var data = JsonSerializer.Deserialize<ExchangeRateData>(json, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                _logger.LogInformation("成功載入本地匯率資料");
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入匯率資料時發生錯誤");
                return null;
            }
        }

        /// <summary>
        /// 計算匯率兌換
        /// </summary>
        public async Task<ExchangeCalculationResult> CalculateExchangeAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            try
            {
                var data = await LoadExchangeRatesAsync();
                if (data == null || !data.Rates.Any())
                {
                    return new ExchangeCalculationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "無匯率資料可用，請先更新匯率"
                    };
                }

                // 台幣轉外幣
                if (fromCurrency.ToUpper() == "TWD")
                {
                    var rate = data.Rates.FirstOrDefault(r => r.CurrencyCode.ToUpper() == toCurrency.ToUpper());
                    if (rate == null)
                    {
                        return new ExchangeCalculationResult
                        {
                            IsSuccess = false,
                            ErrorMessage = $"找不到 {toCurrency} 的匯率資料"
                        };
                    }

                    var exchangeRate = rate.CashSellRate; // 使用現金賣出匯率
                    var result = Math.Round(amount / exchangeRate, 6);

                    return new ExchangeCalculationResult
                    {
                        Amount = result,
                        ExchangeRate = exchangeRate,
                        FromCurrency = fromCurrency,
                        ToCurrency = toCurrency,
                        IsSuccess = true
                    };
                }
                // 外幣轉台幣
                else if (toCurrency.ToUpper() == "TWD")
                {
                    var rate = data.Rates.FirstOrDefault(r => r.CurrencyCode.ToUpper() == fromCurrency.ToUpper());
                    if (rate == null)
                    {
                        return new ExchangeCalculationResult
                        {
                            IsSuccess = false,
                            ErrorMessage = $"找不到 {fromCurrency} 的匯率資料"
                        };
                    }

                    var exchangeRate = rate.CashBuyRate; // 使用現金買入匯率
                    var result = Math.Round(amount * exchangeRate, 6);

                    return new ExchangeCalculationResult
                    {
                        Amount = result,
                        ExchangeRate = exchangeRate,
                        FromCurrency = fromCurrency,
                        ToCurrency = toCurrency,
                        IsSuccess = true
                    };
                }
                else
                {
                    return new ExchangeCalculationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "目前僅支援台幣與外幣間的兌換"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算匯率時發生錯誤");
                return new ExchangeCalculationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "計算過程發生錯誤，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 檢查匯率資料是否過期（超過24小時）
        /// </summary>
        public async Task<bool> IsRateDataStaleAsync()
        {
            var data = await LoadExchangeRatesAsync();
            if (data == null) return true;

            return DateTime.Now.Subtract(data.LastUpdated).TotalHours > 24;
        }
    }
}
