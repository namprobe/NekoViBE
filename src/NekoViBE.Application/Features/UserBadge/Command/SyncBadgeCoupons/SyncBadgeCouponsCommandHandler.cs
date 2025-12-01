using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserBadge.Command.SyncBadgeCoupons
{
    public class SyncBadgeCouponsCommandHandler : IRequestHandler<SyncBadgeCouponsCommand, Result>
    {
        private readonly ILogger<SyncBadgeCouponsCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SyncBadgeCouponsCommandHandler(
            ILogger<SyncBadgeCouponsCommandHandler> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(SyncBadgeCouponsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var targetUserId = request.UserId ?? currentUserId;

                _logger.LogInformation("🔄 Starting badge coupon sync for user {UserId}", targetUserId);

                // Get all active user badges with their linked coupons
                var userBadgesQuery = _unitOfWork.Repository<Domain.Entities.UserBadge>()
                    .GetQueryable()
                    .Include(ub => ub.Badge)
                        .ThenInclude(b => b.LinkedCoupon)
                    .Where(ub => ub.Status == EntityStatusEnum.Active);

                if (request.UserId.HasValue)
                {
                    userBadgesQuery = userBadgesQuery.Where(ub => ub.UserId == request.UserId.Value);
                }

                var userBadges = await userBadgesQuery.ToListAsync(cancellationToken);

                _logger.LogInformation("📊 Found {Count} active user badges to process", userBadges.Count);

                int syncedCount = 0;
                int skippedCount = 0;
                int errorCount = 0;

                foreach (var userBadge in userBadges)
                {
                    try
                    {
                        // Check if badge has a linked coupon
                        if (userBadge.Badge.LinkedCouponId == null)
                        {
                            _logger.LogWarning("⚠️ Badge {BadgeName} (ID: {BadgeId}) has no LinkedCouponId",
                                userBadge.Badge.Name, userBadge.BadgeId);
                            skippedCount++;
                            continue;
                        }

                        // Check if user already has this coupon
                        var existingUserCoupon = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
                            .GetQueryable()
                            .AnyAsync(uc =>
                                uc.UserId == userBadge.UserId &&
                                uc.CouponId == userBadge.Badge.LinkedCouponId.Value &&
                                uc.Status == EntityStatusEnum.Active,
                                cancellationToken);

                        if (existingUserCoupon)
                        {
                            _logger.LogDebug("⏭️ User {UserId} already has coupon for badge {BadgeName}",
                                userBadge.UserId, userBadge.Badge.Name);
                            skippedCount++;
                            continue;
                        }

                        // Create UserCoupon entry
                        var userCoupon = new Domain.Entities.UserCoupon
                        {
                            Id = Guid.NewGuid(),
                            UserId = userBadge.UserId,
                            CouponId = userBadge.Badge.LinkedCouponId.Value,
                            Status = EntityStatusEnum.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = currentUserId
                        };

                        await _unitOfWork.Repository<Domain.Entities.UserCoupon>().AddAsync(userCoupon);
                        syncedCount++;

                        _logger.LogInformation("✅ Synced coupon {CouponCode} for user {UserId} badge {BadgeName}",
                            userBadge.Badge.LinkedCoupon?.Code ?? "UNKNOWN",
                            userBadge.UserId,
                            userBadge.Badge.Name);
                    }
                    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_UserCoupons_UserId_CouponId_Active") == true)
                    {
                        _logger.LogDebug("⏭️ Duplicate key: User {UserId} already has coupon {CouponCode} (race condition caught by DB)",
                            userBadge.UserId, userBadge.Badge.LinkedCoupon?.Code ?? "UNKNOWN");
                        skippedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error syncing coupon for badge {BadgeId} user {UserId}",
                            userBadge.BadgeId, userBadge.UserId);
                        errorCount++;
                    }
                }

                if (syncedCount > 0)
                {
                    // Save in a transaction to ensure atomicity
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    // Log final verification
                    _logger.LogInformation("💾 Saved {Count} new UserCoupon entries", syncedCount);
                }

                var message = $"Badge coupon sync complete: {syncedCount} synced, {skippedCount} skipped, {errorCount} errors";
                _logger.LogInformation("🎯 {Message}", message);

                return Result.Success(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing badge coupons");
                return Result.Failure("Error syncing badge coupons", ErrorCodeEnum.InternalError);
            }
        }
    }
}
