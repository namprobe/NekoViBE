using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<AppRole> _roleManager;
    public IdentityService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }
    public async Task<Result<AppUser>> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null)
            {
                return Result<AppUser>.Failure("Invalid email", ErrorCodeEnum.InvalidCredentials);
            }

            if (user.Status != EntityStatusEnum.Active)
                return Result<AppUser>.Failure("User is not active", ErrorCodeEnum.InvalidCredentials);
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Result<AppUser>.Failure("Invalid password", ErrorCodeEnum.InvalidCredentials);
            }

            // Check if email is confirmed (future feature)
            // if (!user.EmailConfirmed)
            //     return Result<AppUser>.Failure("Email is not confirmed", ErrorCodeEnum.InvalidCredentials);
            
            return Result<AppUser>.Success(user);
        }
        catch
        {
            throw;
        }
    }

    public async Task<IdentityResult> CreateUserAsync(AppUser user, string password)
    {
        try
        {
            IdentityResult result = new();
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var isExists = await IsPhoneNumberDuplicateAsync(user, user.PhoneNumber!);
                if (isExists.IsSuccess && isExists.Data)
                {
                    result = IdentityResult.Failed(new IdentityError { Code = "PhoneNumberExists", Description = "Phone number already exists" });
                    return result;
                }
            }
            result = await _userManager.CreateAsync(user, password);
            return result;
        }
        catch
        {
            throw;
        }
    }
    
    public async Task<IdentityResult> AddUserToRoleAsync(AppUser user, string role)
    {
        try
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result;
        }
        catch
        {
            throw;
        }
    }

    public async Task<Result<IList<string>>> GetUserRolesAsync(AppUser user)
    {
        try
        {
            var result = await _userManager.GetRolesAsync(user);
            if (!result.Any())
                return Result<IList<string>>.Failure("User has no roles", ErrorCodeEnum.InvalidCredentials);
            return Result<IList<string>>.Success(result);
        }
        catch
        {
            throw;
        }
    }

    public async Task<Result<AppUser>> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<AppUser>.Failure("User not found", ErrorCodeEnum.NotFound);
            return Result<AppUser>.Success(user);
        }
        catch
        {
            throw;
        }
    }

    public async Task<Result<bool>> IsEmailDuplicateAsync(AppUser user, string email)
    {
        try
        {
            var isExists = await _userManager.Users.AnyAsync(x => x.Email == email && x.Id != user.Id);
            if (isExists)
                return Result<bool>.Success(true);
            return Result<bool>.Success(false);
        }
        catch
        {
            throw;
        }
    }

    public async Task<Result<bool>> IsPhoneNumberDuplicateAsync(AppUser user, string phoneNumber)
    {
        try
        {
            var isExists = await _userManager.Users.AnyAsync(x => x.PhoneNumber == phoneNumber && x.Id != user.Id);
            if (isExists)
                return Result<bool>.Success(true);
            return Result<bool>.Success(false);
        }
        catch
        {
            throw;
        }
    }

    public async Task<IdentityResult> RemoveUserRolesAsync(AppUser user, string role)
    {
        try
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result;
        }
        catch
        {
            throw;
        }
    }

    public async Task<IdentityResult> UpdateUserAsync(AppUser user)
    {
        try
        {
            IdentityResult result = new();
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var isExists = await IsPhoneNumberDuplicateAsync(user, user.PhoneNumber!);
                if (isExists.IsSuccess && isExists.Data)
                {
                    result = IdentityResult.Failed(new IdentityError { Code = "PhoneNumberExists", Description = "Phone number already exists" });
                    return result;
                }
            }
            result = await _userManager.UpdateAsync(user);
            return result;
        }
        catch
        {
            throw;
        }
    }
}
