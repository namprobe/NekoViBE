using AutoMapper;
using Google;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

namespace NekoViBE.Application.Features.User.Queries.GetUser
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<UsersResponse>>
    {
        private readonly ILogger<GetUsersQueryHandler> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly INekoViDbContext _context; // Add DbContext

        public GetUsersQueryHandler(
            ILogger<GetUsersQueryHandler> logger,
            IMapper mapper,
            UserManager<AppUser> userManager,
            INekoViDbContext context) // Inject DbContext
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        public async Task<Result<UsersResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get all users with their roles information
                var usersWithRoles = await GetUsersWithRolesAsync(cancellationToken);

                var response = new UsersResponse { Users = usersWithRoles };
                return Result<UsersResponse>.Success(response, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users");
                return Result<UsersResponse>.Failure("Error retrieving users", ErrorCodeEnum.InternalError);
            }
        }

        private async Task<List<UserDTO>> GetUsersWithRolesAsync(CancellationToken cancellationToken)
        {
            // Method 1: Efficient JOIN query to get users with their role information
            var usersWithRoles = await (from user in _context.Users
                                        join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                                        from ur in userRoles.DefaultIfEmpty()
                                        join role in _context.Roles on ur.RoleId equals role.Id into roles
                                        from r in roles.DefaultIfEmpty()
                                        select new { User = user, Role = r })
                                  .ToListAsync(cancellationToken);

            // Group by user and create RoleInfoDTO objects
            var userGroups = usersWithRoles
                .GroupBy(x => x.User.Id)
                .Select(g =>
                {
                    var firstUser = g.First().User;
                    var roles = g.Where(x => x.Role != null)
                               .Select(x => new RoleInfoDTO
                               {
                                   Id = x.Role.Id,
                                   Name = x.Role.Name,
                                   Description = x.Role.Description
                               })
                               .DistinctBy(r => r.Id) // Remove duplicates
                               .ToList();

                    var userDto = _mapper.Map<UserDTO>(firstUser);
                    userDto.Roles = roles;

                    return userDto;
                })
                .ToList();

            return userGroups;
        }

        // Alternative method if you prefer using UserManager for role names only
        private async Task<List<UserDTO>> GetUsersWithRoleNamesAsync(CancellationToken cancellationToken)
        {
            var users = await _userManager.Users.ToListAsync(cancellationToken);
            var userDtos = new List<UserDTO>();

            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDTO>(user);

                // Get role names first
                var roleNames = await _userManager.GetRolesAsync(user);

                // Then get full role information for those role names
                userDto.Roles = await _context.Roles
                    .Where(r => roleNames.Contains(r.Name))
                    .Select(r => new RoleInfoDTO
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description
                    })
                    .ToListAsync(cancellationToken);

                userDtos.Add(userDto);
            }

            return userDtos;
        }
    }
}
