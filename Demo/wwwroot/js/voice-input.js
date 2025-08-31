/**
 * 語音記帳組件
 * 提供語音識別、解析和表單填寫功能
 */
class VoiceInput {
    constructor(options = {}) {
        this.options = {
            context: 'personal', // 'personal' 或 'family'
            container: '#voiceInputArea',
            targetForm: null,
            onResult: null,
            onError: null,
            language: 'zh-TW',
            ...options
        };
        
        this.recognition = null;
        this.isListening = false;
        this.isProcessing = false;
        this.currentTranscript = '';
        this.categories = [];
        
        this.init();
    }

    /**
     * 初始化語音輸入組件
     */
    init() {
        this.setupSpeechRecognition();
        this.setupEventListeners();
        this.loadCategories();
        this.updateUI('idle');
    }

    /**
     * 設定語音識別
     */
    setupSpeechRecognition() {
        if (!('webkitSpeechRecognition' in window) && !('SpeechRecognition' in window)) {
            this.showError('您的瀏覽器不支援語音識別功能，請使用 Chrome 或 Edge 瀏覽器');
            return;
        }

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        this.recognition = new SpeechRecognition();
        
        // 設定語音識別參數
        this.recognition.lang = this.options.language;
        this.recognition.continuous = false;
        this.recognition.interimResults = true;
        this.recognition.maxAlternatives = 1;

        // 事件監聽
        this.recognition.onstart = () => {
            this.isListening = true;
            this.updateUI('listening');
            console.log('語音識別開始');
        };

        this.recognition.onresult = (event) => {
            let interimTranscript = '';
            let finalTranscript = '';

            for (let i = event.resultIndex; i < event.results.length; i++) {
                const transcript = event.results[i][0].transcript;
                if (event.results[i].isFinal) {
                    finalTranscript += transcript;
                } else {
                    interimTranscript += transcript;
                }
            }

            // 更新即時顯示
            this.updateTranscript(finalTranscript || interimTranscript);

            // 如果有最終結果，開始解析
            if (finalTranscript) {
                this.currentTranscript = finalTranscript;
                this.processVoiceResult(finalTranscript);
            }
        };

        this.recognition.onerror = (event) => {
            console.error('語音識別錯誤:', event.error);
            this.isListening = false;
            
            let errorMessage = '語音識別發生錯誤';
            switch (event.error) {
                case 'no-speech':
                    errorMessage = '未偵測到語音，請重試';
                    break;
                case 'audio-capture':
                    errorMessage = '無法存取麥克風，請檢查權限設定';
                    break;
                case 'not-allowed':
                    errorMessage = '麥克風權限被拒絕，請允許使用麥克風';
                    break;
                case 'network':
                    errorMessage = '網路錯誤，請檢查網路連線';
                    break;
            }
            
            this.showError(errorMessage);
            this.updateUI('error');
        };

        this.recognition.onend = () => {
            this.isListening = false;
            if (!this.isProcessing) {
                this.updateUI('idle');
            }
        };
    }

    /**
     * 設定事件監聽器
     */
    setupEventListeners() {
        const container = $(this.options.container);
        
        // 開始錄音按鈕
        container.on('click', '#startVoiceBtn', (e) => {
            e.preventDefault();
            this.startListening();
        });

        // 停止錄音按鈕
        container.on('click', '#stopVoiceBtn', (e) => {
            e.preventDefault();
            this.stopListening();
        });

        // 確認按鈕
        container.on('click', '#confirmVoiceBtn', (e) => {
            e.preventDefault();
            this.confirmResult();
        });

        // 重新錄音按鈕
        container.on('click', '#retryVoiceBtn', (e) => {
            e.preventDefault();
            this.resetAndRetry();
        });

        // 清除按鈕
        container.on('click', '#clearVoiceBtn', (e) => {
            e.preventDefault();
            this.clearResult();
        });
    }

    /**
     * 開始語音識別
     */
    startListening() {
        if (!this.recognition || this.isListening) {
            return;
        }

        try {
            this.clearResult();
            this.recognition.start();
        } catch (error) {
            console.error('啟動語音識別失敗:', error);
            this.showError('啟動語音識別失敗，請重試');
        }
    }

    /**
     * 停止語音識別
     */
    stopListening() {
        if (this.recognition && this.isListening) {
            this.recognition.stop();
        }
    }

