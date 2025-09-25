/**
 * 世界時鐘 JavaScript 主要邏輯
 * 包含時間更新、時區切換、特效動畫、音效管理等功能
 */

class WorldClockApp {
    constructor() {
        this.data = null;
        this.currentTimeZone = 'Asia/Taipei';
        this.selectedLanguage = 'zh-tw';
        this.timeUpdateInterval = null;
        this.audioManager = null;
        this.effectsManager = null;
        this.lastSyncTime = 0;
        
        this.init();
    }

    /**
     * 初始化應用程式
     */
    async init() {
        try {
            // 載入頁面資料
            this.loadPageData();
            
            // 初始化音效管理器
            this.audioManager = new WorldClockAudioManager();
            await this.audioManager.init();
            
            // 初始化特效管理器
            this.effectsManager = new CityEffectsManager();
            
            // 開始時間更新
            this.startTimeUpdates();
            
            // 綁定事件監聽器
            this.bindEventListeners();
            
            // 初始化 UI 狀態
            this.updateUI();
            
            console.log('世界時鐘應用已成功初始化');
        } catch (error) {
            console.error('初始化世界時鐘應用時發生錯誤:', error);
        }
    }

    /**
     * 載入頁面資料
     */
    loadPageData() {
        const dataElement = document.getElementById('worldClockData');
        if (dataElement) {
            this.data = JSON.parse(dataElement.textContent);
            this.currentTimeZone = this.data.currentTimeZone || 'Asia/Taipei';
            this.selectedLanguage = this.data.selectedLanguage || 'zh-tw';
        }
    }

