/**
 * 統計分析相關的 JavaScript 函式
 * Phase 1 - 基礎統計功能
 */

let monthlyTrendChart = null;
let expenseCategoryChart = null;

/**
 * 顯示統計分析對話框
 */
async function showStatisticsModal() {
    const modal = new bootstrap.Modal(document.getElementById('statisticsModal'));
    modal.show();
    
    // 初始化日期範圍 (預設最近 6 個月)
    initializeDateRange();
    
    // 載入統計資料
    await loadStatisticsData();
}

/**
 * 初始化日期範圍
 */
function initializeDateRange() {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setMonth(startDate.getMonth() - 6);
    
    document.getElementById('statsStartDate').value = startDate.toISOString().split('T')[0];
    document.getElementById('statsEndDate').value = endDate.toISOString().split('T')[0];
}

/**
 * 載入統計資料
 */
async function loadStatisticsData() {
    try {
        showStatisticsLoading();
        
        const startDate = document.getElementById('statsStartDate').value;
        const endDate = document.getElementById('statsEndDate').value;
        
        // 驗證日期範圍
        if (!validateDateRange(startDate, endDate)) {
            return;
        }
        
        const params = new URLSearchParams();
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        
        const response = await fetch(`/index7?handler=Statistics&${params.toString()}`);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const data = await response.json();
        
        // 渲染統計資料
        renderStatisticsSummary(data.summary);
        renderMonthlyTrendChart(data.monthlyTrend);
        renderExpenseCategoryChart(data.expenseCategories);
        
        hideStatisticsLoading();
        
    } catch (error) {
        console.error('載入統計資料失敗:', error);
        hideStatisticsLoading();
        showStatisticsError(error.message);
    }
}

/**
 * 驗證日期範圍
 */
function validateDateRange(startDate, endDate) {
    if (!startDate || !endDate) {
        showStatisticsError('請選擇有效的日期範圍');
        return false;
    }
    
    const start = new Date(startDate);
    const end = new Date(endDate);
    
    if (start > end) {
        showStatisticsError('開始日期不能晚於結束日期');
        return false;
    }
    
    // 檢查日期範圍不超過 2 年
    const maxDays = 730; // 2 年
    const daysDiff = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
    
    if (daysDiff > maxDays) {
        showStatisticsError('日期範圍不能超過 2 年');
        return false;
    }
    
    return true;
}

/**
 * 渲染統計摘要
 */
function renderStatisticsSummary(summary) {
    const container = document.getElementById('statisticsSummary');
    
    const summaryHtml = `
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-success h-100">
                <div class="card-body text-center">
                    <h6 class="card-title text-success mb-2">
                        <i class="fas fa-arrow-up"></i> 總收入
                    </h6>
                    <h4 class="text-success mb-1">NT$ ${summary.totalIncome.toLocaleString()}</h4>
                    <small class="text-muted">平均月收入: NT$ ${summary.averageMonthlyIncome.toLocaleString()}</small>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-danger h-100">
                <div class="card-body text-center">
                    <h6 class="card-title text-danger mb-2">
                        <i class="fas fa-arrow-down"></i> 總支出
                    </h6>
                    <h4 class="text-danger mb-1">NT$ ${summary.totalExpense.toLocaleString()}</h4>
                    <small class="text-muted">平均月支出: NT$ ${summary.averageMonthlyExpense.toLocaleString()}</small>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-${summary.netIncome >= 0 ? 'info' : 'warning'} h-100">
                <div class="card-body text-center">
                    <h6 class="card-title text-${summary.netIncome >= 0 ? 'info' : 'warning'} mb-2">
                        <i class="fas fa-${summary.netIncome >= 0 ? 'chart-line' : 'exclamation-triangle'}"></i> 淨收支
                    </h6>
                    <h4 class="text-${summary.netIncome >= 0 ? 'info' : 'warning'} mb-1">NT$ ${summary.netIncome.toLocaleString()}</h4>
                    <small class="text-muted">記錄總數: ${summary.totalRecords} 筆</small>
                </div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 mb-3">
            <div class="card border-primary h-100">
                <div class="card-body text-center">
                    <h6 class="card-title text-primary mb-2">
                        <i class="fas fa-crown"></i> 最大支出分類
                    </h6>
                    <h6 class="text-primary mb-1">${summary.topExpenseCategory}</h6>
                    <small class="text-muted">NT$ ${summary.topExpenseAmount.toLocaleString()}</small>
                </div>
            </div>
        </div>
    `;
    
    container.innerHTML = summaryHtml;
}

/**
 * 渲染月度趨勢圖表
 */
