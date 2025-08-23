// 多時區電子時鐘
const timezones = [
    { city: '紐約', tz: 'America/New_York', timeId: 'ny-time', dateId: 'ny-date', displayName: '美國/紐約' },
    { city: '倫敦', tz: 'Europe/London', timeId: 'ldn-time', dateId: 'ldn-date', displayName: '歐洲/倫敦' },
    { city: '東京', tz: 'Asia/Tokyo', timeId: 'tokyo-time', dateId: 'tokyo-date', displayName: '亞洲/東京' },
    { city: '沙烏地阿拉伯', tz: 'Asia/Riyadh', timeId: 'riyadh-time', dateId: 'riyadh-date', displayName: '亞洲/利雅德' }
];

// 本地時區資料
const localTimezone = {
    city: '本地時間',
    tz: 'local',
    timeId: 'local-time',
    dateId: 'local-date',
    displayName: '本地時區'
};

// 燈箱相關變數
let modalUpdateInterval = null;
let currentModalTimezone = null;

// 鍵盤導航相關變數
let lastFocusedElement = null; // 記錄燈箱開啟前的焦點元素
const KeyboardManager = {
    // 初始化鍵盤導航
    init() {
        this.bindKeyboardEvents();
    },
    
    // 綁定鍵盤事件
    bindKeyboardEvents() {
        // 全域鍵盤事件監聽
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
        
        // 為所有可點擊時鐘添加鍵盤事件
        const clickableClocks = document.querySelectorAll('.clickable-clock');
        clickableClocks.forEach(clock => {
            clock.addEventListener('keydown', this.handleClockKeyDown.bind(this));
        });
    },
    
    // 處理全域鍵盤事件
    handleKeyDown(e) {
        // ESC 鍵關閉燈箱 (已存在，這裡是額外處理)
        if (e.key === 'Escape' && currentModalTimezone) {
            e.preventDefault();
            this.closeModalAndRestoreFocus();
        }
        
        // Tab 鍵循環處理 (在燈箱內)
        if (e.key === 'Tab' && currentModalTimezone) {
            this.handleModalTabNavigation(e);
        }
    },
    
    // 處理時鐘元素的鍵盤事件
    handleClockKeyDown(e) {
        // Enter 或 Space 鍵開啟燈箱
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            
            // 記錄當前焦點元素
            lastFocusedElement = e.target;
            
            const timezone = e.target.getAttribute('data-tz');
            const timezoneData = findTimezoneData(timezone);
            if (timezoneData) {
                openModal(timezoneData);
            }
        }
    },
    
    // 處理燈箱內的 Tab 導航
    handleModalTabNavigation(e) {
        const modal = document.getElementById('timezone-modal');
        if (!modal || modal.style.display === 'none') return;
        
        // 取得燈箱內所有可聚焦元素
        const focusableElements = modal.querySelectorAll(
            'button, [tabindex="0"], [tabindex]:not([tabindex="-1"])'
        );
        
        if (focusableElements.length === 0) return;
        
        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];
        
        // 如果是 Shift + Tab 且在第一個元素，跳到最後一個
        if (e.shiftKey && document.activeElement === firstElement) {
            e.preventDefault();
            lastElement.focus();
        }
        // 如果是 Tab 且在最後一個元素，跳到第一個
        else if (!e.shiftKey && document.activeElement === lastElement) {
            e.preventDefault();
            firstElement.focus();
        }
    },
    
    // 關閉燈箱並恢復焦點
    closeModalAndRestoreFocus() {
        closeModal();
        
        // 恢復到開啟燈箱前的焦點元素
        if (lastFocusedElement) {
            setTimeout(() => {
                lastFocusedElement.focus();
                lastFocusedElement = null;
            }, 100); // 等待燈箱關閉動畫完成
        }
    }
};

