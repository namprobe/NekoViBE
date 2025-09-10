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

    /// <summary>
    /// Check if user has specific role (optimized with AnyAsync)
    /// </summary>
    /// <param name="userManager">User Manager</param>
    /// <param name="userId">User ID from JWT claim</param>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role and is valid, false otherwise</returns>
    public static async Task<bool> HasRoleAsync(
        this UserManager<AppUser> userManager,
        Guid? userId,
        RoleEnum role)
    {
        if (userId == null)
        {
            return false;
        }

        // Check if user is valid and has the specific role in one query
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
            return false;
        }

        // Check if user has the specific role
        return await userManager.IsInRoleAsync(user, role.ToString());
    }

    /// <summary>
    /// Check if user has any of the specified roles (optimized)
    /// </summary>
    /// <param name="userManager">User Manager</param>
    /// <param name="userId">User ID from JWT claim</param>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has any of the roles and is valid, false otherwise</returns>
    public static async Task<bool> HasAnyRoleAsync(
        this UserManager<AppUser> userManager,
        Guid? userId,
        params RoleEnum[] roles)
    {
        if (userId == null || roles == null || !roles.Any())
        {
            return false;
        }

        // Check if user is valid first
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
            return false;
        }

        // Check if user has any of the specified roles
        foreach (var role in roles)
        {
            if (await userManager.IsInRoleAsync(user, role.ToString()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if user has all of the specified roles (optimized)
    /// </summary>
    /// <param name="userManager">User Manager</param>
    /// <param name="userId">User ID from JWT claim</param>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has all of the roles and is valid, false otherwise</returns>
    public static async Task<bool> HasAllRolesAsync(
        this UserManager<AppUser> userManager,
        Guid? userId,
        params RoleEnum[] roles)
    {
        if (userId == null || roles == null || !roles.Any())
        {
            return false;
        }

        // Check if user is valid first
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
            return false;
        }

        // Check if user has all of the specified roles
        foreach (var role in roles)
        {
            if (!await userManager.IsInRoleAsync(user, role.ToString()))
            {
                return false;
            }
        }

        return true;
    }
}