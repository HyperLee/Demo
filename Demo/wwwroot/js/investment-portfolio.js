/**
 * 投資組合總覽頁面 JavaScript
 */

class InvestmentPortfolio {
    constructor() {
        this.portfolios = [];
        this.trendChart = null;
        this.allocationChart = null;
        this.currentPortfolio = null;
        this.init();
    }

    async init() {
        try {
            await this.loadData();
            this.bindEvents();
            this.initCharts();
            this.updateUI();
        } catch (error) {
            console.error('初始化失敗:', error);
            this.showError('初始化失敗，請重新整理頁面');
        }
    }

    async loadData() {
        try {
            const [portfoliosResp, analysisResp] = await Promise.all([
                fetch('/api/InvestmentPortfolio'),
                fetch('/api/InvestmentPortfolio/analysis')
            ]);

            if (!portfoliosResp.ok || !analysisResp.ok) {
                throw new Error('API 請求失敗');
            }

            this.portfolios = await portfoliosResp.json();
            this.analysis = await analysisResp.json();
        } catch (error) {
            console.error('載入資料失敗:', error);
            throw error;
        }
    }

    bindEvents() {
        // 新增投資組合
        $('#savePortfolio').on('click', () => this.savePortfolio());
        
        // 更新投資組合
        $('#updatePortfolio').on('click', () => this.updatePortfolio());
        
        // 確認刪除
        $('#confirmDeletePortfolio').on('click', () => this.deletePortfolio());
        
        // 表單驗證
        $('#addPortfolioForm').on('submit', (e) => e.preventDefault());
        $('#editPortfolioForm').on('submit', (e) => e.preventDefault());
        
        // 重置表單
        $('#addPortfolioModal').on('hidden.bs.modal', () => this.resetForm('add'));
        $('#editPortfolioModal').on('hidden.bs.modal', () => this.resetForm('edit'));
    }

    initCharts() {
        this.initTrendChart();
        this.initAllocationChart();
    }

