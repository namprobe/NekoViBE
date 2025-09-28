using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Coupon.Queries.GetCouponByCode
{
    public class GetCouponByCodeQueryHandler : IRequestHandler<GetCouponByCodeQuery, Result<CouponDto>>
    {
        private readonly ILogger<GetCouponByCodeQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCouponByCodeQueryHandler(
            ILogger<GetCouponByCodeQueryHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CouponDto>> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var coupons = await _unitOfWork.Repository<Domain.Entities.Coupon>()
                    .FindAsync(c => c.Code == request.Code && c.Status == EntityStatusEnum.Active);

                var coupon = coupons.FirstOrDefault();
                if (coupon == null)
                {
                    return Result<CouponDto>.Failure("Coupon not found or inactive", ErrorCodeEnum.NotFound);
                }

                var couponDto = _mapper.Map<CouponDto>(coupon);
                return Result<CouponDto>.Success(couponDto, "Coupon retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon by code {Code}", request.Code);
                return Result<CouponDto>.Failure("Error retrieving coupon", ErrorCodeEnum.InternalError);
            }
        }
    }
}
