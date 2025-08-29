/**
 * 進階統計分析 JavaScript 模組
 * 提供多分頁統計功能、進階圖表和匯出功能
 */

// 全域變數
let currentStatisticsData = null;
let chartInstances = {};
let currentStartDate = null;
let currentEndDate = null;

// 初始化統計模組
function initializeStatistics() {
    // 設定預設日期範圍（最近6個月）
    const today = new Date();
    const sixMonthsAgo = new Date(today);
    sixMonthsAgo.setMonth(today.getMonth() - 6);
    
    document.getElementById('statsStartDate').value = formatDate(sixMonthsAgo);
    document.getElementById('statsEndDate').value = formatDate(today);
    
    // 綁定分頁切換事件
    const tabTriggerList = [].slice.call(document.querySelectorAll('#statisticsTabs button'));
    tabTriggerList.forEach(function (tabTriggerEl) {
        tabTriggerEl.addEventListener('shown.bs.tab', function (event) {
            const targetTab = event.target.getAttribute('data-bs-target');
            handleTabSwitch(targetTab);
        });
    });
}

// 顯示統計模態框
async function showStatisticsModal() {
    const modal = new bootstrap.Modal(document.getElementById('statisticsModal'));
    
    // 顯示載入動畫
    showLoadingState();
    modal.show();
    
    try {
        // 載入基本統計資料
        await loadOverviewData();
        
        // 隱藏載入動畫，顯示內容
        hideLoadingState();
    } catch (error) {
        console.error('載入統計資料失敗:', error);
        showError('載入統計資料失敗，請稍後再試');
    }
}

// 處理分頁切換
async function handleTabSwitch(targetTab) {
    if (!currentStatisticsData) return;
    
    try {
        switch (targetTab) {
            case '#overview':
                await renderOverviewTab();
                break;
            case '#category':
                await renderCategoryTab();
                break;
            case '#pattern':
                await renderPatternTab();
                break;
            case '#ranking':
                await renderRankingTab();
                break;
        }
    } catch (error) {
        console.error('切換分頁時發生錯誤:', error);
        showError('載入分頁內容時發生錯誤');
    }
}

