// 多時區電子時鐘
const timezones = [
    { city: '紐約', tz: 'America/New_York', timeId: 'ny-time', dateId: 'ny-date' },
    { city: '倫敦', tz: 'Europe/London', timeId: 'ldn-time', dateId: 'ldn-date' },
    { city: '東京', tz: 'Asia/Tokyo', timeId: 'tokyo-time', dateId: 'tokyo-date' },
    { city: '沙烏地阿拉伯', tz: 'Asia/Riyadh', timeId: 'riyadh-time', dateId: 'riyadh-date' }
];

function pad(num) {
    return num.toString().padStart(2, '0');
}

function formatTime(date) {
    return `${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`;
}

function formatDate(date) {
    return `${date.getFullYear()}:${pad(date.getMonth()+1)}:${pad(date.getDate())}`;
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

window.addEventListener('DOMContentLoaded', () => {
    updateClocks();
    setInterval(updateClocks, 1000);
});

// 預留：可擴充更多時區，只需在 timezones 陣列中新增即可。
