/**
 * 財務 AI 智慧分析功能
 * Financial AI Analysis Features
 */

class FinancialAI {
    constructor() {
        this.charts = {};
        this.healthScore = null;
        this.insights = [];
        this.alerts = [];
        this.recommendations = [];
        this.initialized = false;
    }

    /**
     * 初始化 AI 分析功能
     */
    async initialize() {
        if (this.initialized) return;

        console.log('正在初始化財務 AI 分析...');
        
        try {
            // 當 AI 分析標籤被點擊時載入資料
            const aiTab = document.getElementById('ai-insights-tab');
            if (aiTab) {
                aiTab.addEventListener('click', () => this.loadAIAnalysis());
            }

            this.initialized = true;
            console.log('財務 AI 分析初始化完成');
        } catch (error) {
            console.error('AI 分析初始化失敗:', error);
        }
    }

    /**
     * 載入 AI 分析資料
     */
    async loadAIAnalysis() {
        console.log('載入 AI 分析資料...');
        
        try {
            await Promise.all([
                this.loadFinancialHealthScore(),
                this.loadSmartInsights(),
                this.loadAnomalyAlerts(),
                this.loadExpenseForecast(),
                this.loadCashFlowProjection(),
                this.loadSavingsOpportunities(),
                this.loadPersonalizedRecommendations()
            ]);
        } catch (error) {
            console.error('載入 AI 分析資料失敗:', error);
            this.showError('載入 AI 分析時發生錯誤，請稍後再試。');
        }
    }

    /**
     * 載入財務健康度評分
     */
    async loadFinancialHealthScore() {
        try {
            const response = await fetch('/index7?handler=FinancialHealthScore', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.healthScore = result.data;
                this.renderHealthScore();
            } else {
                throw new Error(result.message || '載入財務健康度評分失敗');
            }
        } catch (error) {
            console.error('載入財務健康度評分失敗:', error);
            this.showHealthScoreError();
        }
    }

    /**
     * 載入智慧洞察
     */
    async loadSmartInsights() {
        try {
            const response = await fetch('/index7?handler=SmartInsights', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.insights = result.data || [];
                this.renderSmartInsights();
            } else {
                throw new Error(result.message || '載入智慧洞察失敗');
            }
        } catch (error) {
            console.error('載入智慧洞察失敗:', error);
            this.showInsightsError();
        }
    }

    /**
     * 載入異常警報
     */
    async loadAnomalyAlerts() {
        try {
            const response = await fetch('/index7?handler=AnomalyAlerts', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.alerts = result.data || [];
                this.renderAnomalyAlerts();
            } else {
                throw new Error(result.message || '載入異常警報失敗');
            }
        } catch (error) {
            console.error('載入異常警報失敗:', error);
            this.showAlertsError();
        }
    }

    /**
     * 載入支出預測
     */
    async loadExpenseForecast() {
        try {
            const response = await fetch('/index7?handler=ExpenseForecast', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.renderExpenseForecast(result.data || []);
            } else {
                throw new Error(result.message || '載入支出預測失敗');
            }
        } catch (error) {
            console.error('載入支出預測失敗:', error);
            this.showForecastError();
        }
    }

    /**
     * 載入現金流預測
     */
    async loadCashFlowProjection() {
        try {
            const response = await fetch('/index7?handler=CashFlowProjection', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success && result.data) {
                this.renderCashFlowProjection(result.data);
            } else {
                throw new Error(result.message || '載入現金流預測失敗');
            }
        } catch (error) {
            console.error('載入現金流預測失敗:', error);
            this.showCashFlowError();
        }
    }

    /**
     * 載入節約機會
     */
    async loadSavingsOpportunities() {
        try {
            const response = await fetch('/index7?handler=SavingsOpportunities', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.renderSavingsOpportunities(result.data || []);
            } else {
                throw new Error(result.message || '載入節約機會失敗');
            }
        } catch (error) {
            console.error('載入節約機會失敗:', error);
            this.showSavingsError();
        }
    }

