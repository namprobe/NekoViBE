using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserBadge;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.UserBadge.Queries.GetUserBadge
{
    public class GetUserBadgesQueryHandler : IRequestHandler<GetUserBadgesQuery, Result<UserBadgesResponse>>
    {
        private readonly ILogger<GetUserBadgesQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserBadgesQueryHandler(ILogger<GetUserBadgesQueryHandler> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<UserBadgesResponse>> Handle(GetUserBadgesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userBadges = await _unitOfWork.Repository<Domain.Entities.UserBadge>()
                    .FindAsync(ub => ub.UserId == request.UserId && ub.Status == EntityStatusEnum.Active);

                var userBadgeDtos = _mapper.Map<List<UserBadgeDto>>(userBadges);

                // Load additional info
                foreach (var userBadgeDto in userBadgeDtos)
                {
                    var badge = await _unitOfWork.Repository<Domain.Entities.Badge>().GetByIdAsync(userBadgeDto.BadgeId);
                    if (badge != null)
                    {
                        userBadgeDto.BadgeName = badge.Name;
                        userBadgeDto.BadgeDiscount = badge.DiscountPercentage;
                    }
                }

                var response = new UserBadgesResponse { UserBadges = userBadgeDtos };
                return Result<UserBadgesResponse>.Success(response, "User badges retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user badges for user {UserId}", request.UserId);
                return Result<UserBadgesResponse>.Failure("Error retrieving user badges", ErrorCodeEnum.InternalError);
            }
        }
    }
}
