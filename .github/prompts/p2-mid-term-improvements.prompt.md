# 中期改進功能開發規格書（P2 Phase）

## 專案概述
本規格書定義華麗時鐘應用程式的中期改進功能，包含自訂時區選擇、時間格式偏好設定、深色模式支援及多語言支援等四大核心功能模組。

## 1. 自訂時區選擇功能

### 1.1 功能目標
- 允許使用者選擇不同時區顯示時間
- 提供直觀的時區選擇介面
- 支援主要城市時區快速選擇
- 記憶使用者偏好設定

### 1.2 技術需求

#### 1.2.1 時區資料結構
```csharp
public class TimeZoneInfo
{
    public string Id { get; set; }          // 時區識別碼
    public string DisplayName { get; set; } // 顯示名稱
    public string City { get; set; }        // 代表城市
    public string Country { get; set; }     // 國家
    public int UtcOffset { get; set; }      // UTC偏移量（分鐘）
    public bool SupportsDST { get; set; }   // 是否支援日光節約時間
}
```

#### 1.2.2 支援時區清單
- **亞洲地區**：
  - 台北 (UTC+8)
  - 東京 (UTC+9)
  - 首爾 (UTC+9)
  - 香港 (UTC+8)
  - 新加坡 (UTC+8)
  - 曼谷 (UTC+7)
  - 雅加達 (UTC+7)
  - 馬尼拉 (UTC+8)

- **歐美地區**：
  - 倫敦 (UTC+0/+1)
  - 巴黎 (UTC+1/+2)
  - 紐約 (UTC-5/-4)
  - 洛杉磯 (UTC-8/-7)
  - 芝加哥 (UTC-6/-5)
  - 多倫多 (UTC-5/-4)

- **其他地區**：
  - 雪梨 (UTC+10/+11)
  - 墨爾本 (UTC+10/+11)
  - 杜拜 (UTC+4)
  - 開羅 (UTC+2)

#### 1.2.3 UI 設計規格
- **時區選擇器樣式**：
  - 下拉選單設計，支援搜尋功能
  - 分組顯示（按地區分類）
  - 當前時區高亮顯示
  - 時間預覽功能（選擇時顯示該時區當前時間）

- **選擇器位置**：
  - 位於時鐘顯示區域上方
  - 響應式設計，移動裝置友善
  - 最小寬度 280px，最大寬度 400px

### 1.3 實作要求
- 使用 C# `TimeZoneInfo` 類別處理時區轉換
- 客戶端使用 JavaScript `Intl.DateTimeFormat` API
- 設定資料儲存於 LocalStorage
- 預設時區為台北時間 (UTC+8)

## 2. 時間格式偏好設定功能

### 2.1 功能目標
- 提供多種時間顯示格式選項
- 支援 12/24 小時制切換
- 自訂日期顯示格式
- 秒數顯示開關選項

### 2.2 支援格式清單

#### 2.2.1 時間格式選項
- **24小時制**：
  - HH:MM:SS（預設）
  - HH:MM
  - HH時MM分SS秒
  - HH時MM分

- **12小時制**：
  - hh:MM:SS AM/PM
  - hh:MM AM/PM
  - hh時MM分SS秒 上午/下午
  - hh時MM分 上午/下午

#### 2.2.2 日期格式選項
- YYYY/MM/DD（預設）
- YYYY-MM-DD
- MM/DD/YYYY
- DD/MM/YYYY
- YYYY年MM月DD日
- MM月DD日
- 星期X, YYYY年MM月DD日

#### 2.2.3 格式設定資料結構
```csharp
public class TimeFormatSettings
{
    public bool Use24HourFormat { get; set; } = true;
    public bool ShowSeconds { get; set; } = true;
    public bool ShowDate { get; set; } = true;
    public string TimeFormat { get; set; } = "HH:MM:SS";
    public string DateFormat { get; set; } = "YYYY/MM/DD";
    public string DateSeparator { get; set; } = "/";
    public bool ShowWeekday { get; set; } = false;
}
```