    /**
     * 綁定事件監聽器
     */
    bindEventListeners() {
        // 視窗焦點事件 - 用於自動校時
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden) {
                this.syncTime();
            }
        });

        // 鍵盤快捷鍵
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.key === 'r') {
                e.preventDefault();
                this.syncTime();
            }
        });

        // 防止頻繁校時
        this.throttledSyncTime = this.throttle(this.syncTime.bind(this), 5000);
    }

    /**
     * 開始時間更新循環
     */
    startTimeUpdates() {
        // 立即更新一次
        this.updateAllTimes();
        
        // 設定每秒更新
        this.timeUpdateInterval = setInterval(() => {
            this.updateAllTimes();
        }, 1000);
    }

    /**
     * 停止時間更新循環
     */
    stopTimeUpdates() {
        if (this.timeUpdateInterval) {
            clearInterval(this.timeUpdateInterval);
            this.timeUpdateInterval = null;
        }
    }

    /**
     * 更新所有時間顯示
     */
    updateAllTimes() {
        const now = new Date();
        
        // 更新主要時間
        this.updateMainTime(now);
        
        // 更新所有城市時間
        this.updateCityTimes(now);
    }

    /**
     * 更新主要時間顯示
     */
    updateMainTime(baseTime = new Date()) {
        try {
            const timeZone = this.getTimeZoneFromId(this.currentTimeZone);
            const localTime = new Date(baseTime.toLocaleString("en-US", {timeZone: timeZone}));
            
            // 格式化時間
            const timeString = localTime.toLocaleTimeString('en-GB', {
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
                hour12: false
            });
            
            // 格式化日期
            const dateString = this.formatDate(localTime, this.selectedLanguage);
            
            // 更新 DOM
            const mainTimeElement = document.getElementById('mainTime');
            const mainDateElement = document.getElementById('mainDate');
            
            if (mainTimeElement && mainTimeElement.textContent !== timeString) {
                mainTimeElement.textContent = timeString;
                mainTimeElement.classList.add('time-digit-flip');
                setTimeout(() => {
                    mainTimeElement.classList.remove('time-digit-flip');
                }, 600);
            }
            
            if (mainDateElement) {
                mainDateElement.textContent = dateString;
            }
            
        } catch (error) {
            console.error('更新主要時間時發生錯誤:', error);
        }
    }

    /**
     * 更新所有城市時間
     */
    updateCityTimes(baseTime = new Date()) {
        if (!this.data?.timeZones) return;
        
        this.data.timeZones.forEach(timeZoneData => {
            try {
                const timeZone = this.getTimeZoneFromId(timeZoneData.id);
                const localTime = new Date(baseTime.toLocaleString("en-US", {timeZone: timeZone}));
                
                // 格式化時間
                const timeString = localTime.toLocaleTimeString('en-GB', {
                    hour: '2-digit',
                    minute: '2-digit',
                    second: '2-digit',
                    hour12: false
                });
                
                // 格式化日期
                const dateString = this.formatShortDate(localTime);
                
                // 更新 DOM
                const timeId = `time-${timeZoneData.id.replace('/', '-')}`;
                const dateId = `date-${timeZoneData.id.replace('/', '-')}`;
                
                const timeElement = document.getElementById(timeId);
                const dateElement = document.getElementById(dateId);
                
                if (timeElement) {
                    timeElement.textContent = timeString;
                }
                
                if (dateElement) {
                    dateElement.textContent = dateString;
                }
                
            } catch (error) {
                console.error(`更新城市 ${timeZoneData.id} 時間時發生錯誤:`, error);
            }
        });
    }

    /**
     * 格式化日期
     */
    formatDate(date, language) {
        const options = {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            weekday: 'long'
        };
        
        if (language === 'zh-tw') {
            const localeDateString = date.toLocaleDateString('zh-TW', options);
            return localeDateString.replace(/(\d{4})\/(\d{1,2})\/(\d{1,2})/, '$1年$2月$3日');
        } else {
            return date.toLocaleDateString('en-US', options);
        }
    }

    /**
     * 格式化短日期
     */
    formatShortDate(date) {
        return date.toLocaleDateString('en-GB', {
            month: '2-digit',
            day: '2-digit'
        });
    }

    /**
     * 從時區 ID 獲取實際時區字串
     */
    getTimeZoneFromId(timezoneId) {
        // 直接使用 IANA 時區標識符
        return timezoneId;
    }

    /**
     * 選擇時區
     */
    async selectTimeZone(timezoneId, cityName) {
        try {
            const oldTimeZone = this.currentTimeZone;
            this.currentTimeZone = timezoneId;
            
            // 更新 UI 狀態
            this.updateSelectedTimeZone(timezoneId);
            
            // 更新當前位置顯示
            const currentLocationElement = document.getElementById('currentLocationName');
            if (currentLocationElement) {
                currentLocationElement.textContent = cityName;
            }
            
            // 立即更新時間
            this.updateMainTime();
            
            // 播放城市特效
            if (oldTimeZone !== timezoneId) {
                await this.effectsManager.playCityEffect(timezoneId);
                this.audioManager.playCitySound(timezoneId);
            }
            
            console.log(`已切換到 ${cityName} (${timezoneId})`);
        } catch (error) {
            console.error('選擇時區時發生錯誤:', error);
        }
    }

    /**
     * 更新選中的時區 UI 狀態
     */
    updateSelectedTimeZone(timezoneId) {
        // 移除所有選中狀態
        document.querySelectorAll('.city-clock-card').forEach(card => {
            card.classList.remove('selected');
        });
        
        // 添加新的選中狀態
        const selectedCard = document.querySelector(`[data-timezone="${timezoneId}"]`);
        if (selectedCard) {
            selectedCard.classList.add('selected');
        }
    }

    /**
     * 切換語言
     */
    changeLanguage(language) {
        this.selectedLanguage = language;
        
        // 重新載入頁面以更新語言
        const url = new URL(window.location);
        url.searchParams.set('selectedLanguage', language);
        window.location.href = url.toString();
    }

    /**
     * 校時功能
     */
    syncTime() {
        const now = Date.now();
        if (now - this.lastSyncTime < 5000) {
            console.log('校時功能被限制，請稍後再試');
            return;
        }
        
        this.lastSyncTime = now;
        
        try {
            // 停止現有的時間更新
            this.stopTimeUpdates();
            
            // 重新開始時間更新
            this.startTimeUpdates();
            
            // 顯示校時動畫
            this.showSyncAnimation();
            
            console.log('時間已同步');
        } catch (error) {
            console.error('校時時發生錯誤:', error);
        }
    }

    /**
     * 顯示校時動畫
     */
    showSyncAnimation() {
        const syncBtn = document.getElementById('syncTimeBtn');
        if (syncBtn) {
            syncBtn.disabled = true;
            syncBtn.classList.add('pulse-effect');
            
            setTimeout(() => {
                syncBtn.disabled = false;
                syncBtn.classList.remove('pulse-effect');
            }, 2000);
        }
    }

    /**
     * 切換音效
     */
    toggleAudio() {
        this.audioManager.toggle();
        this.updateAudioUI();
    }

    /**
     * 調整音量
     */
    adjustVolume(volume) {
        this.audioManager.setVolume(volume / 100);
    }

    /**
     * 更新音效 UI
     */
    updateAudioUI() {
        const audioIcon = document.getElementById('audioIcon');
        if (audioIcon) {
            audioIcon.className = this.audioManager.enabled ? 'icon-volume-on' : 'icon-volume-off';
        }
    }

    /**
     * 更新整體 UI
     */
    updateUI() {
        this.updateSelectedTimeZone(this.currentTimeZone);
        this.updateAudioUI();
    }

    /**
     * 節流函式
     */
    throttle(func, limit) {
        let inThrottle;
        return function() {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        }
    }

    /**
     * 銷毀應用程式
     */
    destroy() {
        this.stopTimeUpdates();
        if (this.audioManager) {
            this.audioManager.destroy();
        }
        if (this.effectsManager) {
            this.effectsManager.destroy();
        }
    }
}

