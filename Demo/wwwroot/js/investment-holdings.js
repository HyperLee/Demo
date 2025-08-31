/**
 * 投資持倉管理頁面 JavaScript
 */

class InvestmentHoldings {
    constructor() {
        this.holdings = [];
        this.portfolios = [];
        this.currentHolding = null;
        this.filters = {
            portfolioId: null,
            type: null,
            search: null
        };
        this.init();
    }

    async init() {
        try {
            await this.loadData();
            this.bindEvents();
            this.updateUI();
        } catch (error) {
            console.error('初始化失敗:', error);
            this.showError('初始化失敗，請重新整理頁面');
        }
    }

    async loadData() {
        try {
            const [holdingsResp, portfoliosResp] = await Promise.all([
                fetch('/api/InvestmentHoldings'),
                fetch('/api/InvestmentPortfolio')
            ]);

            if (!holdingsResp.ok || !portfoliosResp.ok) {
                throw new Error('API 請求失敗');
            }

            this.holdings = await holdingsResp.json();
            this.portfolios = await portfoliosResp.json();
        } catch (error) {
            console.error('載入資料失敗:', error);
            throw error;
        }
    }

    bindEvents() {
        // 篩選器
        $('#portfolioFilter').on('change', () => this.applyFilters());
        $('#typeFilter').on('change', () => this.applyFilters());
        $('#searchBtn').on('click', () => this.applyFilters());
        $('#searchSymbol').on('keypress', (e) => {
            if (e.which === 13) this.applyFilters();
        });

        // 新增持倉
        $('#saveHolding').on('click', () => this.saveHolding());
        
        // 更新持倉
        $('#updateHolding').on('click', () => this.updateHolding());
        
        // 確認刪除
        $('#confirmDeleteHolding').on('click', () => this.deleteHolding());

        // 股價更新
        $('#refreshPricesBtn').on('click', () => this.refreshPrices());

        // 股票查詢
        $('#lookupSymbol').on('click', () => this.openStockSearch());
        $('#stockSearchBtn').on('click', () => this.searchStocks());
        $('#stockSearchInput').on('keypress', (e) => {
            if (e.which === 13) this.searchStocks();
        });

        // 獲取當前股價
        $('#fetchCurrentPrice').on('click', () => this.fetchCurrentPrice('holding'));
        $('#fetchEditCurrentPrice').on('click', () => this.fetchCurrentPrice('editHolding'));

        // 表單計算
        $('#holdingQuantity, #holdingAvgCost, #holdingCurrentPrice').on('input', () => this.updatePreview());

        // 表單重置
        $('#addHoldingModal').on('hidden.bs.modal', () => this.resetForm('add'));
        $('#editHoldingModal').on('hidden.bs.modal', () => this.resetForm('edit'));
        $('#stockSearchModal').on('hidden.bs.modal', () => this.resetStockSearch());
    }

    updateUI() {
        this.loadPortfolioOptions();
        this.updateHoldingsTable();
    }

    loadPortfolioOptions() {
        const selects = ['#portfolioFilter', '#holdingPortfolioId'];
        
        selects.forEach(selector => {
            const $select = $(selector);
            const currentValue = $select.val();
            
            if (selector === '#portfolioFilter') {
                $select.html('<option value="">全部組合</option>');
            } else {
                $select.html('<option value="">選擇投資組合</option>');
            }
            
            this.portfolios.forEach(portfolio => {
                $select.append(`<option value="${portfolio.id}">${portfolio.name}</option>`);
            });
            
            if (currentValue) {
                $select.val(currentValue);
            }
        });
    }