### 2.3 UI 設計規格
- **設定面板位置**：右上角齒輪圖示，點擊展開設定面板
- **面板樣式**：
  - 半透明背景遮罩
  - 白色圓角卡片設計
  - 平滑展開/收縮動畫
  - 響應式佈局

- **控制項設計**：
  - Toggle 開關：12/24小時制、顯示秒數、顯示日期
  - 下拉選單：時間格式、日期格式
  - 即時預覽：設定變更立即顯示效果

## 3. 深色模式支援功能

### 3.1 功能目標
- 提供深色/淺色主題切換
- 自動檢測系統偏好設定
- 平滑主題過渡動畫
- 所有組件深色模式適配

### 3.2 主題設計規格

#### 3.2.1 淺色主題（預設）
```css
:root {
  /* 背景顏色 */
  --bg-primary: #ffffff;
  --bg-secondary: #f8fafc;
  --bg-card: #ffffff;
  
  /* 文字顏色 */
  --text-primary: #0f172a;
  --text-secondary: #64748b;
  --text-accent: #22d3ee;
  
  /* 時鐘顏色 */
  --clock-bg: #f8fafc;
  --clock-border: #e2e8f0;
  --clock-numbers: #374151;
  --clock-hands: #1f2937;
  
  /* 數位時鐘 */
  --digital-bg: #18181b;
  --digital-text: #22d3ee;
}
```

#### 3.2.2 深色主題
```css
:root[data-theme="dark"] {
  /* 背景顏色 */
  --bg-primary: #0f172a;
  --bg-secondary: #1e293b;
  --bg-card: #334155;
  
  /* 文字顏色 */
  --text-primary: #f1f5f9;
  --text-secondary: #94a3b8;
  --text-accent: #22d3ee;
  
  /* 時鐘顏色 */
  --clock-bg: #1e293b;
  --clock-border: #475569;
  --clock-numbers: #e2e8f0;
  --clock-hands: #f1f5f9;
  
  /* 數位時鐘 */
  --digital-bg: #000000;
  --digital-text: #22d3ee;
}
```

### 3.3 主題切換機制
- **切換觸發器**：右上角月亮/太陽圖示
- **動畫效果**：0.3秒 ease-in-out 過渡動畫
- **儲存機制**：LocalStorage 記憶使用者選擇
- **系統檢測**：使用 `prefers-color-scheme` CSS media query

### 3.4 組件適配清單
- [ ] 主要背景與容器
- [ ] 指針時鐘外觀
- [ ] 數位時鐘樣式
- [ ] 時區選擇器
- [ ] 格式設定面板
- [ ] 語言切換選單
- [ ] 所有按鈕與控制項
- [ ] 文字與圖示顏色

## 4. 多語言支援功能

### 4.1 功能目標
- 支援繁體中文、簡體中文、英文、日文
- 動態語言切換無需重新載入
- 時間格式本地化
- 介面文字完整翻譯

### 4.2 支援語言清單

#### 4.2.1 語言代碼與區域設定
- **繁體中文 (zh-TW)**：台灣繁體中文
- **簡體中文 (zh-CN)**：中國大陸簡體中文  
- **英文 (en-US)**：美式英文
- **日文 (ja-JP)**：日本語

#### 4.2.2 翻譯資源結構
```json
{
  "common": {
    "appTitle": "華麗時鐘",
    "currentTime": "目前時間",
    "settings": "設定",
    "close": "關閉",
    "save": "儲存",
    "cancel": "取消"
  },
  "timezone": {
    "label": "時區設定",
    "placeholder": "選擇時區",
    "search": "搜尋時區"
  },
  "format": {
    "timeFormat": "時間格式",
    "dateFormat": "日期格式", 
    "use24Hour": "24小時制",
    "showSeconds": "顯示秒數",
    "showDate": "顯示日期",
    "showWeekday": "顯示星期"
  },
  "theme": {
    "lightMode": "淺色模式",
    "darkMode": "深色模式",
    "auto": "跟隨系統"
  },
  "weekdays": {
    "sunday": "星期日",
    "monday": "星期一",
    "tuesday": "星期二",
    "wednesday": "星期三",
    "thursday": "星期四",
    "friday": "星期五",
    "saturday": "星期六"
  },
  "ampm": {
    "am": "上午",
    "pm": "下午"
  }
}
```

