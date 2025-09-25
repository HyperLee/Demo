# 世### 專案資訊
### 檔案位置
- 前端頁面: `D:\Demo\Demo\Demo\Pages\index10.cshtml`
- 後端邏輯: `D:\Demo\Demo\Demo\Pages\index10.cshtml.cs`
- 樣式檔案: `D:\Demo\Demo\Demo\wwwroot\css\index10.css`（新建）
- JavaScript: `D:\Demo\Demo\Demo\wwwroot\js\index10.js`（新建）
- 音效檔案: `D:\Demo\Demo\Demo\wwwroot\audio\city-effects\`（新建）格書
- 顯示各種時區的當前時間，提供全球化時間管理體驗

## 專案資訊
### 檔案位置
- 前端頁面: `D:\Demo\Demo\Demo\Pages\index10.cshtml`
- 後端邏輯: `D:\Demo\Demo\Demo\Pages\index10.cshtml.cs`
- 樣式檔案: `D:\Demo\Demo\Demo\wwwroot\css\index10.css`（新建）
- JavaScript: `D:\Demo\Demo\Demo\wwwroot\js\index10.js`（新建）

### 參考資源
- 設計參考: https://time.is/zh_tw/
- 暗黑模式參考: `D:\Demo\Demo\Demo\Pages\Shared\_Layout.cshtml`

## 功能需求規格

### 1. 主要顯示區域
#### 1.1 版面配置
```
┌─────────────────────────────────────┐
│ [當前位置]              [語言切換] │
│                                     │
│            [主要時間]                │
│             HH:mm:ss                │
│                                     │
│                        [當前日期]    │
│                                     │
│      [世界時鐘區域網格]              │
│   ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐   │
│   │台北 │ │東京 │ │紐約 │ │倫敦 │   │
│   └─────┘ └─────┘ └─────┘ └─────┘   │
│   ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐   │
│   │巴黎 │ │柏林 │ │莫斯科│ │雪梨 │   │
│   └─────┘ └─────┘ └─────┘ └─────┘   │
│                                     │
│              [校時按鈕]              │
└─────────────────────────────────────┘
```

#### 1.2 元素規格
- **左上角當前位置**
  - 字體大小: 16px
  - 內容: 顯示當前選中的城市名稱
  - 位置: 距離左邊 20px，距離頂部 20px
  
- **正中間主要時間**
  - 字體大小: 72px（桌面）/ 48px（手機）
  - 格式: HH:mm:ss（24小時制）
  - 顏色: 主要強調色
  - 每秒更新一次
  - 支援平滑過渡動畫
  
- **右下角日期資訊**
  - 字體大小: 14px
  - 格式: 
    - 中文: YYYY年MM月DD日 (星期X)
    - 英文: MM/DD/YYYY (Day of Week)
  - 位置: 距離右邊 20px，距離主要時間下方 10px

### 2. 世界時鐘區域
#### 2.1 支援城市清單
| 城市 | 時區代碼 | GMT偏移 | 夏令時 |
|------|----------|---------|--------|
| 台北 | Asia/Taipei | GMT+8 | 無 |
| 東京 | Asia/Tokyo | GMT+9 | 無 |
| 紐約 | America/New_York | GMT-5/-4 | 有 |
| 倫敦 | Europe/London | GMT+0/+1 | 有 |
| 巴黎 | Europe/Paris | GMT+1/+2 | 有 |
| 柏林 | Europe/Berlin | GMT+1/+2 | 有 |
| 莫斯科 | Europe/Moscow | GMT+3 | 無 |
| 雪梨 | Australia/Sydney | GMT+10/+11 | 有 |

#### 2.2 時鐘卡片設計
- **尺寸**: 150px × 120px
- **排列**: 4×2 網格佈局，響應式設計
- **內容結構**:
  ```
  ┌─────────────┐
  │   城市名稱   │
  │   HH:mm:ss  │
  │ (GMT±X) 日期 │
  └─────────────┘
  ```
- **互動效果**:
  - 滑鼠懸停: 輕微放大效果（scale: 1.05）
  - 點擊效果: 選中狀態高亮
  - 當前選中: 邊框高亮顯示

### 3. 多語系支援
#### 3.1 支援語言
- **繁體中文 (zh-tw)**: 預設語言
- **美式英文 (en-us)**: 次要語言

#### 3.2 語言切換
- **位置**: 右上角
- **形式**: 下拉選單或切換按鈕
- **內容對應**:

| 項目 | 繁體中文 | 美式英文 |
|------|----------|----------|
| 頁面標題 | 世界時鐘 | World Clock |
| 星期一 | 星期一 | Monday |
| 星期二 | 星期二 | Tuesday |
| 星期三 | 星期三 | Wednesday |
| 星期四 | 星期四 | Thursday |
| 星期五 | 星期五 | Friday |
| 星期六 | 星期六 | Saturday |
| 星期日 | 星期日 | Sunday |
| 校時 | 校時 | Sync Time |
| 當前位置 | 當前位置 | Current Location |

### 4. 互動功能
#### 4.1 時區切換
- **觸發方式**: 點擊世界時鐘區域的任一城市卡片
- **執行效果**: 
  1. 主要時間切換為選中城市時間
  2. 左上角當前位置更新為選中城市
  3. 右下角日期更新為選中城市日期
  4. 選中的城市卡片添加高亮效果
  5. 其他城市卡片移除高亮效果
  6. **播放對應城市的特色切換特效**（詳見 4.4 節）

#### 4.2 自動校時
- **觸發時機**: 頁面載入完成時自動執行一次
- **校時方式**: 
  - 前端: 使用 JavaScript 的 Date 物件
  - 後端: 可選使用 NTP 服務器校時（提高精確度）
- **錯誤處理**: 校時失敗時使用本地時間並顯示警告

#### 4.3 手動校時
- **位置**: 頁面底部中央
- **按鈕樣式**: 科幻風格，與整體設計一致
- **執行效果**: 重新同步所有時區時間
- **頻率限制**: 防止頻繁點擊，最少間隔 5 秒

#### 4.4 城市特色切換特效
每個城市切換時都有獨特的視覺特效，體現當地文化特色：

##### 4.4.1 台北 (Asia/Taipei)
- **主題色彩**: 台灣藍 (#003D79) + 梅花粉 (#FF69B4)
- **特效動畫**: 
  - 櫻花飄落效果（粉色花瓣從上方飄落）
  - 101大樓剪影從底部升起
  - 背景漸變為台北夜景色調
- **音效**: 台灣傳統鐘聲 + 夜市環境音
- **音檔**: `taipei-bell-chime.mp3` (1.5秒)
- **持續時間**: 2.5 秒

##### 4.4.2 東京 (Asia/Tokyo)
- **主題色彩**: 日本紅 (#DC143C) + 富士山白 (#F8F8FF)
- **特效動畫**:
  - 和風紙扇展開效果
  - 富士山剪影配合旭日東升
  - 櫻花瓣旋轉飛舞（粉白色）
  - 傳統和紙質感背景過渡
- **音效**: 日式風鈴聲 + 竹筒敲擊
- **音檔**: `tokyo-wind-chime.mp3` (2秒)
- **持續時間**: 3 秒

##### 4.4.3 紐約 (America/New_York)
- **主題色彩**: 自由女神綠 (#478778) + 黃色計程車 (#FFFF00)
- **特效動畫**:
  - 摩天大樓剪影快速建構
  - 黃色計程車橫向移動
  - 霓虹燈閃爍效果
  - 都市網格線條掃描
- **音效**: 都市交通聲響 + 爵士樂片段
- **音檔**: `newyork-city-buzz.mp3` (1.8秒)
- **持續時間**: 2 秒

##### 4.4.4 倫敦 (Europe/London)
- **主題色彩**: 英國藍 (#012169) + 皇家紅 (#C8102E)
- **特效動畫**:
  - 大笨鐘鐘擺搖擺
  - 英式雨滴從天而降
  - 紅色雙層巴士穿越畫面
  - 霧氣瀰漫效果
- **音效**: 大笨鐘鐘聲 + 雨滴聲
- **音檔**: `london-big-ben.mp3` (3秒)
- **持續時間**: 3.5 秒

##### 4.4.5 巴黎 (Europe/Paris)
- **主題色彩**: 法國藍 (#0055A4) + 法國紅 (#EF4135)
- **特效動畫**:
  - 艾菲爾鐵塔線條勾勒
  - 香檳泡泡上升效果
  - 浪漫玫瓣花瓣飄散
  - 金色光芒放射
- **音效**: 法式手風琴旋律 + 香檳開瓶聲
- **音檔**: `paris-accordion.mp3` (2.5秒)
- **持續時間**: 3 秒

##### 4.4.6 柏林 (Europe/Berlin)
- **主題色彩**: 德國黑 (#000000) + 德國紅 (#DD0000) + 德國金 (#FFCE00)
- **特效動畫**:
  - 勃蘭登堡門柱子依序出現
  - 德國國旗色彩條紋掃過
  - 工業齒輪旋轉效果
  - 幾何線條精密組合
- **音效**: 古典音樂片段 + 機械齒輪聲
- **音檔**: `berlin-classical.mp3` (2.2秒)
- **持續時間**: 2.5 秒

##### 4.4.7 莫斯科 (Europe/Moscow)
- **主題色彩**: 俄羅斯紅 (#D52B1E) + 莫斯科金 (#FFD700)
- **特效動畫**:
  - 洋蔥頭教堂圓頂旋轉出現
  - 雪花紛飛效果
  - 紅場磚牆紋理展開
  - 金色星星閃爍
- **音效**: 俄羅斯鐘聲 + 巴拉萊卡琴聲
- **音檔**: `moscow-bells.mp3` (2.8秒)
- **持續時間**: 3 秒

##### 4.4.8 雪梨 (Australia/Sydney)
- **主題色彩**: 澳洲藍 (#0057B8) + 日落橙 (#FF8C00)
- **特效動畫**:
  - 雪梨歌劇院貝殼狀結構展開
  - 海浪起伏效果
  - 袋鼠剪影跳躍
  - 陽光從海平面升起
- **音效**: 海浪聲 + 海鷗叫聲 + 迪吉里杜管
- **音檔**: `sydney-ocean-waves.mp3` (3秒)
- **持續時間**: 3.5 秒

##### 4.4.9 特效技術規格
- **觸發時機**: 點擊城市卡片後 0.3 秒開始播放
- **圖層管理**: 使用 CSS z-index 控制特效層級
- **效能優化**: 
  - 使用 CSS transform 和 opacity 實現流暢動畫
  - 預載入特效資源避免延遲
  - 支援 GPU 加速
- **取消機制**: 使用者在特效播放期間點擊其他城市可立即切換
- **響應式適配**: 手機版簡化特效以維持效能
- **無障礙支援**: 提供關閉動畫選項（遵循 prefers-reduced-motion）

### 4.5 音效系統規格

#### 4.5.1 音檔技術規格
- **檔案格式**: MP3 (主要) / WebM (備用)
- **音質**: 44.1kHz, 16-bit, 單聲道
- **檔案大小**: 每個音檔 < 50KB
- **音量**: 標準化至 -12dB
- **總大小**: 所有音檔總計 < 400KB

#### 4.5.2 音檔來源與製作
##### 免費音效資源庫
| 來源 | 網址 | 授權 | 建議用途 |
|------|------|------|----------|
| Freesound.org | https://freesound.org | CC授權 | 自然音效、環境音 |
| Zapsplat | https://zapsplat.com | 免費註冊 | 城市音效、機械音 |
| BBC Sound Effects | https://sound-effects.bbcrewind.co.uk | 免費個人使用 | 專業音效 |
| Adobe Audition內建 | - | 商業授權 | 基礎音效 |
| YouTube Audio Library | https://studio.youtube.com | 免費 | 背景音樂片段 |

##### 音效製作建議
**台北音效**:
- 搜尋關鍵字: "temple bell", "night market ambience", "taiwanese traditional"
- 建議來源: Freesound.org 的寺廟鐘聲 + 自錄夜市環境音

**東京音效**:
- 搜尋關鍵字: "wind chime", "bamboo", "japanese traditional"
- 建議來源: Zapsplat 的風鈴音效

**紐約音效**:
- 搜尋關鍵字: "city traffic", "jazz music", "urban ambience"
- 建議來源: BBC Sound Effects 的都市音效

**倫敦音效**:
- 搜尋關鍵字: "big ben", "rain drops", "london ambience"
- 建議來源: BBC Sound Effects 的大笨鐘錄音

**巴黎音效**:
- 搜尋關鍵字: "accordion", "champagne cork", "french cafe"
- 建議來源: YouTube Audio Library 的手風琴音樂

**柏林音效**:
- 搜尋關鍵字: "classical music", "gear mechanism", "german traditional"
- 建議來源: 古典音樂片段 + Freesound.org 機械音

**莫斯科音效**:
- 搜尋關鍵字: "church bells", "balalaika", "russian traditional"
- 建議來源: Freesound.org 的教堂鐘聲

**雪梨音效**:
- 搜尋關鍵字: "ocean waves", "seagulls", "didgeridoo"
- 建議來源: 自然音效 + 澳洲傳統樂器

#### 4.5.3 音效控制功能
- **音效開關**: 使用者可全域開啟/關閉音效
- **音量控制**: 0-100% 可調節音量
- **靜音模式**: 自動偵測系統靜音狀態
- **音效預載**: 頁面載入時預載所有音檔
- **設定儲存**: 使用者音效偏好儲存在 localStorage

#### 4.5.4 音效實作細節
```javascript
// 音效管理類別
class WorldClockAudioManager {
    constructor() {
        this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        this.sounds = new Map();
        this.volume = 0.7; // 預設音量 70%
        this.enabled = true; // 預設開啟音效
    }
    
