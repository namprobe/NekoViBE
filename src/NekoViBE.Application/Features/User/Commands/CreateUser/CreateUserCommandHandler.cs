using AutoMapper;
using Azure.Core;
using MediatR;
 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;



namespace NekoViBE.Application.Features.User.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDTO>>
    {
        private readonly ILogger<CreateUserCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IFileService _fileService;

        public CreateUserCommandHandler(ILogger<CreateUserCommandHandler> logger, IMapper mapper, UserManager<AppUser> userManager, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IIdentityService identityService, IFileService fileService)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _fileService = fileService;
        }

        public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate current user
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid || currentUserId == null)
                {
                    return Result<UserDTO>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
                }

                // Check if email already exists
                var existingUser = await _identityService.GetUserByFirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return Result<UserDTO>.Failure("Email already exists", ErrorCodeEnum.DuplicateEntry);
                }

                // Validate role IDs exist
                var validRoles = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
                if (!validRoles.IsValid)
                {
                    return Result<UserDTO>.Failure(validRoles.ErrorMessage, ErrorCodeEnum.DatabaseError);
                }

                // Handle user creation with OTP verification
                var createResult = await HandleVerifyOtpForRegister(cancellationToken, request);

                if (!createResult.IsSuccess)
                {
                    return Result<UserDTO>.Failure(createResult.Message, ErrorCodeEnum.InvalidInput);
                }

                // Get the created user with roles
                var createdUser = await GetUserWithRolesAsync(createResult.Data, cancellationToken);
                return Result<UserDTO>.Success(createdUser, "User created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Result<UserDTO>.Failure("An error occurred while creating user", ErrorCodeEnum.InternalError);
            }
        }

        private async Task<Result<Guid>> HandleVerifyOtpForRegister(CancellationToken cancellationToken, CreateUserCommand userData)
        {
            // Validate current user
            var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || currentUserId == null)
            {
                return Result<Guid>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = userData.Email,
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                PhoneNumber = userData.PhoneNumber,
                Status = EntityStatusEnum.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = currentUserId,
                EmailConfirmed = true
            };

            // Handle avatar upload
            if (userData.AvatarPath != null)
            {
                var imagePath = await _fileService.UploadFileAsync(userData.AvatarPath, "uploads/users", cancellationToken);
                user.AvatarPath = imagePath;
                _logger.LogInformation("ImagePath set to {ImagePath} for user {Name}", imagePath, user.Email);
            }
            else
            {
                _logger.LogWarning("No ImageFile provided for User {Name}", userData.Email);
                // Don't return failure here - avatar is optional
                // return Result<Guid>.Failure("Error uploading file", ErrorCodeEnum.InternalError);
            }

            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(1)
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // Create user
                var createResult = await _identityService.CreateUserAsync(user, userData.Password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    return Result<Guid>.Failure("Failed to create user", ErrorCodeEnum.ValidationFailed, errors);
                }

                // Assign roles
                var roleAssignmentResult = await AssignRolesToUserAsync(user.Id, userData.RoleIds, cancellationToken);
                if (!roleAssignmentResult.IsSuccess)
                {
                    await _userManager.DeleteAsync(user);
                    return Result<Guid>.Failure(roleAssignmentResult.ErrorMessage, ErrorCodeEnum.DatabaseError);
                }

                // Get the assigned roles to determine which profile to create
                var userRoles = await _userManager.GetRolesAsync(user);
                var roleNames = userRoles.ToList();

                // Check if user has CUSTOMER role
                if (roleNames.Any(r => r.Equals(RoleEnum.Customer.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    // Create CustomerProfile for CUSTOMER role
                    var customerProfile = _mapper.Map<CustomerProfile>(userData);
                    customerProfile.UserId = user.Id;
                    customerProfile.InitializeEntity(customerProfile.Id);
                    await _unitOfWork.Repository<CustomerProfile>().AddAsync(customerProfile);
                    _logger.LogInformation("Created CustomerProfile for user {UserId}", user.Id);
                }

                // Check if user has ADMIN or STAFF role
                if (roleNames.Any(r => r.Equals(RoleEnum.Admin.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                      r.Equals(RoleEnum.Staff.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    // Create StaffProfile for ADMIN or STAFF roles
                    var staffProfile = _mapper.Map<StaffProfile>(userData);
                    staffProfile.UserId = user.Id;
                    staffProfile.InitializeEntity(staffProfile.Id);
                    await _unitOfWork.Repository<StaffProfile>().AddAsync(staffProfile);
                    _logger.LogInformation("Created StaffProfile for user {UserId}", user.Id);
                }

                // Save all changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                scope.Complete();
            }

            // Create shopping cart in background ONLY for CUSTOMER role
            var userRolesAfterCreation = await _userManager.GetRolesAsync(user);
            if (userRolesAfterCreation.Any(r => r.Equals(RoleEnum.Customer.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var newShoppingCart = new ShoppingCart
                        {
                            UserId = user.Id,
                        };
                        newShoppingCart.InitializeEntity(user.Id);
                        await _unitOfWork.Repository<ShoppingCart>().AddAsync(newShoppingCart);
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation("Created shopping cart for customer user {UserId}", user.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create shopping cart for user {UserId}", user.Id);
                    }
                }, cancellationToken);
            }

            return Result<Guid>.Success(user.Id, "User registered successfully.");
        }

        
        private async Task<(bool IsValid, string ErrorMessage)> ValidateRoleIdsAsync(List<Guid> roleIds, CancellationToken cancellationToken)
        {
            if (!roleIds.Any())
            {
                return (true, string.Empty);
            }

            var existingRoles = await _unitOfWork.Repository<AppRole>()
                .FindAsync(r => roleIds.Contains(r.Id));

            var existingRoleIds = existingRoles.Select(r => r.Id).ToList();
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
                return (true, string.Empty);
            }

            try
            {
                var user = await _identityService.GetUserByIdAsync(userId.ToString());
                if (user == null)
                {
                    return (false, "User not found");
                }

                var roles = await _unitOfWork.Repository<AppRole>()
                    .FindAsync(r => roleIds.Contains(r.Id));

                var roleNames = roles.Select(r => r.Name).ToArray();

                var result = await _userManager.AddToRolesAsync(user, roleNames);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return (false, $"Failed to assign roles: {string.Join(", ", errors)}");
                }

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
            var user = await _identityService.GetUserByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new Exception($"User with ID {userId} not found");
            }

            var roleNamesResult = await _identityService.GetUserRolesAsync(user);
            var roleNames = roleNamesResult.IsSuccess ? roleNamesResult.Data : new List<string>();

            var userRoles = new List<RoleInfoDTO>();
            if (roleNames != null && roleNames.Any())
            {
                var roles = await _unitOfWork.Repository<AppRole>()
                    .FindAsync(r => roleNames.Contains(r.Name));

                userRoles = roles.Select(r => new RoleInfoDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                }).ToList();
            }

            var userDto = _mapper.Map<UserDTO>(user);
            userDto.Roles = userRoles;

            return userDto;
        }
    }
}

