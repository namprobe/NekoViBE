using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Badge.Queries.GetBadge;
using System.Linq.Expressions;

namespace NekoViBE.Application.Features.Badge.Queries.GetBadges
{
    public class GetBadgesQueryHandler : IRequestHandler<GetBadgesQuery, PaginationResult<BadgeItem>>
    {
        private readonly ILogger<GetBadgesQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetBadgesQueryHandler(
            ILogger<GetBadgesQueryHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<BadgeItem>> Handle(GetBadgesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate current user
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid || currentUserId == null)
                {
                    return PaginationResult<BadgeItem>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
                }

                // Use extension methods for filtering and ordering
                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                // Define includes for related entities
                var includes = new Expression<Func<Domain.Entities.Badge, object>>[]
                {
                    b => b.UserBadges!
                };

                var (badges, totalCount) = await _unitOfWork.Repository<Domain.Entities.Badge>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending,
                    includes: includes);

                var badgeItems = _mapper.Map<List<BadgeItem>>(badges);

                _logger.LogInformation("Badge list retrieved successfully by {CurrentUser}, TotalItems: {TotalCount}",
                    currentUserId, totalCount);

                return PaginationResult<BadgeItem>.Success(
                    badgeItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving badges");
                return PaginationResult<BadgeItem>.Failure("Error retrieving badges", ErrorCodeEnum.InternalError);
            }
        }
    }
}