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

                // Get all active badges
                var allBadges = await _unitOfWork.Repository<Domain.Entities.Badge>()
                    .FindAsync(b => b.Status == EntityStatusEnum.Active);

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
