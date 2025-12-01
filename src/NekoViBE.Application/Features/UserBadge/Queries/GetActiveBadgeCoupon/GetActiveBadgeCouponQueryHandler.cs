using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserBadge.Queries.GetActiveBadgeCoupon
{
    public class GetActiveBadgeCouponQueryHandler : IRequestHandler<GetActiveBadgeCouponQuery, Result<CouponItem?>>
    {
        private readonly ILogger<GetActiveBadgeCouponQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetActiveBadgeCouponQueryHandler(
            ILogger<GetActiveBadgeCouponQueryHandler> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CouponItem?>> Handle(GetActiveBadgeCouponQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result<CouponItem?>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                // Find the user's currently equipped badge
                var activeBadge = await _unitOfWork.Repository<Domain.Entities.UserBadge>()
                    .GetQueryable()
                    .Include(ub => ub.Badge)
                        .ThenInclude(b => b.LinkedCoupon)
                    .Where(ub => 
                        ub.UserId == currentUserId && 
                        ub.IsActive && 
                        ub.Status == EntityStatusEnum.Active)
                    .FirstOrDefaultAsync(cancellationToken);

                // No equipped badge or badge has no linked coupon
                if (activeBadge == null || activeBadge.Badge.LinkedCouponId == null)
                {
                    return Result<CouponItem?>.Success(null);
                }

                var coupon = activeBadge.Badge.LinkedCoupon;
                
                // Check if coupon was loaded
                if (coupon == null)
                {
                    _logger.LogWarning("User {UserId} has equipped badge {BadgeId} but linked coupon could not be loaded",
                        currentUserId, activeBadge.BadgeId);
                    return Result<CouponItem?>.Success(null);
                }
                
                // Check if coupon is still valid
                var now = DateTime.UtcNow;
                if (coupon.Status != EntityStatusEnum.Active || 
                    coupon.StartDate > now || 
                    coupon.EndDate < now)
                {
                    _logger.LogWarning("User {UserId} has equipped badge {BadgeId} but linked coupon {CouponId} is not valid",
                        currentUserId, activeBadge.BadgeId, coupon.Id);
                    return Result<CouponItem?>.Success(null);
                }

                var couponItem = new CouponItem
                {
                    Id = coupon.Id.ToString(),
                    Code = coupon.Code,
                    Description = coupon.Description,
                    DiscountType = coupon.DiscountType,
                    DiscountValue = coupon.DiscountValue,
                    MaxDiscountCap = coupon.MaxDiscountCap,
                    MinOrderAmount = coupon.MinOrderAmount,
                    StartDate = coupon.StartDate,
                    EndDate = coupon.EndDate,
                    UsageLimit = coupon.UsageLimit,
                    CurrentUsage = coupon.CurrentUsage,
                    Status = coupon.Status,
                    CreatedAt = coupon.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = coupon.UpdatedAt,
                    IsActive = coupon.Status == EntityStatusEnum.Active
                };

                _logger.LogInformation("User {UserId} has active badge coupon {CouponId} with code {Code}", 
                    currentUserId, coupon.Id, coupon.Code);

                return Result<CouponItem?>.Success(couponItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active badge coupon for user");
                return Result<CouponItem?>.Failure("Error retrieving badge coupon", ErrorCodeEnum.InternalError);
            }
        }
    }
}