/**
 * 音效管理器
 */
class WorldClockAudioManager {
    constructor() {
        this.audioContext = null;
        this.sounds = new Map();
        this.volume = 0.7;
        this.enabled = true;
        this.isInitialized = false;
    }

    /**
     * 初始化音效管理器
     */
    async init() {
        try {
            // 檢查瀏覽器支援
            if (!window.AudioContext && !window.webkitAudioContext) {
                console.warn('瀏覽器不支援 Web Audio API，音效功能已停用');
                this.enabled = false;
                return;
            }

            // 建立音效上下文
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
            
            // 載入音效設定
            this.loadSettings();
            
            // 預載入音效檔案
            await this.preloadSounds();
            
            this.isInitialized = true;
            console.log('音效管理器初始化完成');
        } catch (error) {
            console.error('初始化音效管理器時發生錯誤:', error);
            this.enabled = false;
        }
    }

    /**
     * 載入音效設定
     */
    loadSettings() {
        const savedSettings = localStorage.getItem('worldClockAudioSettings');
        if (savedSettings) {
            const settings = JSON.parse(savedSettings);
            this.enabled = settings.enabled !== false;
            this.volume = settings.volume || 0.7;
        }
    }

    /**
     * 儲存音效設定
     */
    saveSettings() {
        const settings = {
            enabled: this.enabled,
            volume: this.volume
        };
        localStorage.setItem('worldClockAudioSettings', JSON.stringify(settings));
    }

    /**
     * 預載入音效檔案
     */
    async preloadSounds() {
        const soundFiles = [
            'taipei-bell-chime.mp3',
            'tokyo-wind-chime.mp3',
            'newyork-city-buzz.mp3',
            'london-big-ben.mp3',
            'paris-accordion.mp3',
            'berlin-classical.mp3',
            'moscow-bells.mp3',
            'sydney-ocean-waves.mp3',
            'default-chime.mp3'
        ];

        const loadPromises = soundFiles.map(file => this.loadSound(file));
        
        try {
            await Promise.allSettled(loadPromises);
            console.log('音效檔案預載入完成');
        } catch (error) {
            console.warn('部分音效檔案載入失敗:', error);
        }
    }

    /**
     * 載入單個音效檔案
     */
    async loadSound(filename) {
        try {
            const response = await fetch(`/audio/city-effects/${filename}`);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            const arrayBuffer = await response.arrayBuffer();
            const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
            
            this.sounds.set(filename, audioBuffer);
            return audioBuffer;
        } catch (error) {
            console.warn(`載入音效檔案 ${filename} 失敗:`, error);
            return null;
        }
    }

