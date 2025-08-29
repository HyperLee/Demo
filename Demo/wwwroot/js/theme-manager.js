// 深色模式主題管理器
class ThemeManager {
    constructor() {
        this.STORAGE_KEY = 'preferred-theme';
        this.DEFAULT_THEME = 'light';
        this.init();
    }
    
    init() {
        // 載入已儲存的主題偏好
        this.loadTheme();
        
        // 監聽系統主題變化
        this.watchSystemTheme();
        
        // 綁定主題切換按鈕事件
        this.bindThemeButtons();
        
        // 初始化主題顯示
        this.updateThemeDisplay();
    }
    
    loadTheme() {
        const savedTheme = localStorage.getItem(this.STORAGE_KEY);
        const preferredTheme = savedTheme || 'auto';
        this.setTheme(preferredTheme);
    }
    
    setTheme(theme) {
        const actualTheme = theme === 'auto' ? this.getSystemTheme() : theme;
        
        // 設定 HTML 屬性
        document.documentElement.setAttribute('data-bs-theme', actualTheme);
        
        // 儲存用戶偏好
        localStorage.setItem(this.STORAGE_KEY, theme);
        
        // 更新顯示
        this.updateThemeDisplay(theme);
        
        // 觸發主題變更事件
        this.dispatchThemeChangeEvent(actualTheme);
    }
    
    getSystemTheme() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    
    watchSystemTheme() {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        mediaQuery.addEventListener('change', () => {
            const currentPreference = localStorage.getItem(this.STORAGE_KEY);
            if (currentPreference === 'auto' || !currentPreference) {
                this.setTheme('auto');
            }
        });
    }
    
    bindThemeButtons() {
        document.addEventListener('DOMContentLoaded', () => {
            document.querySelectorAll('[data-bs-theme-value]').forEach(button => {
                button.addEventListener('click', (e) => {
                    const theme = e.currentTarget.getAttribute('data-bs-theme-value');
                    this.setTheme(theme);
                });
            });
        });
    }
    
    updateThemeDisplay(preferredTheme = null) {
        const theme = preferredTheme || localStorage.getItem(this.STORAGE_KEY) || 'auto';
        const actualTheme = theme === 'auto' ? this.getSystemTheme() : theme;
        
        // 更新圖示
        const themeIcon = document.getElementById('themeIcon');
        if (themeIcon) {
            const icons = {
                'light': 'fa-sun',
                'dark': 'fa-moon',
                'auto': 'fa-desktop'
            };
            themeIcon.className = `fas ${icons[theme]}`;
        }
        
        // 更新標籤文字
        const themeLabel = document.getElementById('themeLabel');
        if (themeLabel) {
            const labels = {
                'light': '淺色模式',
                'dark': '深色模式',
                'auto': '跟隨系統'
            };
            themeLabel.textContent = labels[theme];
        }
        
        // 更新下拉選單的選中狀態
        document.querySelectorAll('[data-bs-theme-value]').forEach(button => {
            button.classList.toggle('active', 
                button.getAttribute('data-bs-theme-value') === theme);
        });
    }
    
    dispatchThemeChangeEvent(actualTheme) {
        const event = new CustomEvent('themeChanged', {
            detail: { theme: actualTheme }
        });
        document.dispatchEvent(event);
    }
    
    // 公開方法
    getCurrentTheme() {
        return document.documentElement.getAttribute('data-bs-theme');
    }
    
    getPreferredTheme() {
        return localStorage.getItem(this.STORAGE_KEY) || 'auto';
    }
}

// 初始化主題管理器
let themeManager;
document.addEventListener('DOMContentLoaded', function() {
    themeManager = new ThemeManager();
});

// 監聽主題變更事件
document.addEventListener('themeChanged', function(e) {
    console.log('主題已切換至:', e.detail.theme);
    
    // 可以在這裡執行主題切換後的額外處理
    if (typeof updateChartTheme === 'function') {
        updateChartTheme(e.detail.theme);
    }
    
    if (typeof updateMapTheme === 'function') {
        updateMapTheme(e.detail.theme);
    }
});

// 圖表主題配置
function getChartTheme() {
    const isDark = document.documentElement.getAttribute('data-bs-theme') === 'dark';
    
    return {
        backgroundColor: isDark ? '#2d2d2d' : '#ffffff',
        color: isDark ? '#e9ecef' : '#212529',
        gridColor: isDark ? '#404040' : '#dee2e6',
        tickColor: isDark ? '#adb5bd' : '#6c757d'
    };
}

// 更新圖表主題
function updateChartTheme(theme) {
    if (typeof Chart === 'undefined') return;
    
    const charts = Chart.instances || {};
    
    Object.values(charts).forEach(chart => {
        if (chart && chart.options) {
            const themeConfig = getChartTheme();
            
            // 更新背景色
            chart.options.plugins = chart.options.plugins || {};
            chart.options.plugins.legend = chart.options.plugins.legend || {};
            chart.options.plugins.legend.labels = chart.options.plugins.legend.labels || {};
            chart.options.plugins.legend.labels.color = themeConfig.color;
            
            // 更新網格線顏色
            if (chart.options.scales) {
                Object.keys(chart.options.scales).forEach(scaleKey => {
                    const scale = chart.options.scales[scaleKey];
                    if (scale.grid) scale.grid.color = themeConfig.gridColor;
                    if (scale.ticks) scale.ticks.color = themeConfig.tickColor;
                });
            }
            
            chart.update();
        }
    });
}

// 防抖主題切換
let themeChangeTimeout;
function debouncedThemeChange(theme) {
    clearTimeout(themeChangeTimeout);
    themeChangeTimeout = setTimeout(() => {
        if (themeManager) {
            themeManager.setTheme(theme);
        }
    }, 100);
}

// 使用 requestAnimationFrame 優化動畫
function smoothThemeTransition(callback) {
    document.body.classList.add('theme-transitioning');
    
    requestAnimationFrame(() => {
        callback();
        
        setTimeout(() => {
            document.body.classList.remove('theme-transitioning');
        }, 300);
    });
}

// 檢查瀏覽器支援
function checkThemeSupport() {
    // 檢查 CSS Custom Properties 支援
    const supportsCustomProperties = window.CSS && CSS.supports && CSS.supports('color', 'var(--primary)');
    
    // 檢查 prefers-color-scheme 支援
    const supportsColorScheme = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches !== undefined;
    
    if (!supportsCustomProperties) {
        console.warn('瀏覽器不支援 CSS Custom Properties，主題功能可能異常');
        // 載入備援 CSS 檔案
        loadFallbackCSS();
    }
    
    return {
        customProperties: supportsCustomProperties,
        colorScheme: supportsColorScheme
    };
}

function loadFallbackCSS() {
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = '/css/theme-fallback.css';
    document.head.appendChild(link);
}
