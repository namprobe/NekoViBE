using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Commands.UpdateUser
{

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDTO>>
    {
        private readonly ILogger<UpdateUserCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(
            ILogger<UpdateUserCommandHandler> logger,
            IMapper mapper,
            UserManager<AppUser> userManager,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserDTO>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var request = command.Request;

                // Validate current user
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid || currentUserId == null)
                {
                    return Result<UserDTO>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                // Get the user to update
                var user = await _userManager.FindByIdAsync(command.Id.ToString());
                if (user == null)
                {
                    return Result<UserDTO>.Failure("User not found", ErrorCodeEnum.NotFound);
                }

                // Check if email is being changed and validate uniqueness
                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != command.Id)
                    {
                        return Result<UserDTO>.Failure("Email already exists", ErrorCodeEnum.Conflict);
                    }
                }

                // Validate role IDs exist
                var validRoles = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
                if (!validRoles.IsValid)
                {
                    return Result<UserDTO>.Failure(validRoles.ErrorMessage, ErrorCodeEnum.BusinessRuleViolation);
                }

                // Update user properties
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.AvatarPath = request.AvatarPath;
                user.Status = request.Status;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = currentUserId;

                // Update email if provided and changed
                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    user.Email = request.Email;
                    user.UserName = request.Email; // Assuming username is same as email
                    user.NormalizedEmail = request.Email.ToUpper();
                    user.NormalizedUserName = request.Email.ToUpper();
                }

                // Update user using UserManager
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.Select(e => e.Description).ToList();
                    return Result<UserDTO>.Failure("Failed to update user", ErrorCodeEnum.BusinessRuleViolation, errors);
                }

                // Update user roles
                var roleUpdateResult = await UpdateUserRolesAsync(user.Id, request.RoleIds, cancellationToken);
                if (!roleUpdateResult.IsSuccess)
                {
                    return Result<UserDTO>.Failure(roleUpdateResult.ErrorMessage, ErrorCodeEnum.BusinessRuleViolation);
                }

                // Get updated user with roles
                var updatedUser = await GetUserWithRolesAsync(user.Id, cancellationToken);

                _logger.LogInformation("User {UserId} updated successfully by {CurrentUser}", command.Id, currentUserId);

                return Result<UserDTO>.Success(updatedUser, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user {UserId}", command.Id);
                return Result<UserDTO>.Failure("Error updating user", ErrorCodeEnum.InternalError);
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateRoleIdsAsync(List<Guid> roleIds, CancellationToken cancellationToken)
        {
            if (!roleIds.Any())
            {
                return (false, "At least one role is required");
            }

            // Check if all role IDs exist in the database
            var existingRoleIds = await _unitOfWork.Repository<AppRole>()
                .FindAsync(r => roleIds.Contains(r.Id))
                .ContinueWith(task => task.Result.Select(r => r.Id).ToList(), cancellationToken);

            var missingRoleIds = roleIds.Except(existingRoleIds).ToList();

            if (missingRoleIds.Any())
            {
                return (false, $"The following role IDs do not exist: {string.Join(", ", missingRoleIds)}");
            }

            return (true, string.Empty);
        }

        private async Task<(bool IsSuccess, string ErrorMessage)> UpdateUserRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return (false, "User not found");
                }

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove current roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    var errors = removeResult.Errors.Select(e => e.Description).ToList();
                    return (false, $"Failed to remove existing roles: {string.Join(", ", errors)}");
                }

                // Get role names from role IDs
                var roleNames = new List<string>();
                foreach (var roleId in roleIds)
                {
                    var role = await _unitOfWork.Repository<AppRole>().GetByIdAsync(roleId);
                    if (role != null)
                    {
                        roleNames.Add(role.Name);
                    }
                }

                // Add new roles
                if (roleNames.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, roleNames);
                    if (!addResult.Succeeded)
                    {
                        var errors = addResult.Errors.Select(e => e.Description).ToList();
                        return (false, $"Failed to assign new roles: {string.Join(", ", errors)}");
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update roles for user {UserId}", userId);
                return (false, "Failed to update user roles");
            }
        }

        private async Task<UserDTO> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new Exception($"User with ID {userId} not found after update");
            }

            var userDto = _mapper.Map<UserDTO>(user);
            var roles = await _userManager.GetRolesAsync(user);

            userDto.Roles = roles.Select(role => new RoleInfoDTO
            {
                Id = Guid.NewGuid(), // Generate a new Guid or use an appropriate identifier
                Name = role,
                Description = $"Role: {role}" // Provide a meaningful description if needed
            }).ToList();
            return userDto;

        }
    }
}