    /**
     * 載入個人化建議
     */
    async loadPersonalizedRecommendations() {
        try {
            const response = await fetch('/index7?handler=PersonalizedRecommendations', {
                method: 'GET',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.recommendations = result.data || [];
                this.renderPersonalizedRecommendations();
            } else {
                throw new Error(result.message || '載入個人化建議失敗');
            }
        } catch (error) {
            console.error('載入個人化建議失敗:', error);
            this.showRecommendationsError();
        }
    }

    /**
     * 渲染財務健康度評分
     */
    renderHealthScore() {
        console.log('渲染財務健康度評分:', this.healthScore);
        
        if (!this.healthScore) {
            console.warn('健康度評分資料為空');
            return;
        }

        // 更新整體評分
        const overallScore = document.getElementById('overall-health-score');
        if (overallScore) {
            overallScore.textContent = Math.round(this.healthScore.overallScore);
            overallScore.className = `health-score-number ${this.getScoreClass(this.healthScore.overallScore)}`;
        }

        // 更新各項指標 - 添加調試資訊
        console.log('儲蓄分數:', this.healthScore.savingsScore);
        console.log('平衡分數:', this.healthScore.balanceScore);
        console.log('成長分數:', this.healthScore.growthScore);
        
        this.updateMetric('savings-score', this.healthScore.savingsScore);
        this.updateMetric('balance-score', this.healthScore.balanceScore);
        this.updateMetric('growth-score', this.healthScore.growthScore);
    }

    /**
     * 更新健康度指標
     */
    updateMetric(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            if (typeof value === 'number') {
                element.textContent = Math.round(value);
                element.className = `metric-value ${this.getScoreClass(value)}`;
            } else {
                // 處理 null 或 undefined 的情況
                element.textContent = '--';
                element.className = 'metric-value text-muted';
            }
        }
    }

    /**
     * 取得評分樣式類別
     */
    getScoreClass(score) {
        if (score >= 80) return 'text-success';
        if (score >= 60) return 'text-warning';
        return 'text-danger';
    }

