/**
 * 增強型多時區時鐘 JavaScript - P2 版本
 * 支援自訂時區選擇、格式設定、深色模式及多語言
 */

class EnhancedClock {
    constructor() {
        // 預設設定
        this.defaultSettings = {
            language: 'zh-TW',
            timezone: 'local', // 改為 'local' 作為預設值
            theme: 'auto',
            timeFormat: {
                use24HourFormat: true,
                showSeconds: true,
                showDate: true,
                timeFormat: 'HH:MM:SS',
                dateFormat: 'YYYY/MM/DD',
                dateSeparator: '/',
                showWeekday: false
            }
        };
        
        this.currentSettings = { ...this.defaultSettings };
        this.timezones = [];
        this.timeFormats = {};
        this.languages = [];
        this.clockInterval = null;
        this.isInitialized = false;
        
        this.init();
    }

    /**
     * 初始化時鐘系統
     */
    async init() {
        try {
            // 載入設定
            this.loadSettings();
            
            // 設定主題
            this.applyTheme();
            
            // 載入翻譯資源
            await this.loadI18nResources();
            
            // 載入時區資料
            await this.loadTimezones();
            
            // 載入格式選項
            await this.loadTimeFormats();
            
            // 載入語言選項
            await this.loadLanguages();
            
            // 設定事件監聽器
            this.setupEventListeners();
            
            // 初始化 UI
            this.initializeUI();
            
            // 開始時鐘更新
            this.startClock();
            
            // 隱藏載入畫面
            this.hideLoading();
            
            this.isInitialized = true;
            console.log('Enhanced Clock initialized successfully');
            
        } catch (error) {
            console.error('Failed to initialize Enhanced Clock:', error);
            this.showError('初始化時鐘系統失敗');
        }
    }

    /**
     * 載入使用者設定
     */
    loadSettings() {
        try {
            const saved = localStorage.getItem('clockSettings');
            if (saved) {
                const parsed = JSON.parse(saved);
                this.currentSettings = { ...this.defaultSettings, ...parsed };
                
                // 確保時間格式物件存在
                if (!this.currentSettings.timeFormat) {
                    this.currentSettings.timeFormat = { ...this.defaultSettings.timeFormat };
                }
            }
        } catch (error) {
            console.warn('Failed to load settings, using defaults:', error);
            this.currentSettings = { ...this.defaultSettings };
        }
    }

    /**
     * 儲存使用者設定
     */
    saveSettings() {
        try {
            localStorage.setItem('clockSettings', JSON.stringify(this.currentSettings));
            console.log('Settings saved successfully');
        } catch (error) {
            console.error('Failed to save settings:', error);
            this.showError('儲存設定失敗');
        }
    }

    /**
     * 套用主題
     */
    applyTheme() {
        const theme = this.currentSettings.theme;
        const root = document.documentElement;
        
        if (theme === 'auto') {
            // 檢測系統偏好
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            root.setAttribute('data-theme', prefersDark ? 'dark' : 'light');
        } else {
            root.setAttribute('data-theme', theme);
        }
        
        // 更新主題按鈕圖示
        this.updateThemeIcon();
    }

    /**
     * 更新主題按鈕圖示
     */
    updateThemeIcon() {
        const themeIcon = document.querySelector('.theme-icon');
        const currentTheme = document.documentElement.getAttribute('data-theme');
        
        if (themeIcon) {
            themeIcon.textContent = currentTheme === 'dark' ? '☀️' : '🌙';
        }
    }

    /**
     * 載入翻譯資源
     */
    async loadI18nResources() {
        const language = this.currentSettings.language;
        
        // 如果資源尚未載入，動態載入
        if (!window.i18nResources || !window.i18nResources[language]) {
            try {
                const script = document.createElement('script');
                script.src = `/js/i18n-${language}.js`;
                script.async = true;
                
                await new Promise((resolve, reject) => {
                    script.onload = resolve;
                    script.onerror = reject;
                    document.head.appendChild(script);
                });
                
            } catch (error) {
                console.warn(`Failed to load i18n resources for ${language}, falling back to zh-TW`);
                this.currentSettings.language = 'zh-TW';
            }
        }
        
        this.applyTranslations();
    }

