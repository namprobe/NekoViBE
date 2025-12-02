using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserBadge;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserBadge.Command.ProcessBadgeEligibility
{
    public class ProcessBadgeEligibilityCommandHandler : IRequestHandler<ProcessBadgeEligibilityCommand, Result>
    {
        private readonly ILogger<ProcessBadgeEligibilityCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ProcessBadgeEligibilityCommandHandler(
            ILogger<ProcessBadgeEligibilityCommandHandler> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(ProcessBadgeEligibilityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var targetUserId = request.UserId ?? currentUserId;

                // Get all active badges with their linked coupons
                var allBadges = await _unitOfWork.Repository<Domain.Entities.Badge>()
                    .GetQueryable()
                    .Include(b => b.LinkedCoupon)
                    .Where(b => b.Status == EntityStatusEnum.Active)
                    .ToListAsync(cancellationToken);

                // Get badges user already has
                var existingUserBadges = await _unitOfWork.Repository<Domain.Entities.UserBadge>()
                    .GetQueryable()
                    .Where(ub => ub.UserId == targetUserId && ub.Status == EntityStatusEnum.Active)
                    .Select(ub => ub.BadgeId)
                    .ToListAsync(cancellationToken);

                // Filter out badges user already has
                var eligibleBadges = allBadges.Where(b => !existingUserBadges.Contains(b.Id)).ToList();

                var newlyAwardedBadges = new List<NewlyAwardedBadgeResponse>();

                foreach (var badge in eligibleBadges)
                {
                    var isEligible = await CheckBadgeEligibility(targetUserId!.Value, badge, cancellationToken);
                    if (isEligible)
                    {
                        // Award the badge
                        var userBadge = new Domain.Entities.UserBadge
                        {
                            Id = Guid.NewGuid(),
                            UserId = targetUserId.Value,
                            BadgeId = badge.Id,
                            EarnedDate = DateTime.UtcNow,
                            IsActive = false, // Not equipped by default
                            Status = EntityStatusEnum.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = targetUserId
                        };

                        // Set time limit if badge is time-limited
                        if (badge.IsTimeLimited)
                        {
                            userBadge.ActivatedFrom = badge.StartDate ?? DateTime.UtcNow;
                            userBadge.ActivatedTo = badge.EndDate;
                        }

                        await _unitOfWork.Repository<Domain.Entities.UserBadge>().AddAsync(userBadge);

                        // If badge has a linked coupon, automatically collect it for the user
                        if (badge.LinkedCouponId.HasValue && badge.LinkedCoupon != null)
                        {
                            _logger.LogInformation("🔗 Badge {BadgeName} has LinkedCouponId: {LinkedCouponId}, Code: {CouponCode}",
                                badge.Name, badge.LinkedCouponId, badge.LinkedCoupon.Code);

                            // Check if user already has this coupon
                            var existingUserCoupon = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
                                .GetQueryable()
                                .AnyAsync(uc => 
                                    uc.UserId == targetUserId.Value && 
                                    uc.CouponId == badge.LinkedCouponId.Value &&
                                    uc.Status == EntityStatusEnum.Active,
                                    cancellationToken);

                            if (!existingUserCoupon)
                            {
                                var userCoupon = new Domain.Entities.UserCoupon
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = targetUserId.Value,
                                    CouponId = badge.LinkedCouponId.Value,
                                    Status = EntityStatusEnum.Active,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = targetUserId
                                };

                                try
                                {
                                    await _unitOfWork.Repository<Domain.Entities.UserCoupon>().AddAsync(userCoupon);
                                    
                                    _logger.LogInformation("🎟️ Auto-collected badge coupon {CouponCode} (ID: {CouponId}) for user {UserId} when awarding badge {BadgeName}",
                                        badge.LinkedCoupon.Code, badge.LinkedCouponId, targetUserId, badge.Name);
                                }
                                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_UserCoupons_UserId_CouponId_Active") == true)
                                {
                                    _logger.LogDebug("⏭️ Duplicate key: User {UserId} already has coupon {CouponCode} (race condition caught by DB)",
                                        targetUserId, badge.LinkedCoupon.Code);
                                }
                            }
                            else
                            {
                                _logger.LogInformation("⏭️ User {UserId} already has coupon {CouponCode}, skipping", 
                                    targetUserId, badge.LinkedCoupon.Code);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Badge {BadgeName} (ID: {BadgeId}) has NO linked coupon. LinkedCouponId: {LinkedCouponId}",
                                badge.Name, badge.Id, badge.LinkedCouponId);
                        }

                        newlyAwardedBadges.Add(new NewlyAwardedBadgeResponse
                        {
                            UserBadgeId = userBadge.Id,
                            BadgeId = badge.Id,
                            Name = badge.Name,
                            Description = badge.Description,
                            IconUrl = badge.IconPath,
                            DiscountPercentage = badge.DiscountPercentage,
                            EarnedDate = userBadge.EarnedDate
                        });
                    }
                }

                if (newlyAwardedBadges.Any())
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Awarded {Count} new badges to user {UserId}", newlyAwardedBadges.Count, targetUserId);
                }

                return Result.Success("Badge eligibility processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing badge eligibility for user {UserId}", request.UserId);
                return Result.Failure("Error processing badge eligibility", ErrorCodeEnum.InternalError);
            }
        }

        private async Task<bool> CheckBadgeEligibility(
            Guid userId,
            Domain.Entities.Badge badge,
            CancellationToken cancellationToken)
        {
            if (!decimal.TryParse(badge.ConditionValue, out var threshold))
            {
                _logger.LogWarning("Invalid condition value for badge {BadgeId}: {Value}", badge.Id, badge.ConditionValue);
                return false;
            }

            switch (badge.ConditionType)
            {
                case ConditionTypeEnum.TotalSpent:
                    var totalSpent = await _unitOfWork.Repository<Domain.Entities.Order>()
                        .GetQueryable()
                        .Where(o => o.UserId == userId && o.OrderStatus == OrderStatusEnum.Delivered)
                        .SumAsync(o => o.FinalAmount, cancellationToken);
                    return totalSpent >= threshold;

                case ConditionTypeEnum.OrderCount:
                    var orderCount = await _unitOfWork.Repository<Domain.Entities.Order>()
                        .GetQueryable()
                        .Where(o => o.UserId == userId && o.OrderStatus == OrderStatusEnum.Delivered)
                        .CountAsync(cancellationToken);
                    return orderCount >= (int)threshold;

                case ConditionTypeEnum.ReviewCount:
                    var reviewCount = await _unitOfWork.Repository<Domain.Entities.ProductReview>()
                        .GetQueryable()
                        .Where(pr => pr.UserId == userId && pr.Status == EntityStatusEnum.Active)
                        .CountAsync(cancellationToken);
                    return reviewCount >= (int)threshold;

                case ConditionTypeEnum.Custom:
                    // For custom conditions, always return false (needs manual assignment)
                    return false;

                default:
                    return false;
            }
        }
    }
}
