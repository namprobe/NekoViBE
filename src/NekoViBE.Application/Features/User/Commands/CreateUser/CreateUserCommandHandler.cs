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
        //private readonly string _passwordEncryptKey;

        public CreateUserCommandHandler(
            ILogger<CreateUserCommandHandler> logger,
            IMapper mapper,
            UserManager<AppUser> userManager,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IIdentityService identityService,
             IConfiguration configuration)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            //_passwordEncryptKey = configuration.GetValue<string>("PasswordEncryptKey") ?? throw new Exception("PasswordEncryptKey is not set");
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

            // Decrypt password
            //var decryptedPassword = PasswordCryptoHelper.Decrypt(userData.Password, _passwordEncryptKey);

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = userData.Email,
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                PhoneNumber = userData.PhoneNumber,
                AvatarPath = userData.AvatarPath,
                Status = userData.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = currentUserId,
                EmailConfirmed = true
            };

            var customerProfile = _mapper.Map<CustomerProfile>(userData);
            customerProfile.UserId = user.Id;
            customerProfile.InitializeEntity(customerProfile.Id);

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

                // Add customer profile
                await _unitOfWork.Repository<CustomerProfile>().AddAsync(customerProfile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                scope.Complete();
            }

            // Create shopping cart in background
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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create shopping cart for user {UserId}", user.Id);
                }
            }, cancellationToken);

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

//namespace NekoViBE.Application.Features.User.Commands.CreateUser
//{
//    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDTO>>
//    {
//        private readonly ILogger<CreateUserCommandHandler> _logger;
//        private readonly IMapper _mapper;
//        private readonly UserManager<AppUser> _userManager;
//        private readonly ICurrentUserService _currentUserService;
//        private readonly IUnitOfWork _unitOfWork; // Use UnitOfWork instead of DbContext
//        private readonly IIdentityService _identityService;
//        private readonly string _passwordEncryptKey;

//        public CreateUserCommandHandler(ILogger<CreateUserCommandHandler> logger, IMapper mapper, UserManager<AppUser> userManager, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IIdentityService identityService, string passwordEncryptKey)
//        {
//            _logger = logger;
//            _mapper = mapper;
//            _userManager = userManager;
//            _currentUserService = currentUserService;
//            _unitOfWork = unitOfWork;
//            _identityService = identityService;
//            _passwordEncryptKey = passwordEncryptKey;
//        }

//        public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
//        {
//            // Validate current user
//            var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
//            if (!isValid || currentUserId == null)
//            {
//                return Result<UserDTO>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
//            }

//            // Check if user already exists
//            //var existingUser = await _userManager.FindByNameAsync(request.UserName);
//            //if (existingUser != null)
//            //{
//            //    return Result<UserDTO>.Failure("Username already exists", ErrorCodeEnum.DuplicateEntry);
//            //}

//            //existingUser = await _userManager.FindByEmailAsync(request.Email);
//            var existingUser = await _identityService.GetUserByFirstOrDefaultAsync(u => u.Email == request.Email);
//            if (existingUser != null)
//            {
//                return Result<UserDTO>.Failure("Email already exists", ErrorCodeEnum.DuplicateEntry);
//            }

//            // Validate role IDs exist using UnitOfWork
//            var validRoles = await ValidateRoleIdsAsync(request.RoleIds, cancellationToken);
//            if (!validRoles.IsValid)
//            {
//                return Result<UserDTO>.Failure(validRoles.ErrorMessage, ErrorCodeEnum.DatabaseError);
//            }

//            // Create new user
//            //var user = new AppUser
//            //{
//            //    UserName = request.UserName,
//            //    Email = request.Email,
//            //    FirstName = request.FirstName,
//            //    LastName = request.LastName,
//            //    PhoneNumber = request.PhoneNumber,
//            //    AvatarPath = request.AvatarPath,
//            //    Status = request.Status,
//            //    CreatedAt = DateTime.UtcNow,
//            //    CreatedBy = currentUserId,
//            //    UpdatedAt = DateTime.UtcNow,
//            //    UpdatedBy = currentUserId,
//            //    EmailConfirmed = true
//            //};

//            // Create user with password
//            //var createResult = await _identityService.CreateUserAsync(user, request.Password);
//            var createResult = await HandleVerfiyOtpForRegister(cancellationToken, request);

//            //if (!createResult.IsSuccess)
//            //{
//            //    var errors = createResult.Errors;
//            //    return Result<UserDTO>.Failure("Failed to create user", ErrorCodeEnum.DatabaseError, errors);
//            //}

