using Demo.Models;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 股價資料服務
/// </summary>
public class StockPriceService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly Dictionary<string, StockPrice> _priceCache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public StockPriceService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["StockApi:ApiKey"] ?? "demo"; // 從設定檔讀取 API Key
    }

    /// <summary>
    /// 取得多支股票的即時報價
    /// </summary>
    public async Task<Dictionary<string, StockPrice>> GetStockPricesAsync(List<string> symbols)
    {
        var prices = new Dictionary<string, StockPrice>();
        
        foreach (var symbol in symbols)
        {
            try
            {
                var price = await GetSingleStockPriceAsync(symbol);
                if (price != null)
                {
                    prices[symbol] = price;
                    
                    // 更新快取
                    _priceCache[symbol] = price;
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤但繼續處理其他股票
                Console.WriteLine($"獲取 {symbol} 股價失敗: {ex.Message}");
                
                // 嘗試從快取取得資料
                if (_priceCache.ContainsKey(symbol) && 
                    DateTime.Now - _priceCache[symbol].LastUpdated < _cacheExpiry)
                {
                    prices[symbol] = _priceCache[symbol];
                }
            }
        }
        
        return prices;
    }

    /// <summary>
    /// 取得單一股票的即時報價
    /// </summary>
    public async Task<StockPrice?> GetSingleStockPriceAsync(string symbol)
    {
        // 先檢查快取
        if (_priceCache.ContainsKey(symbol) && 
            DateTime.Now - _priceCache[symbol].LastUpdated < _cacheExpiry)
        {
            return _priceCache[symbol];
        }

        try
        {
            // 根據股票代號判斷使用哪個 API
            if (symbol.Contains(".TW") || symbol.Contains(".TWO"))
            {
                return await GetTaiwanStockPriceAsync(symbol);
            }
            else
            {
                return await GetUSStockPriceAsync(symbol);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取股價失敗 {symbol}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 搜尋股票代號和名稱
    /// </summary>
    public async Task<List<StockSearchResult>> SearchStocksAsync(string query)
    {
        var results = new List<StockSearchResult>();

        try
        {
            // 這裡可以整合各種股票查詢 API
            // 暫時使用模擬資料
            await Task.Delay(100); // 模擬 API 呼叫延遲
            
            if (query.Length >= 2)
            {
                // 台股常見股票模擬資料
                var taiwanStocks = new[]
                {
                    new StockSearchResult { Symbol = "2330.TW", Name = "台積電", Exchange = "TSE", Currency = "TWD" },
                    new StockSearchResult { Symbol = "2317.TW", Name = "鴻海", Exchange = "TSE", Currency = "TWD" },
                    new StockSearchResult { Symbol = "2454.TW", Name = "聯發科", Exchange = "TSE", Currency = "TWD" },
                    new StockSearchResult { Symbol = "2881.TW", Name = "富邦金", Exchange = "TSE", Currency = "TWD" },
                    new StockSearchResult { Symbol = "2412.TW", Name = "中華電", Exchange = "TSE", Currency = "TWD" }
                };

                // 美股常見股票模擬資料
                var usStocks = new[]
                {
                    new StockSearchResult { Symbol = "AAPL", Name = "Apple Inc.", Exchange = "NASDAQ", Currency = "USD" },
                    new StockSearchResult { Symbol = "MSFT", Name = "Microsoft Corporation", Exchange = "NASDAQ", Currency = "USD" },
                    new StockSearchResult { Symbol = "GOOGL", Name = "Alphabet Inc.", Exchange = "NASDAQ", Currency = "USD" },
                    new StockSearchResult { Symbol = "TSLA", Name = "Tesla, Inc.", Exchange = "NASDAQ", Currency = "USD" },
                    new StockSearchResult { Symbol = "AMZN", Name = "Amazon.com, Inc.", Exchange = "NASDAQ", Currency = "USD" }
                };

                var allStocks = taiwanStocks.Concat(usStocks);
                
                results = allStocks
                    .Where(s => s.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                               s.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"搜尋股票失敗: {ex.Message}");
        }

        return results;
    }

    #region 私有方法

    /// <summary>
    /// 獲取台股報價
    /// </summary>
    private async Task<StockPrice?> GetTaiwanStockPriceAsync(string symbol)
    {
        try
        {
            // 這裡可以整合台股 API (如證交所 API、Yahoo Finance 等)
            // 暫時使用模擬資料
            await Task.Delay(200); // 模擬 API 呼叫延遲
            
            var random = new Random();
            var basePrice = GetBasePriceForSymbol(symbol);
            var change = (decimal)(random.NextDouble() - 0.5) * 10;
            var currentPrice = basePrice + change;
            
            return new StockPrice
            {
                Symbol = symbol,
                Price = Math.Round(currentPrice, 2),
                Change = Math.Round(change, 2),
                ChangePercent = basePrice > 0 ? $"{Math.Round((change / basePrice) * 100, 2):+#.##;-#.##;0}%" : "0%",
                Volume = random.Next(1000, 50000),
                LastUpdated = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取台股報價失敗 {symbol}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 獲取美股報價
    /// </summary>
    private async Task<StockPrice?> GetUSStockPriceAsync(string symbol)
    {
        try
        {
            // 可以整合 Alpha Vantage、Yahoo Finance 等 API
            // 暫時使用模擬資料
            await Task.Delay(200); // 模擬 API 呼叫延遲
            
            var random = new Random();
            var basePrice = GetBasePriceForSymbol(symbol);
            var change = (decimal)(random.NextDouble() - 0.5) * 20;
            var currentPrice = basePrice + change;
            
            return new StockPrice
            {
                Symbol = symbol,
                Price = Math.Round(currentPrice, 2),
                Change = Math.Round(change, 2),
                ChangePercent = basePrice > 0 ? $"{Math.Round((change / basePrice) * 100, 2):+#.##;-#.##;0}%" : "0%",
                Volume = random.Next(10000, 1000000),
                LastUpdated = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"獲取美股報價失敗 {symbol}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Alpha Vantage API 實作範例 (需要真實 API Key)
    /// </summary>
    private async Task<StockPrice?> GetAlphaVantageStockPriceAsync(string symbol)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "demo")
        {
            return null; // 沒有有效的 API Key
        }

        try
        {
            var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";
            
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                
                if (document.RootElement.TryGetProperty("Global Quote", out var quoteElement))
                {
                    return new StockPrice
                    {
                        Symbol = symbol,
                        Price = decimal.Parse(quoteElement.GetProperty("05. price").GetString() ?? "0"),
                        Change = decimal.Parse(quoteElement.GetProperty("09. change").GetString() ?? "0"),
                        ChangePercent = quoteElement.GetProperty("10. change percent").GetString() ?? "0%",
                        LastUpdated = DateTime.Now
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alpha Vantage API 錯誤: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// 根據股票代號取得基準價格 (用於模擬)
    /// </summary>
    private decimal GetBasePriceForSymbol(string symbol)
    {
        return symbol.ToUpper() switch
        {
            "2330.TW" => 520m,  // 台積電
            "2317.TW" => 105m,  // 鴻海
            "2454.TW" => 800m,  // 聯發科
            "2881.TW" => 65m,   // 富邦金
            "2412.TW" => 120m,  // 中華電
            "AAPL" => 175m,     // Apple
            "MSFT" => 350m,     // Microsoft
            "GOOGL" => 140m,    // Google
            "TSLA" => 250m,     // Tesla
            "AMZN" => 130m,     // Amazon
            _ => 100m           // 預設價格
        };
    }

    #endregion
}
