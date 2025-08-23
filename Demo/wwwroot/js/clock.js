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
    console.log('openModal 被呼叫，時區資料:', timezone);
    
    currentModalTimezone = timezone;
    const modal = document.getElementById('timezone-modal');
    
    if (!modal) {
        console.error('找不到燈箱元素 #timezone-modal');
        return;
    }
    
    console.log('燈箱元素找到:', modal);
    
    // 設定燈箱內容
    const modalTimezone = document.getElementById('modal-timezone');
    const modalCity = document.getElementById('modal-city');
    const modalOffset = document.getElementById('modal-offset');
    
    if (modalTimezone) modalTimezone.textContent = timezone.displayName;
    if (modalCity) modalCity.textContent = timezone.city;
    if (modalOffset) modalOffset.textContent = calculateTimeOffset(timezone.tz);
    
    console.log('燈箱內容設定完成');
    
    // 顯示燈箱
    modal.style.display = 'flex';
    modal.setAttribute('aria-hidden', 'false');
    console.log('燈箱 display 設為 flex');
    
    setTimeout(() => {
        modal.classList.add('show');
        console.log('燈箱 show 類別已加入');
    }, 10);
    
    // 開始更新燈箱時間
    updateModalTime();
    modalUpdateInterval = setInterval(updateModalTime, 1000);
    
    // 防止背景滾動
    document.body.style.overflow = 'hidden';
    
    console.log('燈箱開啟完成');
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
    console.log('🚀 DOM 載入完成，開始初始化 - 版本 2023.08.23');
    console.log('🔍 當前 URL:', window.location.href);
    console.log('🔍 User Agent:', navigator.userAgent);
    
    updateClocks();
    setInterval(updateClocks, 1000);
    
    // 確認所有必要元素都存在
    const modal = document.getElementById('timezone-modal');
    const clickableClocks = document.querySelectorAll('.clickable-clock');
    
    console.log('🎯 燈箱元素:', modal);
    console.log('🎯 可點擊時鐘元素數量:', clickableClocks.length);
    
    if (!modal) {
        console.error('❌ 找不到燈箱元素！');
    }
    
    if (clickableClocks.length === 0) {
        console.error('❌ 找不到任何可點擊的時鐘元素！');
        console.log('🔍 所有時鐘元素:', document.querySelectorAll('.small-clock'));
    }
    
    clickableClocks.forEach((clock, index) => {
        console.log(`🕐 時鐘 ${index}:`, clock, '時區:', clock.getAttribute('data-tz'));
        
        // 直接綁定每個時鐘的點擊事件作為備份
        clock.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('🖱️ 直接點擊事件觸發:', this);
            
            const timezone = this.getAttribute('data-tz');
            const timezoneData = findTimezoneData(timezone);
            if (timezoneData) {
                console.log('✅ 準備開啟燈箱:', timezoneData);
                openModal(timezoneData);
            } else {
                console.error('❌ 找不到時區資料:', timezone);
            }
        });
    });
    
    // 綁定時區時鐘點擊事件（委派方式）
    document.addEventListener('click', (e) => {
        console.log('全域點擊事件觸發:', e.target);
        
        const clockElement = e.target.closest('.clickable-clock');
        console.log('找到的時鐘元素:', clockElement);
        
        if (clockElement) {
            const timezone = clockElement.getAttribute('data-tz');
            console.log('時區:', timezone);
            
            const timezoneData = findTimezoneData(timezone);
            console.log('時區資料:', timezoneData);
            
            if (timezoneData) {
                console.log('開啟燈箱:', timezoneData);
                openModal(timezoneData);
            } else {
                console.error('找不到時區資料:', timezone);
            }
        }
        
        // 燈箱關閉按鈕
        if (e.target.classList.contains('modal-close')) {
            console.log('關閉按鈕被點擊');
            closeModal();
        }
        
        // 點擊燈箱外部關閉
        if (e.target.classList.contains('modal-overlay')) {
            console.log('燈箱外部被點擊');
            closeModal();
        }
    });
    
    // ESC 鍵關閉燈箱
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && currentModalTimezone) {
            console.log('ESC 鍵關閉燈箱');
            closeModal();
        }
    });
    
    console.log('事件綁定完成');
});

// 預留：可擴充更多時區，只需在 timezones 陣列中新增即可。