    /**
     * 渲染智慧洞察
     */
    renderSmartInsights() {
        const container = document.getElementById('smart-insights-container');
        if (!container) return;

        if (!this.insights || this.insights.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted p-4">
                    <i class="fas fa-lightbulb fa-2x mb-3"></i>
                    <p>目前沒有智慧洞察</p>
                </div>
            `;
            return;
        }

        const insightsHtml = this.insights.map(insight => `
            <div class="insight-card mb-3 p-3 border-start border-${this.getInsightColor(insight.priority)} border-3">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="mb-0">${insight.title}</h6>
                    <span class="badge bg-${this.getInsightColor(insight.priority)}">${this.getInsightPriorityText(insight.priority)}</span>
                </div>
                <p class="mb-2 text-muted">${insight.description}</p>
                <small class="text-muted">
                    <i class="fas fa-calendar-alt"></i> ${new Date(insight.generatedAt).toLocaleDateString('zh-TW')}
                </small>
            </div>
        `).join('');

        container.innerHTML = insightsHtml;
    }

    /**
     * 渲染異常警報
     */
    renderAnomalyAlerts() {
        const container = document.getElementById('anomaly-alerts-container');
        if (!container) return;

        if (!this.alerts || this.alerts.length === 0) {
            container.innerHTML = `
                <div class="text-center text-success p-4">
                    <i class="fas fa-check-circle fa-2x mb-3"></i>
                    <p>目前沒有異常警報</p>
                </div>
            `;
            return;
        }

        const alertsHtml = this.alerts.map(alert => `
            <div class="alert alert-${this.getSeverityColor(alert.severity)} mb-3" role="alert">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <h6 class="alert-heading mb-1">
                            <i class="fas fa-${this.getSeverityIcon(alert.severity)}"></i>
                            ${alert.category}
                        </h6>
                        <p class="mb-1">${alert.message}</p>
                        <small class="text-muted">檢測時間: ${new Date(alert.detectedAt).toLocaleString('zh-TW')}</small>
                    </div>
                    <span class="badge bg-${this.getSeverityColor(alert.severity)}">${this.getSeverityText(alert.severity)}</span>
                </div>
            </div>
        `).join('');

        container.innerHTML = alertsHtml;
    }

    /**
     * 渲染支出預測圖表
     */
    renderExpenseForecast(data) {
        const canvas = document.getElementById('expense-forecast-chart');
        if (!canvas) return;

        // 銷毀舊圖表
        if (this.charts.expenseForecast) {
            this.charts.expenseForecast.destroy();
        }

        const ctx = canvas.getContext('2d');
        this.charts.expenseForecast = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.map(d => d.month),
                datasets: [
                    {
                        label: '預測支出',
                        data: data.map(d => d.predictedAmount),
                        borderColor: '#dc3545',
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        borderWidth: 2,
                        fill: true,
                        tension: 0.4
                    },
                    {
                        label: '置信區間上限',
                        data: data.map(d => d.upperBound),
                        borderColor: '#ffc107',
                        backgroundColor: 'transparent',
                        borderWidth: 1,
                        borderDash: [5, 5],
                        fill: false
                    },
                    {
                        label: '置信區間下限',
                        data: data.map(d => d.lowerBound),
                        borderColor: '#ffc107',
                        backgroundColor: 'transparent',
                        borderWidth: 1,
                        borderDash: [5, 5],
                        fill: false
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: '未來 6 個月支出預測'
                    },
                    legend: {
                        position: 'top'
                    }
                },
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
                elements: {
                    point: {
                        radius: 4,
                        hoverRadius: 6
                    }
                }
            }
        });
    }

    /**
     * 渲染現金流預測圖表
     */
    renderCashFlowProjection(data) {
        const canvas = document.getElementById('cash-flow-chart');
        if (!canvas) return;

        // 檢查資料是否存在
        if (!data || !data.dataPoints || data.dataPoints.length === 0) {
            this.showCashFlowError();
            return;
        }

        // 銷毀舊圖表
        if (this.charts.cashFlow) {
            this.charts.cashFlow.destroy();
        }

        const ctx = canvas.getContext('2d');
        
        // 格式化資料
        const labels = data.dataPoints.map(point => {
            const date = new Date(point.date);
            return date.toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit' });
        });
        
        this.charts.cashFlow = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: '預測收入',
                        data: data.dataPoints.map(point => point.income),
                        backgroundColor: 'rgba(40, 167, 69, 0.8)',
                        borderColor: '#28a745',
                        borderWidth: 1
                    },
                    {
                        label: '預測支出',
                        data: data.dataPoints.map(point => -point.expense),
                        backgroundColor: 'rgba(220, 53, 69, 0.8)',
                        borderColor: '#dc3545',
                        borderWidth: 1
                    },
                    {
                        label: '淨現金流',
                        data: data.dataPoints.map(point => point.netFlow),
                        type: 'line',
                        borderColor: '#007bff',
                        backgroundColor: 'transparent',
                        borderWidth: 3,
                        tension: 0.4,
                        fill: false
                    },
                    {
                        label: '累積餘額',
                        data: data.dataPoints.map(point => point.cumulativeBalance),
                        type: 'line',
                        borderColor: '#6f42c1',
                        backgroundColor: 'rgba(111, 66, 193, 0.1)',
                        borderWidth: 2,
                        tension: 0.4,
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    title: {
                        display: true,
                        text: '未來現金流預測分析',
                        font: {
                            size: 16,
                            weight: 'bold'
                        }
                    },
                    legend: {
                        position: 'top',
                        labels: {
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                let label = context.dataset.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                label += 'NT$ ' + Math.abs(context.parsed.y).toLocaleString();
                                return label;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        position: 'left',
                        title: {
                            display: true,
                            text: '金額 (NT$)'
                        },
                        ticks: {
                            callback: function(value) {
                                return 'NT$ ' + value.toLocaleString();
                            }
                        },
                        grid: {
                            color: 'rgba(0,0,0,0.1)'
                        }
                    },
                    y1: {
                        type: 'linear',
                        display: true,
                        position: 'right',
                        title: {
                            display: true,
                            text: '累積餘額 (NT$)'
                        },
                        ticks: {
                            callback: function(value) {
                                return 'NT$ ' + value.toLocaleString();
                            }
                        },
                        grid: {
                            drawOnChartArea: false
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: '月份'
                        }
                    }
                }
            }
        });
        
        // 顯示警告訊息
        if (data.warnings && data.warnings.length > 0) {
            this.displayCashFlowWarnings(data.warnings);
        }
    }

    /**
     * 顯示現金流警告訊息
     */
    displayCashFlowWarnings(warnings) {
        if (!warnings || warnings.length === 0) return;
        
        // 找到現金流圖表的父容器
        const chartContainer = document.getElementById('cash-flow-chart').closest('.card');
        if (!chartContainer) return;
        
        // 移除舊的警告
        const oldWarnings = chartContainer.querySelector('.cash-flow-warnings');
        if (oldWarnings) {
            oldWarnings.remove();
        }
        
        // 建立警告容器
        const warningsContainer = document.createElement('div');
        warningsContainer.className = 'cash-flow-warnings mt-3 p-3 bg-warning bg-opacity-10 border border-warning rounded';
        
        const warningsHtml = `
            <div class="d-flex align-items-center mb-2">
                <i class="fas fa-exclamation-triangle text-warning me-2"></i>
                <strong>現金流警告</strong>
            </div>
            <div class="warnings-list">
                ${warnings.map(warning => `
                    <div class="alert alert-warning alert-sm mb-2 py-2">
                        <i class="fas fa-info-circle me-1"></i>
                        <small>${warning}</small>
                    </div>
                `).join('')}
            </div>
        `;
        
        warningsContainer.innerHTML = warningsHtml;
        
        // 將警告加到圖表容器的底部
        chartContainer.querySelector('.card-body').appendChild(warningsContainer);
    }

    /**
     * 渲染節約機會
     */
    renderSavingsOpportunities(opportunities) {
        const container = document.getElementById('savings-opportunities-container');
        if (!container) return;

        if (!opportunities || opportunities.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted p-4">
                    <i class="fas fa-hand-holding-usd fa-2x mb-3"></i>
                    <p>目前沒有發現節約機會</p>
                </div>
            `;
            return;
        }

        const opportunitiesHtml = opportunities.map(opp => `
            <div class="savings-opportunity mb-3 p-3 bg-light rounded">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="mb-0">${opp.category}</h6>
                    <span class="badge bg-success">可節約 NT$ ${opp.potentialSaving.toLocaleString()}</span>
                </div>
                <p class="mb-2 text-muted small">${opp.description}</p>
                <div class="progress mb-2" style="height: 8px;">
                    <div class="progress-bar bg-success" role="progressbar" 
                         style="width: ${Math.min((opp.potentialSaving / 10000) * 100, 100)}%" 
                         aria-valuenow="${opp.potentialSaving}" aria-valuemin="0" aria-valuemax="10000"></div>
                </div>
                <small class="text-muted">
                    <i class="fas fa-lightbulb"></i> ${opp.actionRequired}
                </small>
            </div>
        `).join('');

        container.innerHTML = opportunitiesHtml;
    }

