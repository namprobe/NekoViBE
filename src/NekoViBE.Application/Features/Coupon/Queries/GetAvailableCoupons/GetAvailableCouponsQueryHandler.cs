using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Coupon.Queries.GetAvailableCoupons;

public class GetAvailableCouponsQueryHandler : IRequestHandler<GetAvailableCouponsQuery, Result<AvailableCouponsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAvailableCouponsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AvailableCouponsResponse>> Handle(GetAvailableCouponsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        
        // Lấy userId nếu user đã đăng nhập
        Guid? userId = null;
        var userIdString = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        // Lấy tất cả coupon còn hiệu lực
        var coupons = await _unitOfWork.Repository<Domain.Entities.Coupon>()
            .GetQueryable()
            .Include(c => c.UserCoupons)
            .Where(c => c.Status == Domain.Enums.EntityStatusEnum.Active && 
                        c.StartDate <= now && 
                        c.EndDate >= now &&
                        (c.UsageLimit == null || c.CurrentUsage < c.UsageLimit))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = new AvailableCouponsResponse
        {
            Coupons = coupons.Select(c => new AvailableCouponItem
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType.ToString(),
                DiscountValue = c.DiscountValue,
                MinOrderAmount = c.MinOrderAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                CurrentUsage = c.CurrentUsage,
                RemainingSlots = c.UsageLimit.HasValue ? c.UsageLimit.Value - c.CurrentUsage : int.MaxValue,
                IsCollected = userId.HasValue && c.UserCoupons.Any(uc => uc.UserId == userId.Value && uc.UsedDate == null)
            }).ToList()
        };

        return Result<AvailableCouponsResponse>.Success(response);
    }
}
