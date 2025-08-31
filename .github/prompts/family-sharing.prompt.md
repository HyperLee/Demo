# 家庭共享記帳功能開發規格書

## 專案概述
開發一個多用戶家庭記帳管理系統，讓家庭成員可以共同管理家庭財務，包括共同記帳、預算管理、支出分攤、權限控制等功能。此系統將現有的個人記帳功能擴展為多用戶協作模式。

## 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄) + SQLite 資料庫（用戶管理）
- **前端技術**: Bootstrap 5, jQuery, SignalR (即時更新)
- **身份驗證**: ASP.NET Core Identity
- **即時通訊**: SignalR (即時通知和更新)
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 資料結構設計

### 家庭群組 (Family)
```json
{
  "families": [
    {
      "id": "family_001",
      "name": "張家庭",
      "description": "張家庭記帳群組",
      "createdBy": "user_001",
      "createdAt": "2024-01-01T00:00:00Z",
      "settings": {
        "currency": "TWD",
        "timezone": "Asia/Taipei",
        "allowGuestView": false,
        "requireApprovalForLargeExpense": true,
        "largeExpenseThreshold": 5000
      }
    }
  ]
}
```

### 家庭成員 (FamilyMembers)
```json
{
  "familyMembers": [
    {
      "id": "member_001",
      "familyId": "family_001",
      "userId": "user_001",
      "nickname": "爸爸",
      "role": "admin",
      "permissions": {
        "canAddExpense": true,
        "canEditExpense": true,
        "canDeleteExpense": true,
        "canManageUsers": true,
        "canViewReports": true,
        "canExportData": true
      },
      "joinedAt": "2024-01-01T00:00:00Z",
      "isActive": true
    }
  ]
}
```

### 共享記帳記錄 (SharedAccountingRecords)
```json
{
  "sharedRecords": [
    {
      "id": "record_001",
      "familyId": "family_001",
      "userId": "user_001",
      "userNickname": "爸爸",
      "type": "支出",
      "amount": 500.00,
      "category": "餐飲",
      "description": "全家晚餐",
      "date": "2024-01-01",
      "splitType": "平均分攤",
      "splitDetails": {
        "user_001": 250.00,
        "user_002": 250.00
      },
      "attachments": [],
      "status": "已確認",
      "approvedBy": [],
      "needsApproval": false,
      "createdAt": "2024-01-01T00:00:00Z",
      "lastModifiedAt": "2024-01-01T00:00:00Z",
      "lastModifiedBy": "user_001"
    }
  ]
}
```

### 預算共享 (SharedBudgets)
```json
{
  "sharedBudgets": [
    {
      "id": "budget_001",
      "familyId": "family_001",
      "name": "家庭月預算",
      "period": "monthly",
      "year": 2024,
      "month": 1,
      "categories": {
        "餐飲": {
          "budget": 15000,
          "spent": 8500,
          "remaining": 6500
        },
        "交通": {
          "budget": 5000,
          "spent": 3200,
          "remaining": 1800
        }
      },
      "totalBudget": 50000,
      "totalSpent": 25000,
      "createdBy": "user_001",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

## 核心功能模組

### 1. 家庭群組管理頁面
- **前端**: `#file:family-management.cshtml`
- **後端**: `#file:family-management.cshtml.cs`
- **路由**: `/family-management`

### 1.1 功能描述
- **群組建立**: 建立家庭記帳群組
- **成員管理**: 邀請成員、設定權限、移除成員
- **群組設定**: 幣別、時區、批准設定等
- **邀請連結**: 生成邀請連結供成員加入

