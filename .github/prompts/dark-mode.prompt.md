# 深色模式開發規格書

## 專案概述
為整個個人管理系統實施統一的深色模式支援，提升用戶的視覺體驗和夜間使用舒適度。此功能將覆蓋所有現有頁面，並建立可維護的主題切換機制。

## 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **前端技術**: Bootstrap 5, CSS3 Custom Properties, JavaScript
- **主題存儲**: LocalStorage (瀏覽器本地存儲)
- **CSS 方法**: CSS Custom Properties (CSS Variables)
- **兼容性**: 現代瀏覽器 (Chrome 49+, Firefox 31+, Safari 9.1+)

## 核心功能模組

### 1. 主題系統架構

#### 1.1 CSS 變量定義
```css
/* 在 site.css 中定義主題變量 */
:root {
    /* 淺色模式 (預設) */
    --bs-body-bg: #ffffff;
    --bs-body-color: #212529;
    --bs-primary: #0d6efd;
    --bs-secondary: #6c757d;
    --bs-success: #198754;
    --bs-info: #0dcaf0;
    --bs-warning: #ffc107;
    --bs-danger: #dc3545;
    --bs-light: #f8f9fa;
    --bs-dark: #212529;
    
    /* 自定義變量 */
    --surface-color: #ffffff;
    --surface-variant: #f8f9fa;
    --border-color: #dee2e6;
    --shadow-color: rgba(0, 0, 0, 0.1);
    --text-muted: #6c757d;
    --navbar-bg: #ffffff;
    --sidebar-bg: #f8f9fa;
    --card-bg: #ffffff;
    --modal-bg: #ffffff;
    --input-bg: #ffffff;
    --input-border: #ced4da;
    --table-bg: #ffffff;
    --table-striped: rgba(0, 0, 0, 0.05);
}

/* 深色模式 */
[data-bs-theme="dark"] {
    --bs-body-bg: #121212;
    --bs-body-color: #e9ecef;
    --bs-primary: #6ea8fe;
    --bs-secondary: #6c757d;
    --bs-success: #75b798;
    --bs-info: #6edff6;
    --bs-warning: #ffda6a;
    --bs-danger: #ea868f;
    --bs-light: #343a40;
    --bs-dark: #e9ecef;
    
    /* 自定義深色變量 */
    --surface-color: #1e1e1e;
    --surface-variant: #2d2d2d;
    --border-color: #404040;
    --shadow-color: rgba(0, 0, 0, 0.3);
    --text-muted: #adb5bd;
    --navbar-bg: #1e1e1e;
    --sidebar-bg: #2d2d2d;
    --card-bg: #1e1e1e;
    --modal-bg: #2d2d2d;
    --input-bg: #2d2d2d;
    --input-border: #404040;
    --table-bg: #1e1e1e;
    --table-striped: rgba(255, 255, 255, 0.05);
}
```

#### 1.2 主題切換按鈕設計
```html
<!-- 在 _Layout.cshtml 導航列中加入主題切換按鈕 -->
<div class="navbar-nav ms-auto">
    <div class="nav-item dropdown">
        <button class="btn btn-link nav-link dropdown-toggle" type="button" 
                data-bs-toggle="dropdown" aria-expanded="false" id="themeDropdown">
            <i class="fas fa-palette" id="themeIcon"></i>
            <span class="d-none d-md-inline ms-1" id="themeLabel">主題</span>
        </button>
        <ul class="dropdown-menu dropdown-menu-end">
            <li>
                <button class="dropdown-item" type="button" data-bs-theme-value="light">
                    <i class="fas fa-sun me-2"></i>淺色模式
                </button>
            </li>
            <li>
                <button class="dropdown-item" type="button" data-bs-theme-value="dark">
                    <i class="fas fa-moon me-2"></i>深色模式
                </button>
            </li>
            <li>
                <button class="dropdown-item" type="button" data-bs-theme-value="auto">
                    <i class="fas fa-desktop me-2"></i>跟隨系統
                </button>
            </li>
        </ul>
    </div>
</div>
```