// 音效相關變數和設定
const SoundManager = {
    enabled: localStorage.getItem('modalSoundEnabled') !== 'false', // 預設啟用
    volume: 0.4, // 適中音量
    
    // 初始化音效元素
    init() {
        this.openSound = document.getElementById('modal-open-sound');
        this.closeSound = document.getElementById('modal-close-sound');
        
        // 設定音量
        if (this.openSound) {
            this.openSound.volume = this.volume;
        }
        if (this.closeSound) {
            this.closeSound.volume = this.volume;
        }
    },
    
    // 播放開啟音效
    playOpen() {
        if (this.enabled && this.openSound) {
            try {
                this.openSound.currentTime = 0; // 重置到開始位置
                const playPromise = this.openSound.play();
                if (playPromise !== undefined) {
                    playPromise.catch(error => {
                        // 靜默處理音效播放失敗，不影響功能
                        console.warn('開啟音效播放失敗:', error);
                    });
                }
            } catch (error) {
                console.warn('開啟音效播放錯誤:', error);
            }
        }
    },
    
    // 播放關閉音效
    playClose() {
        if (this.enabled && this.closeSound) {
            try {
                this.closeSound.currentTime = 0; // 重置到開始位置
                const playPromise = this.closeSound.play();
                if (playPromise !== undefined) {
                    playPromise.catch(error => {
                        // 靜默處理音效播放失敗，不影響功能
                        console.warn('關閉音效播放失敗:', error);
                    });
                }
            } catch (error) {
                console.warn('關閉音效播放錯誤:', error);
            }
        }
    },
    
    // 切換音效開關
    toggle() {
        this.enabled = !this.enabled;
        localStorage.setItem('modalSoundEnabled', this.enabled.toString());
        return this.enabled;
    },
    
    // 設定音效狀態
    setEnabled(enabled) {
        this.enabled = enabled;
        localStorage.setItem('modalSoundEnabled', enabled.toString());
    }
};

// 載入動畫管理器
const LoadingManager = {
    // 初始化載入管理器
    init() {
        this.overlay = document.getElementById('loading-overlay');
        this.progressBar = document.getElementById('progress-bar-fill');
        this.loadingText = this.overlay?.querySelector('.loading-text');
        this.loadingSteps = [
            { text: '初始化時鐘系統...', duration: 500 },
            { text: '載入時區資料...', duration: 800 },
            { text: '同步時間資訊...', duration: 600 },
            { text: '完成載入...', duration: 300 }
        ];
        this.currentStep = 0;
    },
    
    // 顯示載入動畫
    show(message = '載入中...') {
        if (this.overlay) {
            this.overlay.classList.remove('fade-out');
            if (this.loadingText) {
                this.loadingText.textContent = message;
            }
            this.resetProgress();
        }
    },
    
    // 隱藏載入動畫
    hide() {
        if (this.overlay) {
            this.overlay.classList.add('fade-out');
            setTimeout(() => {
                // 載入完成後添加淡入效果到主要內容
                const mainContent = document.querySelector('.local-clock');
                const timezoneClocks = document.querySelectorAll('.timezone-clock');
                
                if (mainContent) mainContent.classList.add('fade-in');
                timezoneClocks.forEach(clock => clock.classList.add('fade-in'));
            }, 200);
        }
    },
    
    // 更新進度條
    updateProgress(percentage) {
        if (this.progressBar) {
            this.progressBar.style.width = `${Math.min(100, Math.max(0, percentage))}%`;
        }
    },
    
    // 重置進度條
    resetProgress() {
        if (this.progressBar) {
            this.progressBar.style.width = '0%';
        }
        this.currentStep = 0;
    },
    
    // 模擬分步載入過程
    simulateLoading() {
        return new Promise((resolve) => {
            let totalDuration = 0;
            let elapsedTime = 0;
            
            // 計算總時長
            this.loadingSteps.forEach(step => totalDuration += step.duration);
            
            const processStep = (stepIndex) => {
                if (stepIndex >= this.loadingSteps.length) {
                    this.updateProgress(100);
                    setTimeout(() => {
                        this.hide();
                        resolve();
                    }, 200);
                    return;
                }
                
                const step = this.loadingSteps[stepIndex];
                if (this.loadingText) {
                    this.loadingText.textContent = step.text;
                }
                
                // 更新進度條
                const stepProgress = (elapsedTime / totalDuration) * 100;
                this.updateProgress(stepProgress);
                
                setTimeout(() => {
                    elapsedTime += step.duration;
                    this.updateProgress((elapsedTime / totalDuration) * 100);
                    setTimeout(() => processStep(stepIndex + 1), 100);
                }, step.duration);
            };
            
            processStep(0);
        });
    },
    
    // 顯示錯誤狀態
    showError(message = '載入失敗，請重新整理頁面') {
        if (this.loadingText) {
            this.loadingText.textContent = message;
            this.loadingText.style.color = '#ff6b6b';
        }
        
        // 添加搖擺動畫
        const spinner = this.overlay?.querySelector('.loading-spinner');
        if (spinner) {
            spinner.style.display = 'none';
        }
        
        // 5秒後自動隱藏錯誤訊息
        setTimeout(() => {
            this.hide();
        }, 5000);
    },
    
    // 為燈箱添加載入效果
    showModalLoading() {
        const modal = document.getElementById('timezone-modal');
        if (modal) {
            const modalBody = modal.querySelector('.modal-body');
            if (modalBody) {
                modalBody.classList.add('pulse-loading');
                setTimeout(() => {
                    modalBody.classList.remove('pulse-loading');
                }, 800);
            }
        }
    }
};