    /**
     * 渲染個人化建議
     */
    renderPersonalizedRecommendations() {
        const container = document.getElementById('personalized-recommendations-container');
        if (!container) return;

        if (!this.recommendations || this.recommendations.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted p-4">
                    <i class="fas fa-user-check fa-2x mb-3"></i>
                    <p>目前沒有個人化建議</p>
                </div>
            `;
            return;
        }

        const recommendationsHtml = this.recommendations.map(rec => `
            <div class="recommendation-card mb-3 p-3 border rounded">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="mb-0">
                        <i class="fas fa-${this.getRecommendationIcon(rec.type)}"></i>
                        ${rec.title}
                    </h6>
                    <span class="badge bg-${this.getRecommendationColor(rec.impact)}">${this.getImpactText(rec.impact)}</span>
                </div>
                <p class="mb-2 text-muted">${rec.description}</p>
                <div class="d-flex justify-content-between align-items-center">
                    <small class="text-muted">
                        <i class="fas fa-calendar-alt"></i> ${new Date(rec.createdAt).toLocaleDateString('zh-TW')}
                    </small>
                    <button class="btn btn-sm btn-outline-primary" onclick="financialAI.markRecommendationAsRead('${rec.id}')">
                        已讀
                    </button>
                </div>
            </div>
        `).join('');

        container.innerHTML = recommendationsHtml;
    }

    // 輔助方法
    getInsightColor(priority) {
        switch(priority) {
            case 'High': return 'danger';
            case 'Medium': return 'warning';
            case 'Low': return 'info';
            default: return 'secondary';
        }
    }

    getInsightPriorityText(priority) {
        switch(priority) {
            case 'High': return '高';
            case 'Medium': return '中';
            case 'Low': return '低';
            default: return '一般';
        }
    }

    getSeverityColor(severity) {
        switch(severity) {
            case 'High': return 'danger';
            case 'Medium': return 'warning';
            case 'Low': return 'info';
            default: return 'secondary';
        }
    }

    getSeverityIcon(severity) {
        switch(severity) {
            case 'High': return 'exclamation-triangle';
            case 'Medium': return 'exclamation-circle';
            case 'Low': return 'info-circle';
            default: return 'bell';
        }
    }

    getSeverityText(severity) {
        switch(severity) {
            case 'High': return '高';
            case 'Medium': return '中';
            case 'Low': return '低';
            default: return '一般';
        }
    }

    getRecommendationIcon(type) {
        switch(type) {
            case 'Saving': return 'piggy-bank';
            case 'Investment': return 'chart-line';
            case 'Budget': return 'calculator';
            case 'Expense': return 'credit-card';
            default: return 'lightbulb';
        }
    }

    getRecommendationColor(impact) {
        switch(impact) {
            case 'High': return 'success';
            case 'Medium': return 'primary';
            case 'Low': return 'info';
            default: return 'secondary';
        }
    }

    getImpactText(impact) {
        switch(impact) {
            case 'High': return '高影響';
            case 'Medium': return '中影響';
            case 'Low': return '低影響';
            default: return '一般';
        }
    }

    /**
     * 標記建議為已讀
     */
    async markRecommendationAsRead(id) {
        try {
            // 這裡可以添加標記建議為已讀的 API 呼叫
            console.log('標記建議為已讀:', id);
            
            // 重新載入建議
            await this.loadPersonalizedRecommendations();
        } catch (error) {
            console.error('標記建議為已讀失敗:', error);
        }
    }

    // 錯誤處理方法
    showError(message) {
        console.error('AI 分析錯誤:', message);
        // 可以在這裡添加用戶界面錯誤提示
    }

    showHealthScoreError() {
        // 更新整體評分顯示錯誤
        const overallScore = document.getElementById('overall-health-score');
        if (overallScore) {
            overallScore.textContent = '錯誤';
            overallScore.className = 'health-score-number text-danger';
        }
        
        // 更新各項指標顯示錯誤
        this.updateMetric('savings-score', null);
        this.updateMetric('balance-score', null);
        this.updateMetric('growth-score', null);
    }

    showInsightsError() {
        const container = document.getElementById('smart-insights-container');
        if (container) {
            container.innerHTML = `
                <div class="text-center text-danger p-4">
                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                    <p>載入智慧洞察時發生錯誤</p>
                </div>
            `;
        }
    }

    showAlertsError() {
        const container = document.getElementById('anomaly-alerts-container');
        if (container) {
            container.innerHTML = `
                <div class="text-center text-danger p-4">
                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                    <p>載入異常警報時發生錯誤</p>
                </div>
            `;
        }
    }

    showForecastError() {
        const canvas = document.getElementById('expense-forecast-chart');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.fillStyle = '#dc3545';
            ctx.font = '16px Arial';
            ctx.textAlign = 'center';
            ctx.fillText('載入預測資料失敗', canvas.width / 2, canvas.height / 2);
        }
    }

    showCashFlowError() {
        const canvas = document.getElementById('cash-flow-chart');
        if (canvas) {
            const ctx = canvas.getContext('2d');
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.fillStyle = '#dc3545';
            ctx.font = '16px Arial';
            ctx.textAlign = 'center';
            ctx.fillText('載入現金流資料失敗', canvas.width / 2, canvas.height / 2);
        }
    }

    showSavingsError() {
        const container = document.getElementById('savings-opportunities-container');
        if (container) {
            container.innerHTML = `
                <div class="text-center text-danger p-4">
                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                    <p>載入節約機會時發生錯誤</p>
                </div>
            `;
        }
    }

    showRecommendationsError() {
        const container = document.getElementById('personalized-recommendations-container');
        if (container) {
            container.innerHTML = `
                <div class="text-center text-danger p-4">
                    <i class="fas fa-exclamation-triangle fa-2x mb-3"></i>
                    <p>載入個人化建議時發生錯誤</p>
                </div>
            `;
        }
    }

    /**
     * 取得防偽權杖
     */
    getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }
}

// 建立全域實例
const financialAI = new FinancialAI();

// 當頁面載入完成時初始化
document.addEventListener('DOMContentLoaded', function() {
    financialAI.initialize();
});
