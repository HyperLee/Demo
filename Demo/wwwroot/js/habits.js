/**
 * 習慣追蹤器前端 JavaScript 功能
 * 提供習慣管理、進度追蹤、互動式操作等功能
 */

// 習慣追蹤器主類別
class HabitTracker {
    constructor() {
        this.currentHabitId = null;
        this.charts = {}; // 存儲圖表實例
        this.initializeEventListeners();
        this.loadInitialData();
    }

    /**
     * 初始化事件監聽器
     */
    initializeEventListeners() {
        console.log('正在初始化習慣追蹤器事件監聽器...');

        // 防止重複綁定事件
        $(document).off('.habitTracker');

        // 標記完成按鈕
        $(document).on('click.habitTracker', '[onclick*="markComplete"]', (e) => {
            e.preventDefault();
            const habitId = this.extractHabitIdFromOnclick(e.target.getAttribute('onclick'));
            if (habitId) this.markComplete(habitId);
        });

        // 減少完成次數
        $(document).on('click.habitTracker', '[onclick*="decrementHabit"]', (e) => {
            e.preventDefault();
            const habitId = this.extractHabitIdFromOnclick(e.target.getAttribute('onclick'));
            if (habitId) this.decrementHabit(habitId);
        });

        // 完成所有剩餘次數
        $(document).on('click.habitTracker', '[onclick*="completeAllRemaining"]', (e) => {
            e.preventDefault();
            const onclickAttr = e.target.getAttribute('onclick');
            const match = onclickAttr.match(/completeAllRemaining\('([^']+)',\s*(\d+),\s*(\d+)\)/);
            if (match) {
                const [, habitId, targetCount, currentCount] = match;
                this.completeAllRemaining(habitId, parseInt(targetCount), parseInt(currentCount));
            }
        });

        // 查看習慣詳情
        $(document).on('click.habitTracker', '[onclick*="showHabitDetail"]', (e) => {
            e.preventDefault();
            const habitId = this.extractHabitIdFromOnclick(e.target.getAttribute('onclick'));
            if (habitId) this.showHabitDetail(habitId);
        });

        // 刪除習慣
        $(document).on('click.habitTracker', '[onclick*="deleteHabit"]', (e) => {
            e.preventDefault();
            const onclickAttr = e.target.getAttribute('onclick');
            const match = onclickAttr.match(/deleteHabit\('([^']+)',\s*'([^']*)'\)/);
            if (match) {
                const [, habitId, habitName] = match;
                this.deleteHabit(habitId, habitName);
            }
        });

        // 新增習慣表單提交（如果需要 AJAX 處理）
        $('#addHabitForm').off('submit.habitTracker').on('submit.habitTracker', (e) => {
            // 讓表單正常提交，不阻止默認行為
            console.log('習慣表單正在提交...');
        });

        // Modal 關閉事件
        $('#habitDetailModal').on('hidden.bs.modal', () => {
            this.clearModalContent();
        });

        // 分類篩選按鈕
        $('.category-filter-btn').off('click.habitTracker').on('click.habitTracker', (e) => {
            const category = $(e.target).data('category');
            this.filterByCategory(category);
            
            // 更新按鈕狀態
            $('.category-filter-btn').removeClass('active');
            $(e.target).addClass('active');
        });

        console.log('習慣追蹤器事件監聽器初始化完成');
    }

    /**
     * 載入初始資料
     */
    loadInitialData() {
        console.log('正在載入習慣追蹤器初始資料...');
        // 這裡可以載入任何需要的初始資料
    }

