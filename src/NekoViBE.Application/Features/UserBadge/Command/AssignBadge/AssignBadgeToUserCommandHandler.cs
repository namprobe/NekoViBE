using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserBadge;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using NekoViBE.Domain.Common;


namespace NekoViBE.Application.Features.UserBadge.Command.AssignBadge
{
    public class AssignBadgeToUserCommandHandler : IRequestHandler<AssignBadgeToUserCommand, Result<UserBadgeDto>>
    {
        private readonly ILogger<AssignBadgeToUserCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public AssignBadgeToUserCommandHandler(
            ILogger<AssignBadgeToUserCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserBadgeDto>> Handle(AssignBadgeToUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result<UserBadgeDto>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                // Check if user exists
                var user = await _unitOfWork.Repository<AppUser>().GetByIdAsync(command.Request.UserId);
                if (user == null)
                {
                    return Result<UserBadgeDto>.Failure("User not found", ErrorCodeEnum.NotFound);
                }

                // Check if badge exists
                var badge = await _unitOfWork.Repository<Domain.Entities.Badge>()
                    .GetQueryable()
                    .Include(b => b.LinkedCoupon)
                    .FirstOrDefaultAsync(b => b.Id == command.Request.BadgeId, cancellationToken);
                    
                if (badge == null)
                {
                    return Result<UserBadgeDto>.Failure("Badge not found", ErrorCodeEnum.NotFound);
                }

                // Check if user already has this badge
                var existingUserBadge = await _unitOfWork.Repository<Domain.Entities.UserBadge>()
                    .FindAsync(ub => ub.UserId == command.Request.UserId &&
                                    ub.BadgeId == command.Request.BadgeId &&
                                    ub.Status == EntityStatusEnum.Active);

                if (existingUserBadge.Any())
                {
                    return Result<UserBadgeDto>.Failure("User already has this badge", ErrorCodeEnum.Conflict);
                }

                // Automatic logic for badge activation
                var (isActive, activatedFrom, activatedTo) = DetermineBadgeActivation(badge);

                var userBadge = new Domain.Entities.UserBadge
                {
                    UserId = command.Request.UserId,
                    BadgeId = command.Request.BadgeId,
                    EarnedDate = DateTime.UtcNow,
                    IsActive = isActive,           // Automatically determined
                    ActivatedFrom = activatedFrom, // Automatically determined
                    ActivatedTo = activatedTo      // Automatically determined
                };
                userBadge.InitializeEntity(currentUserId);

                await _unitOfWork.Repository<Domain.Entities.UserBadge>().AddAsync(userBadge);

                // If badge has a linked coupon, automatically collect it for the user
                if (badge.LinkedCouponId.HasValue && badge.LinkedCoupon != null)
                {
                    // Check if user already has this coupon
                    var existingUserCoupon = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
                        .GetQueryable()
                        .AnyAsync(uc => 
                            uc.UserId == command.Request.UserId && 
                            uc.CouponId == badge.LinkedCouponId.Value &&
                            uc.Status == EntityStatusEnum.Active,
                            cancellationToken);

                    if (!existingUserCoupon)
                    {
                        var userCoupon = new Domain.Entities.UserCoupon
                        {
                            Id = Guid.NewGuid(),
                            UserId = command.Request.UserId,
                            CouponId = badge.LinkedCouponId.Value,
                            Status = EntityStatusEnum.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = currentUserId
                        };

                        try
                        {
                            await _unitOfWork.Repository<Domain.Entities.UserCoupon>().AddAsync(userCoupon);
                            
                            _logger.LogInformation("🎟️ Auto-collected badge coupon {CouponCode} for user {UserId} when manually assigning badge {BadgeName}",
                                badge.LinkedCoupon.Code, command.Request.UserId, badge.Name);
                        }
                        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_UserCoupons_UserId_CouponId_Active") == true)
                        {
                            _logger.LogDebug("⏭️ Duplicate key: User {UserId} already has coupon {CouponCode} (race condition caught by DB)",
                                command.Request.UserId, badge.LinkedCoupon.Code);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var userBadgeDto = _mapper.Map<UserBadgeDto>(userBadge);
                userBadgeDto.BadgeName = badge.Name;
                userBadgeDto.BadgeDiscount = badge.DiscountPercentage;

                return Result<UserBadgeDto>.Success(userBadgeDto, "Badge assigned to user successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning badge to user");
                return Result<UserBadgeDto>.Failure("Error assigning badge to user", ErrorCodeEnum.InternalError);
            }
        }

        private (bool IsActive, DateTime? ActivatedFrom, DateTime? ActivatedTo) DetermineBadgeActivation(Domain.Entities.Badge badge)
        {
            bool isActive = true; // Default to active
            DateTime? activatedFrom = DateTime.UtcNow;
            DateTime? activatedTo = null;

            // Logic 1: If badge is time-limited, set activation period
            if (badge.IsTimeLimited)
            {
                activatedFrom = DateTime.UtcNow;
                activatedTo = badge.EndDate;

                // Check if badge is currently active based on dates
                isActive = IsBadgeCurrentlyActive(activatedFrom, activatedTo);
            }

            // Logic 2: You can add more conditions here based on badge type or business rules
            // For example, certain badge types might be inactive by default
            // or require manual activation

            // Logic 3: Based on condition type, you might want different activation logic
            switch (badge.ConditionType)
            {
                case ConditionTypeEnum.OrderCount:
                    // Purchase-based badges might be active immediately
                    isActive = true;
                    break;

                case ConditionTypeEnum.ReviewCount:
                    // Duration-based badges might need verification
                    isActive = true; // Or false if needs verification
                    break;

                case ConditionTypeEnum.TotalSpent:
                    // Referral badges might be active immediately
                    isActive = true;
                    break;

                default:
                    isActive = true;
                    break;
            }

            return (isActive, activatedFrom, activatedTo);
        }

        private bool IsBadgeCurrentlyActive(DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;

            // If no start date, badge is not active yet
            if (!fromDate.HasValue)
                return false;

            // If start date is in future, not active yet
            if (fromDate.Value > now)
                return false;

            // If no end date, badge is active indefinitely from start date
            if (!toDate.HasValue)
                return true;

            // Check if current time is within activation period
            return now >= fromDate.Value && now <= toDate.Value;
        }
    }
}


//using AutoMapper;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using NekoViBE.Application.Common.DTOs.UserBadge;
//using NekoViBE.Application.Common.Enums;
//using NekoViBE.Application.Common.Interfaces;
//using NekoViBE.Application.Common.Models;
//using NekoViBE.Domain.Enums;
//using NekoViBE.Domain.Common;



//namespace NekoViBE.Application.Features.UserBadge.Command.AssignBadge
//{
//    public class AssignBadgeToUserCommandHandler : IRequestHandler<AssignBadgeToUserCommand, Result>
//    {
//        private readonly ILogger<AssignBadgeToUserCommandHandler> _logger;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;
//        private readonly ICurrentUserService _currentUserService;

//        public AssignBadgeToUserCommandHandler(
//            ILogger<AssignBadgeToUserCommandHandler> logger,
//            IUnitOfWork unitOfWork,
//            IMapper mapper,
//            ICurrentUserService currentUserService)
//        {
//            _logger = logger;
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;
//            _currentUserService = currentUserService;
//        }

//        public async Task<Result> Handle(AssignBadgeToUserCommand command, CancellationToken cancellationToken)
//        {
//            try
//            {
//                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
//                if (!isValid)
//                {
//                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
//                }

//                // Check if user exists
//                var user = await _unitOfWork.Repository<Domain.Entities.AppUser>().GetByIdAsync(command.Request.UserId);
//                if (user == null)
//                {
//                    return Result.Failure("User not found", ErrorCodeEnum.NotFound);
//                }

//                // Check if badge exists
//                var badge = await _unitOfWork.Repository<Domain.Entities.Badge>().GetByIdAsync(command.Request.BadgeId);
//                if (badge == null)
//                {
//                    return Result.Failure("Badge not found", ErrorCodeEnum.NotFound);
//                }

//                // Check if user already has this badge
//                var existingUserBadge = await _unitOfWork.Repository<Domain.Entities.UserBadge>()
//                    .FindAsync(ub => ub.UserId == command.Request.UserId && ub.BadgeId == command.Request.BadgeId && ub.Status == EntityStatusEnum.Active);

//                if (existingUserBadge.Any())
//                {
//                    return Result.Failure("User already has this badge", ErrorCodeEnum.Conflict);
//                }

//                var userBadge = new Domain.Entities.UserBadge
//                {
//                    UserId = command.Request.UserId,
//                    BadgeId = command.Request.BadgeId,
//                    EarnedDate = DateTime.UtcNow,
//                    IsActive = command.Request.IsActive,
//                    ActivatedFrom = command.Request.ActivatedFrom,
//                    ActivatedTo = command.Request.ActivatedTo
//                };
//                userBadge.InitializeEntity(currentUserId);

//                await _unitOfWork.Repository<Domain.Entities.UserBadge>().AddAsync(userBadge);
//                await _unitOfWork.SaveChangesAsync(cancellationToken);

//                var userBadgeDto = _mapper.Map<UserBadgeDto>(userBadge);
//                userBadgeDto.BadgeName = badge.Name;
//                userBadgeDto.BadgeDiscount = badge.DiscountPercentage;

//                return Result.Success("Badge assigned to user successfully");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error assigning badge to user");
//                return Result.Failure("Error assigning badge to user", ErrorCodeEnum.InternalError);
//            }
//        }
//    }

//}
