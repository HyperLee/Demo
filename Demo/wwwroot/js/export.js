/**
 * 匯出功能 JavaScript 模組
 * 處理前端匯出邏輯和用戶界面互動
 */
class ExportManager {
    constructor() {
        this.selectedDataTypes = [];
        this.selectedFormat = '';
        this.exportInProgress = false;
        this.currentExportResult = null;
        this.initializeEventListeners();
        this.loadExportHistory();
    }

    /**
     * 初始化事件監聽器
     */
    initializeEventListeners() {
        // 資料類型選擇
        $('.export-option input[type="checkbox"]').on('change', this.updateSelection.bind(this));
        
        // 格式選擇
        $('input[name="exportFormat"]').on('change', this.updateSelection.bind(this));
        
        // 快速日期選擇
        $('.quick-date-buttons button').on('click', this.setQuickDateRange.bind(this));
        
        // 日期範圍變更
        $('#startDate, #endDate').on('change', this.updatePreview.bind(this));
        
        // 開始匯出按鈕
        $('#startExport').on('click', this.startExport.bind(this));
        
        // 下載檔案按鈕
        $('#downloadExportFile').on('click', this.downloadExportFile.bind(this));

        // 阻止表單提交
        $('#exportForm').on('submit', function(e) {
            e.preventDefault();
        });
    }

    /**
     * 更新選擇狀態
     */
    updateSelection() {
        // 更新選中的資料類型
        this.selectedDataTypes = $('.export-option input[type="checkbox"]:checked')
            .map((i, el) => $(el).val()).get();
            
        // 更新選中的格式
        this.selectedFormat = $('input[name="exportFormat"]:checked').val() || '';
        
        // 更新預覽和按鈕狀態
        this.updatePreview();
        this.updateExportButton();
    }

    /**
     * 更新匯出預覽
     */
    updatePreview() {
        const startDate = $('#startDate').val();
        const endDate = $('#endDate').val();
        const dateRange = this.getDateRangeText(startDate, endDate);
        
        let preview = '';
        
        if (this.selectedDataTypes.length > 0 && this.selectedFormat) {
            const dataTypesText = this.selectedDataTypes.map(type => this.getDataTypeDisplayName(type)).join('、');
            const formatText = this.getFormatDisplayName(this.selectedFormat);
            
            preview = `將匯出 ${dataTypesText} 為 ${formatText} 格式`;
            if (dateRange) {
                preview += `，日期範圍：${dateRange}`;
            }
        } else {
            preview = '請選擇要匯出的資料類型和格式';
        }
        
        $('#exportPreview').text(preview);
    }

    /**
     * 更新匯出按鈕狀態
     */
    updateExportButton() {
        const canExport = this.selectedDataTypes.length > 0 && 
                         this.selectedFormat && 
                         !this.exportInProgress;
        $('#startExport').prop('disabled', !canExport);
    }

    /**
     * 設定快速日期範圍
     */
    setQuickDateRange(event) {
        const range = $(event.target).data('range');
        const today = new Date();
        
        if (range === 'all') {
            $('#startDate').val('');
            $('#endDate').val('');
        } else {
            const days = parseInt(range);
            const startDate = new Date();
            startDate.setDate(today.getDate() - days);
            
            $('#startDate').val(startDate.toISOString().split('T')[0]);
            $('#endDate').val(today.toISOString().split('T')[0]);
        }
        
        this.updatePreview();
        
        // 移除其他按鈕的 active 狀態
        $('.quick-date-buttons button').removeClass('btn-secondary').addClass('btn-outline-secondary');
        $(event.target).removeClass('btn-outline-secondary').addClass('btn-secondary');
    }