    async preloadSounds() {
        const soundFiles = [
            'taipei-bell-chime.mp3',
            'tokyo-wind-chime.mp3',
            'newyork-city-buzz.mp3',
            'london-big-ben.mp3',
            'paris-accordion.mp3',
            'berlin-classical.mp3',
            'moscow-bells.mp3',
            'sydney-ocean-waves.mp3'
        ];
        // 預載入邏輯...
    }
    
    playSound(cityId) {
        if (!this.enabled) return;
        // 播放對應城市音效...
    }
}
```

#### 4.5.5 音檔目錄結構
```
wwwroot/
├── audio/
│   ├── city-effects/
│   │   ├── taipei-bell-chime.mp3
│   │   ├── tokyo-wind-chime.mp3
│   │   ├── newyork-city-buzz.mp3
│   │   ├── london-big-ben.mp3
│   │   ├── paris-accordion.mp3
│   │   ├── berlin-classical.mp3
│   │   ├── moscow-bells.mp3
│   │   └── sydney-ocean-waves.mp3
│   └── fallback/
│       └── default-chime.mp3
```

#### 4.5.6 音效備用方案
- **載入失敗**: 使用預設鐘聲音效
- **瀏覽器不支援**: 自動停用音效功能
- **網路緩慢**: 延遲載入音效，優先顯示視覺特效
- **行動裝置**: 考慮資料用量，提供選擇性載入

### 5. UI/UX 設計規範
#### 5.1 視覺風格
- **主題**: 科幻未來風格
- **色彩方案**:
  - 主色調: 藍色系 (#0066CC, #003D7A)
  - 強調色: 青色 (#00FFFF)
  - 背景色: 深色漸層
  - 文字色: 白色/淺灰色

#### 5.2 動畫效果
- **時間更新**: 數字翻轉動畫（flip effect）
- **卡片懸停**: 輕微放大 + 發光效果
- **頁面載入**: 漸入動畫
- **切換時區**: 平滑過渡動畫（300ms）
- **城市特效**: 每個城市獨特的文化主題動畫（詳見 4.4 節）
- **特效細節**:
  - 粒子系統: 櫻花、雪花、泡泡等自然元素
  - 建築動畫: 地標建築的線條繪製或剪影效果
  - 色彩轉換: 背景色彩平滑過渡到城市主題色
  - 紋理效果: 和紙、磚牆、海浪等材質動畫

#### 5.3 響應式設計
- **桌面版** (>1024px):
  - 4×2 時鐘網格
  - 主要時間 72px
- **平板版** (768px-1024px):
  - 2×4 時鐘網格
  - 主要時間 60px
- **手機版** (<768px):
  - 1×8 垂直列表
  - 主要時間 48px

### 6. 暗黑模式支援
#### 6.1 模式切換
- **偵測方式**: 跟隨系統主題設定
- **實作方式**: 參考 `_Layout.cshtml` 的現有邏輯
- **CSS變數**: 使用 CSS 自定義屬性實現主題切換

#### 6.2 色彩對應
| 元素 | 淺色模式 | 暗黑模式 |
|------|----------|----------|
| 背景 | #FFFFFF | #1A1A1A |
| 主要文字 | #333333 | #FFFFFF |
| 次要文字 | #666666 | #CCCCCC |
| 卡片背景 | #F5F5F5 | #2A2A2A |
| 邊框 | #E0E0E0 | #404040 |
| 強調色 | #0066CC | #00AAFF |

### 7. 技術實作細節
#### 7.1 前端技術
- **框架**: ASP.NET Core Razor Pages
- **樣式**: CSS3 + CSS Grid/Flexbox
- **腳本**: Vanilla JavaScript (ES6+)
- **時間處理**: Intl.DateTimeFormat API
- **音效處理**: Web Audio API / HTML5 Audio
- **動畫引擎**: CSS3 Animations + requestAnimationFrame

#### 7.2 後端技術
- **語言**: C# (.NET Core)
- **時區處理**: TimeZoneInfo 類別
- **本地化**: IStringLocalizer

#### 7.3 資料結構
```csharp
public class WorldClockViewModel
{
    public string CurrentTimeZone { get; set; }
    public DateTime CurrentTime { get; set; }
    public List<TimeZoneInfo> SupportedTimeZones { get; set; }
    public string SelectedLanguage { get; set; }
    public Dictionary<string, string> Localizations { get; set; }
    public List<CityEffectConfig> CityEffects { get; set; }
}

