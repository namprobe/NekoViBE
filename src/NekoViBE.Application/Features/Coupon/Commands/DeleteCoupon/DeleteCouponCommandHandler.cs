using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Coupon.Commands.DeleteCoupon
{
    public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, Result>
    {
        private readonly ILogger<DeleteCouponCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCouponCommandHandler(
            ILogger<DeleteCouponCommandHandler> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var coupon = await _unitOfWork.Repository<Domain.Entities.Coupon>().GetByIdAsync(request.Id);
                if (coupon == null)
                {
                    return Result.Failure("Coupon not found", ErrorCodeEnum.NotFound);
                }

                // Check if coupon has been used
                if (coupon.CurrentUsage > 0)
                {
                    return Result.Failure("Cannot delete coupon that has been used", ErrorCodeEnum.Conflict);
                }

                _unitOfWork.Repository<Domain.Entities.Coupon>().Delete(coupon);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Coupon deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon {CouponId}", request.Id);
                return Result.Failure("Error deleting coupon", ErrorCodeEnum.InternalError);
            }
        }
    }
}
