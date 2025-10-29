using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Coupon.Commands.UpdateCoupon
{
    public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, Result<CouponDto>>
    {
        private readonly ILogger<UpdateCouponCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCouponCommandHandler(
            ILogger<UpdateCouponCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CouponDto>> Handle(UpdateCouponCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result<CouponDto>.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var coupon = await _unitOfWork.Repository<Domain.Entities.Coupon>().GetByIdAsync(command.Id);
                if (coupon == null)
                {
                    return Result<CouponDto>.Failure("Coupon not found", ErrorCodeEnum.NotFound);
                }

                // Validate dates
                if (command.Request.StartDate >= command.Request.EndDate)
                {
                    return Result<CouponDto>.Failure("End date must be after start date", ErrorCodeEnum.ValidationFailed);
                }

                _mapper.Map(command.Request, coupon);
                coupon.UpdatedAt = DateTime.UtcNow;
                coupon.UpdatedBy = currentUserId;

                _unitOfWork.Repository<Domain.Entities.Coupon>().Update(coupon);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var couponDto = _mapper.Map<CouponDto>(coupon);
                return Result<CouponDto>.Success(couponDto, "Coupon updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon {CouponId}", command.Id);
                return Result<CouponDto>.Failure("Error updating coupon", ErrorCodeEnum.InternalError);
            }
        }
    }
}