### 1.2 前端實作 (family-management.cshtml)
```html
<div class="container mt-4">
    <!-- 群組總覽卡片 -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card border-primary">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <h4 class="mb-0"><i class="fas fa-home me-2"></i>家庭群組管理</h4>
                    <button class="btn btn-light btn-sm" data-bs-toggle="modal" data-bs-target="#createFamilyModal">
                        <i class="fas fa-plus"></i> 建立新群組
                    </button>
                </div>
                <div class="card-body">
                    <div id="familyInfo" class="d-none">
                        <div class="row">
                            <div class="col-md-6">
                                <h5 id="familyName" class="text-primary"></h5>
                                <p id="familyDescription" class="text-muted mb-2"></p>
                                <small class="text-muted">建立於 <span id="familyCreatedAt"></span></small>
                            </div>
                            <div class="col-md-6">
                                <div class="d-flex justify-content-end">
                                    <button class="btn btn-outline-primary btn-sm me-2" data-bs-toggle="modal" data-bs-target="#familySettingsModal">
                                        <i class="fas fa-cog"></i> 設定
                                    </button>
                                    <button class="btn btn-success btn-sm" id="generateInviteLink">
                                        <i class="fas fa-link"></i> 邀請連結
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="noFamilyInfo" class="text-center py-5">
                        <i class="fas fa-users fa-3x text-muted mb-3"></i>
                        <h5>尚未加入任何家庭群組</h5>
                        <p class="text-muted">建立新群組或使用邀請連結加入現有群組</p>
                        <button class="btn btn-primary me-2" data-bs-toggle="modal" data-bs-target="#createFamilyModal">
                            <i class="fas fa-plus"></i> 建立群組
                        </button>
                        <button class="btn btn-outline-primary" data-bs-toggle="modal" data-bs-target="#joinFamilyModal">
                            <i class="fas fa-sign-in-alt"></i> 加入群組
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 成員管理 -->
    <div class="row" id="membersSection" style="display: none;">
        <div class="col-12">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h5 class="mb-0"><i class="fas fa-users me-2"></i>成員管理</h5>
                    <span class="badge bg-primary" id="memberCount">0 位成員</span>
                </div>
                <div class="card-body">
                    <div class="row" id="membersGrid">
                        <!-- 動態載入成員卡片 -->
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 群組統計 -->
    <div class="row mt-4" id="statisticsSection" style="display: none;">
        <div class="col-md-3">
            <div class="card text-white bg-success">
                <div class="card-body text-center">
                    <h4 id="totalIncome">$0</h4>
                    <p class="mb-0">本月收入</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-white bg-danger">
                <div class="card-body text-center">
                    <h4 id="totalExpense">$0</h4>
                    <p class="mb-0">本月支出</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-white bg-info">
                <div class="card-body text-center">
                    <h4 id="budgetUsage">0%</h4>
                    <p class="mb-0">預算使用率</p>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-white bg-warning">
                <div class="card-body text-center">
                    <h4 id="pendingApprovals">0</h4>
                    <p class="mb-0">待審核項目</p>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- 建立家庭群組模態框 -->
<div class="modal fade" id="createFamilyModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">建立家庭群組</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="createFamilyForm">
                    <div class="mb-3">
                        <label class="form-label">群組名稱 *</label>
                        <input type="text" class="form-control" id="newFamilyName" required>
                        <div class="form-text">例如：王家庭、小明家等</div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">群組描述</label>
                        <textarea class="form-control" id="newFamilyDescription" rows="2" placeholder="簡單描述這個群組的用途"></textarea>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">預設幣別</label>
                        <select class="form-select" id="newFamilyCurrency">
                            <option value="TWD">新台幣 (TWD)</option>
                            <option value="USD">美元 (USD)</option>
                            <option value="CNY">人民幣 (CNY)</option>
                            <option value="JPY">日圓 (JPY)</option>
                        </select>
                    </div>
                    <div class="mb-3">
                        <div class="form-check">
                            <input class="form-check-input" type="checkbox" id="requireApproval">
                            <label class="form-check-label" for="requireApproval">
                                大額支出需要審核
                            </label>
                        </div>
                    </div>
                    <div class="mb-3" id="thresholdSection" style="display: none;">
                        <label class="form-label">大額支出門檻</label>
                        <input type="number" class="form-control" id="approvalThreshold" value="5000" min="0">
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" id="saveNewFamily">建立群組</button>
            </div>
        </div>
    </div>
</div>

<!-- 加入家庭群組模態框 -->
<div class="modal fade" id="joinFamilyModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">加入家庭群組</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="joinFamilyForm">
                    <div class="mb-3">
                        <label class="form-label">邀請連結或邀請碼</label>
                        <input type="text" class="form-control" id="inviteCode" placeholder="貼上邀請連結或輸入邀請碼">
                        <div class="form-text">請向家庭管理員索取邀請連結</div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">您的暱稱</label>
                        <input type="text" class="form-control" id="memberNickname" placeholder="在群組中顯示的名稱" required>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" id="joinFamily">加入群組</button>
            </div>
        </div>
    </div>
</div>

<!-- 邀請連結模態框 -->
<div class="modal fade" id="inviteLinkModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">邀請新成員</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="form-label">邀請連結</label>
                    <div class="input-group">
                        <input type="text" class="form-control" id="inviteLink" readonly>
                        <button class="btn btn-outline-primary" type="button" id="copyInviteLink">
                            <i class="fas fa-copy"></i> 複製
                        </button>
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label">邀請碼</label>
                    <div class="input-group">
                        <input type="text" class="form-control" id="inviteCode" readonly>
                        <button class="btn btn-outline-primary" type="button" id="copyInviteCode">
                            <i class="fas fa-copy"></i> 複製
                        </button>
                    </div>
                </div>
                <div class="alert alert-info">
                    <i class="fas fa-info-circle me-2"></i>
                    邀請連結在 7 天後過期，可重新產生。
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
                <button type="button" class="btn btn-primary" id="regenerateInvite">重新產生</button>
            </div>
        </div>
    </div>
</div>
```