function pad(num) {
    return num.toString().padStart(2, '0');
}

function formatTime(date) {
    return `${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
}

function formatDate(date) {
    return `${date.getFullYear()}:${pad(date.getMonth()+1)}:${pad(date.getDate())}`;
}

// 燈箱專用的時間格式化函式 (yyyy/MM/dd HH:mm:ss)
function formatModalTime(date) {
    return `${date.getFullYear()}/${pad(date.getMonth()+1)}/${pad(date.getDate())} ${formatTime(date)}`;
}

// 計算時差
function calculateTimeOffset(timezoneStr) {
    if (timezoneStr === 'local') {
        return '+0 (本地時間)';
    }
    
    const now = new Date();
    const localTime = now.getTime();
    const localOffset = now.getTimezoneOffset() * 60000;
    const utc = localTime + localOffset;
    
    const timezoneDate = new Date(now.toLocaleString('en-US', { timeZone: timezoneStr }));
    const timezoneTime = timezoneDate.getTime();
    
    const offsetMs = timezoneTime - now.getTime();
    const offsetHours = Math.round(offsetMs / (1000 * 60 * 60));
    
    return offsetHours >= 0 ? `+${offsetHours}` : `${offsetHours}`;
}

function updateClocks() {
    try {
        // 本地時間
        const now = new Date();
        document.getElementById('local-time').textContent = formatTime(now);
        document.getElementById('local-date').textContent = formatDate(now);

        // 多時區
        timezones.forEach(tzObj => {
            try {
                const tzDate = new Date(now.toLocaleString('en-US', { timeZone: tzObj.tz }));
                document.getElementById(tzObj.timeId).textContent = formatTime(tzDate);
                document.getElementById(tzObj.dateId).textContent = formatDate(tzDate);
            } catch (e) {
                document.getElementById(tzObj.timeId).textContent = '--:--:--';
                document.getElementById(tzObj.dateId).textContent = '----:--:--';
                showError(`時區「${tzObj.city}」時間取得失敗。`);
            }
        });
        hideError();
    } catch (err) {
        showError('時鐘更新失敗，請重新整理頁面。');
        LoadingManager.showError('時鐘系統初始化失敗');
    }
}

// 初始化時鐘系統（帶載入動畫）
async function initializeClocks() {
    try {
        // 顯示載入動畫
        LoadingManager.show('準備載入時鐘系統...');
        
        // 模擬載入過程
        await LoadingManager.simulateLoading();
        
        // 初始化時鐘
        updateClocks();
        
        // 開始定時更新
        setInterval(updateClocks, 1000);
        
    } catch (error) {
        console.error('時鐘初始化失敗:', error);
        LoadingManager.showError();
    }
}

function showError(msg) {
    const errDiv = document.getElementById('clock-error');
    errDiv.textContent = msg;
    errDiv.style.display = 'block';
}
function hideError() {
    const errDiv = document.getElementById('clock-error');
    errDiv.textContent = '';
    errDiv.style.display = 'none';
}

// 燈箱功能
function openModal(timezone) {
    currentModalTimezone = timezone;
    const modal = document.getElementById('timezone-modal');
    
    if (!modal) {
        console.error('找不到燈箱元素 #timezone-modal');
        return;
    }
    
    // 設定燈箱內容
    const modalTimezone = document.getElementById('modal-timezone');
    const modalCity = document.getElementById('modal-city');
    const modalOffset = document.getElementById('modal-offset');
    
    if (modalTimezone) modalTimezone.textContent = timezone.displayName;
    if (modalCity) modalCity.textContent = timezone.city;
    if (modalOffset) modalOffset.textContent = calculateTimeOffset(timezone.tz);
    
    // 顯示燈箱
    modal.style.display = 'flex';
    modal.setAttribute('aria-hidden', 'false');
    
    // 播放開啟音效
    SoundManager.playOpen();
    
    // 添加載入效果到燈箱內容
    LoadingManager.showModalLoading();
    
    setTimeout(() => {
        modal.classList.add('show');
    }, 10);
    
    // 開始更新燈箱時間
    updateModalTime();
    modalUpdateInterval = setInterval(updateModalTime, 1000);
    
    // 防止背景滾動
    document.body.style.overflow = 'hidden';
    
    // 焦點管理：將焦點移到燈箱的關閉按鈕
    setTimeout(() => {
        const closeButton = modal.querySelector('.modal-close');
        if (closeButton) {
            closeButton.focus();
        }
    }, 100); // 等待顯示動畫開始
}

function closeModal() {
    const modal = document.getElementById('timezone-modal');
    modal.classList.remove('show');
    modal.setAttribute('aria-hidden', 'true');
    
    // 播放關閉音效
    SoundManager.playClose();
    
    setTimeout(() => {
        modal.style.display = 'none';
    }, 300);
    
    // 停止更新燈箱時間
    if (modalUpdateInterval) {
        clearInterval(modalUpdateInterval);
        modalUpdateInterval = null;
    }
    
    currentModalTimezone = null;
    
    // 恢復背景滾動
    document.body.style.overflow = 'auto';
}

function updateModalTime() {
    if (!currentModalTimezone) return;
    
    try {
        const now = new Date();
        let tzDate;
        
        if (currentModalTimezone.tz === 'local') {
            tzDate = now; // 使用本地時間
        } else {
            tzDate = new Date(now.toLocaleString('en-US', { timeZone: currentModalTimezone.tz }));
        }
        
        document.getElementById('modal-time').textContent = formatModalTime(tzDate);
    } catch (e) {
        document.getElementById('modal-time').textContent = '時間取得失敗';
    }
}

// 尋找時區資料
function findTimezoneData(tzString) {
    if (tzString === 'local') {
        return localTimezone;
    }
    return timezones.find(tz => tz.tz === tzString);
}

window.addEventListener('DOMContentLoaded', async () => {
    // 初始化載入管理器
    LoadingManager.init();
    
    // 初始化音效管理器
    SoundManager.init();
    
    // 執行帶載入動畫的初始化
    await initializeClocks();
    
    // 確認所有必要元素都存在
    const modal = document.getElementById('timezone-modal');
    const clickableClocks = document.querySelectorAll('.clickable-clock');
    
    if (!modal) {
        console.error('找不到燈箱元素！');
    }
    
    if (clickableClocks.length === 0) {
        console.error('找不到任何可點擊的時鐘元素！');
    }
    
    clickableClocks.forEach((clock, index) => {
        // 直接綁定每個時鐘的點擊事件作為備份
        clock.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            // 記錄點擊的元素作為返回焦點
            lastFocusedElement = this;
            
            const timezone = this.getAttribute('data-tz');
            const timezoneData = findTimezoneData(timezone);
            if (timezoneData) {
                openModal(timezoneData);
            } else {
                console.error('找不到時區資料:', timezone);
            }
        });
    });
    
    // 綁定時區時鐘點擊事件（委派方式）
    document.addEventListener('click', (e) => {
        const clockElement = e.target.closest('.clickable-clock');
        
        if (clockElement) {
            // 記錄點擊的元素作為返回焦點
            lastFocusedElement = clockElement;
            
            const timezone = clockElement.getAttribute('data-tz');
            const timezoneData = findTimezoneData(timezone);
            
            if (timezoneData) {
                openModal(timezoneData);
            } else {
                console.error('找不到時區資料:', timezone);
            }
        }
        
        // 燈箱關閉按鈕
        if (e.target.classList.contains('modal-close')) {
            KeyboardManager.closeModalAndRestoreFocus();
        }
        
        // 音效控制按鈕
        if (e.target.classList.contains('sound-toggle-btn') || e.target.closest('.sound-toggle-btn')) {
            toggleSoundControl();
        }
        
        // 點擊燈箱外部關閉
        if (e.target.classList.contains('modal-overlay')) {
            KeyboardManager.closeModalAndRestoreFocus();
        }
    });
    
    // 初始化鍵盤管理器
    KeyboardManager.init();
    
    // 更新音效按鈕的UI狀態
    updateSoundButtonUI();
});

// 音效控制函數
function toggleSoundControl() {
    const enabled = SoundManager.toggle();
    updateSoundButtonUI();
    
    // 播放測試音效以提供即時回饋
    if (enabled) {
        SoundManager.playOpen();
    }
}

// 更新音效按鈕UI
function updateSoundButtonUI() {
    const soundButton = document.getElementById('sound-toggle');
    const soundIcon = soundButton?.querySelector('.sound-icon');
    
    if (soundButton && soundIcon) {
        if (SoundManager.enabled) {
            soundButton.classList.remove('disabled');
            soundButton.setAttribute('title', '關閉音效');
            soundIcon.textContent = '🔊';
        } else {
            soundButton.classList.add('disabled');
            soundButton.setAttribute('title', '開啟音效');
            soundIcon.textContent = '🔇';
        }
    }
}

// 預留：可擴充更多時區，只需在 timezones 陣列中新增即可。
