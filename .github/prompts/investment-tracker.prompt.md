# 投資追蹤器功能開發規格書

## 專案概述
開發一個綜合性投資追蹤器，允許使用者記錄和追蹤股票、基金、債券等各種投資工具的表現。此功能將擴展現有的財務管理系統，提供完整的個人投資組合管理解決方案。

## 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, Chart.js, jQuery, HTML5, CSS3
- **圖表函式庫**: Chart.js 4.0+ (用於投資數據視覺化)
- **外部 API**: Yahoo Finance API / Alpha Vantage API (股價即時數據)
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 資料結構設計

### 投資組合 (Portfolio)
```json
{
  "portfolios": [
    {
      "id": 1,
      "name": "主要投資組合",
      "description": "長期投資組合",
      "createdAt": "2024-01-01T00:00:00Z",
      "totalValue": 0,
      "totalCost": 0,
      "totalGainLoss": 0,
      "totalGainLossPercentage": 0
    }
  ]
}
```

### 投資持倉 (Holdings)
```json
{
  "holdings": [
    {
      "id": 1,
      "portfolioId": 1,
      "symbol": "2330.TW",
      "name": "台積電",
      "type": "股票",
      "quantity": 100,
      "averageCost": 500.00,
      "currentPrice": 520.00,
      "marketValue": 52000.00,
      "gainLoss": 2000.00,
      "gainLossPercentage": 4.00,
      "lastUpdated": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 交易記錄 (Transactions)
```json
{
  "transactions": [
    {
      "id": 1,
      "portfolioId": 1,
      "symbol": "2330.TW",
      "type": "買入",
      "quantity": 100,
      "price": 500.00,
      "totalAmount": 50000.00,
      "fee": 150.00,
      "date": "2024-01-01",
      "note": "長期投資",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

## 核心功能模組

### 1. 投資組合總覽頁面
- **前端**: `#file:investment-portfolio.cshtml`
- **後端**: `#file:investment-portfolio.cshtml.cs`
- **路由**: `/investment-portfolio`

### 1.1 功能描述
- **組合列表**: 顯示所有投資組合
- **總覽統計**: 總資產、總損益、投資報酬率
- **圓餅圖**: 資產配置視覺化
- **趨勢圖**: 投資組合價值變化趨勢

### 1.2 前端實作 (investment-portfolio.cshtml)
```html
<div class="container mt-4">
    <!-- 頂部統計卡片 -->
    <div class="row mb-4">
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card text-white bg-primary">
                <div class="card-body">
                    <div class="d-flex justify-content-between">
                        <div>
                            <h4 class="card-title">總資產</h4>
                            <h2 id="totalAssets">$0</h2>
                        </div>
                        <div class="align-self-center">
                            <i class="fas fa-chart-line fa-2x"></i>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card text-white bg-success">
                <div class="card-body">
                    <div class="d-flex justify-content-between">
                        <div>
                            <h4 class="card-title">總損益</h4>
                            <h2 id="totalGainLoss">$0</h2>
                        </div>
                        <div class="align-self-center">
                            <i class="fas fa-dollar-sign fa-2x"></i>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card text-white bg-info">
                <div class="card-body">
                    <div class="d-flex justify-content-between">
                        <div>
                            <h4 class="card-title">報酬率</h4>
                            <h2 id="totalReturn">0%</h2>
                        </div>
                        <div class="align-self-center">
                            <i class="fas fa-percentage fa-2x"></i>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card text-white bg-warning">
                <div class="card-body">
                    <div class="d-flex justify-content-between">
                        <div>
                            <h4 class="card-title">持倉數量</h4>
                            <h2 id="totalHoldings">0</h2>
                        </div>
                        <div class="align-self-center">
                            <i class="fas fa-briefcase fa-2x"></i>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 圖表區域 -->
    <div class="row mb-4">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-header">
                    <h5><i class="fas fa-chart-area me-2"></i>投資組合價值趨勢</h5>
                </div>
                <div class="card-body">
                    <canvas id="portfolioTrendChart"></canvas>
                </div>
            </div>
        </div>
        <div class="col-lg-4">
            <div class="card">
                <div class="card-header">
                    <h5><i class="fas fa-chart-pie me-2"></i>資產配置</h5>
                </div>
                <div class="card-body">
                    <canvas id="assetAllocationChart"></canvas>
                </div>
            </div>
        </div>
    </div>

    <!-- 投資組合列表 -->
    <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <h5><i class="fas fa-folder me-2"></i>投資組合</h5>
            <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addPortfolioModal">
                <i class="fas fa-plus"></i> 新增組合
            </button>
        </div>
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-hover" id="portfolioTable">
                    <thead>
                        <tr>
                            <th>名稱</th>
                            <th>總價值</th>
                            <th>總成本</th>
                            <th>損益</th>
                            <th>報酬率</th>
                            <th>更新時間</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- 動態載入投資組合 -->
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<!-- 新增投資組合模態框 -->
<div class="modal fade" id="addPortfolioModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">新增投資組合</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="addPortfolioForm">
                    <div class="mb-3">
                        <label class="form-label">組合名稱</label>
                        <input type="text" class="form-control" id="portfolioName" required>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">描述</label>
                        <textarea class="form-control" id="portfolioDescription" rows="3"></textarea>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" id="savePortfolio">儲存</button>
            </div>
        </div>
    </div>
</div>
```

### 2. 持倉管理頁面
- **前端**: `#file:investment-holdings.cshtml`
- **後端**: `#file:investment-holdings.cshtml.cs`
- **路由**: `/investment-holdings/{portfolioId?}`

### 2.1 前端實作 (investment-holdings.cshtml)
```html
<div class="container mt-4">
    <!-- 搜尋和篩選 -->
    <div class="card mb-4">
        <div class="card-body">
            <div class="row">
                <div class="col-md-4">
                    <div class="input-group">
                        <span class="input-group-text"><i class="fas fa-search"></i></span>
                        <input type="text" class="form-control" id="searchSymbol" placeholder="搜尋股票代號或名稱">
                        <button class="btn btn-primary" id="searchBtn">搜尋</button>
                    </div>
                </div>
                <div class="col-md-3">
                    <select class="form-select" id="typeFilter">
                        <option value="">全部類型</option>
                        <option value="股票">股票</option>
                        <option value="ETF">ETF</option>
                        <option value="基金">基金</option>
                        <option value="債券">債券</option>
                    </select>
                </div>
                <div class="col-md-3">
                    <select class="form-select" id="portfolioFilter">
                        <option value="">全部組合</option>
                        <!-- 動態載入組合 -->
                    </select>
                </div>
                <div class="col-md-2">
                    <button class="btn btn-success w-100" data-bs-toggle="modal" data-bs-target="#addHoldingModal">
                        <i class="fas fa-plus"></i> 新增持倉
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- 持倉列表 -->
    <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <h5><i class="fas fa-chart-bar me-2"></i>持倉明細</h5>
            <div class="btn-group">
                <button class="btn btn-outline-primary btn-sm" id="refreshPricesBtn">
                    <i class="fas fa-sync-alt"></i> 更新股價
                </button>
                <button class="btn btn-outline-success btn-sm" id="exportBtn">
                    <i class="fas fa-download"></i> 匯出
                </button>
            </div>
        </div>
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-hover" id="holdingsTable">
                    <thead>
                        <tr>
                            <th>股票代號</th>
                            <th>名稱</th>
                            <th>類型</th>
                            <th>持股數量</th>
                            <th>平均成本</th>
                            <th>目前股價</th>
                            <th>市值</th>
                            <th>損益</th>
                            <th>報酬率</th>
                            <th>更新時間</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- 動態載入持倉 -->
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<!-- 新增持倉模態框 -->
<div class="modal fade" id="addHoldingModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">新增持倉</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="addHoldingForm">
                    <div class="row">
                        <div class="col-md-6">
                            <label class="form-label">投資組合</label>
                            <select class="form-select" id="holdingPortfolioId" required>
                                <!-- 動態載入 -->
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">投資類型</label>
                            <select class="form-select" id="holdingType" required>
                                <option value="股票">股票</option>
                                <option value="ETF">ETF</option>
                                <option value="基金">基金</option>
                                <option value="債券">債券</option>
                            </select>
                        </div>
                    </div>
                    <div class="row mt-3">
                        <div class="col-md-6">
                            <label class="form-label">股票代號</label>
                            <div class="input-group">
                                <input type="text" class="form-control" id="holdingSymbol" required>
                                <button type="button" class="btn btn-outline-primary" id="lookupSymbol">
                                    <i class="fas fa-search"></i>
                                </button>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">名稱</label>
                            <input type="text" class="form-control" id="holdingName" required>
                        </div>
                    </div>
                    <div class="row mt-3">
                        <div class="col-md-4">
                            <label class="form-label">持股數量</label>
                            <input type="number" class="form-control" id="holdingQuantity" step="1" required>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">平均成本</label>
                            <input type="number" class="form-control" id="holdingAvgCost" step="0.01" required>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">目前股價</label>
                            <input type="number" class="form-control" id="holdingCurrentPrice" step="0.01">
                        </div>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" id="saveHolding">儲存</button>
            </div>
        </div>
    </div>
</div>
```

### 3. 交易記錄頁面
- **前端**: `#file:investment-transactions.cshtml`
- **後端**: `#file:investment-transactions.cshtml.cs`
- **路由**: `/investment-transactions/{portfolioId?}`

### 3.1 前端實作 (investment-transactions.cshtml)
```html
<div class="container mt-4">
    <!-- 快速新增交易 -->
    <div class="card mb-4">
        <div class="card-header">
            <h5><i class="fas fa-plus-circle me-2"></i>快速新增交易</h5>
        </div>
        <div class="card-body">
            <form id="quickTransactionForm">
                <div class="row">
                    <div class="col-md-2">
                        <select class="form-select" id="quickTransactionType">
                            <option value="買入">買入</option>
                            <option value="賣出">賣出</option>
                            <option value="股息">股息</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <input type="text" class="form-control" id="quickSymbol" placeholder="股票代號">
                    </div>
                    <div class="col-md-2">
                        <input type="number" class="form-control" id="quickQuantity" placeholder="數量" step="1">
                    </div>
                    <div class="col-md-2">
                        <input type="number" class="form-control" id="quickPrice" placeholder="價格" step="0.01">
                    </div>
                    <div class="col-md-2">
                        <input type="number" class="form-control" id="quickFee" placeholder="手續費" step="0.01" value="0">
                    </div>
                    <div class="col-md-2">
                        <button type="button" class="btn btn-primary w-100" id="addQuickTransaction">
                            <i class="fas fa-plus"></i> 新增
                        </button>
                    </div>
                </div>
            </form>
        </div>
    </div>

    <!-- 篩選器 -->
    <div class="card mb-4">
        <div class="card-body">
            <div class="row">
                <div class="col-md-3">
                    <label class="form-label">開始日期</label>
                    <input type="date" class="form-control" id="startDate">
                </div>
                <div class="col-md-3">
                    <label class="form-label">結束日期</label>
                    <input type="date" class="form-control" id="endDate">
                </div>
                <div class="col-md-3">
                    <label class="form-label">交易類型</label>
                    <select class="form-select" id="transactionTypeFilter">
                        <option value="">全部</option>
                        <option value="買入">買入</option>
                        <option value="賣出">賣出</option>
                        <option value="股息">股息</option>
                    </select>
                </div>
                <div class="col-md-3">
                    <label class="form-label">股票代號</label>
                    <input type="text" class="form-control" id="symbolFilter" placeholder="篩選股票">
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-12">
                    <button class="btn btn-primary" id="applyFilters">
                        <i class="fas fa-filter"></i> 套用篩選
                    </button>
                    <button class="btn btn-outline-secondary" id="clearFilters">
                        <i class="fas fa-times"></i> 清除篩選
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- 交易記錄表格 -->
    <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <h5><i class="fas fa-history me-2"></i>交易記錄</h5>
            <div class="btn-group">
                <button class="btn btn-outline-success btn-sm" id="exportTransactionsBtn">
                    <i class="fas fa-download"></i> 匯出 Excel
                </button>
                <button class="btn btn-outline-primary btn-sm" data-bs-toggle="modal" data-bs-target="#addTransactionModal">
                    <i class="fas fa-plus"></i> 詳細新增
                </button>
            </div>
        </div>
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-hover" id="transactionsTable">
                    <thead>
                        <tr>
                            <th>日期</th>
                            <th>類型</th>
                            <th>股票代號</th>
                            <th>數量</th>
                            <th>價格</th>
                            <th>總金額</th>
                            <th>手續費</th>
                            <th>淨金額</th>
                            <th>備註</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- 動態載入交易記錄 -->
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>
```

### 4. JavaScript 核心功能
```javascript
class InvestmentTracker {
    constructor() {
        this.portfolios = [];
        this.holdings = [];
        this.transactions = [];
        this.priceUpdateTimer = null;
        this.init();
    }

    init() {
        this.loadData();
        this.bindEvents();
        this.initCharts();
        this.startPriceUpdates();
    }

    async loadData() {
        try {
            // 載入所有數據
            const [portfoliosResp, holdingsResp, transactionsResp] = await Promise.all([
                fetch('/InvestmentPortfolio/GetPortfolios'),
                fetch('/InvestmentHoldings/GetHoldings'),
                fetch('/InvestmentTransactions/GetTransactions')
            ]);

            this.portfolios = await portfoliosResp.json();
            this.holdings = await holdingsResp.json();
            this.transactions = await transactionsResp.json();

            this.updateUI();
        } catch (error) {
            console.error('載入數據失敗:', error);
        }
    }

    async updateStockPrices() {
        const symbols = [...new Set(this.holdings.map(h => h.symbol))];
        
        try {
            const response = await fetch('/InvestmentPortfolio/UpdatePrices', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(symbols)
            });

            if (response.ok) {
                const updatedPrices = await response.json();
                this.updateHoldingsPrices(updatedPrices);
                this.updateUI();
            }
        } catch (error) {
            console.error('更新股價失敗:', error);
        }
    }

    updateHoldingsPrices(prices) {
        this.holdings.forEach(holding => {
            if (prices[holding.symbol]) {
                holding.currentPrice = prices[holding.symbol].price;
                holding.lastUpdated = prices[holding.symbol].lastUpdated;
                
                // 重新計算損益
                holding.marketValue = holding.quantity * holding.currentPrice;
                holding.gainLoss = holding.marketValue - (holding.quantity * holding.averageCost);
                holding.gainLossPercentage = ((holding.currentPrice - holding.averageCost) / holding.averageCost) * 100;
            }
        });
    }

    calculatePortfolioMetrics() {
        const metrics = {
            totalAssets: 0,
            totalCost: 0,
            totalGainLoss: 0,
            totalReturn: 0,
            holdingsCount: this.holdings.length
        };

        this.holdings.forEach(holding => {
            metrics.totalAssets += holding.marketValue;
            metrics.totalCost += (holding.quantity * holding.averageCost);
            metrics.totalGainLoss += holding.gainLoss;
        });

        metrics.totalReturn = metrics.totalCost > 0 
            ? ((metrics.totalGainLoss / metrics.totalCost) * 100) 
            : 0;

        return metrics;
    }

    initCharts() {
        // 投資組合趨勢圖
        const trendCtx = document.getElementById('portfolioTrendChart')?.getContext('2d');
        if (trendCtx) {
            this.trendChart = new Chart(trendCtx, {
                type: 'line',
                data: {
                    labels: [],
                    datasets: [{
                        label: '投資組合價值',
                        data: [],
                        borderColor: '#0d6efd',
                        backgroundColor: 'rgba(13, 110, 253, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: false,
                            ticks: {
                                callback: function(value) {
                                    return '$' + value.toLocaleString();
                                }
                            }
                        }
                    }
                }
            });
        }

        // 資產配置圓餅圖
        const allocationCtx = document.getElementById('assetAllocationChart')?.getContext('2d');
        if (allocationCtx) {
            this.allocationChart = new Chart(allocationCtx, {
                type: 'doughnut',
                data: {
                    labels: [],
                    datasets: [{
                        data: [],
                        backgroundColor: [
                            '#ff6384', '#36a2eb', '#ffce56', '#4bc0c0',
                            '#9966ff', '#ff9f40', '#ff6384', '#c9cbcf'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        }
    }

    updateCharts() {
        // 更新資產配置圖
        if (this.allocationChart) {
            const allocation = this.calculateAssetAllocation();
            this.allocationChart.data.labels = allocation.labels;
            this.allocationChart.data.datasets[0].data = allocation.values;
            this.allocationChart.update();
        }
    }

    calculateAssetAllocation() {
        const allocation = {};
        
        this.holdings.forEach(holding => {
            if (!allocation[holding.type]) {
                allocation[holding.type] = 0;
            }
            allocation[holding.type] += holding.marketValue;
        });

        return {
            labels: Object.keys(allocation),
            values: Object.values(allocation)
        };
    }

    startPriceUpdates() {
        // 每5分鐘更新一次股價
        this.priceUpdateTimer = setInterval(() => {
            this.updateStockPrices();
        }, 5 * 60 * 1000);
    }

    stopPriceUpdates() {
        if (this.priceUpdateTimer) {
            clearInterval(this.priceUpdateTimer);
            this.priceUpdateTimer = null;
        }
    }

    updateUI() {
        const metrics = this.calculatePortfolioMetrics();
        
        // 更新統計卡片
        $('#totalAssets').text('$' + metrics.totalAssets.toLocaleString());
        $('#totalGainLoss').text(
            (metrics.totalGainLoss >= 0 ? '+' : '') + 
            '$' + metrics.totalGainLoss.toLocaleString()
        );
        $('#totalReturn').text(
            (metrics.totalReturn >= 0 ? '+' : '') + 
            metrics.totalReturn.toFixed(2) + '%'
        );
        $('#totalHoldings').text(metrics.holdingsCount);

        // 更新圖表
        this.updateCharts();
        
        // 更新表格
        this.updateTables();
    }

    updateTables() {
        // 更新投資組合表格
        this.updatePortfolioTable();
        
        // 更新持倉表格
        this.updateHoldingsTable();
        
        // 更新交易記錄表格
        this.updateTransactionsTable();
    }

    bindEvents() {
        // 綁定所有事件處理器
        $('#refreshPricesBtn').on('click', () => this.updateStockPrices());
        $('#savePortfolio').on('click', () => this.savePortfolio());
        $('#saveHolding').on('click', () => this.saveHolding());
        // ... 更多事件綁定
    }
}

// 初始化投資追蹤器
$(document).ready(function() {
    window.investmentTracker = new InvestmentTracker();
});
```

### 5. 後端實作核心方法

#### 5.1 股價更新服務
```csharp
public class StockPriceService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "YOUR_API_KEY"; // Alpha Vantage API Key

    public StockPriceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤但繼續處理其他股票
                Console.WriteLine($"獲取 {symbol} 股價失敗: {ex.Message}");
            }
        }
        
        return prices;
    }

    private async Task<StockPrice> GetSingleStockPriceAsync(string symbol)
    {
        var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";
        
        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            
            if (data["Global Quote"] != null)
            {
                return new StockPrice
                {
                    Symbol = symbol,
                    Price = decimal.Parse(data["Global Quote"]["05. price"].ToString()),
                    Change = decimal.Parse(data["Global Quote"]["09. change"].ToString()),
                    ChangePercent = data["Global Quote"]["10. change percent"].ToString(),
                    LastUpdated = DateTime.Now
                };
            }
        }
        
        return null;
    }
}
```

## 進階功能

### 6. 投資分析工具
- **風險分析**: VaR 計算、Beta 值分析
- **績效指標**: 夏普比率、資訊比率
- **相關性分析**: 持股間相關性矩陣
- **回測功能**: 歷史績效模擬

### 7. 警報系統
- **價格警報**: 股價到達指定價位時通知
- **損益警報**: 損失超過設定百分比時警告
- **新聞警報**: 關注股票的重要新聞推送

### 8. 報表功能
- **月度報告**: 自動生成投資月報
- **年度總結**: 投資績效年度分析
- **稅務報告**: 資本利得稅計算輔助

## 測試規範

### 8.1 功能測試
- [ ] 投資組合 CRUD 操作測試
- [ ] 股價更新準確性測試
- [ ] 損益計算正確性測試
- [ ] 資料匯入/匯出測試

### 8.2 效能測試
- [ ] 大量持倉載入速度測試
- [ ] 即時股價更新效能測試
- [ ] 圖表渲染效能測試

## 安全性考量
- API Key 安全儲存
- 用戶數據加密存儲
- 防止 SQL 注入攻擊
- CSRF 保護機制

## 未來擴展計畫
- 加密貨幣追蹤支援
- 多國股市整合
- 社群投資分享功能
- AI 投資建議系統