### 2. 共享記帳頁面
- **前端**: `#file:family-accounting.cshtml`
- **後端**: `#file:family-accounting.cshtml.cs`
- **路由**: `/family-accounting`

### 2.1 前端實作 (family-accounting.cshtml)
```html
<div class="container mt-4">
    <!-- 快速記帳區域 -->
    <div class="card mb-4">
        <div class="card-header bg-success text-white">
            <h5 class="mb-0"><i class="fas fa-plus-circle me-2"></i>快速新增家庭支出</h5>
        </div>
        <div class="card-body">
            <form id="quickExpenseForm">
                <div class="row">
                    <div class="col-md-2">
                        <select class="form-select" id="quickType">
                            <option value="支出">支出</option>
                            <option value="收入">收入</option>
                        </select>
                    </div>
                    <div class="col-md-2">
                        <input type="number" class="form-control" id="quickAmount" placeholder="金額" step="0.01" required>
                    </div>
                    <div class="col-md-2">
                        <select class="form-select" id="quickCategory">
                            <option value="">選擇類別</option>
                            <!-- 動態載入類別 -->
                        </select>
                    </div>
                    <div class="col-md-3">
                        <input type="text" class="form-control" id="quickDescription" placeholder="描述">
                    </div>
                    <div class="col-md-2">
                        <select class="form-select" id="quickSplitType">
                            <option value="我支付">我支付</option>
                            <option value="平均分攤">平均分攤</option>
                            <option value="自訂分攤">自訂分攤</option>
                        </select>
                    </div>
                    <div class="col-md-1">
                        <button type="submit" class="btn btn-success w-100">
                            <i class="fas fa-plus"></i>
                        </button>
                    </div>
                </div>
            </form>
        </div>
    </div>

    <!-- 篩選器 -->
    <div class="card mb-4">
        <div class="card-body">
            <div class="row">
                <div class="col-md-2">
                    <select class="form-select" id="memberFilter">
                        <option value="">全部成員</option>
                        <!-- 動態載入成員 -->
                    </select>
                </div>
                <div class="col-md-2">
                    <select class="form-select" id="categoryFilter">
                        <option value="">全部類別</option>
                        <!-- 動態載入類別 -->
                    </select>
                </div>
                <div class="col-md-2">
                    <select class="form-select" id="statusFilter">
                        <option value="">全部狀態</option>
                        <option value="已確認">已確認</option>
                        <option value="待審核">待審核</option>
                        <option value="已拒絕">已拒絕</option>
                    </select>
                </div>
                <div class="col-md-2">
                    <input type="date" class="form-control" id="dateFrom">
                </div>
                <div class="col-md-2">
                    <input type="date" class="form-control" id="dateTo">
                </div>
                <div class="col-md-2">
                    <button class="btn btn-primary w-100" id="applyFilters">
                        <i class="fas fa-filter"></i> 篩選
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- 記帳記錄列表 -->
    <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <h5 class="mb-0"><i class="fas fa-list me-2"></i>家庭記帳記錄</h5>
            <div class="btn-group">
                <button class="btn btn-outline-primary btn-sm" id="exportRecordsBtn">
                    <i class="fas fa-download"></i> 匯出
                </button>
                <button class="btn btn-outline-success btn-sm" data-bs-toggle="modal" data-bs-target="#addExpenseModal">
                    <i class="fas fa-plus"></i> 詳細新增
                </button>
            </div>
        </div>
        <div class="card-body">
            <div id="recordsList">
                <!-- 動態載入記錄 -->
            </div>
        </div>
    </div>
</div>

<!-- 自訂分攤模態框 -->
<div class="modal fade" id="customSplitModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">自訂分攤設定</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <div class="d-flex justify-content-between mb-2">
                        <span>總金額: $<span id="totalAmountToSplit">0</span></span>
                        <span>已分配: $<span id="allocatedAmount">0</span></span>
                    </div>
                    <div class="progress mb-3">
                        <div class="progress-bar" id="allocationProgress" style="width: 0%"></div>
                    </div>
                </div>
                <div id="splitMembersList">
                    <!-- 動態載入成員分攤設定 -->
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" id="confirmCustomSplit">確認分攤</button>
            </div>
        </div>
    </div>
</div>
```