    /**
     * 播放城市音效
     */
    playCitySound(timezoneId) {
        if (!this.enabled || !this.isInitialized) return;

        const soundMap = {
            'Asia/Taipei': 'taipei-bell-chime.mp3',
            'Asia/Tokyo': 'tokyo-wind-chime.mp3',
            'America/New_York': 'newyork-city-buzz.mp3',
            'Europe/London': 'london-big-ben.mp3',
            'Europe/Paris': 'paris-accordion.mp3',
            'Europe/Berlin': 'berlin-classical.mp3',
            'Europe/Moscow': 'moscow-bells.mp3',
            'Australia/Sydney': 'sydney-ocean-waves.mp3'
        };

        const soundFile = soundMap[timezoneId] || 'default-chime.mp3';
        this.playSound(soundFile);
    }

    /**
     * 播放音效
     */
    playSound(filename) {
        if (!this.enabled || !this.isInitialized || !this.audioContext) return;

        try {
            const audioBuffer = this.sounds.get(filename);
            if (!audioBuffer) {
                console.warn(`音效檔案 ${filename} 未找到`);
                return;
            }

            const source = this.audioContext.createBufferSource();
            const gainNode = this.audioContext.createGain();
            
            source.buffer = audioBuffer;
            gainNode.gain.value = this.volume;
            
            source.connect(gainNode);
            gainNode.connect(this.audioContext.destination);
            
            source.start(0);
            
        } catch (error) {
            console.error(`播放音效 ${filename} 時發生錯誤:`, error);
        }
    }

    /**
     * 切換音效開關
     */
    toggle() {
        this.enabled = !this.enabled;
        this.saveSettings();
        console.log(`音效${this.enabled ? '已開啟' : '已關閉'}`);
    }

    /**
     * 設定音量
     */
    setVolume(volume) {
        this.volume = Math.max(0, Math.min(1, volume));
        this.saveSettings();
    }

    /**
     * 銷毀音效管理器
     */
    destroy() {
        if (this.audioContext) {
            this.audioContext.close();
        }
        this.sounds.clear();
    }
}

/**
 * 城市特效管理器
 */
class CityEffectsManager {
    constructor() {
        this.activeEffects = new Set();
        this.effectConfigs = new Map();
        this.init();
    }

    /**
     * 初始化特效管理器
     */
    init() {
        // 載入特效設定
        this.loadEffectConfigs();
        console.log('城市特效管理器初始化完成');
    }

    /**
     * 載入特效設定
     */
    loadEffectConfigs() {
        // 台北特效
        this.effectConfigs.set('Asia/Taipei', {
            type: 'sakura',
            primaryColor: '#003D79',
            secondaryColor: '#FF69B4',
            duration: 2500,
            particles: ['sakura-petal'],
            animations: ['taipei-101-rise', 'night-market-glow']
        });

        // 東京特效
        this.effectConfigs.set('Asia/Tokyo', {
            type: 'japanese',
            primaryColor: '#DC143C',
            secondaryColor: '#F8F8FF',
            duration: 3000,
            particles: ['sakura-petal'],
            animations: ['paper-fan-unfold', 'fuji-mountain-rise']
        });

        // 紐約特效
        this.effectConfigs.set('America/New_York', {
            type: 'urban',
            primaryColor: '#478778',
            secondaryColor: '#FFFF00',
            duration: 2000,
            particles: [],
            animations: ['skyscraper-build', 'taxi-drive', 'neon-flash']
        });

        // 倫敦特效
        this.effectConfigs.set('Europe/London', {
            type: 'british',
            primaryColor: '#012169',
            secondaryColor: '#C8102E',
            duration: 3500,
            particles: ['rain-drop'],
            animations: ['big-ben-swing', 'bus-drive', 'fog-spread']
        });

        // 巴黎特效
        this.effectConfigs.set('Europe/Paris', {
            type: 'french',
            primaryColor: '#0055A4',
            secondaryColor: '#EF4135',
            duration: 3000,
            particles: ['bubble', 'rose-petal'],
            animations: ['eiffel-tower-draw', 'golden-rays']
        });

        // 柏林特效
        this.effectConfigs.set('Europe/Berlin', {
            type: 'german',
            primaryColor: '#000000',
            secondaryColor: '#DD0000',
            duration: 2500,
            particles: [],
            animations: ['brandenburg-appear', 'flag-wave', 'gear-rotate']
        });

        // 莫斯科特效
        this.effectConfigs.set('Europe/Moscow', {
            type: 'russian',
            primaryColor: '#D52B1E',
            secondaryColor: '#FFD700',
            duration: 3000,
            particles: ['snowflake'],
            animations: ['onion-dome-spin', 'star-twinkle']
        });

        // 雪梨特效
        this.effectConfigs.set('Australia/Sydney', {
            type: 'australian',
            primaryColor: '#0057B8',
            secondaryColor: '#FF8C00',
            duration: 3500,
            particles: ['bubble'],
            animations: ['opera-house-unfold', 'wave-motion', 'sunrise-glow']
        });
    }

