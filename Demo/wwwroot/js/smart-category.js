/**
 * 智能分類管理器
 * 負責處理智能分類建議、用戶互動和回饋學習
 */
class SmartCategoryManager {
    constructor() {
        this.suggestions = [];
        this.isLearningMode = false;
        this.debounceTimer = null;
        this.initializeEventListeners();
    }

    /**
     * 初始化事件監聽器
     */
    initializeEventListeners() {
        // 描述輸入時觸發智能分類（防抖處理）
        $('#description').on('input', this.handleDescriptionInput.bind(this));
        
        // 商家輸入時觸發智能分類
        $('#merchant').on('input', this.handleMerchantInput.bind(this));
        
        // 金額輸入時觸發智能分類
        $('.money-input').on('input', this.handleAmountInput.bind(this));
        
        // 智能分類按鈕
        $('#smartCategoryBtn').on('click', this.requestSmartCategory.bind(this));
        
        // 採用建議
        $(document).on('click', '.apply-suggestion', this.applySuggestion.bind(this));
        
        // 回饋按鈕
        $('#feedbackCorrect').on('click', () => this.submitFeedback(true));
        $('#feedbackIncorrect').on('click', () => this.submitFeedback(false));
        
        // 分類手動選擇時顯示回饋選項
        $('#categorySelect').on('change', this.handleManualCategoryChange.bind(this));
    }

    /**
     * 處理描述輸入
     */
    handleDescriptionInput() {
        this.triggerSmartCategoryWithDebounce();
    }

    /**
     * 處理商家輸入
     */
    handleMerchantInput() {
        this.triggerSmartCategoryWithDebounce();
    }

    /**
     * 處理金額輸入
     */
    handleAmountInput() {
        this.triggerSmartCategoryWithDebounce();
    }

    /**
     * 防抖觸發智能分類
     */
    triggerSmartCategoryWithDebounce() {
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }
        
