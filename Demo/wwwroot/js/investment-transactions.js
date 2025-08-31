/**
 * 投資交易記錄頁面 JavaScript
 */

class InvestmentTransactions {
    constructor() {
        this.transactions = [];
        this.portfolios = [];
        this.currentTransaction = null;
        this.filters = {
            portfolioId: null,
            startDate: null,
            endDate: null,
            type: null,
            symbol: null
        };
        this.init();
    }

    async init() {
        try {
            await this.loadData();
            this.bindEvents();
            this.updateUI();
            this.setDefaultDates();
        } catch (error) {
            console.error('初始化失敗:', error);
            this.showError('初始化失敗，請重新整理頁面');
        }
    }

    async loadData() {
        try {
            const [transactionsResp, portfoliosResp] = await Promise.all([
                fetch('/api/InvestmentTransactions'),
                fetch('/api/InvestmentPortfolio')
            ]);

            if (!transactionsResp.ok || !portfoliosResp.ok) {
                throw new Error('API 請求失敗');
            }

            this.transactions = await transactionsResp.json();
            this.portfolios = await portfoliosResp.json();
        } catch (error) {
            console.error('載入資料失敗:', error);
            throw error;
        }
    }

    bindEvents() {
        // 快速新增交易
        $('#addQuickTransaction').on('click', () => this.addQuickTransaction());

        // 詳細新增交易
        $('#saveTransaction').on('click', () => this.saveTransaction());
        
        // 確認刪除
        $('#confirmDeleteTransaction').on('click', () => this.deleteTransaction());

        // 篩選器
        $('#applyFilters').on('click', () => this.applyFilters());
        $('#clearFilters').on('click', () => this.clearFilters());

        // 匯出
        $('#exportTransactionsBtn').on('click', () => this.exportTransactions());

        // 表單計算
        $('#transactionQuantity, #transactionPrice, #transactionFee').on('input', () => this.updateCalculation());

        // 日期預設為今天
        $('#transactionDate').val(new Date().toISOString().split('T')[0]);

        // 表單重置
        $('#addTransactionModal').on('hidden.bs.modal', () => this.resetForm());
    }

    updateUI() {
        this.loadPortfolioOptions();
        this.updateTransactionsTable();
        this.updateStatistics();
    }

    setDefaultDates() {
        const today = new Date();
        const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
        
        $('#startDate').val(firstDayOfMonth.toISOString().split('T')[0]);
        $('#endDate').val(today.toISOString().split('T')[0]);
    }