// 載入概覽資料
async function loadOverviewData() {
    const startDate = document.getElementById('statsStartDate').value;
    const endDate = document.getElementById('statsEndDate').value;
    
    if (!startDate || !endDate) {
        throw new Error('請選擇有效的日期範圍');
    }
    
    currentStartDate = startDate;
    currentEndDate = endDate;
    
    try {
        const response = await fetch(`/index7?handler=Statistics&startDate=${startDate}&endDate=${endDate}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        if (data.success === false) {
            throw new Error(data.message || '載入統計資料失敗');
        }
        
        currentStatisticsData = data;
        await renderOverviewTab();
    } catch (error) {
        console.error('載入概覽資料失敗:', error);
        throw error;
    }
}

// 渲染概覽分頁
async function renderOverviewTab() {
    if (!currentStatisticsData) return;
    
    // 渲染統計摘要
    renderStatisticsSummary(currentStatisticsData.summary);
    
    // 渲染月度趨勢圖
    renderMonthlyTrendChart(currentStatisticsData.monthlyTrend);
    
    // 渲染支出分類圖
    renderExpenseCategoryChart(currentStatisticsData.expenseCategories);
}

// 渲染分類分析分頁
async function renderCategoryTab() {
    try {
        // 載入收入分類資料
        const incomeResponse = await fetch(`/index7?handler=IncomeCategoryAnalysis&startDate=${currentStartDate}&endDate=${currentEndDate}`);
        const incomeData = await incomeResponse.json();
        
        if (incomeData.success) {
            renderIncomeCategoryChart(incomeData.data);
        }
        
        // 渲染支出分類詳細圖表
        renderExpenseCategoryDetailChart(currentStatisticsData.expenseCategories);
        
        // 渲染分類詳細表格
        renderCategoryDetailTable(currentStatisticsData.expenseCategories, incomeData.data || []);
    } catch (error) {
        console.error('載入分類分析資料失敗:', error);
    }
}

// 渲染時間模式分頁
async function renderPatternTab() {
    try {
        const response = await fetch(`/index7?handler=TimePatternAnalysis&startDate=${currentStartDate}&endDate=${currentEndDate}`);
        const data = await response.json();
        
        if (data.success) {
            renderWeekdayPatternChart(data.data.weekdayPatterns);
            renderMonthlyPatternChart(data.data.monthlyPatterns);
            renderPatternInsights(data.data.dailySummary);
        }
    } catch (error) {
        console.error('載入時間模式資料失敗:', error);
    }
}

// 渲染排行榜分頁
async function renderRankingTab() {
    try {
        // 載入收入排行榜
        const incomeResponse = await fetch(`/index7?handler=CategoryRanking&startDate=${currentStartDate}&endDate=${currentEndDate}&type=income&topCount=10`);
        const incomeData = await incomeResponse.json();
        
        // 載入支出排行榜
        const expenseResponse = await fetch(`/index7?handler=CategoryRanking&startDate=${currentStartDate}&endDate=${currentEndDate}&type=expense&topCount=10`);
        const expenseData = await expenseResponse.json();
        
        if (incomeData.success) {
            renderRankingTable('incomeRankingTable', incomeData.data, 'success');
        }
        
        if (expenseData.success) {
            renderRankingTable('expenseRankingTable', expenseData.data, 'danger');
        }
    } catch (error) {
        console.error('載入排行榜資料失敗:', error);
    }
}

// 渲染統計摘要
function renderStatisticsSummary(summary) {
    const summaryContainer = document.getElementById('statisticsSummary');
    if (!summary || !summaryContainer) return;
    
    const html = `
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-success h-100">
                <div class="card-body text-center">
                    <i class="fas fa-arrow-up fa-2x text-success mb-2"></i>
                    <h5 class="card-title text-success">總收入</h5>
                    <h4 class="text-success">NT$ ${summary.totalIncome.toLocaleString()}</h4>
                    <small class="text-muted">月平均: NT$ ${summary.averageMonthlyIncome.toLocaleString()}</small>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-danger h-100">
                <div class="card-body text-center">
                    <i class="fas fa-arrow-down fa-2x text-danger mb-2"></i>
                    <h5 class="card-title text-danger">總支出</h5>
                    <h4 class="text-danger">NT$ ${summary.totalExpense.toLocaleString()}</h4>
                    <small class="text-muted">月平均: NT$ ${summary.averageMonthlyExpense.toLocaleString()}</small>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-primary h-100">
                <div class="card-body text-center">
                    <i class="fas fa-chart-line fa-2x text-primary mb-2"></i>
                    <h5 class="card-title text-primary">淨收支</h5>
                    <h4 class="${summary.netIncome >= 0 ? 'text-success' : 'text-danger'}">
                        NT$ ${summary.netIncome.toLocaleString()}
                    </h4>
                    <small class="text-muted">共 ${summary.totalRecords} 筆記錄</small>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-info h-100">
                <div class="card-body text-center">
                    <i class="fas fa-star fa-2x text-info mb-2"></i>
                    <h5 class="card-title text-info">最大支出</h5>
                    <h6 class="text-info">${summary.topExpenseCategory}</h6>
                    <h5 class="text-info">NT$ ${summary.topExpenseAmount.toLocaleString()}</h5>
                </div>
            </div>
        </div>
    `;
    
    summaryContainer.innerHTML = html;
}

// 渲染月度趨勢圖
function renderMonthlyTrendChart(trendData) {
    const ctx = document.getElementById('monthlyTrendChart');
    if (!ctx || !trendData) return;
    
    // 銷毀現有圖表
    if (chartInstances.monthlyTrend) {
        chartInstances.monthlyTrend.destroy();
    }
    
    const labels = trendData.map(item => item.month);
    const incomeData = trendData.map(item => item.income);
    const expenseData = trendData.map(item => item.expense);
    const netIncomeData = trendData.map(item => item.netIncome);
    
    chartInstances.monthlyTrend = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: '收入',
                    data: incomeData,
                    borderColor: '#28a745',
                    backgroundColor: 'rgba(40, 167, 69, 0.1)',
                    fill: true,
                    tension: 0.4
                },
                {
                    label: '支出',
                    data: expenseData,
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    fill: true,
                    tension: 0.4
                },
                {
                    label: '淨收支',
                    data: netIncomeData,
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0, 123, 255, 0.1)',
                    fill: true,
                    tension: 0.4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'NT$ ' + value.toLocaleString();
                        }
                    }
                }
            },
            plugins: {
                legend: {
                    position: 'top'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return context.dataset.label + ': NT$ ' + context.parsed.y.toLocaleString();
                        }
                    }
                }
            }
        }
    });
}

// 渲染支出分類圓餅圖
function renderExpenseCategoryChart(categoryData) {
    const ctx = document.getElementById('expenseCategoryChart');
    if (!ctx || !categoryData || categoryData.length === 0) return;
    
    // 銷毀現有圖表
    if (chartInstances.expenseCategory) {
        chartInstances.expenseCategory.destroy();
    }
    
    const labels = categoryData.map(item => item.category);
    const data = categoryData.map(item => item.amount);
    const colors = categoryData.map(item => item.color);
    
    chartInstances.expenseCategory = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: NT$ ${value.toLocaleString()} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

// 渲染收入分類圖
function renderIncomeCategoryChart(categoryData) {
    const ctx = document.getElementById('incomeCategoryChart');
    if (!ctx || !categoryData || categoryData.length === 0) {
        if (ctx) {
            ctx.getContext('2d').clearRect(0, 0, ctx.width, ctx.height);
            const parent = ctx.parentElement;
            parent.innerHTML = '<div class="text-center text-muted py-5"><i class="fas fa-info-circle"></i><br>此期間無收入資料</div>';
        }
        return;
    }
    
    // 銷毀現有圖表
    if (chartInstances.incomeCategory) {
        chartInstances.incomeCategory.destroy();
    }
    
    const labels = categoryData.map(item => item.category);
    const data = categoryData.map(item => item.amount);
    const colors = generateColors(categoryData.length, 'success');
    
    chartInstances.incomeCategory = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: NT$ ${value.toLocaleString()} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

// 渲染支出分類詳細圖
function renderExpenseCategoryDetailChart(categoryData) {
    const ctx = document.getElementById('expenseCategoryDetailChart');
    if (!ctx || !categoryData || categoryData.length === 0) return;
    
    // 銷毀現有圖表
    if (chartInstances.expenseCategoryDetail) {
        chartInstances.expenseCategoryDetail.destroy();
    }
    
    const labels = categoryData.map(item => `${item.category}${item.subCategory ? ' - ' + item.subCategory : ''}`);
    const data = categoryData.map(item => item.amount);
    const colors = generateColors(categoryData.length, 'danger');
    
    chartInstances.expenseCategoryDetail = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: '支出金額',
                data: data,
                backgroundColor: colors,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'y',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return '支出: NT$ ' + context.parsed.x.toLocaleString();
                        }
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'NT$ ' + value.toLocaleString();
                        }
                    }
                }
            }
        }
    });
}

// 渲染週日模式圖
function renderWeekdayPatternChart(weekdayData) {
    const ctx = document.getElementById('weekdayPatternChart');
    if (!ctx || !weekdayData) return;
    
    // 銷毀現有圖表
    if (chartInstances.weekdayPattern) {
        chartInstances.weekdayPattern.destroy();
    }
    
    const labels = weekdayData.map(item => item.weekday);
    const incomeData = weekdayData.map(item => item.averageIncome);
    const expenseData = weekdayData.map(item => item.averageExpense);
    
    chartInstances.weekdayPattern = new Chart(ctx, {
        type: 'radar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: '平均收入',
                    data: incomeData,
                    borderColor: '#28a745',
                    backgroundColor: 'rgba(40, 167, 69, 0.2)',
                    pointBackgroundColor: '#28a745'
                },
                {
                    label: '平均支出',
                    data: expenseData,
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.2)',
                    pointBackgroundColor: '#dc3545'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                r: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'NT$ ' + value.toLocaleString();
                        }
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return context.dataset.label + ': NT$ ' + context.parsed.r.toLocaleString();
                        }
                    }
                }
            }
        }
    });
}

// 渲染月內模式圖
function renderMonthlyPatternChart(monthlyData) {
    const ctx = document.getElementById('monthlyPatternChart');
    if (!ctx || !monthlyData) return;
    
    // 銷毀現有圖表
    if (chartInstances.monthlyPattern) {
        chartInstances.monthlyPattern.destroy();
    }
    
    const labels = monthlyData.map(item => item.period);
    const incomeData = monthlyData.map(item => item.averageIncome);
    const expenseData = monthlyData.map(item => item.averageExpense);
    const recordCounts = monthlyData.map(item => item.recordCount);
    
    chartInstances.monthlyPattern = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: '平均收入',
                    data: incomeData,
                    backgroundColor: 'rgba(40, 167, 69, 0.7)',
                    yAxisID: 'y'
                },
                {
                    label: '平均支出',
                    data: expenseData,
                    backgroundColor: 'rgba(220, 53, 69, 0.7)',
                    yAxisID: 'y'
                },
                {
                    label: '記錄筆數',
                    data: recordCounts,
                    type: 'line',
                    borderColor: '#ffc107',
                    backgroundColor: 'rgba(255, 193, 7, 0.1)',
                    yAxisID: 'y1',
                    tension: 0.4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'NT$ ' + value.toLocaleString();
                        }
                    }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    beginAtZero: true,
                    grid: {
                        drawOnChartArea: false,
                    },
                    ticks: {
                        callback: function(value) {
                            return value + ' 筆';
                        }
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            if (context.dataset.label === '記錄筆數') {
                                return context.dataset.label + ': ' + context.parsed.y + ' 筆';
                            }
                            return context.dataset.label + ': NT$ ' + context.parsed.y.toLocaleString();
                        }
                    }
                }
            }
        }
    });
}

// 渲染排行榜表格
function renderRankingTable(containerId, rankingData, themeColor) {
    const container = document.getElementById(containerId);
    if (!container || !rankingData || rankingData.length === 0) {
        if (container) {
            container.innerHTML = '<div class="text-center text-muted py-4"><i class="fas fa-info-circle"></i><br>暫無排行榜資料</div>';
        }
        return;
    }
    
    const getTrendIcon = (trend) => {
        switch (trend) {
            case 'up': return '<i class="fas fa-arrow-up text-success"></i>';
            case 'down': return '<i class="fas fa-arrow-down text-danger"></i>';
            default: return '<i class="fas fa-minus text-muted"></i>';
        }
    };
    
    const html = `
        <div class="table-responsive">
            <table class="table table-hover mb-0">
                <thead class="bg-${themeColor} text-white">
                    <tr>
                        <th class="text-center" style="width: 60px;">#</th>
                        <th>分類</th>
                        <th class="text-end">金額</th>
                        <th class="text-center" style="width: 80px;">趨勢</th>
                    </tr>
                </thead>
                <tbody>
                    ${rankingData.map(item => `
                        <tr>
                            <td class="text-center">
                                <span class="badge bg-${themeColor} rounded-pill">${item.rank}</span>
                            </td>
                            <td>
                                <div>
                                    <strong>${item.category}</strong>
                                    ${item.subCategory && item.subCategory !== '其他' ? `<br><small class="text-muted">${item.subCategory}</small>` : ''}
                                </div>
                                <small class="text-muted">${item.recordCount} 筆 | 平均 NT$ ${item.averageAmount.toLocaleString()}</small>
                            </td>
                            <td class="text-end">
                                <strong>NT$ ${item.amount.toLocaleString()}</strong>
                                ${item.percentageChange !== 0 ? 
                                    `<br><small class="${item.percentageChange > 0 ? 'text-success' : 'text-danger'}">
                                        ${item.percentageChange > 0 ? '+' : ''}${item.percentageChange.toFixed(1)}%
                                    </small>` : ''}
                            </td>
                            <td class="text-center">${getTrendIcon(item.trend)}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
    
    container.innerHTML = html;
}

// 渲染分類詳細表格
function renderCategoryDetailTable(expenseCategories, incomeCategories) {
    const container = document.getElementById('categoryDetailTable');
    if (!container) return;
    
    const allCategories = [
        ...expenseCategories.map(cat => ({ ...cat, type: '支出' })),
        ...incomeCategories.map(cat => ({ ...cat, type: '收入' }))
    ].sort((a, b) => b.amount - a.amount);
    
    if (allCategories.length === 0) {
        container.innerHTML = '<div class="text-center text-muted py-4"><i class="fas fa-info-circle"></i><br>暫無分類資料</div>';
        return;
    }
    
    const html = `
        <div class="table-responsive">
            <table class="table table-striped">
                <thead class="table-dark">
                    <tr>
                        <th>類型</th>
                        <th>分類</th>
                        <th>子分類</th>
                        <th class="text-end">金額</th>
                        <th class="text-end">百分比</th>
                        <th class="text-center">筆數</th>
                    </tr>
                </thead>
                <tbody>
                    ${allCategories.map(item => `
                        <tr>
                            <td>
                                <span class="badge bg-${item.type === '收入' ? 'success' : 'danger'} rounded-pill">
                                    ${item.type}
                                </span>
                            </td>
                            <td><strong>${item.category}</strong></td>
                            <td>${item.subCategory || '-'}</td>
                            <td class="text-end"><strong>NT$ ${item.amount.toLocaleString()}</strong></td>
                            <td class="text-end">${item.percentage.toFixed(1)}%</td>
                            <td class="text-center">${item.recordCount}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
    
    container.innerHTML = html;
}

// 渲染時間模式洞察
function renderPatternInsights(dailySummary) {
    const container = document.getElementById('patternInsights');
    if (!container || !dailySummary) return;
    
    const html = `
        <div class="row">
            <div class="col-md-6">
                <div class="alert alert-info">
                    <h6><i class="fas fa-star"></i> 消費習慣分析</h6>
                    <ul class="mb-0">
                        <li><strong>最活躍的星期：</strong> ${dailySummary.mostActiveWeekday}</li>
                        <li><strong>支出最高的星期：</strong> ${dailySummary.highestExpenseWeekday}</li>
                        <li><strong>最活躍的期間：</strong> ${dailySummary.mostActivePeriod}</li>
                    </ul>
                </div>
            </div>
            <div class="col-md-6">
                <div class="alert alert-warning">
                    <h6><i class="fas fa-chart-bar"></i> 平均消費比較</h6>
                    <ul class="mb-0">
                        <li><strong>平日平均支出：</strong> NT$ ${dailySummary.weekdayAverageExpense.toLocaleString()}</li>
                        <li><strong>週末平均支出：</strong> NT$ ${dailySummary.weekendAverageExpense.toLocaleString()}</li>
                        <li><strong>差異：</strong> ${
                            dailySummary.weekendAverageExpense > dailySummary.weekdayAverageExpense 
                                ? `週末比平日多花 NT$ ${(dailySummary.weekendAverageExpense - dailySummary.weekdayAverageExpense).toLocaleString()}`
                                : `平日比週末多花 NT$ ${(dailySummary.weekdayAverageExpense - dailySummary.weekendAverageExpense).toLocaleString()}`
                        }</li>
                    </ul>
                </div>
            </div>
        </div>
    `;
    
    container.innerHTML = html;
}

// 重新整理統計資料
async function refreshStatistics() {
    try {
        showLoadingState();
        await loadOverviewData();
        
        // 重新載入當前分頁的資料
        const activeTab = document.querySelector('#statisticsTabs .nav-link.active');
        if (activeTab) {
            const targetTab = activeTab.getAttribute('data-bs-target');
            await handleTabSwitch(targetTab);
        }
        
        hideLoadingState();
        showSuccess('統計資料已更新');
    } catch (error) {
        console.error('重新整理統計資料失敗:', error);
        showError('重新整理失敗，請稍後再試');
    }
}

// 匯出統計資料
async function exportStatistics(format) {
    try {
        const startDate = document.getElementById('statsStartDate').value;
        const endDate = document.getElementById('statsEndDate').value;
        
        if (!startDate || !endDate) {
            showError('請選擇有效的日期範圍');
            return;
        }
        
        // 準備表單資料
        const formData = new FormData();
        formData.append('StartDate', startDate);
        formData.append('EndDate', endDate);
        formData.append('Format', format);
        formData.append('IncludeCharts', 'false'); // PDF暫不支援圖表
        formData.append('IncludeSummary', 'true');
        formData.append('IncludeDetailedRecords', 'true');
        formData.append('IncludeAnalysis', 'category');
        formData.append('IncludeAnalysis', 'pattern');
        formData.append('IncludeAnalysis', 'ranking');
        
        // 新增防偽權杖
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            formData.append('__RequestVerificationToken', tokenInput.value);
        }
        
        // 顯示載入狀態
        showSuccess('正在準備匯出檔案...');
        
        console.log('匯出格式:', format);
        console.log('日期範圍:', startDate, '-', endDate);
        
        // 發送請求
        const response = await fetch('/index7?handler=ExportStatistics', {
            method: 'POST',
            body: formData
        });
        
        console.log('Response status:', response.status);
        console.log('Response headers:', response.headers);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('Response error:', errorText);
            throw new Error(`匯出失敗: ${response.status} ${response.statusText}`);
        }
        
        // 檢查回應類型
        const contentType = response.headers.get('content-type');
        console.log('Content-Type:', contentType);
        
        // 下載檔案
        const blob = await response.blob();
        console.log('Blob size:', blob.size, 'bytes');
        console.log('Blob type:', blob.type);
        
        if (blob.size === 0) {
            throw new Error('匯出的檔案大小為 0');
        }
        
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `統計分析報告_${startDate.replace(/-/g, '')}-${endDate.replace(/-/g, '')}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        
        showSuccess(`${format.toUpperCase()} 報告匯出成功`);
    } catch (error) {
        console.error('匯出失敗:', error);
        showError(`匯出失敗: ${error.message}`);
    }
}

// 工具函式
function formatDate(date) {
    return date.toISOString().split('T')[0];
}

function generateColors(count, theme) {
    const themes = {
        success: ['#28a745', '#20c997', '#17a2b8', '#6f42c1', '#e83e8c'],
        danger: ['#dc3545', '#fd7e14', '#ffc107', '#28a745', '#17a2b8'],
        primary: ['#007bff', '#6610f2', '#6f42c1', '#e83e8c', '#dc3545']
    };
    
    const baseColors = themes[theme] || themes.primary;
    const colors = [];
    
    for (let i = 0; i < count; i++) {
        colors.push(baseColors[i % baseColors.length]);
    }
    
    return colors;
}

function showLoadingState() {
    document.getElementById('statisticsLoading').style.display = 'block';
    document.getElementById('statisticsContent').style.display = 'none';
}

function hideLoadingState() {
    document.getElementById('statisticsLoading').style.display = 'none';
    document.getElementById('statisticsContent').style.display = 'block';
}

function showError(message) {
    hideLoadingState();
    
    // 創建更好的錯誤通知
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-danger alert-dismissible fade show position-fixed';
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 400px;';
    alertDiv.innerHTML = `
        <i class="fas fa-exclamation-triangle"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    // 自動移除通知
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 5000);
    
    console.error(message);
}

function showSuccess(message) {
    // 創建更好的成功通知
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-success alert-dismissible fade show position-fixed';
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 400px;';
    alertDiv.innerHTML = `
        <i class="fas fa-check-circle"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    // 自動移除通知
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 3000);
    
    console.log(message);
}

// 等待 DOM 載入完成後初始化
document.addEventListener('DOMContentLoaded', function() {
    initializeStatistics();
});

// 全域匯出函式供HTML調用
window.showStatisticsModal = showStatisticsModal;
window.refreshStatistics = refreshStatistics;
window.exportStatistics = exportStatistics;
