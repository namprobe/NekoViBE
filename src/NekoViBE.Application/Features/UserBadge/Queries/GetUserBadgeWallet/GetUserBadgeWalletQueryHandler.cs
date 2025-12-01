using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserBadge;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserBadge.Queries.GetUserBadgeWallet
{
    public class GetUserBadgeWalletQueryHandler : IRequestHandler<GetUserBadgeWalletQuery, Result<object>>
    {
        private readonly ILogger<GetUserBadgeWalletQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetUserBadgeWalletQueryHandler(
            ILogger<GetUserBadgeWalletQueryHandler> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<object>> Handle(GetUserBadgeWalletQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result<object>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var targetUserId = request.UserId ?? currentUserId;

                // Get user's unlocked badges
                var userBadgesQuery = _unitOfWork.Repository<Domain.Entities.UserBadge>()
                    .GetQueryable()
                    .Include(ub => ub.Badge)
                    .Where(ub => ub.UserId == targetUserId && ub.Status == EntityStatusEnum.Active);

                var userBadges = await userBadgesQuery.ToListAsync(cancellationToken);

                var unlockedBadges = userBadges.Select(ub => new UserBadgeWalletItem
                {
                    UserBadgeId = ub.Id,
                    BadgeId = ub.BadgeId,
                    Name = ub.Badge.Name,
                    Description = ub.Badge.Description,
                    IconUrl = ub.Badge.IconPath,
                    DiscountPercentage = ub.Badge.DiscountPercentage,
                    EarnedDate = ub.EarnedDate,
                    IsEquipped = ub.IsActive,
                    IsActive = ub.Status == EntityStatusEnum.Active,
                    IsTimeLimited = ub.Badge.IsTimeLimited,
                    ActivatedFrom = ub.ActivatedFrom,
                    ActivatedTo = ub.ActivatedTo,
                    ConditionType = ub.Badge.ConditionType,
                    ConditionValue = ub.Badge.ConditionValue
                }).ToList();

                if (request.Filter?.ToLower() == "all")
                {
                    // Get all available badges
                    var allBadges = await _unitOfWork.Repository<Domain.Entities.Badge>()
                        .FindAsync(b => b.Status == EntityStatusEnum.Active);

                    var unlockedBadgeIds = userBadges.Select(ub => ub.BadgeId).ToHashSet();

                    // Calculate progress for locked badges
                    var lockedBadges = new List<BadgeProgressItem>();
                    foreach (var badge in allBadges.Where(b => !unlockedBadgeIds.Contains(b.Id)))
                    {
                        var progress = await CalculateBadgeProgress(targetUserId!.Value, badge, cancellationToken);
                        lockedBadges.Add(progress);
                    }

                    return Result<object>.Success(new UserBadgeWalletResponse 
                    { 
                        Unlocked = unlockedBadges, 
                        Locked = lockedBadges 
                    });
                }

                return Result<object>.Success(unlockedBadges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user badge wallet for user {UserId}", request.UserId);
                return Result<object>.Failure("Error retrieving badge wallet", ErrorCodeEnum.InternalError);
            }
        }

        private async Task<BadgeProgressItem> CalculateBadgeProgress(
            Guid userId, 
            Domain.Entities.Badge badge, 
            CancellationToken cancellationToken)
        {
            decimal currentValue = 0;
            decimal targetValue = decimal.TryParse(badge.ConditionValue, out var target) ? target : 0;

            switch (badge.ConditionType)
            {
                case ConditionTypeEnum.TotalSpent:
                    var totalSpent = await _unitOfWork.Repository<Domain.Entities.Order>()
                        .GetQueryable()
                        .Where(o => o.UserId == userId && o.OrderStatus == OrderStatusEnum.Delivered)
                        .SumAsync(o => o.FinalAmount, cancellationToken);
                    currentValue = totalSpent;
                    break;

                case ConditionTypeEnum.OrderCount:
                    var orderCount = await _unitOfWork.Repository<Domain.Entities.Order>()
                        .GetQueryable()
                        .Where(o => o.UserId == userId && o.OrderStatus == OrderStatusEnum.Delivered)
                        .CountAsync(cancellationToken);
                    currentValue = orderCount;
                    break;

                case ConditionTypeEnum.ReviewCount:
                    var reviewCount = await _unitOfWork.Repository<Domain.Entities.ProductReview>()
                        .GetQueryable()
                        .Where(pr => pr.UserId == userId && pr.Status == EntityStatusEnum.Active)
                        .CountAsync(cancellationToken);
                    currentValue = reviewCount;
                    break;
            }

            var progressPercentage = targetValue > 0 ? (currentValue / targetValue) * 100 : 0;

            return new BadgeProgressItem
            {
                BadgeId = badge.Id,
                Name = badge.Name,
                Description = badge.Description,
                IconUrl = badge.IconPath,
                DiscountPercentage = badge.DiscountPercentage,
                Status = "Locked",
                Progress = $"{Math.Min(progressPercentage, 100):F0}%",
                CurrentValue = currentValue,
                TargetValue = targetValue,
                ConditionType = badge.ConditionType,
                ConditionValue = badge.ConditionValue,
                IsUnlocked = false
            };
        }
    }
}