### 2. JavaScript 主題管理器

#### 2.1 主題切換核心邏輯
```javascript
// 建立 theme-manager.js 檔案
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
        document.querySelectorAll('[data-bs-theme-value]').forEach(button => {
            button.addEventListener('click', (e) => {
                const theme = e.currentTarget.getAttribute('data-bs-theme-value');
                this.setTheme(theme);
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
const themeManager = new ThemeManager();

// 監聽主題變更事件
document.addEventListener('themeChanged', function(e) {
    console.log('主題已切換至:', e.detail.theme);
    
    // 可以在這裡執行主題切換後的額外處理
    updateChartTheme(e.detail.theme);
    updateMapTheme(e.detail.theme);
});
```

### 3. 各頁面深色模式適配

#### 3.1 時鐘頁面 (Index.cshtml)
```css
/* 時鐘深色模式樣式 */
[data-bs-theme="dark"] .clock-container {
    background: var(--surface-color);
    border-radius: 20px;
    padding: 2rem;
    box-shadow: 0 8px 32px var(--shadow-color);
}

[data-bs-theme="dark"] .digital-clock {
    color: var(--bs-primary);
    text-shadow: 0 0 20px rgba(110, 168, 254, 0.5);
}

[data-bs-theme="dark"] .analog-clock-face {
    background: radial-gradient(circle, var(--surface-variant) 0%, var(--surface-color) 100%);
    border: 3px solid var(--border-color);
}

[data-bs-theme="dark"] .analog-hour-hand,
[data-bs-theme="dark"] .analog-minute-hand {
    background: var(--bs-body-color);
    box-shadow: 0 0 10px var(--shadow-color);
}

[data-bs-theme="dark"] .analog-second-hand {
    background: var(--bs-primary);
    box-shadow: 0 0 15px rgba(110, 168, 254, 0.7);
}

[data-bs-theme="dark"] .clock-numbers .number {
    color: var(--bs-body-color);
    text-shadow: 0 0 10px var(--shadow-color);
}
```

#### 3.2 記帳系統 (Index7.cshtml)
```css
/* 記帳系統深色模式 */
[data-bs-theme="dark"] .card {
    background-color: var(--card-bg);
    border-color: var(--border-color);
    box-shadow: 0 4px 16px var(--shadow-color);
}

[data-bs-theme="dark"] .table {
    --bs-table-bg: var(--table-bg);
    --bs-table-striped-bg: var(--table-striped);
    --bs-table-border-color: var(--border-color);
}

[data-bs-theme="dark"] .modal-content {
    background-color: var(--modal-bg);
    border-color: var(--border-color);
}

[data-bs-theme="dark"] .form-control {
    background-color: var(--input-bg);
    border-color: var(--input-border);
    color: var(--bs-body-color);
}

[data-bs-theme="dark"] .form-control:focus {
    background-color: var(--input-bg);
    border-color: var(--bs-primary);
    color: var(--bs-body-color);
    box-shadow: 0 0 0 0.25rem rgba(110, 168, 254, 0.25);
}
```

#### 3.3 備忘錄系統 (Index4.cshtml)
```css
/* 備忘錄深色模式 */
[data-bs-theme="dark"] .search-container .form-control {
    background-color: var(--input-bg);
    border-color: var(--input-border);
    color: var(--bs-body-color);
}

[data-bs-theme="dark"] .memo-card {
    background-color: var(--card-bg);
    border-color: var(--border-color);
    transition: all 0.3s ease;
}

[data-bs-theme="dark"] .memo-card:hover {
    box-shadow: 0 8px 24px var(--shadow-color);
    transform: translateY(-2px);
}

[data-bs-theme="dark"] .tag-badge {
    background-color: var(--surface-variant);
    color: var(--bs-body-color);
    border: 1px solid var(--border-color);
}

[data-bs-theme="dark"] .priority-high {
    background-color: rgba(234, 134, 143, 0.2);
    color: var(--bs-danger);
}
```