    /**
     * 播放城市特效
     */
    async playCityEffect(timezoneId) {
        try {
            // 停止所有現有特效
            this.stopAllEffects();
            
            const config = this.effectConfigs.get(timezoneId);
            if (!config) {
                console.warn(`未找到城市 ${timezoneId} 的特效設定`);
                return;
            }

            // 開始播放特效
            this.activeEffects.add(timezoneId);
            
            // 建立特效容器
            const effectContainer = this.createEffectContainer(timezoneId, config);
            
            // 播放粒子效果
            if (config.particles.length > 0) {
                this.playParticleEffects(effectContainer, config);
            }
            
            // 播放動畫效果
            this.playAnimationEffects(effectContainer, config);
            
            // 應用背景色彩變化
            this.applyColorTransition(config);
            
            // 設定自動清理
            setTimeout(() => {
                this.cleanupEffect(timezoneId, effectContainer);
            }, config.duration);
            
            console.log(`播放城市特效: ${timezoneId}`);
        } catch (error) {
            console.error(`播放城市特效時發生錯誤:`, error);
        }
    }

    /**
     * 建立特效容器
     */
    createEffectContainer(timezoneId, config) {
        const overlay = document.getElementById('globalEffectsOverlay');
        if (!overlay) return null;

        const container = document.createElement('div');
        container.className = `city-effect-active ${config.type}-effect`;
        container.id = `effect-active-${timezoneId.replace('/', '-')}`;
        container.style.cssText = `
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 1000;
        `;

        overlay.appendChild(container);
        return container;
    }

    /**
     * 播放粒子效果
     */
    playParticleEffects(container, config) {
        if (!container || !config.particles.length) return;

        config.particles.forEach(particleType => {
            this.createParticleSystem(container, particleType, config.duration);
        });
    }

    /**
     * 建立粒子系統
     */
    createParticleSystem(container, particleType, duration) {
        const particleCount = particleType === 'snowflake' ? 30 : 20;
        
        for (let i = 0; i < particleCount; i++) {
            setTimeout(() => {
                if (!container.parentNode) return;
                
                const particle = document.createElement('div');
                particle.className = `particle ${particleType}`;
                
                // 隨機位置
                particle.style.left = Math.random() * 100 + '%';
                particle.style.animationDelay = Math.random() * 2 + 's';
                
                container.appendChild(particle);
                
                // 自動清理粒子
                setTimeout(() => {
                    if (particle.parentNode) {
                        particle.parentNode.removeChild(particle);
                    }
                }, 4000);
            }, i * 100);
        }
    }

    /**
     * 播放動畫效果
     */
    playAnimationEffects(container, config) {
        if (!container || !config.animations.length) return;

        config.animations.forEach((animation, index) => {
            setTimeout(() => {
                this.createAnimationElement(container, animation, config);
            }, index * 500);
        });
    }