function renderMonthlyTrendChart(data) {
    const ctx = document.getElementById('monthlyTrendChart').getContext('2d');
    
    // 銷毀現有圖表
    if (monthlyTrendChart) {
        monthlyTrendChart.destroy();
    }
    
    monthlyTrendChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.monthName),
            datasets: [{
                label: '收入',
                data: data.map(d => d.income),
                borderColor: '#28a745',
                backgroundColor: 'rgba(40, 167, 69, 0.1)',
                tension: 0.3,
                fill: true
            }, {
                label: '支出',
                data: data.map(d => d.expense),
                borderColor: '#dc3545',
                backgroundColor: 'rgba(220, 53, 69, 0.1)',
                tension: 0.3,
                fill: true
            }, {
                label: '淨收支',
                data: data.map(d => d.netIncome),
                borderColor: '#007bff',
                backgroundColor: 'rgba(0, 123, 255, 0.1)',
                tension: 0.3,
                fill: false,
                borderDash: [5, 5]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: false
                },
                legend: {
                    position: 'top'
                },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        label: function(context) {
                            return `${context.dataset.label}: NT$ ${context.parsed.y.toLocaleString()}`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: '月份'
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: true,
                        text: '金額 (NT$)'
                    },
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'NT$ ' + value.toLocaleString();
                        }
                    }
                }
            },
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            }
        }
    });
}

/**
 * 渲染支出分類圓餅圖
 */
function renderExpenseCategoryChart(data) {
    const ctx = document.getElementById('expenseCategoryChart').getContext('2d');
    
    // 銷毀現有圖表
    if (expenseCategoryChart) {
        expenseCategoryChart.destroy();
    }
    
    if (!data || data.length === 0) {
        ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
        const centerX = ctx.canvas.width / 2;
        const centerY = ctx.canvas.height / 2;
        ctx.textAlign = 'center';
        ctx.fillStyle = '#6c757d';
        ctx.font = '16px Arial';
        ctx.fillText('無支出資料', centerX, centerY);
        return;
    }
    
    expenseCategoryChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(d => d.category),
            datasets: [{
                data: data.map(d => d.amount),
                backgroundColor: data.map(d => d.color),
                borderWidth: 2,
                borderColor: '#fff',
                hoverBorderWidth: 3,
                hoverBorderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        usePointStyle: true,
                        font: {
                            size: 12
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const data = context.raw;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = ((data / total) * 100).toFixed(1);
                            return `${context.label}: NT$ ${data.toLocaleString()} (${percentage}%)`;
                        }
                    }
                }
            },
            cutout: '60%',
            animation: {
                animateRotate: true,
                animateScale: true
            }
        }
    });
}

/**
 * 重新整理統計資料
 */
async function refreshStatistics() {
    await loadStatisticsData();
}

/**
 * 顯示載入動畫
 */
function showStatisticsLoading() {
    document.getElementById('statisticsLoading').style.display = 'block';
    document.getElementById('statisticsContent').style.display = 'none';
}

/**
 * 隱藏載入動畫
 */
function hideStatisticsLoading() {
    document.getElementById('statisticsLoading').style.display = 'none';
    document.getElementById('statisticsContent').style.display = 'block';
}

/**
 * 顯示錯誤訊息
 */
function showStatisticsError(message) {
    hideStatisticsLoading();
    
    const errorHtml = `
        <div class="text-center py-5">
            <i class="fas fa-exclamation-circle fa-3x text-warning mb-3"></i>
            <h5 class="text-muted">載入統計資料時發生錯誤</h5>
            <p class="text-muted">${message}</p>
            <button type="button" class="btn btn-primary" onclick="loadStatisticsData()">
                <i class="fas fa-redo"></i> 重試
            </button>
        </div>
    `;
    
    document.getElementById('statisticsContent').innerHTML = errorHtml;
    document.getElementById('statisticsContent').style.display = 'block';
}

/**
 * 日期變更事件處理
 */
document.addEventListener('DOMContentLoaded', function() {
    // 綁定日期變更事件
    const startDateInput = document.getElementById('statsStartDate');
    const endDateInput = document.getElementById('statsEndDate');
    
    if (startDateInput && endDateInput) {
        startDateInput.addEventListener('change', function() {
            if (document.getElementById('statisticsModal').classList.contains('show')) {
                loadStatisticsData();
            }
        });
        
        endDateInput.addEventListener('change', function() {
            if (document.getElementById('statisticsModal').classList.contains('show')) {
                loadStatisticsData();
            }
        });
    }
});

/**
 * 產生隨機顏色 (備用)
 */
function getRandomColor() {
    const colors = [
        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF',
        '#FF9F40', '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4',
        '#FFEAA7', '#DDA0DD', '#98D8C8', '#F7DC6F', '#BB8FCE'
    ];
    return colors[Math.floor(Math.random() * colors.length)];
}
