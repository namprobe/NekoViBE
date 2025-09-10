using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.Interfaces;

/// <summary>
/// Service to get information about the current user (simplified for login/logout scenarios)
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID of the current user from JWT claims
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Whether the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Roles of the current user from JWT claims (for quick access - may be outdated)
    /// Note: Use GetCurrentRolesAsync() for up-to-date roles from database
    /// </summary>
    IEnumerable<RoleEnum> Roles { get; }
    
    /// <summary>
    /// Validate user existence and status (for register/logout scenarios)
    /// </summary>
    /// <returns>Tuple with isValid and userId (null if invalid)</returns>
    Task<(bool isValid, Guid? userId)> IsUserValidAsync();

    /// <summary>
    /// Get current user roles from database (always up-to-date)
    /// Use this for authorization decisions instead of JWT claims
    /// </summary>
    /// <returns>Current roles from database, empty if user invalid</returns>
    Task<IList<RoleEnum>> GetCurrentRolesAsync();

    /// <summary>
    /// Validate user and get current roles from database in one call
    /// </summary>
    /// <returns>Tuple with isValid, userId, and current roles from database</returns>
    Task<(bool isValid, Guid? userId, IList<RoleEnum> roles)> ValidateUserWithRolesAsync();

    /// <summary>
    /// Check if current user has specific role (optimized)
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    Task<bool> HasRoleAsync(RoleEnum role);

    /// <summary>
    /// Check if current user has any of the specified roles (optimized)
    /// </summary>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has any of the roles, false otherwise</returns>
    Task<bool> HasAnyRoleAsync(params RoleEnum[] roles);

    /// <summary>
    /// Check if current user has all of the specified roles (optimized)
    /// </summary>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has all of the roles, false otherwise</returns>
    Task<bool> HasAllRolesAsync(params RoleEnum[] roles);
} 