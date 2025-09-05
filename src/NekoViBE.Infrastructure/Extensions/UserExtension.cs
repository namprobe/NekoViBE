using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Infrastructure.Extensions;

/// <summary>
/// User extension methods for authentication/authorization (simplified for login/logout scenarios)
/// </summary>
public static class UserExtension
{
    /// <summary>
    /// Check if user is valid and active (for login/logout validation)
    /// </summary>
    /// <param name="userManager">User Manager</param>
    /// <param name="userId">User ID from JWT claim</param>
    /// <returns>True if user is valid, false otherwise</returns>
    public static async Task<bool> IsUserValidAsync(
        this UserManager<AppUser> userManager,
        Guid? userId)
    {
        if (userId == null)
        {
            return false;
        }

        return await userManager.Users
            .AsNoTracking()
            .AnyAsync(u =>
                u.Id == userId && 
                u.Status == EntityStatusEnum.Active && 
                u.RefreshToken != null &&
                u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                (!u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTime.UtcNow));
    }

    /// <summary>
    /// Validate user and get current roles from database (always up-to-date)
    /// </summary>
    /// <param name="userManager">User Manager</param>
    /// <param name="userId">User ID from JWT claim</param>
    /// <returns>Tuple with isValid, userId, and current roles from database</returns>
    public static async Task<(bool isValid, Guid? userId, IList<RoleEnum> roles)> ValidateUserWithRolesAsync(
        this UserManager<AppUser> userManager,
        Guid? userId)
    {
        if (userId == null)
        {
            return (false, null, new List<RoleEnum>());
        }

        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == userId && 
                u.Status == EntityStatusEnum.Active && 
                u.RefreshToken != null &&
                u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                (!u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTime.UtcNow));

        if (user == null)
        {
            return (false, null, new List<RoleEnum>());
        }

        // Lấy roles hiện tại từ database (luôn up-to-date)
        var userRoleStrings = await userManager.GetRolesAsync(user);
        var userRoles = userRoleStrings
            .Where(roleString => Enum.TryParse<RoleEnum>(roleString, true, out _))
            .Select(roleString => Enum.Parse<RoleEnum>(roleString, true))
            .Distinct()
            .ToList();

        return (true, userId, userRoles);
    }
}