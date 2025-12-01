using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Coupon.Commands.CollectCoupon;

public class CollectCouponCommandHandler : IRequestHandler<CollectCouponCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CollectCouponCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CollectCouponCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Result.Failure("User not authenticated", Common.Enums.ErrorCodeEnum.Unauthorized);

        // Kiểm tra coupon có tồn tại không
        var coupon = await _unitOfWork.Repository<Domain.Entities.Coupon>()
            .GetQueryable()
            .Include(c => c.UserCoupons)
            .FirstOrDefaultAsync(c => c.Id == request.CouponId, cancellationToken);

        if (coupon == null)
            return Result.Failure("Coupon not found", Common.Enums.ErrorCodeEnum.NotFound);

        // CRITICAL: Prevent collection of badge coupons
        // Badge coupons are auto-applied when badge is equipped, not manually collected
        if (coupon.IsBadgeCoupon)
            return Result.Failure("This coupon is linked to a badge and cannot be collected manually. Equip the badge to use this discount.", Common.Enums.ErrorCodeEnum.InvalidOperation);

        // Kiểm tra coupon còn hiệu lực không
        var now = DateTime.UtcNow;
        if (coupon.Status != Domain.Enums.EntityStatusEnum.Active)
            return Result.Failure("Coupon is not active", Common.Enums.ErrorCodeEnum.InvalidOperation);

        if (coupon.StartDate > now)
            return Result.Failure("Coupon has not started yet", Common.Enums.ErrorCodeEnum.InvalidOperation);

        if (coupon.EndDate < now)
            return Result.Failure("Coupon has expired", Common.Enums.ErrorCodeEnum.InvalidOperation);

        // Kiểm tra còn slot không
        if (coupon.UsageLimit.HasValue && coupon.CurrentUsage >= coupon.UsageLimit.Value)
            return Result.Failure("Coupon usage limit has been reached", Common.Enums.ErrorCodeEnum.InvalidOperation);

        // Kiểm tra user đã collect coupon này chưa
        var existingUserCoupon = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
            .GetQueryable()
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CouponId == request.CouponId && uc.UsedDate == null, cancellationToken);

        if (existingUserCoupon != null)
            return Result.Failure("You have already collected this coupon", Common.Enums.ErrorCodeEnum.DuplicateEntry);

        // Tạo UserCoupon mới
        var userCoupon = new Domain.Entities.UserCoupon
        {
            UserId = userId,
            CouponId = request.CouponId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Repository<Domain.Entities.UserCoupon>().AddAsync(userCoupon);
        
        // Tăng CurrentUsage của coupon
        coupon.CurrentUsage++;
        coupon.UpdatedAt = now;
        _unitOfWork.Repository<Domain.Entities.Coupon>().Update(coupon);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Coupon collected successfully");
    }
}