    loadPortfolioOptions() {
        const selects = ['#portfolioFilter', '#transactionPortfolioId'];
        
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

    updateTransactionsTable() {
        const tbody = $('#transactionsTable tbody');
        tbody.empty();

        let filteredTransactions = this.getFilteredTransactions();

        if (filteredTransactions.length === 0) {
            $('#noTransactionsMessage').removeClass('d-none');
            tbody.closest('.table-responsive').addClass('d-none');
            $('#transactionStats').addClass('d-none');
            return;
        }

        $('#noTransactionsMessage').addClass('d-none');
        tbody.closest('.table-responsive').removeClass('d-none');
        $('#transactionStats').removeClass('d-none');

        filteredTransactions.forEach(transaction => {
            const portfolio = this.portfolios.find(p => p.id === transaction.portfolioId);
            const typeColor = this.getTypeColor(transaction.type);
            const netAmount = transaction.type === '買入' 
                ? -(transaction.totalAmount + transaction.fee)
                : transaction.totalAmount - transaction.fee;

            tbody.append(`
                <tr>
                    <td>${new Date(transaction.date).toLocaleDateString()}</td>
                    <td>
                        <span class="badge ${typeColor}">${transaction.type}</span>
                    </td>
                    <td>
                        <strong>${transaction.symbol}</strong><br>
                        <small class="text-muted">${portfolio?.name || '未知組合'}</small>
                    </td>
                    <td>${transaction.quantity.toLocaleString()}</td>
                    <td>$${transaction.price.toFixed(2)}</td>
                    <td>$${transaction.totalAmount.toLocaleString()}</td>
                    <td>$${transaction.fee.toFixed(2)}</td>
                    <td class="${netAmount >= 0 ? 'text-success' : 'text-danger'}">
                        ${netAmount >= 0 ? '+' : ''}$${Math.abs(netAmount).toLocaleString()}
                    </td>
                    <td>
                        <span title="${transaction.note}" class="text-truncate d-inline-block" style="max-width: 150px;">
                            ${transaction.note || '-'}
                        </span>
                    </td>
                    <td>
                        <button class="btn btn-outline-danger btn-sm" 
                                onclick="investmentTransactions.confirmDeleteTransaction(${transaction.id})" 
                                title="刪除">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
        });
    }

    updateStatistics() {
        const filteredTransactions = this.getFilteredTransactions();
        
        const stats = {
            total: filteredTransactions.length,
            buyAmount: 0,
            sellAmount: 0,
            dividendAmount: 0,
            totalFees: 0
        };

        filteredTransactions.forEach(tx => {
            stats.totalFees += tx.fee;
            
            switch (tx.type) {
                case '買入':
                    stats.buyAmount += tx.totalAmount;
                    break;
                case '賣出':
                    stats.sellAmount += tx.totalAmount;
                    break;
                case '股息':
                    stats.dividendAmount += tx.totalAmount;
                    break;
            }
        });

        $('#totalTransactions').text(stats.total);
        $('#totalBuyAmount').text('$' + stats.buyAmount.toLocaleString());
        $('#totalSellAmount').text('$' + stats.sellAmount.toLocaleString());
        $('#totalFees').text('$' + stats.totalFees.toLocaleString());
    }

    getFilteredTransactions() {
        return this.transactions.filter(transaction => {
            if (this.filters.portfolioId && transaction.portfolioId != this.filters.portfolioId) {
                return false;
            }
            if (this.filters.type && transaction.type !== this.filters.type) {
                return false;
            }
            if (this.filters.symbol) {
                const search = this.filters.symbol.toLowerCase();
                if (!transaction.symbol.toLowerCase().includes(search)) {
                    return false;
                }
            }
            if (this.filters.startDate) {
                if (new Date(transaction.date) < new Date(this.filters.startDate)) {
                    return false;
                }
            }
            if (this.filters.endDate) {
                if (new Date(transaction.date) > new Date(this.filters.endDate)) {
                    return false;
                }
            }
            return true;
        });
    }

    getTypeColor(type) {
        switch (type) {
            case '買入': return 'bg-success';
            case '賣出': return 'bg-warning';
            case '股息': return 'bg-info';
            default: return 'bg-secondary';
        }
    }

    applyFilters() {
        this.filters.portfolioId = $('#portfolioFilter').val() || null;
        this.filters.startDate = $('#startDate').val() || null;
        this.filters.endDate = $('#endDate').val() || null;
        this.filters.type = $('#transactionTypeFilter').val() || null;
        this.filters.symbol = $('#symbolFilter').val().trim() || null;
        
        this.updateTransactionsTable();
        this.updateStatistics();
    }

    clearFilters() {
        $('#portfolioFilter').val('');
        $('#startDate').val('');
        $('#endDate').val('');
        $('#transactionTypeFilter').val('');
        $('#symbolFilter').val('');
        
        this.filters = {
            portfolioId: null,
            startDate: null,
            endDate: null,
            type: null,
            symbol: null
        };
        
        this.updateTransactionsTable();
        this.updateStatistics();
    }

    async addQuickTransaction() {
        const type = $('#quickTransactionType').val();
        const symbol = $('#quickSymbol').val().trim().toUpperCase();
        const quantity = parseInt($('#quickQuantity').val());
        const price = parseFloat($('#quickPrice').val());
        const fee = parseFloat($('#quickFee').val()) || 0;

        if (!symbol || !quantity || !price) {
            this.showError('請填寫股票代號、數量和價格');
            return;
        }

        if (this.portfolios.length === 0) {
            this.showError('請先建立投資組合');
            return;
        }

        const portfolioId = this.portfolios[0].id; // 使用第一個投資組合

        const transaction = {
            portfolioId,
            type,
            symbol,
            quantity,
            price,
            fee,
            date: new Date().toISOString().split('T')[0],
            note: ''
        };

        const button = $('#addQuickTransaction');
        const originalText = button.html();
        
        try {
            button.html('<span class="spinner-border spinner-border-sm"></span> 新增中...').prop('disabled', true);

            const response = await fetch('/api/InvestmentTransactions', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(transaction)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '建立失敗');
            }

            // 清空快速輸入表單
            $('#quickSymbol').val('');
            $('#quickQuantity').val('');
            $('#quickPrice').val('');
            $('#quickFee').val('0');

            await this.loadData();
            this.updateUI();
            this.showSuccess('交易記錄建立成功');
        } catch (error) {
            console.error('建立交易記錄失敗:', error);
            this.showError(error.message || '建立失敗');
        } finally {
            button.html(originalText).prop('disabled', false);
        }
    }

    async saveTransaction() {
        const formData = this.getTransactionFormData();
        if (!formData) return;

        const spinner = $('#saveTransactionSpinner');
        const button = $('#saveTransaction');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch('/api/InvestmentTransactions', {
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
            $('#addTransactionModal').modal('hide');
            this.showSuccess('交易記錄建立成功');
        } catch (error) {
            console.error('建立交易記錄失敗:', error);
            this.showError(error.message || '建立失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
        }
    }

    getTransactionFormData() {
        const portfolioId = parseInt($('#transactionPortfolioId').val());
        const type = $('#transactionType').val();
        const symbol = $('#transactionSymbol').val().trim().toUpperCase();
        const date = $('#transactionDate').val();
        const quantity = parseInt($('#transactionQuantity').val());
        const price = parseFloat($('#transactionPrice').val());
        const fee = parseFloat($('#transactionFee').val()) || 0;
        const note = $('#transactionNote').val().trim();

        // 驗證
        if (!portfolioId || !type || !symbol || !date || !quantity || !price) {
            this.showError('請填寫所有必填欄位');
            return null;
        }

        return {
            portfolioId,
            type,
            symbol,
            date,
            quantity,
            price,
            fee,
            note
        };
    }

    confirmDeleteTransaction(id) {
        const transaction = this.transactions.find(t => t.id === id);
        if (!transaction) return;

        this.currentTransaction = transaction;
        $('#confirmDeleteTransactionModal').modal('show');
    }

    async deleteTransaction() {
        if (!this.currentTransaction) return;

        const spinner = $('#deleteTransactionSpinner');
        const button = $('#confirmDeleteTransaction');
        
        try {
            spinner.removeClass('d-none');
            button.prop('disabled', true);

            const response = await fetch(`/api/InvestmentTransactions/${this.currentTransaction.id}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || '刪除失敗');
            }

            await this.loadData();
            this.updateUI();
            $('#confirmDeleteTransactionModal').modal('hide');
            this.showSuccess('交易記錄已刪除');
        } catch (error) {
            console.error('刪除交易記錄失敗:', error);
            this.showError(error.message || '刪除失敗');
        } finally {
            spinner.addClass('d-none');
            button.prop('disabled', false);
            this.currentTransaction = null;
        }
    }

    updateCalculation() {
        const quantity = parseInt($('#transactionQuantity').val()) || 0;
        const price = parseFloat($('#transactionPrice').val()) || 0;
        const fee = parseFloat($('#transactionFee').val()) || 0;

        const amount = quantity * price;
        const net = amount - fee;

        $('#calculatedAmount').text('$' + amount.toLocaleString());
        $('#calculatedNet').text('$' + net.toLocaleString());
    }

    async exportTransactions() {
        try {
            const queryParams = new URLSearchParams();
            
            if (this.filters.portfolioId) {
                queryParams.append('portfolioId', this.filters.portfolioId);
            }
            if (this.filters.startDate) {
                queryParams.append('startDate', this.filters.startDate);
            }
            if (this.filters.endDate) {
                queryParams.append('endDate', this.filters.endDate);
            }
            
            queryParams.append('format', 'csv');

            const url = `/api/InvestmentTransactions/export?${queryParams.toString()}`;
            window.open(url, '_blank');
            
            this.showSuccess('匯出檔案已下載');
        } catch (error) {
            console.error('匯出失敗:', error);
            this.showError('匯出失敗');
        }
    }

    resetForm() {
        $('#transactionPortfolioId').val('');
        $('#transactionType').val('');
        $('#transactionSymbol').val('');
        $('#transactionDate').val(new Date().toISOString().split('T')[0]);
        $('#transactionQuantity').val('');
        $('#transactionPrice').val('');
        $('#transactionFee').val('0');
        $('#transactionNote').val('');
        
        $('#calculatedAmount').text('$0');
        $('#calculatedNet').text('$0');

        $('.form-control').removeClass('is-invalid');
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
let investmentTransactions;
$(document).ready(function() {
    investmentTransactions = new InvestmentTransactions();
});