    /**
     * 處理語音識別結果
     */
    async processVoiceResult(transcript) {
        this.isProcessing = true;
        this.updateUI('processing');

        try {
            const parseResult = await this.parseVoiceText(transcript);
            this.displayParseResult(parseResult);
            this.updateUI('completed');
            
            // 執行回調
            if (typeof this.options.onResult === 'function') {
                this.options.onResult(parseResult);
            }
        } catch (error) {
            console.error('解析語音結果失敗:', error);
            this.showError('解析語音內容失敗，請重試');
            this.updateUI('error');
        } finally {
            this.isProcessing = false;
        }
    }

    /**
     * 解析語音文字
     */
    async parseVoiceText(text) {
        // 前端解析邏輯
        const result = {
            originalText: text,
            type: 'Expense',
            amount: null,
            category: null,
            description: text.trim(),
            splitType: '我支付',
            parseConfidence: 0.8,
            isSuccess: true,
            errorMessage: null
        };

        try {
            // 1. 解析金額
            const amountPatterns = [
                /(\d+(?:\.\d{1,2})?)\s*[元塊]/g,
                /(\d+(?:\.\d{1,2})?)\s*[dollardollar]/gi,
                /花[了費]*\s*(\d+(?:\.\d{1,2})?)/g,
                /(\d+(?:\.\d{1,2})?)\s*塊錢/g
            ];

            for (const pattern of amountPatterns) {
                const match = text.match(pattern);
                if (match) {
                    const numMatch = match[0].match(/(\d+(?:\.\d{1,2})?)/);
                    if (numMatch) {
                        result.amount = parseFloat(numMatch[1]);
                        break;
                    }
                }
            }

            // 2. 判斷收支類型
            const incomeKeywords = ['收入', '賺', '薪水', '獎金', '紅利', '退款', '返現'];
            const expenseKeywords = ['支出', '花', '買', '付', '繳', '費'];
            
            if (incomeKeywords.some(keyword => text.includes(keyword))) {
                result.type = 'Income';
            } else if (expenseKeywords.some(keyword => text.includes(keyword))) {
                result.type = 'Expense';
            }

            // 3. 類別推測
            const categoryMappings = {
                '餐飲': ['吃', '喝', '咖啡', '午餐', '晚餐', '早餐', '餐廳', '飲料', '茶', '奶茶', '食物', '麵', '飯'],
                '交通': ['車', '計程車', '公車', '捷運', '地鐵', '油錢', '停車', '加油', '搭車', '坐車', 'uber', 'taxi'],
                '購物': ['買', '購買', '商品', '衣服', '鞋子', '包包', '化妝品', '日用品'],
                '娛樂': ['電影', '遊戲', '唱歌', 'KTV', '看電影', '玩', '娛樂'],
                '醫療': ['看病', '藥', '醫院', '診所', '健檢', '醫療'],
                '學習': ['書', '課程', '學費', '補習', '教育', '培訓'],
                '居家': ['房租', '水電', '瓦斯', '網路', '電話', '家具', '家電'],
                '其他': []
            };

            for (const [category, keywords] of Object.entries(categoryMappings)) {
                if (keywords.some(keyword => text.includes(keyword))) {
                    result.category = category;
                    break;
                }
            }

            // 4. 家庭模式的分攤判斷
            if (this.options.context === 'family') {
                if (text.includes('平均分攤') || text.includes('大家分') || text.includes('一起分')) {
                    result.splitType = '平均分攤';
                } else if (text.includes('自訂分攤') || text.includes('按比例')) {
                    result.splitType = '自訂分攤';
                }
            }

            // 5. 清理描述文字
            result.description = text
                .replace(/(\d+(?:\.\d{1,2})?)\s*[元塊]/g, '')
                .replace(/[花了花費支出收入我]/g, '')
                .replace(/\s+/g, ' ')
                .trim();

            if (!result.description) {
                result.description = result.category || '語音記帳';
            }

            // 6. 計算信心度
            let confidence = 0.5;
            if (result.amount) confidence += 0.3;
            if (result.category) confidence += 0.2;
            result.parseConfidence = Math.min(confidence, 1.0);

        } catch (error) {
            console.error('解析過程發生錯誤:', error);
            result.isSuccess = false;
            result.errorMessage = '解析語音內容時發生錯誤';
        }

        // 如果有後端解析 API，可以在這裡調用
        if (this.options.useServerParsing) {
            try {
                const serverResult = await this.callServerParser(text);
                if (serverResult && serverResult.isSuccess) {
                    return serverResult;
                }
            } catch (error) {
                console.warn('後端解析失敗，使用前端解析結果:', error);
            }
        }

        return result;
    }

