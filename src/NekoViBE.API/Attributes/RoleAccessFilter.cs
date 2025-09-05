using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.API.Attributes;

/// <summary>
/// Base filter for system access control
/// </summary>
public abstract class SystemAccessFilterBase : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Method == "OPTIONS")
            return;
            
        if (context.HttpContext.Request.Method == "POST" && 
            context.HttpContext.Request.HasJsonContentType())
        {
            context.HttpContext.Items["ProcessLoginResult"] = true;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items["ProcessLoginResult"] == null)
            return;
            
        if (context.Result is not ObjectResult objectResult || objectResult.StatusCode != 200)
            return;
        
        if (objectResult.Value is not Result<AuthResponse> authResult || !authResult.IsSuccess)
            return;
            
        var authResponse = authResult.Data;
        
        if (authResponse != null && !IsAuthorizedForSystem(authResponse))
        {
            var systemName = GetSystemName();
            var allowedRoles = GetAllowedRolesDescription();
            
            context.Result = new ObjectResult(Result.Failure(
                $"You do not have access to the {systemName}.",
                ErrorCodeEnum.InsufficientPermissions,
                new List<string> { $"This area is only accessible to {allowedRoles}. Please use an appropriate account." }))
            {
                StatusCode = 403
            };
        }
    }
    
    protected abstract bool IsAuthorizedForSystem(AuthResponse user);
    protected abstract string GetSystemName();
    protected abstract string GetAllowedRolesDescription();
    
    /// <summary>
    /// Check if the user has a specific role based on the AppRole list
    /// </summary>
    protected bool HasRole(AuthResponse user, string role)
    {
        return user.Roles?.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false;
    }
}

/// <summary>
/// Filter that allows only user access
/// </summary>
public class CustomerRoleAccessFilter : SystemAccessFilterBase
{
    protected override bool IsAuthorizedForSystem(AuthResponse user)
    {
        return HasRole(user, RoleEnum.Customer.ToString()) || HasRole(user, RoleEnum.Staff.ToString()) || HasRole(user, RoleEnum.Admin.ToString());
    }
    
    protected override string GetSystemName()
    {
        return "Customer Website";
    }
    
    protected override string GetAllowedRolesDescription()
    {
        return "Customers, Staff members, and Administrators";
    }
}

/// <summary>
/// Filter that allows only staff and admin access
/// </summary>
public class StaffRoleAccessFilter : SystemAccessFilterBase
{
    protected override bool IsAuthorizedForSystem(AuthResponse user)
    {
        return HasRole(user, RoleEnum.Staff.ToString()) || HasRole(user, RoleEnum.Admin.ToString());
    }
    
    protected override string GetSystemName()
    {
        return "Staff Portal";
    }
    
    protected override string GetAllowedRolesDescription()
    {
        return "Staff members and Administrators";
    }
}

/// <summary>
/// Filter that allows only admin and staff access to admin portal
/// </summary>
public class AdminRoleAccessFilter : SystemAccessFilterBase
{
    protected override bool IsAuthorizedForSystem(AuthResponse user)
    {
        return HasRole(user, RoleEnum.Admin.ToString()) || HasRole(user, RoleEnum.Staff.ToString());
    }
    
    protected override string GetSystemName()
    {
        return "CMS System";
    }
    
    protected override string GetAllowedRolesDescription()
    {
        return "Staff members and Administrators";
    }
} 