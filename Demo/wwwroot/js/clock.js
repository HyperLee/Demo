// 多時區電子時鐘
const timezones = [
    { city: '紐約', tz: 'America/New_York', timeId: 'ny-time', dateId: 'ny-date', displayName: '美國/紐約' },
    { city: '倫敦', tz: 'Europe/London', timeId: 'ldn-time', dateId: 'ldn-date', displayName: '歐洲/倫敦' },
    { city: '東京', tz: 'Asia/Tokyo', timeId: 'tokyo-time', dateId: 'tokyo-date', displayName: '亞洲/東京' },
    { city: '沙烏地阿拉伯', tz: 'Asia/Riyadh', timeId: 'riyadh-time', dateId: 'riyadh-date', displayName: '亞洲/利雅德' }
];

// 燈箱相關變數
let modalUpdateInterval = null;
let currentModalTimezone = null;

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
    
    // 設定燈箱內容
    document.getElementById('modal-timezone').textContent = timezone.displayName;
    document.getElementById('modal-city').textContent = timezone.city;
    document.getElementById('modal-offset').textContent = calculateTimeOffset(timezone.tz);
    
    // 顯示燈箱
    modal.style.display = 'flex';
    modal.setAttribute('aria-hidden', 'false');
    setTimeout(() => modal.classList.add('show'), 10);
    
    // 開始更新燈箱時間
    updateModalTime();
    modalUpdateInterval = setInterval(updateModalTime, 1000);
    
    // 防止背景滾動
    document.body.style.overflow = 'hidden';
}

function closeModal() {
    const modal = document.getElementById('timezone-modal');
    modal.classList.remove('show');
    modal.setAttribute('aria-hidden', 'true');
    
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
        const tzDate = new Date(now.toLocaleString('en-US', { timeZone: currentModalTimezone.tz }));
        document.getElementById('modal-time').textContent = formatModalTime(tzDate);
    } catch (e) {
        document.getElementById('modal-time').textContent = '時間取得失敗';
    }
}

// 尋找時區資料
function findTimezoneData(tzString) {
    return timezones.find(tz => tz.tz === tzString);
}

window.addEventListener('DOMContentLoaded', () => {
    updateClocks();
    setInterval(updateClocks, 1000);
    
    // 綁定時區時鐘點擊事件
    document.addEventListener('click', (e) => {
        const clockElement = e.target.closest('.clickable-clock');
        if (clockElement) {
            const timezone = clockElement.getAttribute('data-tz');
            const timezoneData = findTimezoneData(timezone);
            if (timezoneData) {
                openModal(timezoneData);
            }
        }
        
        // 燈箱關閉按鈕
        if (e.target.classList.contains('modal-close')) {
            closeModal();
        }
        
        // 點擊燈箱外部關閉
        if (e.target.classList.contains('modal-overlay')) {
            closeModal();
        }
    });
    
    // ESC 鍵關閉燈箱
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && currentModalTimezone) {
            closeModal();
        }
    });
});

// 預留：可擴充更多時區，只需在 timezones 陣列中新增即可。
