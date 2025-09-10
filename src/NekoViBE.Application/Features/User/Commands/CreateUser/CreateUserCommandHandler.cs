using MediatR;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Application.Common.DTOs.Role;

namespace NekoViBE.Application.Features.User.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDTO>>
    {
        private readonly ILogger<CreateUserCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly INekoViDbContext _context; // Add DbContext for direct role handling

        public CreateUserCommandHandler(
            ILogger<CreateUserCommandHandler> logger,
            IMapper mapper,
            UserManager<AppUser> userManager,
            ICurrentUserService currentUserService,
            INekoViDbContext context) // Inject DbContext
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Validate current user
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid || currentUserId == null)
                {
                    return Result<UserDTO>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(request.UserName);
                if (existingUser != null)
                {
                    return Result<UserDTO>.Failure("Username already exists", ErrorCodeEnum.ResourceConflict);
                }

                existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return Result<UserDTO>.Failure("Email already exists", ErrorCodeEnum.ResourceConflict);
                }

                // Validate role IDs exist
                var validRoles = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
                if (!validRoles.IsValid)
                {
                    return Result<UserDTO>.Failure(validRoles.ErrorMessage, ErrorCodeEnum.ResourceConflict);
                }

                // Create new user
                var user = new AppUser
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    AvatarPath = request.AvatarPath,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = currentUserId,
                    EmailConfirmed = true,
                    JoiningAt = DateTime.UtcNow
                };

                // Create user with password
                var createResult = await _userManager.CreateAsync(user, request.Password);

                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    return Result<UserDTO>.Failure("Failed to create user", ErrorCodeEnum.ResourceConflict, errors);
                }

                // Assign roles using Role IDs
                var roleAssignmentResult = await AssignRolesToUserAsync(user.Id, request.RoleIds, cancellationToken);
                if (!roleAssignmentResult.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<UserDTO>.Failure(roleAssignmentResult.ErrorMessage, ErrorCodeEnum.ResourceConflict);
                }

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                // Get the complete user with roles
                var createdUser = await GetUserWithRolesAsync(user.Id, cancellationToken);

                _logger.LogInformation("User {UserName} created successfully by {CurrentUser} with {RoleCount} roles",
                    request.UserName, currentUserId, request.RoleIds.Count);

                return Result<UserDTO>.Success(createdUser, "User created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while creating user {UserName}", request.UserName);
                return Result<UserDTO>.Failure("An error occurred while creating user", ErrorCodeEnum.InternalError);
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateRoleIdsAsync(List<Guid> roleIds, CancellationToken cancellationToken)
        {
            if (!roleIds.Any())
            {
                return (true, string.Empty); // No roles is valid
            }

            // Check if all role IDs exist in the database
            var existingRoleIds = await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            var missingRoleIds = roleIds.Except(existingRoleIds).ToList();

            if (missingRoleIds.Any())
            {
                return (false, $"The following role IDs do not exist: {string.Join(", ", missingRoleIds)}");
            }

            return (true, string.Empty);
        }

        private async Task<(bool IsSuccess, string ErrorMessage)> AssignRolesToUserAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken)
        {
            if (!roleIds.Any())
            {
                return (true, string.Empty); // No roles to assign
            }

            try
            {
                // Create UserRole entries directly
                var userRoles = roleIds.Select(roleId => new IdentityUserRole<Guid>
                {
                    UserId = userId,
                    RoleId = roleId
                }).ToList();

                _context.UserRoles.AddRange(userRoles);
                await _context.SaveChangesAsync(cancellationToken);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign roles to user {UserId}", userId);
                return (false, "Failed to assign roles to user");
            }
        }

        private async Task<UserDTO> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new Exception($"User with ID {userId} not found after creation");
            }

            // Get user roles with role information
            var userRoles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where ur.UserId == userId
                                   select new RoleInfoDTO
                                   {
                                       Id = r.Id,
                                       Name = r.Name,
                                       Description = r.Description
                                   }).ToListAsync(cancellationToken);

            var UserDTO = _mapper.Map<UserDTO>(user);
            UserDTO.Roles = userRoles;

            return UserDTO;
        }
    }

}
