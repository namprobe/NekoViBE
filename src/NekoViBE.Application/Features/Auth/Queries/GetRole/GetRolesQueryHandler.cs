using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Auth;
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

namespace NekoViBE.Application.Features.Auth.Queries.GetRole
{
    public class GetRolesQueryHandler : IRequestHandler<GetRoleQuery, Result<RoleResponse>>
    {
        private readonly ILogger<GetRolesQueryHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public GetRolesQueryHandler(
            ILogger<GetRolesQueryHandler> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RoleResponse>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get all roles from database - optionally filter by status if needed
                var roles = await _unitOfWork.Repository<AppRole>().GetAllAsync();

                // Alternative: Only get active roles
                // var roles = await _unitOfWork.Repository<AppRole>().FindAsync(r => r.Status == EntityStatusEnum.Active);

                if (roles == null || !roles.Any())
                {
                    return Result<RoleResponse>.Failure("No roles found", ErrorCodeEnum.NotFound);
                }

                // Map to DTO
                var roleDtos = _mapper.Map<List<RoleDTO>>(roles);

                var response = new RoleResponse
                {
                    Roles = roleDtos
                };

                return Result<RoleResponse>.Success(response, "Roles retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving roles");
                return Result<RoleResponse>.Failure(
                    "An error occurred while retrieving roles",
                    ErrorCodeEnum.InternalError
                );
            }
        }
    }
}
