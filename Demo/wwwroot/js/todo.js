/**
 * 待辦清單 JavaScript 功能
 * 提供任務管理的前端互動功能
 */

class TodoManager {
    constructor() {
        this.currentFilter = 'all';
        this.currentView = 'sections';
        this.isLoading = false;
        this.searchTimeout = null;
        
        this.init();
    }

    /**
     * 建立包含防偽標記的 FormData
     */
    createFormDataWithToken(data = {}) {
        const formData = new FormData();
        
        // 添加防偽標記
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }
        
        // 添加其他資料
        Object.entries(data).forEach(([key, value]) => {
            formData.append(key, value);
        });
        
        return formData;
    }

    /**
     * 初始化功能
     */
    init() {
        this.bindEvents();
        this.initSortable();
        this.initTooltips();
        this.updateCharacterCount();
        
        console.log('待辦清單管理器已初始化');
    }

    /**
     * 綁定事件處理器
     */
    bindEvents() {
        // 新增/編輯任務模態框
        document.getElementById('saveTodoBtn')?.addEventListener('click', () => this.saveTodoTask());
        
        // 搜尋功能
        document.getElementById('searchInput')?.addEventListener('input', (e) => this.handleSearch(e.target.value));
        
        // 篩選功能
        document.querySelectorAll('[data-filter]').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                this.filterTasks(e.target.dataset.filter);
            });
        });

        // 檢視模式切換
        document.querySelectorAll('[data-view]').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                this.switchView(e.target.dataset.view);
            });
        });

        // 任務操作按鈕
        document.addEventListener('click', (e) => {
            if (e.target.closest('.edit-task-btn')) {
                const taskId = e.target.closest('.edit-task-btn').dataset.taskId;
                this.editTask(parseInt(taskId));
            } else if (e.target.closest('.delete-task-btn')) {
                const taskId = e.target.closest('.delete-task-btn').dataset.taskId;
                this.deleteTask(parseInt(taskId));
            } else if (e.target.closest('.complete-task-btn') || e.target.closest('.uncomplete-task-btn')) {
                const taskId = e.target.closest('[data-task-id]').dataset.taskId;
                this.toggleTaskComplete(parseInt(taskId));
            }
        });

        // 任務完成狀態切換（複選框）
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('todo-checkbox')) {
                const taskId = e.target.dataset.taskId;
                this.toggleTaskComplete(parseInt(taskId));
            }
        });

        // 模態框重置
        document.getElementById('addTodoModal')?.addEventListener('hidden.bs.modal', () => {
            this.resetForm();
        });

        // 字元計數
        document.getElementById('taskTitle')?.addEventListener('input', () => this.updateCharacterCount());

        // 快捷鍵支援
        document.addEventListener('keydown', (e) => this.handleKeyboardShortcuts(e));
    }

    /**
     * 初始化可排序功能
     */
    initSortable() {
        if (typeof Sortable === 'undefined') {
            console.warn('SortableJS 未載入，拖拽排序功能不可用');
            return;
        }

        document.querySelectorAll('.sortable').forEach(container => {
            new Sortable(container, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                chosenClass: 'sortable-chosen',
                dragClass: 'sortable-drag',
                handle: '.drag-handle',
                onEnd: (evt) => {
                    const taskId = evt.item.getAttribute('data-id');
                    const newIndex = evt.newIndex;
                    this.updateTaskOrder(parseInt(taskId), newIndex);
                }
            });
        });
    }

    /**
     * 初始化工具提示
     */
    initTooltips() {
        if (typeof bootstrap !== 'undefined') {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[title]'));
            tooltipTriggerList.map(tooltipTriggerEl => {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    /**
     * 更新字元計數
     */
    updateCharacterCount() {
        const titleInput = document.getElementById('taskTitle');
        const countDisplay = document.getElementById('titleCount');
        
        if (titleInput && countDisplay) {
            countDisplay.textContent = titleInput.value.length;
            
            // 當接近上限時變紅
            if (titleInput.value.length > 90) {
                countDisplay.classList.add('text-danger');
            } else {
                countDisplay.classList.remove('text-danger');
            }
        }
    }

    /**
     * 儲存任務（新增或更新）
     */
    async saveTodoTask() {
        if (this.isLoading) return;
        
        const form = document.getElementById('todoForm');
        const formData = new FormData(form);
        const saveBtn = document.getElementById('saveTodoBtn');
        
        // 顯示載入狀態
        this.setLoadingState(saveBtn, true);
        
        try {
            const response = await fetch('/todo?handler=Save', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                this.showNotification('任務儲存成功！', 'success');
                bootstrap.Modal.getInstance(document.getElementById('addTodoModal')).hide();
                
                // 重新載入頁面或更新UI
                setTimeout(() => location.reload(), 500);
            } else {
                this.showNotification(result.message || '儲存失敗，請稍後再試', 'error');
            }
        } catch (error) {
            console.error('儲存任務時發生錯誤:', error);
            this.showNotification('儲存時發生錯誤，請稍後再試', 'error');
        } finally {
            this.setLoadingState(saveBtn, false);
        }
    }

    /**
     * 編輯任務
     */
    async editTask(taskId) {
        try {
            const response = await fetch(`/todo?handler=Task&id=${taskId}`);
            const result = await response.json();
            
            if (result.success) {
                this.populateForm(result.task);
                document.getElementById('modal-title').textContent = '編輯任務';
                
                const modal = new bootstrap.Modal(document.getElementById('addTodoModal'));
                modal.show();
            } else {
                this.showNotification(result.message || '無法載入任務資料', 'error');
            }
        } catch (error) {
            console.error('載入任務資料時發生錯誤:', error);
            this.showNotification('載入任務資料時發生錯誤', 'error');
        }
    }

    /**
     * 刪除任務
     */
    async deleteTask(taskId) {
        if (!confirm('確定要刪除這個任務嗎？此操作無法復原。')) {
            return;
        }

        try {
            const formData = this.createFormDataWithToken({ id: taskId });
            
            const response = await fetch('/todo?handler=Delete', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                this.showNotification('任務已刪除', 'success');
                
                // 動畫移除任務項目
                const taskElement = document.querySelector(`[data-id="${taskId}"]`);
                if (taskElement) {
                    taskElement.style.transition = 'all 0.3s ease';
                    taskElement.style.opacity = '0';
                    taskElement.style.transform = 'translateX(100%)';
                    
                    setTimeout(() => {
                        taskElement.remove();
                        this.updateStatistics();
                    }, 300);
                }
            } else {
                this.showNotification(result.message || '刪除失敗', 'error');
            }
        } catch (error) {
            console.error('刪除任務時發生錯誤:', error);
            this.showNotification('刪除任務時發生錯誤', 'error');
        }
    }

    /**
     * 切換任務完成狀態
     */
    async toggleTaskComplete(taskId) {
        try {
            const formData = this.createFormDataWithToken({ id: taskId });
            
            const response = await fetch('/todo?handler=ToggleComplete', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                this.updateTaskStatus(taskId, result.isCompleted);
                this.updateStatistics();
                
                const statusText = result.isCompleted ? '已完成' : '未完成';
                this.showNotification(`任務狀態已更新為：${statusText}`, 'info');
            } else {
                this.showNotification(result.message || '狀態更新失敗', 'error');
                
                // 恢復複選框狀態
                const checkbox = document.querySelector(`[data-task-id="${taskId}"]`);
                if (checkbox) {
                    checkbox.checked = !checkbox.checked;
                }
            }
        } catch (error) {
            console.error('更新任務狀態時發生錯誤:', error);
            this.showNotification('更新任務狀態時發生錯誤', 'error');
        }
    }

    /**
     * 更新任務排序
     */
    async updateTaskOrder(taskId, newOrder) {
        try {
            const formData = this.createFormDataWithToken({ 
                id: taskId, 
                order: newOrder 
            });
            
            await fetch('/todo?handler=UpdateOrder', {
                method: 'POST',
                body: formData
            });
            
            console.log(`任務 ${taskId} 排序已更新為 ${newOrder}`);
        } catch (error) {
            console.error('更新排序時發生錯誤:', error);
        }
    }

    /**
     * 搜尋任務
     */
    handleSearch(query) {
        clearTimeout(this.searchTimeout);
        
        this.searchTimeout = setTimeout(() => {
            const searchTerm = query.toLowerCase().trim();
            
            document.querySelectorAll('.todo-item').forEach(item => {
                const title = item.querySelector('.todo-title')?.textContent.toLowerCase() || '';
                const description = item.querySelector('.todo-description')?.textContent.toLowerCase() || '';
                const category = item.dataset.category || '';
                
                const matches = title.includes(searchTerm) || 
                               description.includes(searchTerm) ||
                               category.includes(searchTerm);
                
                item.style.display = matches || searchTerm === '' ? 'block' : 'none';
            });
            
            // 更新分組標題中的計數
            this.updateSectionCounts();
        }, 300);
    }

    /**
     * 篩選任務
     */
    filterTasks(filterType) {
        this.currentFilter = filterType;
        
        document.querySelectorAll('.todo-item').forEach(item => {
            let show = true;
            
            switch(filterType) {
                case 'pending':
                    show = item.dataset.status === 'pending';
                    break;
                case 'in-progress':
                    show = item.dataset.status === 'inprogress';
                    break;
                case 'completed':
                    show = item.dataset.status === 'completed';
                    break;
                case 'high':
                    show = item.dataset.priority === 'high';
                    break;
                case 'today':
                    show = item.closest('[data-section="today"]') !== null;
                    break;
                case 'overdue':
                    show = item.querySelector('.due-date.overdue') !== null;
                    break;
                default:
                    show = true;
            }
            
            item.style.display = show ? 'block' : 'none';
        });
        
        // 更新篩選按鈕狀態
        document.querySelectorAll('[data-filter]').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.filter === filterType);
        });
        
        this.updateSectionCounts();
    }

    /**
     * 切換檢視模式
     */
    switchView(viewType) {
        this.currentView = viewType;
        const todoList = document.querySelector('.todo-list');
        
        // 移除所有檢視模式類別
        todoList.classList.remove('view-sections', 'view-list', 'view-priority');
        
        // 添加新的檢視模式類別
        todoList.classList.add(`view-${viewType}`);
        
        // 更新檢視按鈕狀態
        document.querySelectorAll('[data-view]').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.view === viewType);
        });
    }

    /**
     * 處理鍵盤快捷鍵
     */
    handleKeyboardShortcuts(e) {
        // Ctrl+N 或 Cmd+N: 新增任務
        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            document.querySelector('[data-bs-target="#addTodoModal"]').click();
        }
        
        // ESC: 關閉模態框
        if (e.key === 'Escape') {
            const modal = bootstrap.Modal.getInstance(document.getElementById('addTodoModal'));
            if (modal) {
                modal.hide();
            }
        }
    }

    /**
     * 更新任務狀態顯示
     */
    updateTaskStatus(taskId, isCompleted) {
        const taskElement = document.querySelector(`[data-id="${taskId}"]`);
        if (!taskElement) return;
        
        const titleElement = taskElement.querySelector('.todo-title');
        const checkbox = taskElement.querySelector('.todo-checkbox');
        
        if (isCompleted) {
            titleElement.classList.add('completed');
            taskElement.dataset.status = 'completed';
        } else {
            titleElement.classList.remove('completed');
            taskElement.dataset.status = 'pending';
        }
        
        if (checkbox) {
            checkbox.checked = isCompleted;
        }
        
        // 更新操作按鈕
        const completeBtn = taskElement.querySelector('.complete-task-btn');
        const uncompleteBtn = taskElement.querySelector('.uncomplete-task-btn');
        
        if (completeBtn && uncompleteBtn) {
            if (isCompleted) {
                completeBtn.style.display = 'none';
                uncompleteBtn.style.display = 'inline-block';
            } else {
                completeBtn.style.display = 'inline-block';
                uncompleteBtn.style.display = 'none';
            }
        }
    }

    /**
     * 更新統計資料顯示
     */
    async updateStatistics() {
        try {
            const response = await fetch('/todo?handler=Statistics');
            const result = await response.json();
            
            if (result.success) {
                const stats = result.statistics;
                
                document.querySelectorAll('.stat-number').forEach((el, index) => {
                    const values = [stats.pendingCount, stats.inProgressCount, stats.completedCount, stats.overdueCount];
                    if (values[index] !== undefined) {
                        el.textContent = values[index];
                    }
                });
            }
        } catch (error) {
            console.error('更新統計資料時發生錯誤:', error);
        }
    }

    /**
     * 更新分組計數
     */
    updateSectionCounts() {
        document.querySelectorAll('.todo-section').forEach(section => {
            const visibleItems = section.querySelectorAll('.todo-item[style*="display: block"], .todo-item:not([style*="display: none"])');
            const badge = section.querySelector('.badge');
            if (badge) {
                badge.textContent = visibleItems.length;
            }
        });
    }

    /**
     * 填充表單資料
     */
    populateForm(task) {
        document.getElementById('taskId').value = task.id;
        document.getElementById('taskTitle').value = task.title;
        document.getElementById('taskDescription').value = task.description;
        document.getElementById('taskPriority').value = task.priority;
        document.getElementById('taskStatus').value = task.status;
        document.getElementById('taskCategory').value = task.category;
        document.getElementById('taskTags').value = task.tagsString;
        document.getElementById('taskDueDate').value = task.dueDate || '';
        document.getElementById('taskEstimated').value = task.estimatedMinutes || '';
        
        // 更新字元計數
        this.updateCharacterCount();
    }

    /**
     * 重置表單
     */
    resetForm() {
        document.getElementById('todoForm').reset();
        document.getElementById('taskId').value = '0';
        document.getElementById('modal-title').textContent = '新增任務';
        document.getElementById('titleCount').textContent = '0';
    }

    /**
     * 設定載入狀態
     */
    setLoadingState(button, loading) {
        this.isLoading = loading;
        
        if (loading) {
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 處理中...';
        } else {
            button.disabled = false;
            button.innerHTML = '<i class="fas fa-save"></i> 儲存';
        }
    }

    /**
     * 顯示通知訊息
     */
    showNotification(message, type = 'info') {
        // 建立通知元素
        const notification = document.createElement('div');
        notification.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        // 自動移除通知
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }
}

// 當頁面載入完成時初始化
document.addEventListener('DOMContentLoaded', () => {
    window.todoManager = new TodoManager();
});

// 匯出給其他腳本使用
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TodoManager;
}
