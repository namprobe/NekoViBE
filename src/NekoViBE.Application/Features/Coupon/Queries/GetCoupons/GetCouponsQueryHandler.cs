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

namespace NekoViBE.Application.Features.Coupon.Queries.GetCoupons
{
    public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, Result<CouponsResponse>>
    {
        private readonly ILogger<GetCouponsQueryHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCouponsQueryHandler(
            ILogger<GetCouponsQueryHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CouponsResponse>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var coupons = await _unitOfWork.Repository<Domain.Entities.Coupon>().GetAllAsync();
                var couponDtos = _mapper.Map<List<CouponDto>>(coupons);

                var response = new CouponsResponse { Coupons = couponDtos };
                return Result<CouponsResponse>.Success(response, "Coupons retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupons");
                return Result<CouponsResponse>.Failure("Error retrieving coupons", ErrorCodeEnum.InternalError);
            }
        }
    }

}