    updateHoldingsTable() {
        const tbody = $('#holdingsTable tbody');
        tbody.empty();

        let filteredHoldings = this.getFilteredHoldings();

        if (filteredHoldings.length === 0) {
            $('#noHoldingsMessage').removeClass('d-none');
            tbody.closest('.table-responsive').addClass('d-none');
            return;
        }

        $('#noHoldingsMessage').addClass('d-none');
        tbody.closest('.table-responsive').removeClass('d-none');

        filteredHoldings.forEach(holding => {
            const portfolio = this.portfolios.find(p => p.id === holding.portfolioId);
            const gainLossColor = holding.gainLoss >= 0 ? 'text-success' : 'text-danger';
            const returnColor = holding.gainLossPercentage >= 0 ? 'text-success' : 'text-danger';

            tbody.append(`
                <tr>
                    <td>
                        <strong>${holding.symbol}</strong><br>
                        <small class="text-muted">${portfolio?.name || '未知組合'}</small>
                    </td>
                    <td>${holding.name}</td>
                    <td>
                        <span class="badge bg-secondary">${holding.type}</span>
                    </td>
                    <td>${holding.quantity.toLocaleString()}</td>
                    <td>$${holding.averageCost.toFixed(2)}</td>
                    <td>
                        $${holding.currentPrice.toFixed(2)}<br>
                        <small class="text-muted">${new Date(holding.lastUpdated).toLocaleString()}</small>
                    </td>
                    <td>$${holding.marketValue.toLocaleString()}</td>
                    <td class="${gainLossColor}">
                        ${holding.gainLoss >= 0 ? '+' : ''}$${holding.gainLoss.toLocaleString()}
                    </td>
                    <td class="${returnColor}">
                        ${holding.gainLossPercentage >= 0 ? '+' : ''}${holding.gainLossPercentage.toFixed(2)}%
                    </td>
                    <td>
                        <small>${new Date(holding.lastUpdated).toLocaleDateString()}</small>
                    </td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary" onclick="investmentHoldings.editHolding(${holding.id})" title="編輯">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-outline-danger" onclick="investmentHoldings.confirmDeleteHolding(${holding.id})" title="刪除">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `);
        });
    }

    getFilteredHoldings() {
        return this.holdings.filter(holding => {
            if (this.filters.portfolioId && holding.portfolioId != this.filters.portfolioId) {
                return false;
            }
            if (this.filters.type && holding.type !== this.filters.type) {
                return false;
            }
            if (this.filters.search) {
                const search = this.filters.search.toLowerCase();
                return holding.symbol.toLowerCase().includes(search) || 
                       holding.name.toLowerCase().includes(search);
            }
            return true;
        });
    }

    applyFilters() {
        this.filters.portfolioId = $('#portfolioFilter').val() || null;
        this.filters.type = $('#typeFilter').val() || null;
        this.filters.search = $('#searchSymbol').val().trim() || null;
        
        this.updateHoldingsTable();
    }

    async saveHolding() {
        const formData = this.getHoldingFormData();
        if (!formData) return;

        const spinner = $('#saveHoldingSpinner');
        const button = $('#saveHolding');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch('/api/InvestmentHoldings', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '建立失敗');
            }