public class TimeZoneData
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string LocalizedName { get; set; }
    public TimeSpan Offset { get; set; }
    public bool SupportsDaylightSavingTime { get; set; }
}

public class CityEffectConfig
{
    public string CityId { get; set; }
    public string CityName { get; set; }
    public string PrimaryColor { get; set; }
    public string SecondaryColor { get; set; }
    public string EffectType { get; set; }
    public int DurationMs { get; set; }
    public List<string> AnimationElements { get; set; }
    public string SoundEffect { get; set; }
    public string SoundFile { get; set; }
    public decimal SoundDuration { get; set; }
    public bool HasParticleSystem { get; set; }
    public bool SoundEnabled { get; set; }
}

public class AudioSettings
{
    public bool Enabled { get; set; } = true;
    public decimal Volume { get; set; } = 0.7m;
    public bool PreloadSounds { get; set; } = true;
    public string FallbackSound { get; set; } = "default-chime.mp3";
}
```

### 8. 效能要求
#### 8.1 載入效能
- **首次載入時間**: < 2 秒
- **資源大小**: 總計 < 900KB (包含音效檔案)
- **圖片優化**: 使用 WebP 格式
- **音效預載**: 背景非同步載入，不阻塞頁面顯示

#### 8.2 運行效能
- **時間更新頻率**: 每秒一次
- **記憶體使用**: < 80MB (包含音效緩存)
- **CPU使用率**: < 8%
- **音效延遲**: < 100ms 觸發延遲

### 9. 測試需求
#### 9.1 功能測試
- [ ] 時區切換正確性
- [ ] 時間顯示準確性
- [ ] 多語系切換
- [ ] 暗黑模式切換
- [ ] 響應式佈局
- [ ] 城市特效播放
- [ ] 特效效能測試
- [ ] 音效播放（可選）
- [ ] 特效取消機制
- [ ] 無障礙模式相容性

#### 9.2 相容性測試
- **瀏覽器**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **裝置**: Desktop, Tablet, Mobile
- **作業系統**: Windows 10+, macOS 10.15+, iOS 13+, Android 9+

### 10. 部署注意事項
- **時區資料**: 確保伺服器時區資料庫為最新版本
- **CDN**: 考慮使用 CDN 加速資源載入
- **快取策略**: 靜態資源設定適當的快取標頭
- **監控**: 設定性能監控和錯誤追蹤
- **特效資源**: 
  - 預載入所有城市特效的 CSS 和圖像資源
  - 音效檔案壓縮優化（建議使用 WebM 或 MP3 格式）
  - 特效動畫的 GPU 加速測試
- **效能監控**: 特別關注特效播放時的 FPS 和記憶體使用情況
- **音效部署**:
  - 確保所有音檔都已正確上傳到 `/wwwroot/audio/city-effects/` 目錄
  - 設定音效檔案的適當 MIME 類型
  - 考慮音效檔案的 CDN 分發
  - 監控音效載入失敗率和播放成功率

### 11. 音效資源清單
#### 11.1 需要準備的音檔
```
taipei-bell-chime.mp3      (1.5秒, ~45KB)
tokyo-wind-chime.mp3       (2.0秒, ~48KB)
newyork-city-buzz.mp3      (1.8秒, ~44KB)
london-big-ben.mp3         (3.0秒, ~50KB)
paris-accordion.mp3        (2.5秒, ~49KB)
berlin-classical.mp3       (2.2秒, ~46KB)
moscow-bells.mp3           (2.8秒, ~47KB)
sydney-ocean-waves.mp3     (3.0秒, ~50KB)
default-chime.mp3          (1.0秒, ~25KB) [備用]
```

#### 11.2 音檔授權注意事項
- 確認所有音效檔案都有適當的使用授權
- 建議使用 Creative Commons 或公共領域音效
- 保留音效來源的歸屬資訊
- 考慮購買商業授權以避免版權問題

#### 11.3 音效品質檢查清單
- [ ] 所有音檔音量已標準化
- [ ] 沒有明顯的噪音或雜音
- [ ] 音效長度符合特效動畫時間
- [ ] 檔案大小在限制範圍內
- [ ] 在不同裝置上測試播放效果