//            //// Assign roles using UnitOfWork
//            //var roleAssignmentResult = await AssignRolesToUserAsync(user.Id, request.RoleIds, cancellationToken);
//            //if (!roleAssignmentResult.IsSuccess)
//            //{
//            //    // Rollback user creation if role assignment fails
//            //    await _userManager.DeleteAsync(user);
//            //    return Result<UserDTO>.Failure(roleAssignmentResult.ErrorMessage, ErrorCodeEnum.DatabaseError);
//            //}

//            // Get the complete user with roles

//            //var createdUser = await GetUserWithRolesAsync(user.Id, cancellationToken);

//            //_logger.LogInformation("User {UserName} created successfully by {CurrentUser} with {RoleCount} roles",
//            //    request.UserName, currentUserId, request.RoleIds.Count);

//            //return Result<UserDTO>.Success(createdUser, "User created successfully");
//        }


//        private async Task<Result> HandleVerfiyOtpForRegister(CancellationToken cancellationToken, CreateUserCommand userData)
//        {

//            // Validate current user
//            var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
//            if (!isValid || currentUserId == null)
//            {
//                return Result.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
//            }

//            var registerRequest = userData;
//            //decrypt password
//            registerRequest.Password = PasswordCryptoHelper.Decrypt(registerRequest.Password, _passwordEncryptKey);
//            var user = _mapper.Map<AppUser>(registerRequest);
//            user = new AppUser
//            {
//                UserName = userData.UserName,
//                Email = userData.Email,
//                FirstName = userData.FirstName,
//                LastName = userData.LastName,
//                PhoneNumber = userData.PhoneNumber,
//                AvatarPath = userData.AvatarPath,
//                Status = userData.Status,
//                CreatedAt = DateTime.UtcNow,
//                CreatedBy = currentUserId,
//                UpdatedAt = DateTime.UtcNow,
//                UpdatedBy = currentUserId,
//                EmailConfirmed = true
//            };
//            user.Id = Guid.NewGuid();
//            var customerProfile = _mapper.Map<CustomerProfile>(registerRequest);
//            user.InitializeEntity(user.Id);
//            customerProfile.UserId = user.Id;
//            customerProfile.InitializeEntity(customerProfile.Id);



//            using (var scope = new TransactionScope(
//                TransactionScopeOption.Required,
//                new TransactionOptions
//                {
//                    IsolationLevel = IsolationLevel.ReadCommitted,
//                    Timeout = TimeSpan.FromMinutes(1)
//                },
//                TransactionScopeAsyncFlowOption.Enabled
//            ))
//            {
//                var createResult = await _identityService.CreateUserAsync(user, registerRequest.Password);
//                if (!createResult.Succeeded)
//                {
//                    var errors = createResult.Errors.Select(e => e.Description).ToList();
//                    return Result.Failure("Failed to create user", ErrorCodeEnum.ValidationFailed, errors);
//                }



//                //var roleResult = await _identityService.AddUserToRoleAsync(user, RoleEnum.Customer.ToString());
//                //if (!roleResult.Succeeded)
//                //{
//                //    var errors = roleResult.Errors.Select(e => e.Description).ToList();
//                //    return Result.Failure("Failed to add user to role", ErrorCodeEnum.ValidationFailed, errors);
//                //}

//                // Assign roles using UnitOfWork
//                var roleAssignmentResult = await AssignRolesToUserAsync(user.Id, userData.RoleIds, cancellationToken);
//                if (!roleAssignmentResult.IsSuccess)
//                {
//                    // Rollback user creation if role assignment fails
//                    await _userManager.DeleteAsync(user);
//                    return Result.Failure(roleAssignmentResult.ErrorMessage, ErrorCodeEnum.DatabaseError);
//                }

//                var createdUser = await GetUserWithRolesAsync(user.Id, cancellationToken);

//                _logger.LogInformation("User {UserName} created successfully by {CurrentUser} with {RoleCount} roles",
//                    userData.UserName, currentUserId, userData.RoleIds.Count);

//                //return Result<UserDTO>.Success(createdUser, "User created successfully");

//                //await _unitOfWork.Repository<CustomerProfile>().AddAsync(customerProfile);

//                await _unitOfWork.SaveChangesAsync(cancellationToken);

//                scope.Complete();
//            }

//            // create shopping cart for customer and remove OTP from cache (no need to wait for the task to complete)
//            _ = Task.Run(async () =>
//            {
//                var newShoppingCart = new ShoppingCart
//                {
//                    UserId = user.Id,
//                };
//                newShoppingCart.InitializeEntity(user.Id);
//                await _unitOfWork.Repository<ShoppingCart>().AddAsync(newShoppingCart);
//                await _unitOfWork.SaveChangesAsync();
//            });


