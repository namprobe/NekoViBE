# Database-Based Role Validation

## Vấn đề với JWT Claims

JWT claims có thể **outdated** khi:
- Admin thu hồi roles của user
- User bị demote/promote trong hệ thống
- JWT chưa expire nhưng roles đã thay đổi

## Giải pháp: Database-Based Role Checking

### 🎯 **Nguyên tắc:**
- **JWT Claims**: Chỉ để xác thực nhanh (may be outdated)
- **Database Roles**: Source of truth cho authorization decisions

## API Methods

### 1. **JWT Claims (Quick Access - May be Outdated)**
```csharp
// ⚠️ Có thể outdated - chỉ dùng để display/reference
var rolesFromJWT = _currentUserService.Roles;
```

### 2. **Database Roles (Always Up-to-Date)**
```csharp
// ✅ Luôn up-to-date từ database
var currentRoles = await _currentUserService.GetCurrentRolesAsync();

// ✅ Hoặc validate user + lấy roles cùng lúc
var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
```

## Cách sử dụng đúng

### ❌ **SAI - Chỉ dựa vào JWT Claims:**
```csharp
public IActionResult AdminAction()
{
    // SAI: JWT claims có thể outdated
    if (_currentUserService.Roles.Contains(RoleEnum.Admin))
    {
        return Ok("Admin access granted");
    }
    return Forbid();
}
```

### ✅ **ĐÚNG - Kiểm tra Database:**
```csharp
public async Task<IActionResult> AdminAction()
{
    // ĐÚNG: Lấy roles từ database
    var currentRoles = await _currentUserService.GetCurrentRolesAsync();
    
    if (currentRoles.Contains(RoleEnum.Admin))
    {
        return Ok("Admin access granted");
    }
    return Forbid($"Access denied. Current roles: {string.Join(", ", currentRoles)}");
}
```

### ✅ **TỐI ƯU - Validate User + Roles:**
```csharp
public async Task<IActionResult> AdminAction()
{
    // TỐI ƯU: 1 call để validate user và lấy roles
    var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
    
    if (!isValid)
    {
        return Unauthorized("User is not valid");
    }
    
    if (!roles.Contains(RoleEnum.Admin))
    {
        return Forbid($"Access denied. Current roles: {string.Join(", ", roles)}");
    }
    
    return Ok($"Welcome Admin! UserId: {userId}");
}
```

## Scenarios thực tế

### 1. **Authorization Middleware/Filter**
```csharp
public class AdminRequiredAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        
        // ✅ Kiểm tra roles từ database
        var (isValid, _, roles) = await currentUserService.ValidateUserWithRolesAsync();
        
        if (!isValid || !roles.Contains(RoleEnum.Admin))
        {
            context.Result = new ForbidResult($"Admin role required. Current roles: {string.Join(", ", roles)}");
            return;
        }
        
        await next();
    }
}

// Sử dụng
[AdminRequired]
public async Task<IActionResult> ManageUsers() { ... }
```

### 2. **RegisterCommandHandler với Role Checking**
```csharp
public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
{
    try
    {
        // ✅ Validate user và lấy current roles từ database
        var (isValid, currentUserId, roles) = await _currentUserService.ValidateUserWithRolesAsync();

        if (!isValid || !currentUserId.HasValue)
        {
            return Result.Failure("User is not authenticated or invalid", ErrorCodeEnum.Unauthorized);
        }

        // ✅ Kiểm tra quyền tạo user (chỉ Admin hoặc Staff)
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Staff))
        {
            return Result.Failure(
                $"Access denied. Required: Admin or Staff. Current roles: {string.Join(", ", roles)}", 
                ErrorCodeEnum.Forbidden
            );
        }

        // Logic tạo user...
        var user = _mapper.Map<AppUser>(request.Request);
        user.CreatedBy = currentUserId.Value;
        
        return Result.Success("Register successfully!");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error registering user");
        return Result.Failure(ex.Message, ErrorCodeEnum.InternalError);
    }
}
```

