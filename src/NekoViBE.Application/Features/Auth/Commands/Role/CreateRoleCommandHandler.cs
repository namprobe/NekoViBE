using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Auth.Commands.Role
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDTO>>
    {
        private readonly ILogger<CreateRoleCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly RoleManager<AppRole> _roleManager;

        public CreateRoleCommandHandler(
            ILogger<CreateRoleCommandHandler> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            RoleManager<AppRole> roleManager)
        {
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _roleManager = roleManager;
        }

        public async Task<Result<RoleDTO>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate user is authenticated
                var (isValid, userId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid || userId == null)
                {
                    return Result<RoleDTO>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
                }

                // Check if role already exists
                var existingRole = await _roleManager.FindByNameAsync(request.Name);
                if (existingRole != null)
                {
                    return Result<RoleDTO>.Failure($"Role '{request.Name}' already exists", ErrorCodeEnum.DuplicateEntry);
                }

                // Create new role
                var role = new AppRole
                {
                    Name = request.Name,
                    NormalizedName = request.Name.ToUpper(),
                    Description = request.Description,
                    Status = request.Status,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = userId
                };

                // Use RoleManager to create the role (handles normalization and validation)
                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Failed to create role {RoleName}. Errors: {Errors}", request.Name, errors);

                    return Result<RoleDTO>.Failure(
                        "Failed to create role",
                        ErrorCodeEnum.NotFound,
                        errors
                    );
                }

                // Commit transaction if using UnitOfWork with transactions
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Role {RoleName} created successfully by user {UserId}", request.Name, userId);

                // Map to DTO and return
                var roleDto = _mapper.Map<RoleDTO>(role);
                return Result<RoleDTO>.Success(roleDto, "Role created successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role {RoleName}", request.Name);
                return Result<RoleDTO>.Failure(
                    "An error occurred while creating role",
                    ErrorCodeEnum.InternalError
                );
            }
        }
    }
}
