using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.UserBadge.Command.EquipBadge
{
    public class EquipBadgeCommandHandler : IRequestHandler<EquipBadgeCommand, Result>
    {
        private readonly ILogger<EquipBadgeCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public EquipBadgeCommandHandler(
            ILogger<EquipBadgeCommandHandler> logger,
            IUnitOfWork _unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            this._unitOfWork = _unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(EquipBadgeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                _logger.LogInformation("🔵 Starting EquipBadge - UserId: {UserId}, BadgeId: {BadgeId}", currentUserId, request.BadgeId);

                // IMPORTANT: Use direct DbContext query WITH TRACKING (not AsNoTracking)
                // We need change tracking for Update() to work properly
                var dbContext = _unitOfWork.Repository<Domain.Entities.UserBadge>().GetQueryable();
                
                // Get all user's badges in one query WITH TRACKING
                var allUserBadges = await dbContext
                    .Where(ub => ub.UserId == currentUserId && ub.Status == EntityStatusEnum.Active)
                    .AsTracking() // Enable change tracking
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("🔵 Found {Count} total badges for user", allUserBadges.Count);
                foreach (var b in allUserBadges)
                {
                    _logger.LogInformation("🔵 Badge {BadgeId}: IsActive={IsActive}", b.BadgeId, b.IsActive);
                }

                // Find the badge to equip
                var userBadge = allUserBadges.FirstOrDefault(ub => ub.BadgeId == request.BadgeId);

                if (userBadge == null)
                {
                    _logger.LogWarning("❌ Badge {BadgeId} not found for user {UserId}", request.BadgeId, currentUserId);
                    return Result.Failure("Badge not found or not owned by user", ErrorCodeEnum.NotFound);
                }

                // If this badge is already equipped, no need to do anything
                if (userBadge.IsActive)
                {
                    _logger.LogInformation("⚠️ Badge {BadgeId} is already equipped", request.BadgeId);
                    return Result.Success("Badge is already equipped");
                }

                _logger.LogInformation("🔵 Unequipping all badges...");

                // Unequip ALL badges first - entities are now tracked so updates will work
                foreach (var badge in allUserBadges)
                {
                    if (badge.IsActive)
                    {
                        _logger.LogInformation("🔵 Unequipping badge {BadgeId}", badge.BadgeId);
                        badge.IsActive = false;
                        badge.UpdatedAt = DateTime.UtcNow;
                        badge.UpdatedBy = currentUserId;
                        // No need to call Update() - entities are already tracked
                    }
                }

                _logger.LogInformation("🔵 Equipping badge {BadgeId}", request.BadgeId);

                // Equip the selected badge - entity is already tracked
                userBadge.IsActive = true;
                userBadge.UpdatedAt = DateTime.UtcNow;
                userBadge.UpdatedBy = currentUserId;

                _logger.LogInformation("🔵 Saving changes to database...");
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("✅ User {UserId} successfully equipped badge {BadgeId}", currentUserId, request.BadgeId);

                return Result.Success("Badge equipped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error equipping badge {BadgeId} for user", request.BadgeId);
                return Result.Failure("Error equipping badge", ErrorCodeEnum.InternalError);
            }
        }
    }
}