            await this.loadData();
            this.updateUI();
            $('#addHoldingModal').modal('hide');
            this.showSuccess('持倉建立成功');
        } catch (error) {
            console.error('建立持倉失敗:', error);
            this.showError(error.message || '建立失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
        }
    }

    getHoldingFormData(isEdit = false) {
        const prefix = isEdit ? 'editHolding' : 'holding';
        
        const portfolioId = parseInt($(`#${prefix}PortfolioId`).val());
        const symbol = $(`#${prefix}Symbol`).val().trim();
        const name = $(`#${prefix}Name`).val().trim();
        const type = $(`#${prefix}Type`).val();
        const quantity = parseInt($(`#${prefix}Quantity`).val());
        const averageCost = parseFloat($(`#${prefix}AvgCost`).val());
        const currentPrice = parseFloat($(`#${prefix}CurrentPrice`).val()) || 0;

        // 驗證
        if (!portfolioId || !symbol || !name || !type || !quantity || !averageCost) {
            this.showError('請填寫所有必填欄位');
            return null;
        }

        const data = {
            portfolioId,
            symbol: symbol.toUpperCase(),
            name,
            type,
            quantity,
            averageCost,
            currentPrice
        };

        if (isEdit) {
            data.id = parseInt($('#editHoldingId').val());
        }

        return data;
    }

    editHolding(id) {
        const holding = this.holdings.find(h => h.id === id);
        if (!holding) return;

        $('#editHoldingId').val(holding.id);
        $('#editHoldingPortfolioId').val(holding.portfolioId);
        $('#editHoldingSymbol').val(holding.symbol);
        $('#editHoldingName').val(holding.name);
        $('#editHoldingType').val(holding.type);
        $('#editHoldingQuantity').val(holding.quantity);
        $('#editHoldingAvgCost').val(holding.averageCost);
        $('#editHoldingCurrentPrice').val(holding.currentPrice);

        $('#editHoldingModal').modal('show');
    }

    async updateHolding() {
        const formData = this.getHoldingFormData(true);
        if (!formData) return;

        const spinner = $('#updateHoldingSpinner');
        const button = $('#updateHolding');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch(`/api/InvestmentHoldings/${formData.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '更新失敗');
            }

            await this.loadData();
            this.updateUI();
            $('#editHoldingModal').modal('hide');
            this.showSuccess('持倉更新成功');
        } catch (error) {
            console.error('更新持倉失敗:', error);
            this.showError(error.message || '更新失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
        }
    }

    confirmDeleteHolding(id) {
        const holding = this.holdings.find(h => h.id === id);
        if (!holding) return;

        $('#deleteHoldingName').text(`${holding.symbol} - ${holding.name}`);
        this.currentHolding = holding;
        $('#confirmDeleteHoldingModal').modal('show');
    }

    async deleteHolding() {
        if (!this.currentHolding) return;

        const spinner = $('#deleteHoldingSpinner');
        const button = $('#confirmDeleteHolding');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch(`/api/InvestmentHoldings/${this.currentHolding.id}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '刪除失敗');
            }

            await this.loadData();
            this.updateUI();
            $('#confirmDeleteHoldingModal').modal('hide');
            this.showSuccess('持倉已刪除');
        } catch (error) {
            console.error('刪除持倉失敗:', error);
            this.showError(error.message || '刪除失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
            this.currentHolding = null;
        }
    }

    async refreshPrices() {
        const button = $('#refreshPricesBtn');
        const icon = button.find('i');
        
        try {
            button.prop('disabled', true);
            icon.addClass('fa-spin');

            const response = await fetch('/api/InvestmentPortfolio/update-prices', {
                method: 'POST'
            });

            if (!response.ok) {
                throw new Error('更新股價失敗');
            }

            await this.loadData();
            this.updateUI();
            this.showSuccess('股價已更新');
        } catch (error) {
            console.error('更新股價失敗:', error);
            this.showError('更新股價失敗');
        } finally {
            button.prop('disabled', false);
            icon.removeClass('fa-spin');
        }
    }

    openStockSearch() {
        $('#stockSearchModal').modal('show');
    }

    async searchStocks() {
        const query = $('#stockSearchInput').val().trim();
        if (!query) return;

        const loading = $('#stockSearchLoading');
        const results = $('#stockSearchResults');
        
        try {
            loading.removeClass('d-none');
            results.empty();

            const response = await fetch(`/api/InvestmentPortfolio/search-stocks?query=${encodeURIComponent(query)}`);
            
            if (!response.ok) {
                throw new Error('搜尋失敗');
            }

            const stocks = await response.json();
            
            if (stocks.length === 0) {
                results.html('<div class="alert alert-info">找不到相關股票</div>');
                return;
            }

            stocks.forEach(stock => {
                results.append(`
                    <div class="border rounded p-2 mb-2 stock-search-item" style="cursor: pointer;" 
                         onclick="investmentHoldings.selectStock('${stock.symbol}', '${stock.name}')">
                        <div class="d-flex justify-content-between">
                            <div>
                                <strong>${stock.symbol}</strong>
                                <span class="ms-2">${stock.name}</span>
                            </div>
                            <small class="text-muted">${stock.exchange}</small>
                        </div>
                    </div>
                `);
            });
        } catch (error) {
            console.error('搜尋股票失敗:', error);
            results.html('<div class="alert alert-danger">搜尋失敗</div>');
        } finally {
            loading.addClass('d-none');
        }
    }

    selectStock(symbol, name) {
        $('#holdingSymbol').val(symbol);
        $('#holdingName').val(name);
        $('#stockSearchModal').modal('hide');
    }

    async fetchCurrentPrice(prefix) {
        const symbol = $(`#${prefix}Symbol`).val().trim();
        if (!symbol) {
            this.showError('請先輸入股票代號');
            return;
        }

        const button = $(`#fetch${prefix === 'holding' ? 'Current' : 'Edit'}Price`);
        const icon = button.find('i');
        
        try {
            button.prop('disabled', true);
            icon.addClass('fa-spin');

            const response = await fetch(`/api/InvestmentPortfolio/stock-price/${encodeURIComponent(symbol)}`);
            
            if (!response.ok) {
                throw new Error('獲取股價失敗');
            }

            const price = await response.json();
            $(`#${prefix}CurrentPrice`).val(price.price);
            
            if (prefix === 'holding') {
                this.updatePreview();
            }
            
            this.showSuccess(`已獲取 ${symbol} 最新股價: $${price.price}`);
        } catch (error) {
            console.error('獲取股價失敗:', error);
            this.showError('獲取股價失敗');
        } finally {
            button.prop('disabled', false);
            icon.removeClass('fa-spin');
        }
    }

    updatePreview() {
        const quantity = parseInt($('#holdingQuantity').val()) || 0;
        const avgCost = parseFloat($('#holdingAvgCost').val()) || 0;
        const currentPrice = parseFloat($('#holdingCurrentPrice').val()) || 0;

        const totalCost = quantity * avgCost;
        const marketValue = quantity * currentPrice;
        const gainLoss = marketValue - totalCost;
        const gainLossPercentage = avgCost > 0 ? ((currentPrice - avgCost) / avgCost) * 100 : 0;

        if (quantity > 0 && avgCost > 0) {
            const previewContent = `
                總成本: $${totalCost.toLocaleString()}<br>
                市場價值: $${marketValue.toLocaleString()}<br>
                損益: <span class="${gainLoss >= 0 ? 'text-success' : 'text-danger'}">
                    ${gainLoss >= 0 ? '+' : ''}$${gainLoss.toLocaleString()}
                </span><br>
                報酬率: <span class="${gainLossPercentage >= 0 ? 'text-success' : 'text-danger'}">
                    ${gainLossPercentage >= 0 ? '+' : ''}${gainLossPercentage.toFixed(2)}%
                </span>
            `;
            
            $('#holdingPreviewContent').html(previewContent);
            $('#holdingPreview').removeClass('d-none');
        } else {
            $('#holdingPreview').addClass('d-none');
        }
    }

    resetForm(type) {
        const prefix = type === 'add' ? 'holding' : 'editHolding';
        
        $(`#${prefix}PortfolioId`).val('');
        $(`#${prefix}Symbol`).val('');
        $(`#${prefix}Name`).val('');
        $(`#${prefix}Type`).val('');
        $(`#${prefix}Quantity`).val('');
        $(`#${prefix}AvgCost`).val('');
        $(`#${prefix}CurrentPrice`).val('');
        
        if (type === 'add') {
            $('#holdingPreview').addClass('d-none');
        } else {
            $('#editHoldingId').val('');
        }

        $(`.form-control`).removeClass('is-invalid');
    }

    resetStockSearch() {
        $('#stockSearchInput').val('');
        $('#stockSearchResults').empty();
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
let investmentHoldings;
$(document).ready(function() {
    investmentHoldings = new InvestmentHoldings();
});