    /**
     * 開始匯出
     */
    async startExport() {
        if (this.exportInProgress) return;
        
        this.exportInProgress = true;
        this.updateExportButton();
        
        const exportRequest = {
            dataTypes: this.selectedDataTypes,
            format: this.selectedFormat,
            startDate: $('#startDate').val() || null,
            endDate: $('#endDate').val() || null,
            templateName: 'default'
        };
        
        // 顯示進度模態框
        this.showProgressModal(exportRequest);
        
        try {
            const response = await fetch('/api/export/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(exportRequest)
            });
            
            const result = await response.json();
            
            if (response.ok && result.success) {
                this.currentExportResult = result.data;
                this.showExportSuccess(result.data);
                
                // 重新載入歷史記錄
                setTimeout(() => {
                    this.refreshHistory();
                }, 1000);
            } else {
                this.showExportError(result.message || '匯出失敗');
            }
        } catch (error) {
            console.error('匯出錯誤:', error);
            this.showExportError('匯出失敗：' + error.message);
        } finally {
            this.exportInProgress = false;
            this.updateExportButton();
        }
    }

    /**
     * 顯示進度模態框
     */
    showProgressModal(request) {
        $('#exportDataType').text(request.dataTypes.map(t => this.getDataTypeDisplayName(t)).join('、'));
        $('#exportFormat').text(this.getFormatDisplayName(request.format));
        $('#exportRange').text(this.getDateRangeText(request.startDate, request.endDate) || '全部資料');
        
        $('#exportProgressModal').modal('show');
        $('#exportComplete').hide();
        $('#exportResult').hide();
        $('#exportError').hide();
        
        this.updateProgress(50, '正在匯出...');
    }

    /**
     * 更新進度
     */
    updateProgress(percentage, message) {
        $('#progressPercent').text(Math.round(percentage) + '%');
        $('#exportProgressBar').css('width', percentage + '%');
        $('#currentStep').text(message);
    }

    /**
     * 顯示匯出成功
     */
    showExportSuccess(result) {
        this.updateProgress(100, '匯出完成');
        
        $('#resultFileName').text(result.fileName);
        $('#resultFileSize').text(this.formatFileSize(result.fileSize));
        
        $('#exportResult').show();
        $('#exportComplete').show();
        $('#exportProgressBar').removeClass('progress-bar-striped progress-bar-animated');
    }

    /**
     * 顯示匯出錯誤
     */
    showExportError(message) {
        this.updateProgress(0, '匯出失敗');
        $('#errorMessage').text(message);
        $('#exportError').show();
        $('#exportProgressBar').removeClass('progress-bar-striped progress-bar-animated')
                                .addClass('bg-danger');
    }

    /**
     * 下載匯出檔案
     */
    async downloadExportFile() {
        if (this.currentExportResult && this.currentExportResult.downloadUrl) {
            window.open(this.currentExportResult.downloadUrl, '_blank');
            $('#exportProgressModal').modal('hide');
        }
    }

    /**
     * 重新整理歷史記錄
     */
    async refreshHistory() {
        try {
            const response = await fetch('/api/export/history');
            if (response.ok) {
                // 重新載入頁面來更新歷史記錄
                window.location.reload();
            }
        } catch (error) {
            console.error('重新整理歷史失敗:', error);
            this.showToast('重新整理失敗', 'error');
        }
    }

    /**
     * 載入匯出歷史（用於初始化）
     */
    async loadExportHistory() {
        // 歷史記錄由伺服器端渲染，這裡不需要額外載入
        console.log('匯出歷史已載入');
    }

    /**
     * 取得資料類型顯示名稱
     */
    getDataTypeDisplayName(type) {
        const names = {
            'accounting': '記帳資料',
            'habits': '習慣追蹤',
            'notes': '備忘錄',
            'todo': '待辦事項'
        };
        return names[type] || type;
    }

    /**
     * 取得格式顯示名稱
     */
    getFormatDisplayName(format) {
        const names = {
            'pdf': 'PDF 報表',
            'excel': 'Excel 試算表',
            'csv': 'CSV 檔案',
            'json': 'JSON 資料'
        };
        return names[format] || format;
    }

    /**
     * 取得日期範圍文字
     */
    getDateRangeText(startDate, endDate) {
        if (!startDate && !endDate) {
            return '';
        }
        
        if (startDate && endDate) {
            return `${startDate} 至 ${endDate}`;
        }
        
        if (startDate) {
            return `${startDate} 之後`;
        }
        
        if (endDate) {
            return `${endDate} 之前`;
        }
        
        return '';
    }

    /**
     * 格式化檔案大小
     */
    formatFileSize(bytes) {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    /**
     * 顯示提示訊息
     */
    showToast(message, type = 'info') {
        // 簡單的提示實作，可以使用 Bootstrap Toast 或其他組件
        const alertClass = type === 'error' ? 'alert-danger' : 'alert-info';
        const toast = $(`
            <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999;">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);
        
        $('body').append(toast);
        
        setTimeout(() => {
            toast.alert('close');
        }, 5000);
    }
}

/**
 * 全域函式 - 下載匯出檔案
 */
window.downloadExport = function(exportId) {
    window.open(`/Export?handler=Download&id=${exportId}`, '_blank');
};

/**
 * 全域函式 - 刪除匯出記錄
 */
window.deleteExport = async function(exportId) {
    if (!confirm('確定要刪除這筆匯出記錄嗎？')) {
        return;
    }
    
    try {
        const response = await fetch('/api/export/history/' + exportId, {
            method: 'DELETE'
        });
        
        const result = await response.json();
        
        if (response.ok && result.success) {
            // 重新載入頁面
            window.location.reload();
        } else {
            alert('刪除失敗：' + (result.message || '未知錯誤'));
        }
    } catch (error) {
        console.error('刪除失敗:', error);
        alert('刪除失敗：' + error.message);
    }
};

/**
 * 全域函式 - 重設表單
 */
window.resetForm = function() {
    // 清除所有選擇
    $('#exportForm input[type="checkbox"]').prop('checked', false);
    $('#exportForm input[type="radio"]').prop('checked', false);
    $('#startDate, #endDate').val('');
    
    // 重設按鈕狀態
    $('.quick-date-buttons button').removeClass('btn-secondary').addClass('btn-outline-secondary');
    
    // 重新初始化
    if (window.exportManager) {
        window.exportManager.selectedDataTypes = [];
        window.exportManager.selectedFormat = '';
        window.exportManager.updatePreview();
        window.exportManager.updateExportButton();
    }
};

/**
 * 全域函式 - 重新整理歷史
 */
window.refreshHistory = function() {
    window.location.reload();
};

// 初始化匯出管理器
$(document).ready(function() {
    window.exportManager = new ExportManager();
    console.log('匯出管理器已初始化');
});

// 處理模態框關閉事件
$('#exportProgressModal').on('hidden.bs.modal', function () {
    if (window.exportManager) {
        window.exportManager.currentExportResult = null;
        $('#exportResult').hide();
        $('#exportError').hide();
        $('#exportComplete').hide();
        $('#exportProgressBar').removeClass('bg-danger').addClass('progress-bar-striped progress-bar-animated');
    }
});