    /**
     * 套用翻譯
     */
    applyTranslations() {
        const language = this.currentSettings.language;
        const resources = window.i18nResources && window.i18nResources[language];
        
        if (!resources) return;
        
        // 翻譯所有標記了 data-i18n 的元素
        document.querySelectorAll('[data-i18n]').forEach(element => {
            const key = element.getAttribute('data-i18n');
            const translation = this.getTranslation(key, resources);
            
            if (translation) {
                if (element.tagName === 'INPUT' && element.type === 'submit') {
                    element.value = translation;
                } else {
                    element.textContent = translation;
                }
            }
        });
        
        // 更新 placeholder
        document.querySelectorAll('[data-i18n-placeholder]').forEach(element => {
            const key = element.getAttribute('data-i18n-placeholder');
            const translation = this.getTranslation(key, resources);
            
            if (translation) {
                element.placeholder = translation;
            }
        });
    }

    /**
     * 取得翻譯文字
     */
    getTranslation(key, resources) {
        const keys = key.split('.');
        let current = resources;
        
        for (const k of keys) {
            if (current && current[k]) {
                current = current[k];
            } else {
                return null;
            }
        }
        
        return current;
    }

    /**
     * 載入時區資料
     */
    async loadTimezones() {
        try {
            const response = await fetch('?handler=WorldTimes');
            const data = await response.json();
            
            if (data.success) {
                this.timezones = data.data;
                this.populateTimezoneSelector();
            } else {
                throw new Error(data.error || 'Failed to load timezones');
            }
        } catch (error) {
            console.error('Failed to load timezones:', error);
            this.showError('載入時區資料失敗');
        }
    }

    /**
     * 填充時區選擇器
     */
    populateTimezoneSelector() {
        const select = document.getElementById('timezone-select');
        if (!select) return;
        
        // 清空現有選項
        select.innerHTML = '<option value="local">本地時間</option>';
        
        // 按地區分組
        const regions = {};
        this.timezones.forEach(tz => {
            if (!regions[tz.region]) {
                regions[tz.region] = [];
            }
            regions[tz.region].push(tz);
        });
        
        // 建立分組選項
        Object.keys(regions).sort().forEach(region => {
            const optgroup = document.createElement('optgroup');
            optgroup.label = region;
            
            regions[region].forEach(tz => {
                const option = document.createElement('option');
                option.value = tz.timezone;
                option.textContent = `${tz.city} (${tz.displayName})`;
                optgroup.appendChild(option);
            });
            
            select.appendChild(optgroup);
        });
        
        // 設定目前選擇的時區
        select.value = this.currentSettings.timezone === 'Asia/Taipei' ? 'local' : this.currentSettings.timezone;
    }

    /**
     * 載入時間格式選項
     */
    async loadTimeFormats() {
        try {
            const response = await fetch('?handler=TimeFormats');
            const data = await response.json();
            
            if (data.success) {
                this.timeFormats = data.data;
                this.populateFormatSelectors();
            } else {
                throw new Error(data.error || 'Failed to load time formats');
            }
        } catch (error) {
            console.error('Failed to load time formats:', error);
            this.showError('載入時間格式失敗');
        }
    }

    /**
     * 填充格式選擇器
     */
    populateFormatSelectors() {
        const timeSelect = document.getElementById('timeFormatSelect');
        const dateSelect = document.getElementById('dateFormatSelect');
        
        if (timeSelect && this.timeFormats.time24Hour) {
            timeSelect.innerHTML = '';
            const formats = this.currentSettings.timeFormat.use24HourFormat 
                ? this.timeFormats.time24Hour 
                : this.timeFormats.time12Hour;
                
            formats.forEach(format => {
                const option = document.createElement('option');
                option.value = format.value;
                option.textContent = `${format.label} (${format.example})`;
                timeSelect.appendChild(option);
            });
            
            timeSelect.value = this.currentSettings.timeFormat.timeFormat;
        }
        
        if (dateSelect && this.timeFormats.dateFormats) {
            dateSelect.innerHTML = '';
            this.timeFormats.dateFormats.forEach(format => {
                const option = document.createElement('option');
                option.value = format.value;
                option.textContent = `${format.label} (${format.example})`;
                dateSelect.appendChild(option);
            });
            
            dateSelect.value = this.currentSettings.timeFormat.dateFormat;
        }
    }