#### 3.4 月曆系統 (Index3.cshtml)
```css
/* 月曆深色模式 */
[data-bs-theme="dark"] .calendar-container {
    background-color: var(--surface-color);
    border-radius: 12px;
    overflow: hidden;
}

[data-bs-theme="dark"] .calendar-header {
    background: linear-gradient(135deg, var(--bs-primary) 0%, var(--bs-info) 100%);
    color: white;
}

[data-bs-theme="dark"] .calendar-cell {
    background-color: var(--surface-variant);
    border-color: var(--border-color);
    color: var(--bs-body-color);
}

[data-bs-theme="dark"] .calendar-cell:hover {
    background-color: var(--bs-primary);
    color: white;
    transform: scale(1.05);
}

[data-bs-theme="dark"] .calendar-cell.other-month {
    color: var(--text-muted);
    opacity: 0.6;
}

[data-bs-theme="dark"] .note-badge {
    background-color: var(--bs-warning);
    color: var(--bs-dark);
}
```

### 4. 圖表和視覺化元件適配

#### 4.1 Chart.js 主題適配
```javascript
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
    const charts = Chart.getChart ? Chart.instances : window.chartInstances || {};
    
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
```

### 5. 動畫和轉場效果

#### 5.1 主題切換動畫
```css
/* 主題切換平滑過渡 */
* {
    transition: background-color 0.3s ease, 
                color 0.3s ease, 
                border-color 0.3s ease,
                box-shadow 0.3s ease !important;
}

/* 特殊元件需要排除過渡 */
.no-transition,
.no-transition * {
    transition: none !important;
}

/* 主題切換按鈕動畫 */
#themeDropdown {
    position: relative;
    overflow: hidden;
}

#themeDropdown::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
    transition: left 0.5s ease;
}

#themeDropdown:hover::before {
    left: 100%;
}

/* 載入動畫 */
.theme-loading {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0,0,0,0.1);
    z-index: 9999;
    display: flex;
    align-items: center;
    justify-content: center;
}

.theme-loading .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid var(--bs-primary);
    border-top-color: transparent;
    border-radius: 50%;
    animation: spin 1s linear infinite;
}
```

### 6. 特殊組件適配

#### 6.1 Bootstrap 組件深色適配
```css
/* 下拉選單 */
[data-bs-theme="dark"] .dropdown-menu {
    background-color: var(--surface-variant);
    border-color: var(--border-color);
}

[data-bs-theme="dark"] .dropdown-item {
    color: var(--bs-body-color);
}

[data-bs-theme="dark"] .dropdown-item:hover,
[data-bs-theme="dark"] .dropdown-item:focus {
    background-color: var(--bs-primary);
    color: white;
}

/* 按鈕 */
[data-bs-theme="dark"] .btn-outline-secondary {
    color: var(--bs-body-color);
    border-color: var(--border-color);
}

[data-bs-theme="dark"] .btn-outline-secondary:hover {
    background-color: var(--bs-secondary);
    border-color: var(--bs-secondary);
    color: white;
}

/* 分頁元件 */
[data-bs-theme="dark"] .pagination .page-link {
    background-color: var(--surface-variant);
    border-color: var(--border-color);
    color: var(--bs-body-color);
}

[data-bs-theme="dark"] .pagination .page-item.active .page-link {
    background-color: var(--bs-primary);
    border-color: var(--bs-primary);
}

/* 警告框 */
[data-bs-theme="dark"] .alert {
    border-width: 1px;
    background-color: var(--surface-variant);
}

[data-bs-theme="dark"] .alert-primary {
    background-color: rgba(110, 168, 254, 0.1);
    border-color: rgba(110, 168, 254, 0.2);
    color: var(--bs-primary);
}
```

