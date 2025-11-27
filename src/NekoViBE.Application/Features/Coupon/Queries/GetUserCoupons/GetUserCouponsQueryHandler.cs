using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Coupon.Queries.GetUserCoupons;

public class GetUserCouponsQueryHandler : IRequestHandler<GetUserCouponsQuery, Result<UserCouponsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUserCouponsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserCouponsResponse>> Handle(GetUserCouponsQuery request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Result<UserCouponsResponse>.Failure("User not authenticated", Common.Enums.ErrorCodeEnum.Unauthorized);

        var now = DateTime.UtcNow;

        // Lấy tất cả coupon mà user đã collect
        var userCoupons = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
            .GetQueryable()
            .Include(uc => uc.Coupon)
            .Where(uc => uc.UserId == userId)
            .OrderByDescending(uc => uc.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = new UserCouponsResponse
        {
            Coupons = userCoupons.Select(uc => new UserCouponItem
            {
                UserCouponId = uc.Id,
                CouponId = uc.CouponId,
                Code = uc.Coupon.Code,
                Description = uc.Coupon.Description,
                DiscountType = uc.Coupon.DiscountType.ToString(),
                DiscountValue = uc.Coupon.DiscountValue,
                MinOrderAmount = uc.Coupon.MinOrderAmount,
                StartDate = uc.Coupon.StartDate,
                EndDate = uc.Coupon.EndDate,
                CollectedDate = (DateTime)uc.CreatedAt,
                UsedDate = uc.UsedDate,
                IsUsed = uc.UsedDate.HasValue,
                IsExpired = uc.Coupon.EndDate < now
            }).ToList()
        };

        return Result<UserCouponsResponse>.Success(response);
    }
}