    /**
     * 載入語言選項
     */
    async loadLanguages() {
        try {
            const response = await fetch('?handler=Languages');
            const data = await response.json();
            
            if (data.success) {
                this.languages = data.data;
                this.populateLanguagePanel();
            } else {
                throw new Error(data.error || 'Failed to load languages');
            }
        } catch (error) {
            console.error('Failed to load languages:', error);
            this.showError('載入語言選項失敗');
        }
    }

    /**
     * 填充語言面板
     */
    populateLanguagePanel() {
        const panel = document.querySelector('.language-options');
        if (!panel) return;
        
        panel.innerHTML = '';
        
        this.languages.forEach(lang => {
            const option = document.createElement('div');
            option.className = 'language-option';
            option.setAttribute('data-lang', lang.code);
            
            if (lang.code === this.currentSettings.language) {
                option.classList.add('active');
            }
            
            option.innerHTML = `
                <span class="flag">${lang.flag}</span>
                <span class="name">${lang.nativeName}</span>
            `;
            
            panel.appendChild(option);
        });
    }

    /**
     * 設定事件監聽器
     */
    setupEventListeners() {
        // 主題切換按鈕
        const themeToggle = document.getElementById('theme-toggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', () => this.toggleTheme());
        }
        
        // 設定按鈕
        const settingsToggle = document.getElementById('settings-toggle');
        if (settingsToggle) {
            settingsToggle.addEventListener('click', () => this.openSettings());
        }
        
        // 語言切換按鈕
        const languageToggle = document.getElementById('language-toggle');
        if (languageToggle) {
            languageToggle.addEventListener('click', (e) => this.toggleLanguagePanel(e));
        }
        
        // 時區選擇器
        const timezoneSelect = document.getElementById('timezone-select');
        if (timezoneSelect) {
            timezoneSelect.addEventListener('change', (e) => this.changeTimezone(e.target.value));
        }
        
        // 設定面板事件
        this.setupSettingsPanelEvents();
        
        // 語言面板事件
        this.setupLanguagePanelEvents();
        