#### 6.2 自訂組件適配
```css
/* 統計卡片 */
[data-bs-theme="dark"] .stat-card {
    background: linear-gradient(135deg, var(--surface-variant) 0%, var(--surface-color) 100%);
    border: 1px solid var(--border-color);
}

/* 進度條 */
[data-bs-theme="dark"] .progress {
    background-color: var(--surface-variant);
}

/* 標籤雲 */
[data-bs-theme="dark"] .tag-cloud .tag {
    background-color: var(--surface-variant);
    color: var(--bs-body-color);
    border: 1px solid var(--border-color);
}

[data-bs-theme="dark"] .tag-cloud .tag:hover {
    background-color: var(--bs-primary);
    color: white;
    border-color: var(--bs-primary);
}
```

### 7. 性能優化

#### 7.1 CSS 優化
```css
/* 使用 CSS 變量減少重複程式碼 */
.theme-aware-component {
    background-color: var(--surface-color);
    color: var(--bs-body-color);
    border-color: var(--border-color);
    box-shadow: var(--bs-box-shadow);
}

/* 減少重繪，使用 transform 而不是改變 position */
.theme-transition {
    transform: translateZ(0); /* 觸發硬體加速 */
    will-change: background-color, color;
}
```

#### 7.2 JavaScript 優化
```javascript
// 防抖主題切換
let themeChangeTimeout;
function debouncedThemeChange(theme) {
    clearTimeout(themeChangeTimeout);
    themeChangeTimeout = setTimeout(() => {
        applyTheme(theme);
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
```

### 8. 兼容性處理

#### 8.1 舊瀏覽器備援方案
```javascript
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
```

### 9. 測試和品質保證

#### 9.1 測試清單
- [ ] 所有頁面的深色模式顯示正常
- [ ] 主題切換按鈕功能正常
- [ ] 系統主題偏好自動偵測
- [ ] 用戶偏好設定持久化
- [ ] 圖表和視覺化元件適配
- [ ] 響應式設計在深色模式下正常
- [ ] 動畫和過渡效果流暢
- [ ] 不同瀏覽器兼容性測試
- [ ] 性能影響評估

#### 9.2 對比度和可訪問性
```css
/* 確保對比度符合 WCAG 標準 */
[data-bs-theme="dark"] {
    /* 主要文字對比度至少 4.5:1 */
    --bs-body-color: #e9ecef; /* 對 #121212 背景對比度 13.4:1 */
    
    /* 次要文字對比度至少 3:1 */
    --text-muted: #adb5bd; /* 對 #121212 背景對比度 6.6:1 */
    
    /* 互動元素對比度至少 3:1 */
    --bs-primary: #6ea8fe; /* 對 #121212 背景對比度 8.2:1 */
}

/* 高對比度模式支援 */
@media (prefers-contrast: high) {
    [data-bs-theme="dark"] {
        --bs-body-color: #ffffff;
        --text-muted: #cccccc;
        --border-color: #666666;
    }
}
```

### 10. 部署和維護

#### 10.1 漸進式部署
```javascript
// 功能開關，可以控制深色模式的啟用
const FEATURE_FLAGS = {
    DARK_MODE_ENABLED: true,
    AUTO_THEME_ENABLED: true,
    THEME_ANIMATIONS_ENABLED: true
};

// 根據功能開關初始化
if (FEATURE_FLAGS.DARK_MODE_ENABLED) {
    const themeManager = new ThemeManager();
}
```

#### 10.2 監控和分析
```javascript
// 主題使用情況統計
function trackThemeUsage(theme) {
    // 可以整合到現有的分析系統
    if (typeof gtag !== 'undefined') {
        gtag('event', 'theme_change', {
            'theme': theme,
            'timestamp': new Date().toISOString()
        });
    }
}

// 性能監控
function monitorThemePerformance() {
    const observer = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry) => {
            if (entry.name.includes('theme')) {
                console.log('主題相關性能:', entry);
            }
        });
    });
    observer.observe({ entryTypes: ['measure', 'navigation'] });
}
```

這個深色模式實施方案將為整個系統提供統一、優雅的深色主題體驗，同時保持良好的性能和可維護性。