//            //todo : send welcome email via background task (future)
//            return Result.Success("User registered successfully.");
//        }


//        private async Task<(bool IsValid, string ErrorMessage)> ValidateRoleIdsAsync(List<Guid> roleIds, CancellationToken cancellationToken)
//        {
//            if (!roleIds.Any())
//            {
//                return (true, string.Empty);
//            }

//            // Use UnitOfWork to check role existence
//            var existingRoles = await _unitOfWork.Repository<AppRole>()
//                .FindAsync(r => roleIds.Contains(r.Id)); // Adjusted to use FindAsync with a predicate

//            var existingRoleIds = existingRoles.Select(r => r.Id).ToList();
//            var missingRoleIds = roleIds.Except(existingRoleIds).ToList();

//            if (missingRoleIds.Any())
//            {
//                return (false, $"The following role IDs do not exist: {string.Join(", ", missingRoleIds)}");
//            }

//            return (true, string.Empty);
//        }

//        private async Task<(bool IsSuccess, string ErrorMessage)> AssignRolesToUserAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken)
//        {
//            if (!roleIds.Any())
//            {
//                return (true, string.Empty);
//            }

//            try
//            {
//                var user = await _identityService.GetUserByIdAsync(userId.ToString());
//                if (user == null)
//                {
//                    return (false, "User not found");
//                }

//                // Get role names from role IDs
//                var roles = await _unitOfWork.Repository<AppRole>()
//                    .FindAsync(r => roleIds.Contains(r.Id)); // Adjusted to use FindAsync with a predicate

//                var roleNames = roles.Select(r => r.Name);

//                // Assign roles using UserManager (recommended for Identity)
//                //var result = await _identityService.AddUserToRoleAsync(user, roleNames.ToString());
//                var result = await _userManager.AddToRolesAsync(user, roleNames);
//                if (!result.Succeeded)
//                {
//                    var errors = result.Errors.Select(e => e.Description).ToList();
//                    return (false, $"Failed to assign roles: {string.Join(", ", errors)}");
//                }



//                return (true, string.Empty);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to assign roles to user {UserId}", userId);
//                return (false, "Failed to assign roles to user");
//            }
//        }

//        private async Task<UserDTO> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken)
//        {
//            // Get user using UserManager
//            var user = await _identityService.GetUserByIdAsync(userId.ToString());
//            if (user == null)
//            {
//                throw new Exception($"User with ID {userId} not found after creation");
//            }

//            // Get role names using UserManager
//            var roleNamesResult = await _identityService.GetUserRolesAsync(user);

//            if (!roleNamesResult.IsSuccess || roleNamesResult.Data == null)
//            {
//                throw new Exception("Failed to retrieve user roles");
//            }

//            var roleNames = roleNamesResult.Data;

//            // Get role details from RoleRepository
//            var userRoles = new List<RoleInfoDTO>();

//            if (roleNames.Any())
//            {
//                userRoles = await _unitOfWork.Repository<AppRole>()
//                    .FindAsync(r => roleNames.Contains(r.Name))
//                    .ContinueWith(task => task.Result.Select(r => new RoleInfoDTO
//                    {
//                        Id = r.Id,
//                        Name = r.Name,
//                        Description = r.Description
//                    }).ToList(), cancellationToken);
//            }

//            var userDto = _mapper.Map<UserDTO>(user);
//            userDto.Roles = userRoles;

//            return userDto;
//        }

//        //// Get user using UserManager
//        //    var user = await _identityService.GetUserByIdAsync(userId.ToString());
//        //    if (user == null)
//        //    {
//        //        throw new Exception($"User with ID {userId} not found after creation");
//        //    }

//        //    // Get role names using UserManager
//        //    //var roleNames = await _userManager.GetRolesAsync(user);
//        //    var roleNames = await _identityService.GetUserRolesAsync(user);

//        //    // Get role details from RoleRepository
//        //    var userRoles = new List<RoleInfoDTO>();

//        //    if (roleNames.Any())
//        //    {
//        //        userRoles = await _unitOfWork.Repository<AppRole>()
//        //            .FindAsync(r => roleNames.Contains(r.Name))
//        //            .ContinueWith(task => task.Result.Select(r => new RoleInfoDTO
//        //            {
//        //                Id = r.Id,
//        //                Name = r.Name,
//        //                Description = r.Description
//        //            }).ToList(), cancellationToken);
//        //    }

//        //    var userDto = _mapper.Map<UserDTO>(user);
//        //    userDto.Roles = userRoles;

//        //    return userDto;
//        //}

//    }
//}