        // 系統主題變更監聽
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
            if (this.currentSettings.theme === 'auto') {
                this.applyTheme();
            }
        });
        
        // 點擊外部關閉面板
        document.addEventListener('click', (e) => this.handleOutsideClick(e));
        
        // 鍵盤快捷鍵
        document.addEventListener('keydown', (e) => this.handleKeyboard(e));
    }

    /**
     * 設定面板事件
     */
    setupSettingsPanelEvents() {
        // 設定面板關閉
        const settingsClose = document.querySelector('.settings-close');
        if (settingsClose) {
            settingsClose.addEventListener('click', () => this.closeSettings());
        }
        
        // 24小時制切換
        const use24hour = document.getElementById('use24hour');
        if (use24hour) {
            use24hour.checked = this.currentSettings.timeFormat.use24HourFormat;
            use24hour.addEventListener('change', (e) => {
                this.currentSettings.timeFormat.use24HourFormat = e.target.checked;
                this.populateFormatSelectors();
                this.updatePreview();
            });
        }
        
        // 顯示秒數切換
        const showSeconds = document.getElementById('showSeconds');
        if (showSeconds) {
            showSeconds.checked = this.currentSettings.timeFormat.showSeconds;
            showSeconds.addEventListener('change', (e) => {
                this.currentSettings.timeFormat.showSeconds = e.target.checked;
                this.updatePreview();
            });
        }
        
        // 顯示日期切換
        const showDate = document.getElementById('showDate');
        if (showDate) {
            showDate.checked = this.currentSettings.timeFormat.showDate;
            showDate.addEventListener('change', (e) => {
                this.currentSettings.timeFormat.showDate = e.target.checked;
                this.updatePreview();
            });
        }
        
        // 顯示星期切換
        const showWeekday = document.getElementById('showWeekday');
        if (showWeekday) {
            showWeekday.checked = this.currentSettings.timeFormat.showWeekday;
            showWeekday.addEventListener('change', (e) => {
                this.currentSettings.timeFormat.showWeekday = e.target.checked;
                this.updatePreview();
            });
        }
        
        // 時間格式選擇
        const timeFormatSelect = document.getElementById('timeFormatSelect');
        if (timeFormatSelect) {
            timeFormatSelect.addEventListener('change', (e) => {
                this.currentSettings.timeFormat.timeFormat = e.target.value;
                this.updatePreview();
            });
        }
        
        // 日期格式選擇
        const dateFormatSelect = document.getElementById('dateFormatSelect');
        if (dateFormatSelect) {
            dateFormatSelect.addEventListener('change', (e) => {
                this.currentSettings.timeFormat.dateFormat = e.target.value;
                this.updatePreview();
            });
        }
        
        // 主題單選按鈕
        document.querySelectorAll('input[name="themeMode"]').forEach(radio => {
            if (radio.value === this.currentSettings.theme) {
                radio.checked = true;
            }
            
            radio.addEventListener('change', (e) => {
                if (e.target.checked) {
                    this.currentSettings.theme = e.target.value;
                    this.applyTheme();
                }
            });
        });
        
        // 重設按鈕
        const resetButton = document.getElementById('resetSettings');
        if (resetButton) {
            resetButton.addEventListener('click', () => this.resetSettings());
        }
        
        // 儲存按鈕
        const saveButton = document.getElementById('saveSettings');
        if (saveButton) {
            saveButton.addEventListener('click', () => this.saveAndCloseSettings());
        }
    }

    /**
     * 語言面板事件
     */
    setupLanguagePanelEvents() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('.language-option')) {
                const langCode = e.target.closest('.language-option').getAttribute('data-lang');
                this.changeLanguage(langCode);
            }
        });
    }

    /**
     * 初始化 UI
     */
    initializeUI() {
        // 設定時區選擇器當前值
        const timezoneSelect = document.getElementById('timezone-select');
        if (timezoneSelect) {
            const currentTz = this.currentSettings.timezone;
            timezoneSelect.value = (currentTz === 'local' || currentTz === 'Asia/Taipei') ? 'local' : currentTz;
        }
        
        // 更新設定面板的值
        this.syncSettingsPanel();
        
        // 更新預覽
        this.updatePreview();
        
        // 確保主時鐘標籤正確顯示
        this.updateClock();
    }

    /**
     * 同步設定面板的值
     */
    syncSettingsPanel() {
        const format = this.currentSettings.timeFormat;
        
        const use24hour = document.getElementById('use24hour');
        if (use24hour) use24hour.checked = format.use24HourFormat;
        
        const showSeconds = document.getElementById('showSeconds');
        if (showSeconds) showSeconds.checked = format.showSeconds;
        
        const showDate = document.getElementById('showDate');
        if (showDate) showDate.checked = format.showDate;
        
        const showWeekday = document.getElementById('showWeekday');
        if (showWeekday) showWeekday.checked = format.showWeekday;
        
        const timeFormatSelect = document.getElementById('timeFormatSelect');
        if (timeFormatSelect) timeFormatSelect.value = format.timeFormat;
        
        const dateFormatSelect = document.getElementById('dateFormatSelect');
        if (dateFormatSelect) dateFormatSelect.value = format.dateFormat;
        
        document.querySelectorAll('input[name="themeMode"]').forEach(radio => {
            radio.checked = radio.value === this.currentSettings.theme;
        });
    }

    /**
     * 開始時鐘更新
     */
    startClock() {
        if (this.clockInterval) {
            clearInterval(this.clockInterval);
        }
        
        // 立即更新一次
        this.updateClock();
        
        // 每秒更新
        this.clockInterval = setInterval(() => {
            this.updateClock();
        }, 1000);
    }

    /**
     * 更新時鐘顯示
     */
    updateClock() {
        const now = new Date();
        
        // 更新本地時間
        this.updateLocalClock(now);
        
        // 更新各時區時間
        this.updateTimezoneClocks(now);
        
        // 更新設定預覽
        this.updatePreview(now);
    }

    /**
     * 更新本地時鐘（支援自訂時區）
     */
    updateLocalClock(now) {
        const localTime = document.getElementById('local-time');
        const localDate = document.getElementById('local-date');
        const clockLabel = document.querySelector('.large-clock .clock-label');
        
        // 取得選擇的時區，預設為本地時區
        const selectedTimezone = this.currentSettings.timezone;
        let displayTime, displayDate, labelText;
        
        try {
            if (selectedTimezone && selectedTimezone !== 'local' && selectedTimezone !== 'Asia/Taipei') {
                // 使用選擇的時區
                const tzTime = new Date(now.toLocaleString("en-US", { timeZone: selectedTimezone }));
                displayTime = this.formatTime(tzTime);
                displayDate = this.formatDate(tzTime);
                
                // 找到時區的顯示名稱
                const timezoneInfo = this.timezones.find(tz => tz.timezone === selectedTimezone);
                labelText = timezoneInfo ? `${timezoneInfo.city}時間` : '選擇的時區';
            } else {
                // 使用本地時區
                displayTime = this.formatTime(now);
                displayDate = this.formatDate(now);
                labelText = '本地時間';
            }
        } catch (error) {
            console.error('Error updating timezone:', error);
            // 降級到本地時間
            displayTime = this.formatTime(now);
            displayDate = this.formatDate(now);
            labelText = '本地時間';
        }
        
        if (localTime) {
            localTime.textContent = displayTime;
        }
        
        if (localDate) {
            localDate.textContent = displayDate;
        }
        
        if (clockLabel) {
            clockLabel.textContent = labelText;
        }
    }

    /**
     * 更新時區時鐘
     */
    updateTimezoneClocks(now) {
        const clocks = [
            { id: 'ny-time', dateId: 'ny-date', timezone: 'America/New_York' },
            { id: 'ldn-time', dateId: 'ldn-date', timezone: 'Europe/London' },
            { id: 'tokyo-time', dateId: 'tokyo-date', timezone: 'Asia/Tokyo' },
            { id: 'riyadh-time', dateId: 'riyadh-date', timezone: 'Asia/Riyadh' }
        ];
        
        clocks.forEach(clock => {
            const timeElement = document.getElementById(clock.id);
            const dateElement = document.getElementById(clock.dateId);
            
            if (timeElement && dateElement) {
                try {
                    const tzTime = new Date(now.toLocaleString("en-US", { timeZone: clock.timezone }));
                    timeElement.textContent = this.formatTime(tzTime);
                    dateElement.textContent = this.formatDate(tzTime);
                } catch (error) {
                    timeElement.textContent = '時間錯誤';
                    dateElement.textContent = '日期錯誤';
                }
            }
        });
    }

    /**
     * 格式化時間
     */
    formatTime(date) {
        const format = this.currentSettings.timeFormat;
        const use24Hour = format.use24HourFormat;
        const showSeconds = format.showSeconds;
        
        let timeString;
        
        if (use24Hour) {
            if (showSeconds) {
                timeString = date.toLocaleTimeString('en-GB'); // HH:MM:SS
            } else {
                timeString = date.toLocaleTimeString('en-GB', { 
                    hour: '2-digit', 
                    minute: '2-digit' 
                }); // HH:MM
            }
        } else {
            const options = { 
                hour: '2-digit', 
                minute: '2-digit',
                hour12: true 
            };
            
            if (showSeconds) {
                options.second = '2-digit';
            }
            
            timeString = date.toLocaleTimeString('en-US', options);
        }
        
        return timeString;
    }

    /**
     * 格式化日期
     */
    formatDate(date) {
        const format = this.currentSettings.timeFormat;
        
        if (!format.showDate) return '';
        
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        
        let dateString;
        
        switch (format.dateFormat) {
            case 'YYYY/MM/DD':
                dateString = `${year}/${month}/${day}`;
                break;
            case 'YYYY-MM-DD':
                dateString = `${year}-${month}-${day}`;
                break;
            case 'MM/DD/YYYY':
                dateString = `${month}/${day}/${year}`;
                break;
            case 'DD/MM/YYYY':
                dateString = `${day}/${month}/${year}`;
                break;
            case 'YYYY年MM月DD日':
                dateString = `${year}年${month}月${day}日`;
                break;
            case 'MM月DD日':
                dateString = `${month}月${day}日`;
                break;
            case 'dddd, YYYY年MM月DD日':
                const weekday = this.getWeekday(date.getDay());
                dateString = `${weekday}, ${year}年${month}月${day}日`;
                break;
            default:
                dateString = `${year}/${month}/${day}`;
        }
        
        return dateString;
    }

    /**
     * 取得星期名稱
     */
    getWeekday(dayIndex) {
        const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
        const dayKey = days[dayIndex];
        
        const language = this.currentSettings.language;
        const resources = window.i18nResources && window.i18nResources[language];
        
        if (resources && resources.weekdays && resources.weekdays[dayKey]) {
            return resources.weekdays[dayKey];
        }
        
        // 預設繁體中文
        const defaultDays = ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'];
        return defaultDays[dayIndex];
    }

    /**
     * 更新預覽
     */
    updatePreview(date = new Date()) {
        const previewTime = document.getElementById('preview-time');
        const previewDate = document.getElementById('preview-date');
        
        // 根據選擇的時區調整預覽時間
        let previewDisplayTime = date;
        const selectedTimezone = this.currentSettings.timezone;
        
        try {
            if (selectedTimezone && selectedTimezone !== 'local' && selectedTimezone !== 'Asia/Taipei') {
                previewDisplayTime = new Date(date.toLocaleString("en-US", { timeZone: selectedTimezone }));
            }
        } catch (error) {
            console.error('Error in preview timezone conversion:', error);
            // 使用原始時間作為降級
        }
        
        if (previewTime) {
            previewTime.textContent = this.formatTime(previewDisplayTime);
        }
        
        if (previewDate) {
            previewDate.textContent = this.formatDate(previewDisplayTime);
        }
    }

    /**
     * 切換主題
     */
    toggleTheme() {
        const themes = ['light', 'dark', 'auto'];
        const currentIndex = themes.indexOf(this.currentSettings.theme);
        const nextIndex = (currentIndex + 1) % themes.length;
        
        this.currentSettings.theme = themes[nextIndex];
        this.applyTheme();
        this.saveSettings();
        
        // 更新設定面板中的單選按鈕
        const themeRadios = document.querySelectorAll('input[name="themeMode"]');
        themeRadios.forEach(radio => {
            radio.checked = radio.value === this.currentSettings.theme;
        });
    }

    /**
     * 開啟設定面板
     */
    openSettings() {
        const panel = document.getElementById('settings-panel');
        if (panel) {
            panel.style.display = 'flex';
            panel.setAttribute('aria-hidden', 'false');
            
            // 同步目前設定值
            this.syncSettingsPanel();
            this.updatePreview();
        }
    }

    /**
     * 關閉設定面板
     */
    closeSettings() {
        const panel = document.getElementById('settings-panel');
        if (panel) {
            panel.style.display = 'none';
            panel.setAttribute('aria-hidden', 'true');
        }
    }

    /**
     * 切換語言面板
     */
    toggleLanguagePanel(event) {
        event.stopPropagation();
        const panel = document.getElementById('language-panel');
        
        if (panel) {
            const isVisible = panel.style.display === 'block';
            panel.style.display = isVisible ? 'none' : 'block';
            panel.setAttribute('aria-hidden', isVisible ? 'true' : 'false');
            
            if (!isVisible) {
                // 定位面板到按鈕下方
                const button = event.currentTarget;
                const rect = button.getBoundingClientRect();
                panel.style.position = 'fixed';
                panel.style.top = `${rect.bottom + 5}px`;
                panel.style.right = `${window.innerWidth - rect.right}px`;
            }
        }
    }

    /**
     * 變更時區
     */
    changeTimezone(timezone) {
        if (timezone !== undefined) {
            // 處理本地時間選項
            if (timezone === 'local' || timezone === '') {
                this.currentSettings.timezone = 'local';
            } else {
                this.currentSettings.timezone = timezone;
            }
            
            this.saveSettings();
            
            // 立即更新時鐘顯示
            this.updateClock();
            
            // 更新預覽（如果設定面板開啟的話）
            this.updatePreview();
            
            console.log('Timezone changed to:', this.currentSettings.timezone);
            
            // 顯示成功訊息
            const displayName = this.currentSettings.timezone === 'local' 
                ? '本地時間' 
                : this.getTimezoneDisplayName(this.currentSettings.timezone);
            this.showSuccess(`時區已切換至 ${displayName}`);
        }
    }

    /**
     * 取得時區顯示名稱
     */
    getTimezoneDisplayName(timezone) {
        const timezoneInfo = this.timezones.find(tz => tz.timezone === timezone);
        return timezoneInfo ? `${timezoneInfo.city} (${timezoneInfo.displayName})` : timezone;
    }

    /**
     * 變更語言
     */
    async changeLanguage(langCode) {
        if (langCode !== this.currentSettings.language) {
            this.currentSettings.language = langCode;
            this.saveSettings();
            
            // 重新載入翻譯資源
            await this.loadI18nResources();
            
            // 更新語言面板的active狀態
            document.querySelectorAll('.language-option').forEach(option => {
                option.classList.toggle('active', option.getAttribute('data-lang') === langCode);
            });
            
            // 隱藏語言面板
            const panel = document.getElementById('language-panel');
            if (panel) {
                panel.style.display = 'none';
            }
        }
    }

    /**
     * 重設設定
     */
    resetSettings() {
        if (confirm('確定要重設所有設定嗎？')) {
            this.currentSettings = { ...this.defaultSettings };
            this.syncSettingsPanel();
            this.populateFormatSelectors();
            this.applyTheme();
            this.updatePreview();
            
            // 更新時區選擇器
            const timezoneSelect = document.getElementById('timezone-select');
            if (timezoneSelect) {
                timezoneSelect.value = this.currentSettings.timezone;
            }
        }
    }

    /**
     * 儲存並關閉設定
     */
    saveAndCloseSettings() {
        this.saveSettings();
        this.closeSettings();
        this.showSuccess('設定已儲存');
    }

    /**
     * 處理外部點擊
     */
    handleOutsideClick(event) {
        // 關閉語言面板
        const languagePanel = document.getElementById('language-panel');
        const languageButton = document.getElementById('language-toggle');
        
        if (languagePanel && languagePanel.style.display === 'block') {
            if (!languagePanel.contains(event.target) && !languageButton.contains(event.target)) {
                languagePanel.style.display = 'none';
            }
        }
    }

    /**
     * 處理鍵盤快捷鍵
     */
    handleKeyboard(event) {
        // ESC 鍵關閉面板
        if (event.key === 'Escape') {
            this.closeSettings();
            
            const languagePanel = document.getElementById('language-panel');
            if (languagePanel) {
                languagePanel.style.display = 'none';
            }
        }
    }

    /**
     * 隱藏載入畫面
     */
    hideLoading() {
        const loadingOverlay = document.getElementById('loading-overlay');
        if (loadingOverlay) {
            setTimeout(() => {
                loadingOverlay.style.opacity = '0';
                setTimeout(() => {
                    loadingOverlay.style.display = 'none';
                }, 300);
            }, 500);
        }
    }

    /**
     * 顯示錯誤訊息
     */
    showError(message) {
        const errorElement = document.getElementById('clock-error');
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.style.display = 'block';
            
            setTimeout(() => {
                errorElement.style.display = 'none';
            }, 5000);
        }
        
        console.error(message);
    }

    /**
     * 顯示成功訊息
     */
    showSuccess(message) {
        // 建立成功訊息元素
        let successElement = document.getElementById('success-message');
        
        if (!successElement) {
            successElement = document.createElement('div');
            successElement.id = 'success-message';
            successElement.style.cssText = `
                position: fixed;
                top: 100px;
                right: 20px;
                background: var(--btn-primary);
                color: white;
                padding: 0.75rem 1rem;
                border-radius: 0.5rem;
                box-shadow: var(--shadow-lg);
                z-index: 2000;
                opacity: 0;
                transform: translateX(100%);
                transition: all 0.3s ease;
                font-weight: 500;
                max-width: 300px;
            `;
            document.body.appendChild(successElement);
        }
        
        successElement.textContent = message;
        
        // 顯示動畫
        setTimeout(() => {
            successElement.style.opacity = '1';
            successElement.style.transform = 'translateX(0)';
        }, 10);
        
        // 隱藏動畫
        setTimeout(() => {
            successElement.style.opacity = '0';
            successElement.style.transform = 'translateX(100%)';
        }, 3000);
        
        console.log(message);
    }

    /**
     * 銷毀時鐘系統
     */
    destroy() {
        if (this.clockInterval) {
            clearInterval(this.clockInterval);
            this.clockInterval = null;
        }
        
        this.isInitialized = false;
        console.log('Enhanced Clock destroyed');
    }
}

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', () => {
    // 等待 DOM 完全載入後再初始化
    setTimeout(() => {
        window.enhancedClock = new EnhancedClock();
    }, 100);
});

// 頁面卸載時清理
window.addEventListener('beforeunload', () => {
    if (window.enhancedClock) {
        window.enhancedClock.destroy();
    }
});
