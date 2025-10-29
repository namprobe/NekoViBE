using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDTO>>
    {
        private readonly ILogger<GetUserByIdQueryHandler> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public GetUserByIdQueryHandler(
            ILogger<GetUserByIdQueryHandler> logger,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<Result<UserDTO>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Remove the Include and use UserManager directly
                var user = await _userManager.FindByIdAsync(request.Id.ToString());

                if (user == null)
                {
                    return Result<UserDTO>.Failure("User not found", ErrorCodeEnum.NotFound);
                }

                var userDto = _mapper.Map<UserDTO>(user);

                // Map roles from List<string> to List<RoleInfoDTO>
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Roles = roles.Select(role => new RoleInfoDTO
                {
                    Id = Guid.NewGuid(), // Generate a new Guid or use an appropriate identifier
                    Name = role,
                    Description = $"Role: {role}" // Provide a meaningful description if needed
                }).ToList();

                return Result<UserDTO>.Success(userDto, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user {UserId}", request.Id);
                return Result<UserDTO>.Failure("Error retrieving user", ErrorCodeEnum.InternalError);
            }
        }
    }
}