    /**
     * 從 onclick 屬性中提取 habitId
     */
    extractHabitIdFromOnclick(onclickAttr) {
        if (!onclickAttr) return null;
        const match = onclickAttr.match(/'([^']+)'/);
        return match ? match[1] : null;
    }

    /**
     * 標記習慣完成
     */
    async markComplete(habitId) {
        console.log('標記習慣完成:', habitId);
        
        try {
            this.showLoadingIndicator(habitId);
            
            const response = await fetch('/habits?handler=MarkComplete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({ 
                    habitId: habitId, 
                    date: new Date().toISOString(),
                    notes: '' 
                })
            });

            const result = await response.json();
            
            if (result.success) {
                this.showSuccess('習慣已標記為完成！');
                await this.refreshHabitCard(habitId);
                this.updateDashboardStats();
                this.triggerCompletionAnimation(habitId);
            } else {
                this.showError(result.message || '標記完成時發生錯誤');
            }
        } catch (error) {
            console.error('標記完成時發生錯誤:', error);
            this.showError('網路錯誤，請稍後再試');
        } finally {
            this.hideLoadingIndicator(habitId);
        }
    }

    /**
     * 減少習慣完成次數
     */
    async decrementHabit(habitId) {
        console.log('減少習慣完成次數:', habitId);
        
        if (!confirm('確定要減少一次完成記錄嗎？')) {
            return;
        }

        try {
            // 這裡需要實作減少完成次數的 API
            // 暫時使用重新載入頁面的方式
            this.showInfo('功能開發中，請重新整理頁面');
        } catch (error) {
            console.error('減少完成次數時發生錯誤:', error);
            this.showError('操作失敗，請稍後再試');
        }
    }

    /**
     * 完成所有剩餘次數
     */
    async completeAllRemaining(habitId, targetCount, currentCount) {
        const remainingCount = targetCount - currentCount;
        
        if (remainingCount <= 0) {
            this.showInfo('已達成今日目標！');
            return;
        }

        if (!confirm(`確定要一次完成剩餘的 ${remainingCount} 次嗎？`)) {
            return;
        }

        try {
            this.showLoadingIndicator(habitId);
            
            // 連續呼叫標記完成 API
            for (let i = 0; i < remainingCount; i++) {
                await this.callMarkCompleteAPI(habitId);
            }

            this.showSuccess(`已完成剩餘的 ${remainingCount} 次！`);
            await this.refreshHabitCard(habitId);
            this.updateDashboardStats();
            this.triggerCompletionAnimation(habitId);
            
        } catch (error) {
            console.error('完成剩餘次數時發生錯誤:', error);
            this.showError('操作失敗，請稍後再試');
        } finally {
            this.hideLoadingIndicator(habitId);
        }
    }

    /**
     * 呼叫標記完成 API
     */
    async callMarkCompleteAPI(habitId) {
        const response = await fetch('/habits?handler=MarkComplete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify({ 
                habitId: habitId, 
                date: new Date().toISOString(),
                notes: '' 
            })
        });

        const result = await response.json();
        if (!result.success) {
            throw new Error(result.message || '標記完成失敗');
        }
    }

    /**
     * 顯示習慣詳情
     */
    async showHabitDetail(habitId) {
        console.log('顯示習慣詳情:', habitId);
        
        try {
            // 載入習慣詳細資料
            const habitData = await this.loadHabitDetailData(habitId);
            
            if (habitData) {
                this.displayHabitDetailModal(habitData);
            } else {
                this.showError('載入習慣詳情失敗');
            }
        } catch (error) {
            console.error('載入習慣詳情時發生錯誤:', error);
            this.showError('載入詳情時發生錯誤');
        }
    }

    /**
     * 載入習慣詳細資料
     */
    async loadHabitDetailData(habitId) {
        // 由於我們已經有了習慣資料在頁面上，可以直接從 DOM 中獲取
        const habitCard = $(`.habit-card[data-habit-id="${habitId}"]`);
        if (habitCard.length === 0) return null;

        // 創建基本的習慣資料物件
        const habitData = {
            id: habitId,
            name: habitCard.find('.habit-name').text().trim(),
            // 可以根據需要添加更多資料
        };

        return habitData;
    }

    /**
     * 顯示習慣詳情 Modal
     */
    displayHabitDetailModal(habitData) {
        // 更新 Modal 標題
        $('#habitDetailModalLabel').html(`
            <i class="fas fa-chart-line"></i>
            ${habitData.name} - 詳細統計
        `);

        // 載入進度視覺化內容
        const content = `
            <div class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">載入中...</span>
                </div>
                <p class="mt-2">正在載入習慣詳情...</p>
            </div>
        `;
        
        $('#habitDetailContent').html(content);
        
        // 顯示 Modal
        const modal = new bootstrap.Modal('#habitDetailModal');
        modal.show();

        // 異步載入詳細內容
        this.loadHabitProgressView(habitData.id);
    }

    /**
     * 載入習慣進度視覺化檢視
     */
    async loadHabitProgressView(habitId) {
        try {
            // 這裡應該載入 _HabitProgress.cshtml 的內容
            // 由於是靜態示例，我們創建一個簡單的統計顯示
            const content = `
                <div class="row g-4">
                    <div class="col-md-6">
                        <div class="card">
                            <div class="card-body text-center">
                                <h5 class="card-title">7天完成率</h5>
                                <canvas id="habit-chart-${habitId}" width="300" height="200"></canvas>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card">
                            <div class="card-body">
                                <h5 class="card-title">統計資料</h5>
                                <div id="habit-stats-${habitId}">
                                    <div class="spinner-border spinner-border-sm" role="status"></div>
                                    載入中...
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            $('#habitDetailContent').html(content);
            
            // 載入統計資料
            await this.loadHabitStats(habitId);
            
        } catch (error) {
            console.error('載入進度視覺化時發生錯誤:', error);
            $('#habitDetailContent').html(`
                <div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle"></i>
                    載入詳情時發生錯誤，請稍後再試。
                </div>
            `);
        }
    }

    /**
     * 載入習慣統計資料
     */
    async loadHabitStats(habitId) {
        try {
            const response = await fetch(`/habits?handler=HabitStats&habitId=${habitId}`);
            const result = await response.json();
            
            if (result.success) {
                const stats = result.data;
                const statsHtml = `
                    <div class="row g-3">
                        <div class="col-6">
                            <div class="d-flex align-items-center">
                                <i class="fas fa-fire text-warning me-2"></i>
                                <div>
                                    <div class="fw-bold">${stats.currentStreak}</div>
                                    <small class="text-muted">目前連續</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="d-flex align-items-center">
                                <i class="fas fa-trophy text-success me-2"></i>
                                <div>
                                    <div class="fw-bold">${stats.longestStreak}</div>
                                    <small class="text-muted">最長連續</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="d-flex align-items-center">
                                <i class="fas fa-check-circle text-primary me-2"></i>
                                <div>
                                    <div class="fw-bold">${stats.totalCompletions}</div>
                                    <small class="text-muted">總完成次數</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="d-flex align-items-center">
                                <i class="fas fa-percentage text-info me-2"></i>
                                <div>
                                    <div class="fw-bold">${stats.completionRate}%</div>
                                    <small class="text-muted">30天成功率</small>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                $(`#habit-stats-${habitId}`).html(statsHtml);
                
                // 載入進度圖表
                await this.loadHabitProgressChart(habitId);
                
            } else {
                $(`#habit-stats-${habitId}`).html(`
                    <div class="text-danger">
                        <i class="fas fa-exclamation-circle"></i>
                        載入統計資料失敗
                    </div>
                `);
            }
        } catch (error) {
            console.error('載入統計資料時發生錯誤:', error);
            $(`#habit-stats-${habitId}`).html(`
                <div class="text-danger">
                    <i class="fas fa-exclamation-circle"></i>
                    載入統計資料時發生錯誤
                </div>
            `);
        }
    }

    /**
     * 載入習慣進度圖表
     */
    async loadHabitProgressChart(habitId) {
        try {
            const response = await fetch(`/habits?handler=HabitProgress&habitId=${habitId}&days=7`);
            const result = await response.json();
            
            if (result.success) {
                const ctx = document.getElementById(`habit-chart-${habitId}`);
                if (ctx) {
                    const labels = result.data.map(d => {
                        const date = new Date(d.date);
                        return date.toLocaleDateString('zh-TW', { month: 'numeric', day: 'numeric' });
                    });
                    
                    const data = result.data.map(d => d.completed);
                    
                    new Chart(ctx, {
                        type: 'line',
                        data: {
                            labels: labels,
                            datasets: [{
                                label: '完成次數',
                                data: data,
                                borderColor: 'rgb(75, 192, 192)',
                                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                                tension: 0.1,
                                fill: true
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                y: {
                                    beginAtZero: true,
                                    ticks: {
                                        stepSize: 1
                                    }
                                }
                            }
                        }
                    });
                }
            }
        } catch (error) {
            console.error('載入進度圖表時發生錯誤:', error);
        }
    }

    /**
     * 刪除習慣
     */
    async deleteHabit(habitId, habitName) {
        console.log('刪除習慣:', habitId, habitName);
        
        const confirmMessage = habitName ? 
            `確定要刪除習慣「${habitName}」嗎？此操作無法復原。` :
            '確定要刪除此習慣嗎？此操作無法復原。';
            
        if (!confirm(confirmMessage)) {
            return;
        }

        try {
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/habits?handler=DeleteHabit';
            
            const habitIdInput = document.createElement('input');
            habitIdInput.type = 'hidden';
            habitIdInput.name = 'habitId';
            habitIdInput.value = habitId;
            
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = this.getAntiForgeryToken();
            
            form.appendChild(habitIdInput);
            form.appendChild(tokenInput);
            document.body.appendChild(form);
            form.submit();
            
        } catch (error) {
            console.error('刪除習慣時發生錯誤:', error);
            this.showError('刪除失敗，請稍後再試');
        }
    }

    /**
     * 根據分類篩選習慣
     */
    filterByCategory(categoryId) {
        console.log('篩選分類:', categoryId);
        
        const habitCards = $('.habit-card');
        
        habitCards.each(function() {
            const $card = $(this);
            const cardCategory = $card.data('category');
            
            if (categoryId === 'all' || cardCategory === categoryId) {
                $card.show().css('opacity', '0').animate({ opacity: 1 }, 300);
            } else {
                $card.animate({ opacity: 0 }, 200, function() {
                    $(this).hide();
                });
            }
        });
    }

    /**
     * 重新整理習慣卡片
     */
    async refreshHabitCard(habitId) {
        // 重新載入頁面以更新資料
        // 在實際應用中，這裡可以通過 AJAX 更新特定的習慣卡片
        console.log('重新整理習慣卡片:', habitId);
        
        // 暫時添加視覺回饋
        const card = $(`.habit-card[data-habit-id="${habitId}"]`);
        card.addClass('updating');
        
        setTimeout(() => {
            card.removeClass('updating');
            // 這裡可以通過 AJAX 載入新的卡片內容
        }, 1000);
    }

    /**
     * 更新儀表板統計
     */
    async updateDashboardStats() {
        console.log('更新儀表板統計');
        // 這裡可以通過 AJAX 更新統計數據
        // 暫時重新載入頁面
        setTimeout(() => {
            window.location.reload();
        }, 1500);
    }

    /**
     * 觸發完成動畫
     */
    triggerCompletionAnimation(habitId) {
        const card = $(`.habit-card[data-habit-id="${habitId}"]`);
        card.addClass('completing');
        
        setTimeout(() => {
            card.removeClass('completing').addClass('completed');
        }, 300);
    }

    /**
     * 顯示載入指示器
     */
    showLoadingIndicator(habitId) {
        const card = $(`.habit-card[data-habit-id="${habitId}"]`);
        card.find('.habit-actions-bottom').html(`
            <button class="btn btn-primary btn-sm w-100" disabled>
                <div class="spinner-border spinner-border-sm me-2" role="status"></div>
                處理中...
            </button>
        `);
    }

    /**
     * 隱藏載入指示器
     */
    hideLoadingIndicator(habitId) {
        // 載入指示器會在 refreshHabitCard 中被清除
    }

    /**
     * 清除 Modal 內容
     */
    clearModalContent() {
        $('#habitDetailContent').empty();
        this.currentHabitId = null;
    }

    /**
     * 取得防偽權杖
     */
    getAntiForgeryToken() {
        return $('input[name="__RequestVerificationToken"]').val() || 
               $('meta[name="__RequestVerificationToken"]').attr('content') || '';
    }

    /**
     * 顯示成功訊息
     */
    showSuccess(message) {
        this.showToast(message, 'success');
    }

    /**
     * 顯示錯誤訊息
     */
    showError(message) {
        this.showToast(message, 'error');
    }

    /**
     * 顯示資訊訊息
     */
    showInfo(message) {
        this.showToast(message, 'info');
    }

    /**
     * 顯示 Toast 訊息
     */
    showToast(message, type = 'info') {
        const toastClass = {
            'success': 'bg-success text-white',
            'error': 'bg-danger text-white',
            'info': 'bg-info text-white',
            'warning': 'bg-warning text-dark'
        }[type] || 'bg-info text-white';

        const toastId = `toast-${Date.now()}`;
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center ${toastClass}" role="alert" aria-live="assertive" aria-atomic="true" style="position: fixed; top: 20px; right: 20px; z-index: 9999;">
                <div class="d-flex">
                    <div class="toast-body">
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        $('body').append(toastHtml);
        
        const toastElement = new bootstrap.Toast(document.getElementById(toastId), {
            autohide: true,
            delay: 3000
        });
        
        toastElement.show();
        
        // 自動清理
        setTimeout(() => {
            $(`#${toastId}`).remove();
        }, 4000);
    }
}

// 全域函式（供 HTML 中的 onclick 屬性呼叫）
function markComplete(habitId) {
    if (window.habitTracker) {
        window.habitTracker.markComplete(habitId);
    }
}

function decrementHabit(habitId) {
    if (window.habitTracker) {
        window.habitTracker.decrementHabit(habitId);
    }
}

function completeAllRemaining(habitId, targetCount, currentCount) {
    if (window.habitTracker) {
        window.habitTracker.completeAllRemaining(habitId, targetCount, currentCount);
    }
}

function showHabitDetail(habitId) {
    if (window.habitTracker) {
        window.habitTracker.showHabitDetail(habitId);
    }
}

function editHabit(habitId) {
    // 編輯功能暫未實作
    if (window.habitTracker) {
        window.habitTracker.showInfo('編輯功能開發中');
    }
}

function deleteHabit(habitId, habitName) {
    if (window.habitTracker) {
        window.habitTracker.deleteHabit(habitId, habitName);
    }
}

// 初始化
$(document).ready(function() {
    console.log('正在初始化習慣追蹤器...');
    window.habitTracker = new HabitTracker();
    console.log('習慣追蹤器初始化完成');
});
