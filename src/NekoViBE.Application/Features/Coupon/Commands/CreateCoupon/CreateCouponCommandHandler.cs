using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Coupon.Commands.CreateCoupon
{
    public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Result<CouponDto>>
    {
        private readonly ILogger<CreateCouponCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreateCouponCommandHandler(
            ILogger<CreateCouponCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CouponDto>> Handle(CreateCouponCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result<CouponDto>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                // Check if coupon code already exists
                var existingCoupons = await _unitOfWork.Repository<Domain.Entities.Coupon>()
                    .FindAsync(c => c.Code == command.Request.Code);

                if (existingCoupons.Any())
                {
                    return Result<CouponDto>.Failure("Coupon code already exists", ErrorCodeEnum.Conflict);
                }

                // Ensure discount value is zero for free shipping coupons
                if (command.Request.DiscountType == Domain.Enums.DiscountTypeEnum.FreeShipping)
                {
                    command.Request.DiscountValue = 0;
                }

                // Validate dates
                if (command.Request.StartDate >= command.Request.EndDate)
                {
                    return Result<CouponDto>.Failure("End date must be after start date", ErrorCodeEnum.ValidationFailed);
                }

                var coupon = _mapper.Map<Domain.Entities.Coupon>(command.Request);
                coupon.InitializeEntity(currentUserId);

                await _unitOfWork.Repository<Domain.Entities.Coupon>().AddAsync(coupon);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var couponDto = _mapper.Map<CouponDto>(coupon);
                return Result<CouponDto>.Success(couponDto, "Coupon created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon");
                return Result<CouponDto>.Failure("Error creating coupon", ErrorCodeEnum.InternalError);
            }
        }
    }
}