### 3. **Role-based Business Logic**
```csharp
public async Task<IActionResult> GetDashboardData()
{
    var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
    
    if (!isValid)
    {
        return Unauthorized();
    }

    // Different data based on current roles
    if (roles.Contains(RoleEnum.Admin))
    {
        // Admin gets full dashboard
        var adminData = await _dashboardService.GetAdminDashboardAsync();
        return Ok(adminData);
    }
    else if (roles.Contains(RoleEnum.Staff))
    {
        // Staff gets limited dashboard
        var staffData = await _dashboardService.GetStaffDashboardAsync(userId.Value);
        return Ok(staffData);
    }
    else if (roles.Contains(RoleEnum.Customer))
    {
        // Customer gets customer dashboard
        var customerData = await _dashboardService.GetCustomerDashboardAsync(userId.Value);
        return Ok(customerData);
    }
    
    return Forbid($"No dashboard available for roles: {string.Join(", ", roles)}");
}
```

### 4. **Multiple Role Requirements**
```csharp
public async Task<IActionResult> ManageInventory()
{
    var currentRoles = await _currentUserService.GetCurrentRolesAsync();
    
    // Requires Admin OR (Staff AND has inventory permission)
    bool hasPermission = currentRoles.Contains(RoleEnum.Admin) || 
                        (currentRoles.Contains(RoleEnum.Staff) && 
                         await _permissionService.HasPermissionAsync("inventory.manage"));
    
    if (!hasPermission)
    {
        return Forbid($"Insufficient permissions. Current roles: {string.Join(", ", currentRoles)}");
    }
    
    return Ok("Inventory management access granted");
}
```

## Performance Considerations

### ✅ **Caching trong Request Scope**
```csharp
// Lần gọi đầu tiên: Query database
var (isValid1, userId1, roles1) = await _currentUserService.ValidateUserWithRolesAsync();

// Lần gọi thứ 2: Sử dụng cache
var (isValid2, userId2, roles2) = await _currentUserService.ValidateUserWithRolesAsync();

// Chỉ 1 database query cho cả request!
```

### ✅ **Optimized Database Query**
```csharp
// UserExtension.ValidateUserWithRolesAsync() thực hiện:
// 1. Single query để check user validity
// 2. Single call để lấy roles nếu user valid
// Total: 2 efficient database operations
```

## Security Benefits

### 🔒 **Real-time Role Enforcement**
- Admin thu hồi role → Ngay lập tức có hiệu lực
- Không cần đợi JWT expire
- Tăng cường security cho sensitive operations

### 🔒 **Audit Trail**
```csharp
public async Task<IActionResult> DeleteUser(Guid targetUserId)
{
    var (isValid, currentUserId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
    
    // Log với current roles từ database
    _logger.LogWarning("User {CurrentUserId} with roles {CurrentRoles} attempting to delete user {TargetUserId}", 
        currentUserId, string.Join(",", roles), targetUserId);
    
    if (!roles.Contains(RoleEnum.Admin))
    {
        _logger.LogWarning("DELETE_USER_DENIED: Insufficient privileges");
        return Forbid();
    }
    
    // Proceed with deletion...
}
```

## Best Practices

### 1. **Luôn dùng Database Roles cho Authorization**
```csharp
// ✅ ĐÚNG
var roles = await _currentUserService.GetCurrentRolesAsync();

// ❌ SAI cho authorization decisions
var roles = _currentUserService.Roles; // JWT claims
```

### 2. **JWT Claims chỉ để Display/Reference**
```csharp
// ✅ OK cho display
<p>Your roles (from JWT): @string.Join(", ", currentUserService.Roles)</p>

// ✅ Nhưng authorization phải dùng database
if (await currentUserService.GetCurrentRolesAsync().Contains(RoleEnum.Admin)) { ... }
```

### 3. **Batch Operations**
```csharp
// ✅ TỐI ƯU - 1 call cho cả validation và roles
var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();

// ❌ KHÔNG TỐI ƯU - 2 calls riêng biệt
var (isValid, userId) = await _currentUserService.IsUserValidAsync();
var roles = await _currentUserService.GetCurrentRolesAsync();
```

**Với cách này, bạn có real-time role enforcement và bảo mật tốt hơn!** 🔒