    /**
     * 建立動畫元素
     */
    createAnimationElement(container, animationType, config) {
        if (!container.parentNode) return;

        const element = document.createElement('div');
        element.className = `animation-element ${animationType}`;
        
        // 根據動畫類型設定樣式
        switch (animationType) {
            case 'taipei-101-rise':
                element.style.cssText = `
                    position: absolute;
                    bottom: 0;
                    right: 20%;
                    width: 20px;
                    height: 100px;
                    background: linear-gradient(to top, ${config.primaryColor}, ${config.secondaryColor});
                    animation: slideInUp 2s ease;
                `;
                break;
                
            case 'eiffel-tower-draw':
                element.style.cssText = `
                    position: absolute;
                    bottom: 20%;
                    left: 50%;
                    transform: translateX(-50%);
                    width: 0;
                    height: 80px;
                    border-left: 3px solid ${config.secondaryColor};
                    animation: drawLine 2.5s ease;
                `;
                break;
                
            case 'opera-house-unfold':
                element.style.cssText = `
                    position: absolute;
                    bottom: 10%;
                    left: 30%;
                    width: 60px;
                    height: 40px;
                    background: ${config.primaryColor};
                    border-radius: 50% 50% 0 0;
                    animation: scaleIn 3s ease;
                `;
                break;
                
            default:
                element.style.cssText = `
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    width: 50px;
                    height: 50px;
                    background: ${config.primaryColor};
                    border-radius: 50%;
                    animation: pulse 2s ease;
                `;
        }
        
        container.appendChild(element);
    }

    /**
     * 應用背景色彩變化
     */
    applyColorTransition(config) {
        const container = document.querySelector('.world-clock-container');
        if (!container) return;

        const originalBackground = container.style.background;
        
        container.style.background = `
            linear-gradient(135deg, 
                ${config.primaryColor}22 0%, 
                ${config.secondaryColor}11 50%, 
                ${config.primaryColor}22 100%)
        `;
        
        setTimeout(() => {
            container.style.background = originalBackground;
        }, config.duration);
    }

    /**
     * 停止所有特效
     */
    stopAllEffects() {
        this.activeEffects.forEach(timezoneId => {
            const effectElement = document.getElementById(`effect-active-${timezoneId.replace('/', '-')}`);
            if (effectElement) {
                effectElement.remove();
            }
        });
        this.activeEffects.clear();
    }

    /**
     * 清理特定特效
     */
    cleanupEffect(timezoneId, container) {
        if (container && container.parentNode) {
            container.remove();
        }
        this.activeEffects.delete(timezoneId);
    }

    /**
     * 銷毀特效管理器
     */
    destroy() {
        this.stopAllEffects();
        this.effectConfigs.clear();
    }
}

// 全域函式 - 供 HTML 呼叫
let worldClockApp = null;

/**
 * 選擇時區 - 全域函式
 */
function selectTimeZone(timezoneId, cityName) {
    if (worldClockApp) {
        worldClockApp.selectTimeZone(timezoneId, cityName);
    }
}

/**
 * 切換語言 - 全域函式
 */
function changeLanguage(language) {
    if (worldClockApp) {
        worldClockApp.changeLanguage(language);
    }
}

/**
 * 校時 - 全域函式
 */
function syncTime() {
    if (worldClockApp) {
        worldClockApp.syncTime();
    }
}

/**
 * 切換音效 - 全域函式
 */
function toggleAudio() {
    if (worldClockApp) {
        worldClockApp.toggleAudio();
    }
}

/**
 * 調整音量 - 全域函式
 */
function adjustVolume(volume) {
    if (worldClockApp) {
        worldClockApp.adjustVolume(volume);
    }
}

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', () => {
    worldClockApp = new WorldClockApp();
});

// 頁面卸載時清理
window.addEventListener('beforeunload', () => {
    if (worldClockApp) {
        worldClockApp.destroy();
    }
});

// 添加額外的 CSS 動畫
const additionalStyles = `
@keyframes slideInUp {
    from { transform: translateY(100%); opacity: 0; }
    to { transform: translateY(0); opacity: 1; }
}

@keyframes drawLine {
    from { height: 0; }
    to { height: 80px; }
}

@keyframes scaleIn {
    from { transform: translateX(-50%) scale(0); }
    to { transform: translateX(-50%) scale(1); }
}

@keyframes pulse {
    0%, 100% { transform: translate(-50%, -50%) scale(1); opacity: 1; }
    50% { transform: translate(-50%, -50%) scale(1.2); opacity: 0.7; }
}
`;

// 動態添加樣式
const styleSheet = document.createElement('style');
styleSheet.textContent = additionalStyles;
document.head.appendChild(styleSheet);
