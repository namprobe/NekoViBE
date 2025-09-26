using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Queries.GetUser
{
    public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, PaginationResult<UserItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<AppUser> _userManager;

        public GetUserListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetUserListQueryHandler> logger,
            ICurrentUserService currentUserService,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<PaginationResult<UserItem>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
        {
            var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || currentUserId == null)
            {
                return PaginationResult<UserItem>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
            }

            // Use the extension methods
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.IsAscending ?? false;

            var (users, totalCount) = await _unitOfWork.Repository<AppUser>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: isAscending,
                includes: new Expression<Func<AppUser, object>>[] {
            x => x.CustomerProfile,
            x => x.StaffProfile
                });

            var userItems = new List<UserItem>();

            foreach (var user in users)
            {
                var userItem = _mapper.Map<UserItem>(user);

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var roleEntities = await _unitOfWork.Repository<AppRole>()
                    .FindAsync(r => roles.Contains(r.Name));

                userItem.Roles = roleEntities.Select(r => new RoleInfoDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                }).ToList();

                // Determine user type
                userItem.UserType = DetermineUserType(roles, user);

                userItems.Add(userItem);
            }

            _logger.LogInformation("User list retrieved successfully by {CurrentUser}, TotalItems: {TotalCount}",
                currentUserId, totalCount);

            return PaginationResult<UserItem>.Success(
                userItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }


        private Expression<Func<AppUser, bool>>? BuildPredicate(UserFilter filter)
        {
            var predicates = new List<Expression<Func<AppUser, bool>>>();

            if (!string.IsNullOrEmpty(filter.Email))
                predicates.Add(u => u.Email.Contains(filter.Email));

            if (!string.IsNullOrEmpty(filter.FirstName))
                predicates.Add(u => u.FirstName.Contains(filter.FirstName));

            if (!string.IsNullOrEmpty(filter.LastName))
                predicates.Add(u => u.LastName.Contains(filter.LastName));

            if (!string.IsNullOrEmpty(filter.PhoneNumber))
                predicates.Add(u => u.PhoneNumber.Contains(filter.PhoneNumber));

            if (filter.Status.HasValue)
                predicates.Add(u => u.Status == filter.Status.Value);

            if (filter.HasAvatar.HasValue)
            {
                if (filter.HasAvatar.Value)
                    predicates.Add(u => !string.IsNullOrEmpty(u.AvatarPath));
                else
                    predicates.Add(u => string.IsNullOrEmpty(u.AvatarPath));
            }

            if (predicates.Count == 0)
                return null;

            return predicates.Aggregate((current, next) =>
            {
                var invokedExpr = Expression.Invoke(next, current.Parameters);
                return Expression.Lambda<Func<AppUser, bool>>(
                    Expression.AndAlso(current.Body, invokedExpr), current.Parameters);
            });
        }

        private string DetermineUserType(IList<string> roles, AppUser user)
        {
            if (roles.Contains("Admin")) return "Admin";
            if (roles.Contains("Staff")) return "Staff";
            if (roles.Contains("Customer") || user.CustomerProfile != null) return "Customer";
            return "User";
        }
    }
}