### 3. JavaScript 核心功能
```javascript
class FamilyAccounting {
    constructor() {
        this.currentFamily = null;
        this.familyMembers = [];
        this.sharedRecords = [];
        this.connection = null; // SignalR 連接
        this.init();
    }

    init() {
        this.loadFamilyInfo();
        this.bindEvents();
        this.initSignalR();
        this.loadCategories();
    }

    async loadFamilyInfo() {
        try {
            const response = await fetch('/FamilyManagement/GetCurrentFamily');
            const data = await response.json();
            
            if (data.success && data.family) {
                this.currentFamily = data.family;
                this.familyMembers = data.members;
                this.showFamilyInfo();
                this.loadSharedRecords();
            } else {
                this.showNoFamilyInfo();
            }
        } catch (error) {
            console.error('載入家庭資訊失敗:', error);
        }
    }

    showFamilyInfo() {
        $('#familyName').text(this.currentFamily.name);
        $('#familyDescription').text(this.currentFamily.description);
        $('#familyCreatedAt').text(new Date(this.currentFamily.createdAt).toLocaleDateString());
        $('#familyInfo').removeClass('d-none');
        $('#noFamilyInfo').addClass('d-none');
        $('#membersSection').show();
        $('#statisticsSection').show();
        
        this.updateMembersList();
        this.updateStatistics();
    }

    showNoFamilyInfo() {
        $('#familyInfo').addClass('d-none');
        $('#noFamilyInfo').removeClass('d-none');
        $('#membersSection').hide();
        $('#statisticsSection').hide();
    }

    updateMembersList() {
        const grid = $('#membersGrid');
        grid.empty();
        
        this.familyMembers.forEach(member => {
            const memberCard = `
                <div class="col-md-4 mb-3">
                    <div class="card ${member.isActive ? '' : 'opacity-50'}">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <h6 class="card-title mb-1">${member.nickname}</h6>
                                    <small class="text-muted">${member.role}</small>
                                </div>
                                <div class="dropdown">
                                    <button class="btn btn-sm btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown">
                                        <i class="fas fa-ellipsis-v"></i>
                                    </button>
                                    <ul class="dropdown-menu">
                                        <li><a class="dropdown-item" href="#" onclick="familyAccounting.editMember('${member.id}')">編輯權限</a></li>
                                        <li><a class="dropdown-item" href="#" onclick="familyAccounting.viewMemberStats('${member.id}')">查看統計</a></li>
                                        <li><hr class="dropdown-divider"></li>
                                        <li><a class="dropdown-item text-danger" href="#" onclick="familyAccounting.removeMember('${member.id}')">移除成員</a></li>
                                    </ul>
                                </div>
                            </div>
                            <div class="mt-2">
                                ${member.permissions.canAddExpense ? '<span class="badge bg-success me-1">記帳</span>' : ''}
                                ${member.permissions.canViewReports ? '<span class="badge bg-info me-1">報表</span>' : ''}
                                ${member.permissions.canManageUsers ? '<span class="badge bg-warning">管理</span>' : ''}
                            </div>
                        </div>
                    </div>
                </div>
            `;
            grid.append(memberCard);
        });
        
        $('#memberCount').text(`${this.familyMembers.length} 位成員`);
    }

    async addQuickExpense() {
        const expenseData = {
            type: $('#quickType').val(),
            amount: parseFloat($('#quickAmount').val()),
            category: $('#quickCategory').val(),
            description: $('#quickDescription').val(),
            splitType: $('#quickSplitType').val(),
            date: new Date().toISOString().split('T')[0]
        };

        // 驗證必填欄位
        if (!expenseData.amount || !expenseData.category) {
            alert('請填寫金額和類別');
            return;
        }

        try {
            // 如果是自訂分攤，顯示分攤設定模態框
            if (expenseData.splitType === '自訂分攤') {
                this.showCustomSplitModal(expenseData);
                return;
            }

            // 否則直接儲存
            await this.saveExpense(expenseData);
            
            // 清空表單
            $('#quickExpenseForm')[0].reset();
            
            // 重新載入記錄
            this.loadSharedRecords();
            
        } catch (error) {
            console.error('新增支出失敗:', error);
            alert('新增失敗，請重試');
        }
    }

    showCustomSplitModal(expenseData) {
        $('#totalAmountToSplit').text(expenseData.amount);
        $('#allocatedAmount').text('0');
        $('#allocationProgress').css('width', '0%');
        
        const membersList = $('#splitMembersList');
        membersList.empty();
        
        this.familyMembers.forEach(member => {
            if (member.isActive) {
                const memberSplit = `
                    <div class="row mb-2 align-items-center">
                        <div class="col-6">
                            <span>${member.nickname}</span>
                        </div>
                        <div class="col-4">
                            <input type="number" class="form-control form-control-sm split-amount" 
                                   data-member-id="${member.id}" 
                                   placeholder="0.00" 
                                   step="0.01" 
                                   max="${expenseData.amount}">
                        </div>
                        <div class="col-2">
                            <button type="button" class="btn btn-sm btn-outline-primary equal-split" 
                                    data-member-id="${member.id}">平分</button>
                        </div>
                    </div>
                `;
                membersList.append(memberSplit);
            }
        });
        
        // 綁定分攤計算事件
        $('.split-amount').on('input', () => this.calculateSplitProgress(expenseData.amount));
        $('.equal-split').on('click', () => this.setEqualSplit(expenseData.amount));
        
        // 儲存支出數據以便後續使用
        this.pendingExpense = expenseData;
        
        $('#customSplitModal').modal('show');
    }

    calculateSplitProgress(totalAmount) {
        let allocatedAmount = 0;
        $('.split-amount').each(function() {
            const value = parseFloat($(this).val()) || 0;
            allocatedAmount += value;
        });
        
        $('#allocatedAmount').text(allocatedAmount.toFixed(2));
        const percentage = Math.min((allocatedAmount / totalAmount) * 100, 100);
        $('#allocationProgress').css('width', percentage + '%');
        
        // 變更進度條顏色
        if (percentage === 100) {
            $('#allocationProgress').removeClass('bg-warning').addClass('bg-success');
        } else {
            $('#allocationProgress').removeClass('bg-success').addClass('bg-warning');
        }
    }

    setEqualSplit(totalAmount) {
        const activeMembers = this.familyMembers.filter(m => m.isActive).length;
        const equalAmount = (totalAmount / activeMembers).toFixed(2);
        
        $('.split-amount').val(equalAmount);
        this.calculateSplitProgress(totalAmount);
    }

    async confirmCustomSplit() {
        const splitDetails = {};
        let totalAllocated = 0;
        
        $('.split-amount').each(function() {
            const memberId = $(this).data('member-id');
            const amount = parseFloat($(this).val()) || 0;
            if (amount > 0) {
                splitDetails[memberId] = amount;
                totalAllocated += amount;
            }
        });
        
        if (Math.abs(totalAllocated - this.pendingExpense.amount) > 0.01) {
            alert('分攤總額必須等於支出金額');
            return;
        }
        
        this.pendingExpense.splitDetails = splitDetails;
        
        try {
            await this.saveExpense(this.pendingExpense);
            $('#customSplitModal').modal('hide');
            $('#quickExpenseForm')[0].reset();
            this.loadSharedRecords();
        } catch (error) {
            console.error('儲存分攤支出失敗:', error);
            alert('儲存失敗，請重試');
        }
    }

    async saveExpense(expenseData) {
        // 處理分攤邏輯
        if (expenseData.splitType === '平均分攤' && !expenseData.splitDetails) {
            const activeMembers = this.familyMembers.filter(m => m.isActive);
            const equalAmount = expenseData.amount / activeMembers.length;
            expenseData.splitDetails = {};
            activeMembers.forEach(member => {
                expenseData.splitDetails[member.userId] = equalAmount;
            });
        } else if (expenseData.splitType === '我支付' && !expenseData.splitDetails) {
            // 獲取當前用戶ID
            const currentUserId = await this.getCurrentUserId();
            expenseData.splitDetails = {
                [currentUserId]: expenseData.amount
            };
        }

        const response = await fetch('/FamilyAccounting/SaveExpense', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(expenseData)
        });

        if (!response.ok) {
            throw new Error('儲存失敗');
        }

        const result = await response.json();
        if (!result.success) {
            throw new Error(result.message);
        }

        // 透過 SignalR 通知其他成員
        if (this.connection) {
            await this.connection.invoke('NotifyExpenseAdded', this.currentFamily.id, result.record);
        }
    }

    async loadSharedRecords() {
        try {
            const response = await fetch(`/FamilyAccounting/GetSharedRecords?familyId=${this.currentFamily.id}`);
            this.sharedRecords = await response.json();
            this.renderRecordsList();
            this.updateStatistics();
        } catch (error) {
            console.error('載入共享記錄失敗:', error);
        }
    }

    renderRecordsList() {
        const container = $('#recordsList');
        container.empty();
        
        if (this.sharedRecords.length === 0) {
            container.html(`
                <div class="text-center py-5">
                    <i class="fas fa-receipt fa-3x text-muted mb-3"></i>
                    <h5>尚無記帳記錄</h5>
                    <p class="text-muted">開始記錄您的家庭收支吧！</p>
                </div>
            `);
            return;
        }
        
        // 按日期分組顯示記錄
        const groupedRecords = this.groupRecordsByDate();
        
        Object.keys(groupedRecords).forEach(date => {
            const dateSection = `
                <div class="mb-4">
                    <h6 class="text-muted mb-3">${date}</h6>
                    <div class="records-group">
                        ${groupedRecords[date].map(record => this.renderRecordCard(record)).join('')}
                    </div>
                </div>
            `;
            container.append(dateSection);
        });
    }

    groupRecordsByDate() {
        const grouped = {};
        
        this.sharedRecords.forEach(record => {
            const date = new Date(record.date).toLocaleDateString('zh-TW', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
            
            if (!grouped[date]) {
                grouped[date] = [];
            }
            grouped[date].push(record);
        });
        
        return grouped;
    }

    renderRecordCard(record) {
        const statusBadge = this.getStatusBadge(record.status);
        const typeColor = record.type === '收入' ? 'text-success' : 'text-danger';
        const typeIcon = record.type === '收入' ? 'fas fa-arrow-up' : 'fas fa-arrow-down';
        
        return `
            <div class="card mb-2 record-card" data-record-id="${record.id}">
                <div class="card-body py-3">
                    <div class="row align-items-center">
                        <div class="col-md-1">
                            <div class="text-center ${typeColor}">
                                <i class="${typeIcon}"></i>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div>
                                <strong>${record.description}</strong>
                                <br><small class="text-muted">${record.category}</small>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="${typeColor}">
                                <strong>${record.type === '支出' ? '-' : '+'}$${record.amount}</strong>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <small class="text-muted">
                                記錄者：${record.userNickname}
                            </small>
                        </div>
                        <div class="col-md-2">
                            ${statusBadge}
                        </div>
                        <div class="col-md-2">
                            <div class="dropdown">
                                <button class="btn btn-sm btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown">
                                    操作
                                </button>
                                <ul class="dropdown-menu">
                                    <li><a class="dropdown-item" href="#" onclick="familyAccounting.viewRecordDetails('${record.id}')">查看詳情</a></li>
                                    ${this.canEditRecord(record) ? `<li><a class="dropdown-item" href="#" onclick="familyAccounting.editRecord('${record.id}')">編輯</a></li>` : ''}
                                    ${this.canDeleteRecord(record) ? `<li><a class="dropdown-item text-danger" href="#" onclick="familyAccounting.deleteRecord('${record.id}')">刪除</a></li>` : ''}
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getStatusBadge(status) {
        switch (status) {
            case '已確認':
                return '<span class="badge bg-success">已確認</span>';
            case '待審核':
                return '<span class="badge bg-warning">待審核</span>';
            case '已拒絕':
                return '<span class="badge bg-danger">已拒絕</span>';
            default:
                return '<span class="badge bg-secondary">未知</span>';
        }
    }

    updateStatistics() {
        const currentMonth = new Date().getMonth();
        const currentYear = new Date().getFullYear();
        
        let totalIncome = 0;
        let totalExpense = 0;
        let pendingApprovals = 0;
        
        this.sharedRecords.forEach(record => {
            const recordDate = new Date(record.date);
            if (recordDate.getMonth() === currentMonth && recordDate.getFullYear() === currentYear) {
                if (record.type === '收入') {
                    totalIncome += record.amount;
                } else {
                    totalExpense += record.amount;
                }
            }
            
            if (record.status === '待審核') {
                pendingApprovals++;
            }
        });
        
        $('#totalIncome').text('$' + totalIncome.toLocaleString());
        $('#totalExpense').text('$' + totalExpense.toLocaleString());
        $('#pendingApprovals').text(pendingApprovals);
        
        // 計算預算使用率（如果有設定預算）
        // 這裡需要從預算API獲取數據
        this.updateBudgetUsage(totalExpense);
    }

    async updateBudgetUsage(currentExpense) {
        try {
            const response = await fetch(`/FamilyAccounting/GetCurrentBudget?familyId=${this.currentFamily.id}`);
            const budget = await response.json();
            
            if (budget && budget.totalBudget > 0) {
                const usage = (currentExpense / budget.totalBudget * 100).toFixed(1);
                $('#budgetUsage').text(usage + '%');
            } else {
                $('#budgetUsage').text('未設定');
            }
        } catch (error) {
            console.error('載入預算資訊失敗:', error);
            $('#budgetUsage').text('N/A');
        }
    }

    initSignalR() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/familyHub')
            .build();

        this.connection.start().then(() => {
            console.log('SignalR 連接成功');
            
            // 加入家庭群組
            if (this.currentFamily) {
                this.connection.invoke('JoinFamily', this.currentFamily.id);
            }
        }).catch(err => {
            console.error('SignalR 連接失敗:', err);
        });

        // 監聽新記錄通知
        this.connection.on('ExpenseAdded', (record) => {
            this.showNotification(`${record.userNickname} 新增了一筆${record.type}: ${record.description} $${record.amount}`);
            this.loadSharedRecords();
        });

        // 監聽記錄更新通知
        this.connection.on('ExpenseUpdated', (record) => {
            this.showNotification(`${record.userNickname} 更新了記錄: ${record.description}`);
            this.loadSharedRecords();
        });

        // 監聽成員加入通知
        this.connection.on('MemberJoined', (member) => {
            this.showNotification(`${member.nickname} 加入了家庭群組`);
            this.loadFamilyInfo();
        });
    }

    showNotification(message) {
        // 使用 Bootstrap Toast 顯示通知
        const toast = `
            <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header">
                    <i class="fas fa-bell text-primary me-2"></i>
                    <strong class="me-auto">家庭記帳</strong>
                    <small>剛剛</small>
                    <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;
        
        // 如果沒有toast容器，先建立
        if ($('#toastContainer').length === 0) {
            $('body').append('<div id="toastContainer" class="position-fixed top-0 end-0 p-3" style="z-index: 9999;"></div>');
        }
        
        const $toast = $(toast);
        $('#toastContainer').append($toast);
        
        const bsToast = new bootstrap.Toast($toast[0]);
        bsToast.show();
        
        // 3秒後自動移除
        setTimeout(() => {
            $toast.remove();
        }, 3000);
    }

    bindEvents() {
        // 快速記帳表單提交
        $('#quickExpenseForm').on('submit', (e) => {
            e.preventDefault();
            this.addQuickExpense();
        });

        // 自訂分攤確認
        $('#confirmCustomSplit').on('click', () => this.confirmCustomSplit());

        // 篩選器
        $('#applyFilters').on('click', () => this.applyFilters());

        // 大額支出核取框
        $('#requireApproval').on('change', function() {
            if ($(this).is(':checked')) {
                $('#thresholdSection').show();
            } else {
                $('#thresholdSection').hide();
            }
        });

        // 建立新家庭
        $('#saveNewFamily').on('click', () => this.createNewFamily());

        // 加入家庭
        $('#joinFamily').on('click', () => this.joinFamilyGroup());

        // 產生邀請連結
        $('#generateInviteLink').on('click', () => this.generateInviteLink());
    }

    // ... 更多方法實作
}

// 初始化家庭記帳系統
$(document).ready(function() {
    window.familyAccounting = new FamilyAccounting();
});
```

### 4. 後端 SignalR Hub
```csharp
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Demo.Hubs
{
    [Authorize]
    public class FamilyHub : Hub
    {
        public async Task JoinFamily(string familyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Family_{familyId}");
        }

        public async Task LeaveFamily(string familyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Family_{familyId}");
        }

        public async Task NotifyExpenseAdded(string familyId, object record)
        {
            await Clients.Group($"Family_{familyId}").SendAsync("ExpenseAdded", record);
        }

        public async Task NotifyExpenseUpdated(string familyId, object record)
        {
            await Clients.Group($"Family_{familyId}").SendAsync("ExpenseUpdated", record);
        }

        public async Task NotifyMemberJoined(string familyId, object member)
        {
            await Clients.Group($"Family_{familyId}").SendAsync("MemberJoined", member);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
```

## 進階功能

### 5. 預算管理
- **家庭預算設定**: 各類別預算限制
- **預算警報**: 超支預警通知
- **預算分析**: 預算執行狀況分析

### 6. 支出分析與報表
- **消費模式分析**: 家庭消費習慣分析
- **成員消費統計**: 各成員支出統計
- **類別趨勢**: 各類別支出趨勢分析

### 7. 權限與安全
- **角色管理**: 管理員、成員、訪客權限
- **操作審核**: 重要操作需管理員確認
- **資料加密**: 敏感資料加密存儲

## 測試規範

### 7.1 功能測試
- [ ] 多用戶協作測試
- [ ] 即時通知功能測試
- [ ] 權限控制測試
- [ ] 資料同步測試

### 7.2 安全測試
- [ ] 身份驗證測試
- [ ] 資料隔離測試
- [ ] 權限提升攻擊測試

## 部署考量
- SignalR 在負載平衡環境下的設定
- Redis 作為 SignalR Backplane
- 資料庫遷移策略
- 多租戶資料隔離

## 未來擴展計畫
- 行動 App 開發
- 語音記帳整合
- AI 支出分類與建議
- 第三方銀行 API 整合
