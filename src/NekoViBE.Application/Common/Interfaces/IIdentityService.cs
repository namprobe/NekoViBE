
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AppUser>> AuthenticateAsync(LoginRequest request);
    Task<IdentityResult> CreateUserAsync(AppUser user, string password);
    Task<IdentityResult> AddUserToRoleAsync(AppUser user, string role);
    Task<Result<IList<string>>> GetUserRolesAsync(AppUser user);
    Task<Result<AppUser>> GetUserByIdAsync(string userId);
    Task<AppUser> GetUserByFirstOrDefaultAsync(Expression<Func<AppUser, bool>> predicate);
    Task<Result<bool>> IsEmailDuplicateAsync(AppUser user, string email);
    Task<Result<bool>> IsPhoneNumberDuplicateAsync(AppUser user, string phoneNumber);
    Task<IdentityResult> UpdateUserAsync(AppUser user);
    Task<IdentityResult> RemoveUserRolesAsync(AppUser user, string role);
    string HashPassword(string password);
    Task<IdentityResult> ResetUserPasswordAsync(Expression<Func<AppUser, bool>> contactPredicate, string newPasswordHash);

}