    initTrendChart() {
        const ctx = document.getElementById('portfolioTrendChart')?.getContext('2d');
        if (!ctx) return;

        this.trendChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: '投資組合價值',
                    data: [],
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return '價值: $' + context.parsed.y.toLocaleString();
                            }
                        }
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

    initAllocationChart() {
        const ctx = document.getElementById('assetAllocationChart')?.getContext('2d');
        if (!ctx) return;

        this.allocationChart = new Chart(ctx, {
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
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.parsed;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${label}: $${value.toLocaleString()} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    }

    updateUI() {
        this.updateStatistics();
        this.updateCharts();
        this.updatePortfolioTable();
    }

    updateStatistics() {
        if (!this.analysis) return;

        const gainLossClass = this.analysis.totalGainLoss >= 0 ? 'bg-success' : 'bg-danger';
        const returnClass = this.analysis.totalReturn >= 0 ? 'bg-info' : 'bg-warning';

        $('#totalAssets').text('$' + this.analysis.totalAssets.toLocaleString());
        $('#totalGainLoss').text(
            (this.analysis.totalGainLoss >= 0 ? '+' : '') + 
            '$' + this.analysis.totalGainLoss.toLocaleString()
        );
        $('#totalReturn').text(
            (this.analysis.totalReturn >= 0 ? '+' : '') + 
            this.analysis.totalReturn.toFixed(2) + '%'
        );
        $('#totalHoldings').text(this.analysis.holdingsCount);

        // 更新卡片顏色
        $('#gainLossCard').attr('class', `card text-white ${gainLossClass}`);
        $('#returnCard').attr('class', `card text-white ${returnClass}`);
    }

    updateCharts() {
        this.updateAllocationChart();
        this.updateTrendChart();
    }

    updateAllocationChart() {
        if (!this.allocationChart || !this.analysis?.assetAllocations) return;

        const labels = this.analysis.assetAllocations.map(a => a.type);
        const values = this.analysis.assetAllocations.map(a => a.value);

        this.allocationChart.data.labels = labels;
        this.allocationChart.data.datasets[0].data = values;
        this.allocationChart.update();
    }

    updateTrendChart() {
        if (!this.trendChart) return;

        // 生成模擬的趨勢數據
        const dates = [];
        const values = [];
        const today = new Date();
        
        for (let i = 29; i >= 0; i--) {
            const date = new Date(today);
            date.setDate(date.getDate() - i);
            dates.push(date.toLocaleDateString());
            
            // 模擬價值變化
            const baseValue = this.analysis?.totalAssets || 100000;
            const variance = (Math.random() - 0.5) * 0.1;
            values.push(baseValue * (1 + variance));
        }

        this.trendChart.data.labels = dates;
        this.trendChart.data.datasets[0].data = values;
        this.trendChart.update();
    }

    updatePortfolioTable() {
        const tbody = $('#portfolioTable tbody');
        tbody.empty();

        if (!this.portfolios || this.portfolios.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="7" class="text-center text-muted py-4">
                        <i class="fas fa-folder-open fa-2x mb-2"></i><br>
                        尚未建立任何投資組合
                    </td>
                </tr>
            `);
            return;
        }

        this.portfolios.forEach(portfolio => {
            const gainLossColor = portfolio.totalGainLoss >= 0 ? 'text-success' : 'text-danger';
            const returnColor = portfolio.totalGainLossPercentage >= 0 ? 'text-success' : 'text-danger';

            tbody.append(`
                <tr>
                    <td>
                        <strong>${portfolio.name}</strong><br>
                        <small class="text-muted">${portfolio.description || '無描述'}</small>
                    </td>
                    <td>$${portfolio.totalValue.toLocaleString()}</td>
                    <td>$${portfolio.totalCost.toLocaleString()}</td>
                    <td class="${gainLossColor}">
                        ${portfolio.totalGainLoss >= 0 ? '+' : ''}$${portfolio.totalGainLoss.toLocaleString()}
                    </td>
                    <td class="${returnColor}">
                        ${portfolio.totalGainLossPercentage >= 0 ? '+' : ''}${portfolio.totalGainLossPercentage.toFixed(2)}%
                    </td>
                    <td>
                        <small>${new Date(portfolio.createdAt).toLocaleDateString()}</small>
                    </td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-info" onclick="investmentPortfolio.viewPortfolio(${portfolio.id})">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-outline-primary" onclick="investmentPortfolio.editPortfolio(${portfolio.id})">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-outline-danger" onclick="investmentPortfolio.confirmDeletePortfolio(${portfolio.id})">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `);
        });
    }

    async savePortfolio() {
        const name = $('#portfolioName').val().trim();
        const description = $('#portfolioDescription').val().trim();

        if (!name) {
            this.showValidationError('portfolioName', '請輸入組合名稱');
            return;
        }

        const spinner = $('#savePortfolioSpinner');
        const button = $('#savePortfolio');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch('/api/InvestmentPortfolio', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ name, description })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '建立失敗');
            }

            await this.loadData();
            this.updateUI();
            $('#addPortfolioModal').modal('hide');
            this.showSuccess('投資組合建立成功');
        } catch (error) {
            console.error('建立投資組合失敗:', error);
            this.showError(error.message || '建立失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
        }
    }

    editPortfolio(id) {
        const portfolio = this.portfolios.find(p => p.id === id);
        if (!portfolio) return;

        $('#editPortfolioId').val(portfolio.id);
        $('#editPortfolioName').val(portfolio.name);
        $('#editPortfolioDescription').val(portfolio.description);
        
        $('#editPortfolioModal').modal('show');
    }

    async updatePortfolio() {
        const id = parseInt($('#editPortfolioId').val());
        const name = $('#editPortfolioName').val().trim();
        const description = $('#editPortfolioDescription').val().trim();

        if (!name) {
            this.showValidationError('editPortfolioName', '請輸入組合名稱');
            return;
        }

        const spinner = $('#updatePortfolioSpinner');
        const button = $('#updatePortfolio');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch(`/api/InvestmentPortfolio/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ id, name, description })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '更新失敗');
            }

            await this.loadData();
            this.updateUI();
            $('#editPortfolioModal').modal('hide');
            this.showSuccess('投資組合更新成功');
        } catch (error) {
            console.error('更新投資組合失敗:', error);
            this.showError(error.message || '更新失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
        }
    }

    confirmDeletePortfolio(id) {
        const portfolio = this.portfolios.find(p => p.id === id);
        if (!portfolio) return;

        $('#deletePortfolioName').text(portfolio.name);
        this.currentPortfolio = portfolio;
        $('#confirmDeleteModal').modal('show');
    }

    async deletePortfolio() {
        if (!this.currentPortfolio) return;

        const spinner = $('#deletePortfolioSpinner');
        const button = $('#confirmDeletePortfolio');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch(`/api/InvestmentPortfolio/${this.currentPortfolio.id}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '刪除失敗');
            }

            await this.loadData();
            this.updateUI();
            $('#confirmDeleteModal').modal('hide');
            this.showSuccess('投資組合已刪除');
        } catch (error) {
            console.error('刪除投資組合失敗:', error);
            this.showError(error.message || '刪除失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
            this.currentPortfolio = null;
        }
    }

    viewPortfolio(id) {
        window.location.href = `/investment-holdings?portfolioId=${id}`;
    }

    resetForm(type) {
        const prefix = type === 'add' ? '' : 'edit';
        $(`#${prefix}portfolioName`).val('').removeClass('is-invalid');
        $(`#${prefix}portfolioDescription`).val('');
        if (type === 'edit') {
            $('#editPortfolioId').val('');
        }
    }

    showValidationError(fieldId, message) {
        $(`#${fieldId}`).addClass('is-invalid');
        $(`#${fieldId}`).siblings('.invalid-feedback').text(message);
    }

    showSuccess(message) {
        this.showToast(message, 'success');
    }

    showError(message) {
        this.showToast(message, 'danger');
    }

    showToast(message, type = 'info') {
        const toastId = 'toast-' + Date.now();
        const toast = $(`
            <div class="toast align-items-center text-white bg-${type} border-0" id="${toastId}" role="alert">
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `);

        // 確保有 toast container
        if (!$('.toast-container').length) {
            $('body').append('<div class="toast-container position-fixed bottom-0 end-0 p-3"></div>');
        }

        $('.toast-container').append(toast);
        
        const bsToast = new bootstrap.Toast(toast[0], { delay: 3000 });
        bsToast.show();

        toast[0].addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }
}

// 初始化
let investmentPortfolio;
$(document).ready(function() {
    investmentPortfolio = new InvestmentPortfolio();
});