### 4.3 本地化格式規格

#### 4.3.1 時間格式本地化
- **繁體中文**：YYYY年MM月DD日 HH時MM分SS秒
- **簡體中文**：YYYY年MM月DD日 HH时MM分SS秒
- **英文**：MM/DD/YYYY hh:MM:SS AM/PM
- **日文**：YYYY年MM月DD日 HH時MM分SS秒

#### 4.3.2 數字本地化
- **阿拉伯數字**：0123456789（預設）
- **繁體中文數字**：〇一二三四五六七八九
- **日文數字**：〇一二三四五六七八九

### 4.4 語言切換 UI 設計
- **切換器位置**：右上角地球圖示
- **選單樣式**：
  - 下拉選單顯示國旗與語言名稱
  - 當前語言高亮顯示
  - 平滑展開動畫
  - 選擇後立即生效

## 5. 整合與相容性需求

### 5.1 資料儲存結構
```csharp
public class UserPreferences
{
    public string Language { get; set; } = "zh-TW";
    public string TimeZone { get; set; } = "Asia/Taipei";
    public string Theme { get; set; } = "auto"; // "light", "dark", "auto"
    public TimeFormatSettings TimeFormat { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

### 5.2 設定同步機制
- LocalStorage 主要儲存
- 設定變更即時生效
- 預設值降級機制
- 錯誤處理與復原

### 5.3 效能要求
- 語言切換延遲 < 200ms
- 主題切換動畫 < 300ms
- 時區切換延遲 < 100ms
- 記憶體使用 < 50MB

### 5.4 瀏覽器相容性
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Mobile Safari 14+
- Chrome Android 90+

## 6. 測試規格

### 6.1 功能測試項目
- [ ] 時區選擇與時間更新正確性
- [ ] 格式設定即時預覽功能
- [ ] 深色/淺色主題完整切換
- [ ] 多語言介面翻譯完整性
- [ ] 設定資料持久化儲存
- [ ] 預設值與降級處理

### 6.2 視覺測試項目
- [ ] 不同主題下 UI 一致性
- [ ] 響應式設計各螢幕尺寸
- [ ] 動畫過渡效果流暢性
- [ ] 文字排版與對齊
- [ ] 色彩對比度符合標準

### 6.3 效能測試項目
- [ ] 語言切換響應速度
- [ ] 主題切換動畫幀率
- [ ] 記憶體使用量監控
- [ ] CPU 使用率測試

## 7. 開發時程規劃

### 7.1 第一階段（週 1-2）
- 時區選擇功能開發
- 基礎設定面板建立
- 資料儲存機制實作

### 7.2 第二階段（週 3-4）
- 時間格式偏好設定功能
- UI 控制項開發
- 即時預覽功能

### 7.3 第三階段（週 5-6）
- 深色模式主題系統
- CSS 變數架構建立
- 主題切換動畫

### 7.4 第四階段（週 7-8）
- 多語言支援系統
- 翻譯資源整合
- 本地化格式處理

### 7.5 第五階段（週 9-10）
- 整合測試與除錯
- 效能優化
- 文件撰寫與部署

## 8. 風險評估與應對

### 8.1 技術風險
- **時區資料準確性**：使用官方時區資料庫，定期更新
- **瀏覽器相容性**：Polyfill 支援舊版瀏覽器
- **本地化複雜度**：採用成熟國際化框架

### 8.2 使用者體驗風險
- **設定複雜度**：提供預設值與快速設定選項
- **效能影響**：延遲載入與資源優化
- **學習成本**：直觀 UI 設計與說明提示

### 8.3 維護風險
- **翻譯品質**：專業翻譯與社群校正
- **時區更新**：自動化更新機制
- **向後相容**：版本遷移策略

---

**規格書版本**：1.0  
**建立日期**：2025年8月23日  
**最後更新**：2025年8月23日  
**狀態**：待開發
