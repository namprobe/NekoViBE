# CurrentUserService - Simplified Version

## Tổng quan

CurrentUserService đã được tối ưu hóa để chỉ giữ lại những tính năng cần thiết cho login/logout scenarios. Phiên bản này đơn giản, hiệu quả và type-safe với RoleEnum.

## Các tính năng

### 🎯 **ICurrentUserService Interface**

```csharp
public interface ICurrentUserService
{
    /// <summary>
    /// ID của user hiện tại từ JWT claims
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// User có được authenticated không
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Roles của user từ JWT claims (may be outdated - use GetCurrentRolesAsync() for up-to-date)
    /// </summary>
    IEnumerable<RoleEnum> Roles { get; }
    
    /// <summary>
    /// Validate user tồn tại và active
    /// </summary>
    Task<(bool isValid, Guid? userId)> IsUserValidAsync();
    
    /// <summary>
    /// Get current user roles from database (always up-to-date)
    /// </summary>
    Task<IList<RoleEnum>> GetCurrentRolesAsync();
    
    /// <summary>
    /// Validate user and get current roles from database in one call
    /// </summary>
    Task<(bool isValid, Guid? userId, IList<RoleEnum> roles)> ValidateUserWithRolesAsync();
}
```

### 🎯 **RoleEnum Support**

```csharp
public enum RoleEnum
{
    Admin,
    Staff,
    Customer
}
```

### 🎯 **UserExtension (Simplified)**

```csharp
public static class UserExtension
{
    /// <summary>
    /// Check if user is valid and active (tối ưu với single query)
    /// </summary>
    public static async Task<bool> IsUserValidAsync(
        this UserManager<AppUser> userManager,
        Guid? userId)
}
```

## Cách sử dụng

### 1. **Logout Command Handler**

```csharp
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // ✅ Chỉ cần UserId property
        var userId = _currentUserService.UserId;
        
        if (userId == null)
        {
            return Result.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
        }
        
        // Logic logout...
        return Result.Success("Logout successfully!");
    }
}
```

### 2. **Register Command Handler**

```csharp
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    
    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // ✅ Validate user với single method call
        var (isValid, currentUserId) = await _currentUserService.IsUserValidAsync();
        
        if (!isValid || !currentUserId.HasValue)
        {
            return Result.Failure("User is not authenticated or invalid", ErrorCodeEnum.Unauthorized);
        }
        
        // Logic register...
        var user = _mapper.Map<AppUser>(request.Request);
        user.CreatedBy = currentUserId.Value;
        
        return Result.Success("Register successfully!");
    }
}
```

### 3. **Authorization với Database Roles (Recommended)**

```csharp
public class SomeController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    
    [HttpPost("admin-only")]
    public async Task<IActionResult> AdminOnly()
    {
        // ✅ Database-based role checking (always up-to-date)
        var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
        
        if (!isValid)
        {
            return Unauthorized("User is not valid");
        }
        
        if (!roles.Contains(RoleEnum.Admin))
        {
            return Forbid($"Required role: Admin. Current roles: {string.Join(", ", roles)}");
        }
        
        return Ok($"Welcome Admin! UserId: {userId}");
    }
    
    [HttpPost("staff-or-admin")]
    public async Task<IActionResult> StaffOrAdmin()
    {
        // ✅ Get current roles from database
        var currentRoles = await _currentUserService.GetCurrentRolesAsync();
        
        if (currentRoles.Contains(RoleEnum.Admin) || currentRoles.Contains(RoleEnum.Staff))
        {
            return Ok($"Welcome! Current roles: {string.Join(", ", currentRoles)}");
        }
        
        return Forbid($"Access denied. Current roles: {string.Join(", ", currentRoles)}");
    }
}
```

## Performance & Architecture Benefits

### ✅ **Simplified & Clean**
- Chỉ 4 members trong interface (vs 12+ trước đây)
- Không có deprecated methods
- Code dễ hiểu và maintain

### ✅ **Type Safety với RoleEnum**
- Compile-time checking cho roles
- IntelliSense support
- Tránh typos trong role names

### ✅ **Optimized Database Queries**
```csharp
// UserExtension.IsUserValidAsync() - Single optimized query
return await userManager.Users
    .AsNoTracking()
    .AnyAsync(u =>
        u.Id == userId && 
        u.Status == EntityStatusEnum.Active && 
        u.RefreshToken != null &&
        u.RefreshTokenExpiryTime > DateTime.UtcNow &&
        (!u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTime.UtcNow));
```

### ✅ **Simple Caching**
- Chỉ cache bool result thay vì complex objects
- Ít memory footprint
- Faster cache lookup

### ✅ **Perfect for Login/Logout Scenarios**
- Chứa đủ thông tin cần thiết
- Không có overhead từ unused features
- Focus on core authentication tasks

## So sánh với phiên bản trước

| Aspect | Before (Complex) | After (Simplified) |
|--------|------------------|-------------------|
| **Interface methods** | 12+ methods | 4 methods |
| **Role type** | `string` (error-prone) | `RoleEnum` (type-safe) |
| **Cache complexity** | Multiple caches | Single bool cache |
| **Database queries** | 1-2 queries | 1 optimized query |
| **Code lines** | ~190 lines | ~37 lines |
| **Maintenance** | Complex | Simple |
| **Learning curve** | High | Low |

## Migration Guide

### **Old Code:**
```csharp
// ❌ Complex, multiple calls
var (isValid, userId, roles, hasAdmin) = await _currentUserService.ValidateUserAndRoleAsync(
    requiredRole: "Admin"
);

if (isValid && hasAdmin) { ... }
```

### **New Code:**
```csharp
// ✅ Simple, type-safe
var (isValid, userId) = await _currentUserService.IsUserValidAsync();
var hasAdmin = _currentUserService.Roles.Contains(RoleEnum.Admin);

if (isValid && hasAdmin) { ... }
```

## Best Practices

### 1. **Use RoleEnum for role checking**
```csharp
// ✅ Good
if (_currentUserService.Roles.Contains(RoleEnum.Admin))

// ❌ Bad (old way)
if (_currentUserService.Roles.Contains("Admin"))
```

### 2. **JWT Claims cho quick role access**
```csharp
// ✅ Good - JWT claims (fast, no DB query)
var roles = _currentUserService.Roles;

// ✅ Good - Database validation (for critical operations)
var (isValid, userId) = await _currentUserService.IsUserValidAsync();
```

### 3. **Combine validation với role checking**
```csharp
// ✅ Good pattern
var (isValid, userId) = await _currentUserService.IsUserValidAsync();
var hasRequiredRole = _currentUserService.Roles.Contains(RoleEnum.Admin);

if (isValid && hasRequiredRole)
{
    // Proceed with logic
}
```

## Kết luận

Phiên bản simplified này:
- **Perfect fit** cho login/logout scenarios
- **Type-safe** với RoleEnum
- **High performance** với optimized queries
- **Easy to understand** và maintain
- **Future-proof** - dễ extend khi cần

Đây là foundation vững chắc cho authentication system! 🚀