    /**
     * 調用後端解析 API
     */
    async callServerParser(text) {
        const response = await fetch('/api/voice/parse', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify({
                voiceText: text,
                context: this.options.context
            })
        });

        if (!response.ok) {
            throw new Error('後端解析請求失敗');
        }

        return await response.json();
    }

    /**
     * 顯示解析結果
     */
    displayParseResult(result) {
        const container = $(this.options.container);
        
        if (!result.isSuccess) {
            this.showError(result.errorMessage || '解析失敗');
            return;
        }

        const previewHtml = `
            <div class="voice-result">
                <h6><i class="fas fa-check-circle text-success me-2"></i>解析結果：</h6>
                <div class="result-item">
                    <span class="result-label">類型：</span>
                    <span class="result-value">${result.type === 'Income' ? '收入' : '支出'}</span>
                </div>
                <div class="result-item">
                    <span class="result-label">金額：</span>
                    <span class="result-value">${result.amount ? '$' + result.amount.toLocaleString() : '未識別'}</span>
                </div>
                <div class="result-item">
                    <span class="result-label">類別：</span>
                    <span class="result-value">${result.category || '未分類'}</span>
                </div>
                <div class="result-item">
                    <span class="result-label">描述：</span>
                    <span class="result-value">${result.description}</span>
                </div>
                ${this.options.context === 'family' ? `
                <div class="result-item">
                    <span class="result-label">分攤：</span>
                    <span class="result-value">${result.splitType}</span>
                </div>
                ` : ''}
                <div class="result-item">
                    <span class="result-label">信心度：</span>
                    <span class="result-value">${Math.round(result.parseConfidence * 100)}%</span>
                </div>
                <div class="mt-3">
                    <button type="button" id="confirmVoiceBtn" class="btn voice-btn voice-btn-success me-2">
                        <i class="fas fa-check me-1"></i>確認填入
                    </button>
                    <button type="button" id="retryVoiceBtn" class="btn voice-btn voice-btn-primary me-2">
                        <i class="fas fa-redo me-1"></i>重新錄音
                    </button>
                    <button type="button" id="clearVoiceBtn" class="btn btn-outline-secondary">
                        <i class="fas fa-times me-1"></i>清除
                    </button>
                </div>
            </div>
        `;

        container.find('#parsedPreview').html(previewHtml).removeClass('d-none');
        this.parseResult = result;
    }

    /**
     * 確認結果並填入表單
     */
    confirmResult() {
        if (!this.parseResult) {
            return;
        }

        try {
            this.fillForm(this.parseResult);
            this.showSuccess('語音內容已成功填入表單');
            this.clearResult();
        } catch (error) {
            console.error('填寫表單失敗:', error);
            this.showError('填寫表單失敗，請手動填寫');
        }
    }

    /**
     * 填寫表單
     */
    fillForm(result) {
        const form = this.options.targetForm ? $(this.options.targetForm) : $('form').first();

        // 填寫類型
        if (result.type) {
            const typeValue = result.type === 'Income' ? '收入' : '支出';
            form.find('input[name="Record.Type"]').each(function() {
                $(this).prop('checked', $(this).val() === typeValue);
            });
            form.find('select[name="request.Type"]').val(typeValue);
        }

        // 填寫金額
        if (result.amount) {
            form.find('input[name="Record.Amount"], input[name="request.Amount"], .money-input').val(result.amount);
        }

        // 填寫類別
        if (result.category) {
            form.find('select[name="Record.Category"], select[name="request.Category"]').val(result.category);
        }

        // 填寫描述
        if (result.description) {
            form.find('input[name="Record.Description"], input[name="request.Description"], textarea[name="Record.Note"]').val(result.description);
        }

        // 家庭模式的分攤方式
        if (this.options.context === 'family' && result.splitType) {
            form.find('select[name="request.SplitType"]').val(result.splitType);
        }

        // 觸發相關事件
        form.find('input, select, textarea').trigger('change');
        
        // 如果有類別變更事件，觸發子類別載入
        if (result.category) {
            form.find('select[name="Record.Category"]').trigger('change');
        }
    }

    /**
     * 更新UI狀態
     */
    updateUI(status) {
        const container = $(this.options.container);
        const statusElement = container.find('#voiceStatus');
        const iconElement = container.find('#voiceIcon');
        const textElement = container.find('#statusText');
        const startBtn = container.find('#startVoiceBtn');
        const stopBtn = container.find('#stopVoiceBtn');

        // 清除所有狀態類別
        statusElement.removeClass('voice-status-idle voice-status-listening voice-status-processing voice-status-completed voice-status-error');
        
        switch (status) {
            case 'idle':
                statusElement.addClass('voice-status-idle');
                iconElement.html('<i class="fas fa-microphone voice-icon"></i>');
                textElement.text('點擊開始語音輸入');
                startBtn.removeClass('d-none');
                stopBtn.addClass('d-none');
                break;
                
            case 'listening':
                statusElement.addClass('voice-status-listening');
                iconElement.html('<i class="fas fa-microphone voice-icon"></i>');
                textElement.text('正在聆聽中...');
                startBtn.addClass('d-none');
                stopBtn.removeClass('d-none');
                break;
                
            case 'processing':
                statusElement.addClass('voice-status-processing');
                iconElement.html('<i class="fas fa-cog voice-icon"></i>');
                textElement.text('正在解析語音內容...');
                startBtn.addClass('d-none');
                stopBtn.addClass('d-none');
                break;
                
            case 'completed':
                statusElement.addClass('voice-status-completed');
                iconElement.html('<i class="fas fa-check-circle voice-icon"></i>');
                textElement.text('解析完成！請確認結果');
                startBtn.removeClass('d-none');
                stopBtn.addClass('d-none');
                break;
                
            case 'error':
                statusElement.addClass('voice-status-error');
                iconElement.html('<i class="fas fa-exclamation-triangle voice-icon"></i>');
                textElement.text('發生錯誤，請重試');
                startBtn.removeClass('d-none');
                stopBtn.addClass('d-none');
                break;
        }
    }

    /**
     * 更新轉錄文字顯示
     */
    updateTranscript(text) {
        const container = $(this.options.container);
        container.find('#speechText').text(text);
        container.find('#speechResult').removeClass('d-none');
    }

    /**
     * 載入類別選項
     */
    async loadCategories() {
        try {
            // 這裡可以調用 API 載入類別，或者使用預設類別
            this.categories = [
                '餐飲', '交通', '購物', '娛樂', '醫療', 
                '學習', '居家', '其他'
            ];
        } catch (error) {
            console.error('載入類別失敗:', error);
            this.categories = ['其他'];
        }
    }

    /**
     * 重新錄音
     */
    resetAndRetry() {
        this.clearResult();
        this.startListening();
    }

    /**
     * 清除結果
     */
    clearResult() {
        const container = $(this.options.container);
        container.find('#speechResult, #parsedPreview').addClass('d-none');
        container.find('#speechText').text('');
        this.currentTranscript = '';
        this.parseResult = null;
        this.updateUI('idle');
    }

    /**
     * 顯示錯誤訊息
     */
    showError(message) {
        const container = $(this.options.container);
        const errorHtml = `
            <div class="voice-error">
                <div class="error-title">
                    <i class="fas fa-exclamation-circle me-2"></i>錯誤
                </div>
                <div>${message}</div>
            </div>
        `;
        
        container.find('#voiceError').remove();
        container.append(`<div id="voiceError">${errorHtml}</div>`);
        
        setTimeout(() => {
            container.find('#voiceError').fadeOut(() => {
                container.find('#voiceError').remove();
            });
        }, 5000);
    }

    /**
     * 顯示成功訊息
     */
    showSuccess(message) {
        // 使用 Bootstrap Toast 或 Alert 顯示成功訊息
        const alertHtml = `
            <div class="alert alert-success alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999; max-width: 400px;">
                <i class="fas fa-check-circle me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        $('body').append(alertHtml);
        
        setTimeout(() => {
            $('.alert').alert('close');
        }, 3000);
    }

    /**
     * 銷毀組件
     */
    destroy() {
        if (this.recognition) {
            this.recognition.abort();
        }
        $(this.options.container).off();
    }
}

// 工廠函數
window.createVoiceInput = function(options) {
    return new VoiceInput(options);
};

// jQuery 插件形式
if (typeof jQuery !== 'undefined') {
    $.fn.voiceInput = function(options) {
        return this.each(function() {
            const $this = $(this);
            let instance = $this.data('voiceInput');
            
            if (!instance) {
                options = $.extend({}, options, { container: this });
                instance = new VoiceInput(options);
                $this.data('voiceInput', instance);
            }
            
            return instance;
        });
    };
}
