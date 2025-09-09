using System.Linq.Expressions;
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
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    public IdentityService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, IPasswordHasher<AppUser> passwordHasher)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _passwordHasher = passwordHasher;
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

    public string HashPassword(string password)
    {
        try
        {
            return _passwordHasher.HashPassword(null!, password);
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

    public async Task<AppUser> GetUserByFirstOrDefaultAsync(Expression<Func<AppUser, bool>> predicate)
    {
        try
        {
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(predicate);
            return user;
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

    public async Task<IdentityResult> ResetUserPasswordAsync(Expression<Func<AppUser, bool>> contactPredicate, string newPasswordHash)
    {
        try
        {
            // Single query: Find user directly using tracked context
            // This avoids the double-query approach and tracking conflicts
            var user = await _userManager.Users.FirstOrDefaultAsync(contactPredicate);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "User not found" });
            }

            // Gán password mới đã hash
            user.PasswordHash = newPasswordHash;

            // Update vào DB
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return updateResult; // trả về lỗi nếu có
            }

            // Invalidate tất cả session/token cũ
            await _userManager.UpdateSecurityStampAsync(user);

            return IdentityResult.Success;
        }
        catch
        {
            throw;
        }

    }
}
