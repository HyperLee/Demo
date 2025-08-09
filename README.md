# Demo 專案

> 一個現代化的 .NET 8 Web 應用程式範本，適合快速啟動、學習與展示。

---

## 目錄
- [Demo 專案](#demo-專案)
  - [目錄](#目錄)
  - [簡介](#簡介)
  - [功能特色](#功能特色)
  - [快速開始](#快速開始)
  - [專案結構](#專案結構)
  - [設定說明](#設定說明)
  - [常見問題](#常見問題)

---

## 簡介

本專案為 .NET 8 建構的 Web 應用程式範本，結合 Razor Pages、靜態資源管理與現代化開發流程，適合用於學習、展示或作為新專案的起點。

## 功能特色
- 基於 .NET 8
- Razor Pages 架構
- 靜態資源（CSS/JS）管理
- 多環境設定（Development/Production）
- 友善的專案結構，易於擴充
- 首頁內建「電子時鐘」與「指針時鐘」元件，支援即時台北時間顯示，並具備現代化美觀設計。

## 快速開始

```sh
# 1. 取得原始碼
$ git clone <your-repo-url>
$ cd Demo

# 2. 建置專案
$ dotnet build Demo/Demo.csproj

# 3. 執行專案
$ dotnet run --project Demo/Demo.csproj
```

> [!TIP]
> 可透過 `appsettings.Development.json` 與 `appsettings.json` 進行環境參數調整。

## 專案結構

```
Demo/
├── Pages/                # Razor Pages 與後端程式
│   ├── Shared/           # 共用頁面元件
│   └── ...
├── wwwroot/              # 靜態資源 (CSS, JS, 圖片)
├── appsettings.json      # 主要設定檔
├── appsettings.Development.json # 開發環境設定
├── Program.cs            # 進入點
└── Demo.csproj           # 專案描述檔
```

## 首頁時鐘元件說明

首頁預設同時顯示電子時鐘（數字顯示）與指針時鐘（類比鐘面），兩者皆以台北時間（UTC+8）為基準，每秒自動更新。

- **電子時鐘**：24小時制，動態閃爍冒號，字型與配色現代易讀。
- **指針時鐘**：圓形鐘面，時針、分針、秒針動態旋轉，刻度清晰。
- **響應式設計**：自適應桌機與行動裝置。
- **無 Tailwind 依賴**：所有樣式皆以原生 CSS 撰寫，確保跨環境相容。
- **無切換功能**：兩種時鐘同時顯示，無需手動切換。

如需調整樣式，可直接編輯 `Pages/Index.cshtml` 內的 `<style>` 區塊。

---
## 設定說明
- **appsettings.json**：全域設定檔，適用於所有環境。
- **appsettings.Development.json**：開發環境專用設定，會覆蓋部分全域設定。
- **wwwroot/**：放置網站靜態資源（如 CSS、JS、圖片等）。

## 常見問題

> [!NOTE]
> 若遇到執行錯誤，請確認已安裝 .NET 8 SDK，並檢查專案相依套件是否完整。

---

如需更多協助，請參閱原始碼或提出 Issue。
