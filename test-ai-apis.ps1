# 測試 AI 智慧分析 API
Write-Host "測試智慧洞察 API..." -ForegroundColor Green

try {
    $insightsResponse = Invoke-WebRequest -Uri "http://localhost:5112/index7?handler=SmartInsights" -Method GET
    $insightsData = $insightsResponse.Content | ConvertFrom-Json
    
    Write-Host "智慧洞察 API 回應:" -ForegroundColor Yellow
    $insightsData | ConvertTo-Json -Depth 10
    
    Write-Host "`n測試異常警報 API..." -ForegroundColor Green
    
    $alertsResponse = Invoke-WebRequest -Uri "http://localhost:5112/index7?handler=AnomalyAlerts" -Method GET
    $alertsData = $alertsResponse.Content | ConvertFrom-Json
    
    Write-Host "異常警報 API 回應:" -ForegroundColor Yellow
    $alertsData | ConvertTo-Json -Depth 10
    
} catch {
    Write-Host "API 測試失敗: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "狀態碼: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}