        this.debounceTimer = setTimeout(() => {
            this.requestSmartCategory();
        }, 500);
    }

    /**
     * 請求智能分類建議
     */
    async requestSmartCategory() {
        const description = $('#description').val()?.trim() || '';
        const amount = parseFloat($('.money-input').val()) || 0;
        const merchant = $('#merchant').val()?.trim() || '';

        // 如果沒有輸入任何資訊，隱藏建議
        if (!description && !merchant && amount === 0) {
            this.hideSuggestions();
            return;
        }

        // 顯示載入狀態
        this.showLoadingState();

        try {
            const response = await fetch('/api/smart-category/suggest', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify({
                    description: description,
                    amount: amount,
                    merchant: merchant,
                    maxSuggestions: 3
                })
            });

            if (response.ok) {
                const data = await response.json();
                this.suggestions = data.suggestions || [];
                this.displaySuggestions();
            } else {
                console.error('智能分類請求失敗:', response.status);
                this.hideLoadingState();
            }
        } catch (error) {
            console.error('智能分類請求錯誤:', error);
            this.hideLoadingState();
        }
    }

    /**
     * 顯示分類建議
     */
    displaySuggestions() {
        const container = $('#categorySuggestions');
        const template = $('#suggestionCardTemplate').html();
        
        container.empty();
        
        if (!this.suggestions || this.suggestions.length === 0) {
            container.html(`
                <div class="text-muted text-center py-3">
                    <i class="fas fa-search"></i> 找不到合適的分類建議
                </div>
            `);
        } else {
            this.suggestions.forEach(suggestion => {
                const confidencePercent = Math.round(suggestion.confidence * 100);
                let card = template;
                
                // 替換樣板變數
                card = card.replace(/\{\{categoryId\}\}/g, suggestion.categoryId);
                card = card.replace(/\{\{categoryName\}\}/g, suggestion.categoryName);
                card = card.replace(/\{\{iconClass\}\}/g, suggestion.iconClass || 'fas fa-tag');
                card = card.replace(/\{\{reason\}\}/g, suggestion.reason);
                card = card.replace(/\{\{confidence\}\}/g, suggestion.confidence);
                card = card.replace(/\{\{confidencePercent\}\}/g, confidencePercent);
                
                container.append(card);
            });
        }
        
        $('#smartCategorySection').show();
        this.hideLoadingState();
    }

    /**
     * 採用分類建議
     */
    applySuggestion(event) {
        const $card = $(event.target).closest('.suggestion-card');
        const categoryId = $card.data('category-id');
        const confidence = $card.data('confidence');
        
        // 設定分類選擇（這裡需要根據實際的分類系統調整）
        this.setCategorySelection(categoryId);
        
        // 顯示回饋選項
        $('#categoryFeedback').show();
        
        // 高亮選中的建議
        $('.suggestion-card').removeClass('selected');
        $card.addClass('selected');
        
        // 記錄選擇供學習使用
        this.recordSuggestionUsage(categoryId, confidence);
        
        this.showSuccessMessage('已採用智能分類建議！');
    }

    /**
     * 設定分類選擇
     */
    setCategorySelection(categoryId) {
        // 根據 categoryId 設定對應的大分類
        // 這裡需要建立 categoryId 到實際分類選項的對應關係
        const categoryMapping = {
            'food': '餐飲',
            'transport': '交通',
            'shopping': '購物',
            'medical': '醫療',
            'entertainment': '娛樂',
            'daily': '日常'
        };

        const categoryName = categoryMapping[categoryId];
        if (categoryName) {
            // 嘗試在分類選擇器中找到對應的選項
            const $categorySelect = $('#categorySelect');
            const $option = $categorySelect.find('option').filter(function() {
                return $(this).text().includes(categoryName);
            });
            
            if ($option.length > 0) {
                $categorySelect.val($option.val()).trigger('change');
            }
        }
    }

    /**
     * 提交用戶回饋
     */
    async submitFeedback(isCorrect) {
        const categoryId = $('#categorySelect').val();
        const description = $('#description').val() || '';
        const amount = parseFloat($('.money-input').val()) || 0;
        const merchant = $('#merchant').val() || '';
        
        if (!categoryId) {
            alert('請先選擇分類');
            return;
        }
        
        try {
            const response = await fetch('/api/smart-category/feedback', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify({
                    categoryId: categoryId,
                    description: description,
                    amount: amount,
                    merchant: merchant,
                    isCorrect: isCorrect
                })
            });
            
            if (response.ok) {
                if (isCorrect) {
                    this.showLearningAlert('感謝回饋！系統已記錄這個正確分類。');
                } else {
                    this.showLearningAlert('感謝回饋！系統將調整分類建議邏輯。');
                }
            } else {
                console.error('回饋提交失敗:', response.status);
            }
            
            $('#categoryFeedback').hide();
            
        } catch (error) {
            console.error('回饋提交錯誤:', error);
        }
    }

    /**
     * 處理手動分類變更
     */
    handleManualCategoryChange() {
        // 如果是手動選擇分類且有輸入描述，顯示回饋選項
        const description = $('#description').val()?.trim();
        const categoryValue = $('#categorySelect').val();
        
        if (description && categoryValue) {
            $('#categoryFeedback').show();
        }
    }

    /**
     * 記錄建議使用情況
     */
    recordSuggestionUsage(categoryId, confidence) {
        // 這裡可以記錄用戶採用建議的統計數據
        console.log(`用戶採用分類建議: ${categoryId}, 信心度: ${confidence}`);
    }

    /**
     * 顯示載入狀態
     */
    showLoadingState() {
        const loadingHtml = `
            <div class="smart-category-loading">
                <div class="spinner-border spinner-border-sm" role="status"></div>
                正在分析中...
            </div>
        `;
        $('#categorySuggestions').html(loadingHtml);
        $('#smartCategorySection').show();
    }

    /**
     * 隱藏載入狀態
     */
    hideLoadingState() {
        // 載入狀態會在 displaySuggestions 中被取代
    }

    /**
     * 隱藏建議區域
     */
    hideSuggestions() {
        $('#smartCategorySection').hide();
    }

    /**
     * 顯示成功訊息
     */
    showSuccessMessage(message) {
        const alert = $(`
            <div class="alert alert-success alert-dismissible fade show mt-2" role="alert">
                <i class="fas fa-check-circle"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `);
        
        $('#smartCategorySection').after(alert);
        
        // 3秒後自動消失
        setTimeout(() => {
            alert.fadeOut(() => alert.remove());
        }, 3000);
    }

    /**
     * 顯示學習提醒
     */
    showLearningAlert(message) {
        const $learningAlert = $('#learningAlert');
        $learningAlert.find('strong').next().text(' ' + message);
        $learningAlert.show();
        
        setTimeout(() => {
            $learningAlert.fadeOut();
        }, 4000);
    }
}

/**
 * 防抖工具函式
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * 頁面載入完成後初始化
 */
$(document).ready(function() {
    // 初始化智能分類管理器
    if (typeof window.smartCategoryManager === 'undefined') {
        window.smartCategoryManager = new SmartCategoryManager();
    }
    
    // 初始化工具提示
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});
