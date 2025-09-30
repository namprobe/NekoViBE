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

namespace NekoViBE.Application.Features.Coupon.Queries.GetCouponById
{
    public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, Result<CouponDto>>
    {
        private readonly ILogger<GetCouponByIdQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCouponByIdQueryHandler(
            ILogger<GetCouponByIdQueryHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CouponDto>> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var coupon = await _unitOfWork.Repository<Domain.Entities.Coupon>().GetByIdAsync(request.Id);
                if (coupon == null)
                {
                    return Result<CouponDto>.Failure("Coupon not found", ErrorCodeEnum.NotFound);
                }

                var couponDto = _mapper.Map<CouponDto>(coupon);
                return Result<CouponDto>.Success(couponDto, "Coupon retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon {CouponId}", request.Id);
                return Result<CouponDto>.Failure("Error retrieving coupon", ErrorCodeEnum.InternalError);
            }
        }
    }
}
